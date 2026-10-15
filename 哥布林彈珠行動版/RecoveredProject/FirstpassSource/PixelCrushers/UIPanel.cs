using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class UIPanel : MonoBehaviour
	{
		public enum StartState
		{
			GameObjectState,
			Open,
			Closed
		}

		public enum PanelState
		{
			Uninitialized,
			Opening,
			Open,
			Closing,
			Closed
		}

		[Tooltip("When enabling the panel, select this if input device is Joystick or Keyboard.")]
		public GameObject firstSelected;

		[Tooltip("If non-zero, seconds between checks to ensure that one of the panel's Selectables is focused when this panel is active and on top.")]
		public float focusCheckFrequency = 0.2f;

		[Tooltip("If non-zero, refresh list of Selectables at this frequency when this panel is active and on top. Use if Selectables are added dynamically.")]
		public float refreshSelectablesFrequency;

		[Tooltip("Reselect previous selectable when disabling this panel.")]
		public bool selectPreviousOnDisable = true;

		[Tooltip("When opening, set this animator trigger.")]
		public string showAnimationTrigger = "Show";

		[Tooltip("When closing, set this animator trigger.")]
		public string hideAnimationTrigger = "Hide";

		[Tooltip("Normally the panel considers itself open at start if the GameObject starts active (GameObjectState). To explicitly specify whether the panel should start open or closed, select Open or Closed from the dropdown.")]
		public StartState startState;

		[Tooltip("Do not set panel state to Open until Show animation has finished.")]
		public bool waitForShowAnimationToSetOpen;

		[Tooltip("Deactivate panel GameObject when panel is closed.")]
		[SerializeField]
		protected bool m_deactivateOnHidden = true;

		public UnityEvent onOpen = new UnityEvent();

		public UnityEvent onClose = new UnityEvent();

		public UnityEvent onClosed = new UnityEvent();

		public UnityEvent onBackButtonDown = new UnityEvent();

		protected GameObject m_previousSelected;

		protected GameObject m_lastSelected;

		protected List<GameObject> selectables = new List<GameObject>();

		private float m_timeNextCheck;

		private float m_timeNextRefresh;

		private int m_frameLastOpened = -1;

		public static bool monitorSelection = true;

		protected static List<UIPanel> panelStack = new List<UIPanel>();

		private PanelState m_panelState;

		private UIAnimatorMonitor m_animatorMonitor;

		public bool deactivateOnHidden
		{
			get
			{
				return m_deactivateOnHidden;
			}
			set
			{
				m_deactivateOnHidden = value;
			}
		}

		protected static UIPanel topPanel
		{
			get
			{
				if (panelStack.Count <= 0)
				{
					return null;
				}
				return panelStack[panelStack.Count - 1];
			}
		}

		public PanelState panelState
		{
			get
			{
				return m_panelState;
			}
			set
			{
				m_panelState = value;
			}
		}

		public virtual bool waitForShowAnimation
		{
			get
			{
				return waitForShowAnimationToSetOpen;
			}
			set
			{
				waitForShowAnimationToSetOpen = value;
			}
		}

		public bool isOpen
		{
			get
			{
				if (panelState != PanelState.Opening && panelState != PanelState.Open)
				{
					if (panelState == PanelState.Uninitialized)
					{
						return base.gameObject.activeInHierarchy;
					}
					return false;
				}
				return true;
			}
		}

		public UIAnimatorMonitor animatorMonitor
		{
			get
			{
				if (m_animatorMonitor == null)
				{
					m_animatorMonitor = new UIAnimatorMonitor(base.gameObject);
				}
				return m_animatorMonitor;
			}
		}

		protected virtual void Start()
		{
			if (panelState != 0)
			{
				return;
			}
			switch (startState)
			{
			case StartState.Open:
				panelState = PanelState.Opening;
				base.gameObject.SetActive(value: true);
				RefreshSelectablesList();
				animatorMonitor.SetTrigger(showAnimationTrigger, OnVisible, wait: false);
				return;
			case StartState.Closed:
				panelState = PanelState.Closed;
				if (deactivateOnHidden)
				{
					base.gameObject.SetActive(value: false);
				}
				return;
			}
			if (base.gameObject.activeInHierarchy)
			{
				panelState = PanelState.Opening;
				RefreshSelectablesList();
				animatorMonitor.SetTrigger(showAnimationTrigger, OnVisible, wait: false);
			}
			else
			{
				panelState = PanelState.Closed;
			}
		}

		public void RefreshSelectablesList()
		{
			selectables.Clear();
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
			foreach (Selectable selectable in componentsInChildren)
			{
				if (selectable.IsActive() && selectable.IsInteractable())
				{
					selectables.Add(selectable.gameObject);
				}
			}
		}

		public void RefreshAfterOneFrame()
		{
			StartCoroutine(RefreshAfterOneFrameCoroutine());
		}

		private IEnumerator RefreshAfterOneFrameCoroutine()
		{
			yield return null;
			RefreshSelectablesList();
		}

		protected void PushToPanelStack()
		{
			if (panelStack.Contains(this))
			{
				panelStack.Remove(this);
			}
			panelStack.Add(this);
		}

		protected void PopFromPanelStack()
		{
			panelStack.Remove(this);
		}

		public void TakeFocus()
		{
			PushToPanelStack();
			RefreshSelectablesList();
			CheckFocus();
		}

		protected virtual void OnEnable()
		{
			PushToPanelStack();
			RefreshAfterOneFrame();
		}

		protected virtual void OnDisable()
		{
			StopAllCoroutines();
			if (monitorSelection && selectPreviousOnDisable && InputDeviceManager.autoFocus && EventSystem.current != null && m_previousSelected != null && !selectables.Contains(m_previousSelected))
			{
				EventSystem.current.SetSelectedGameObject(m_previousSelected);
			}
			PopFromPanelStack();
		}

		public virtual void Open()
		{
			if (panelState != PanelState.Open && panelState != PanelState.Opening)
			{
				if (panelState == PanelState.Closing)
				{
					animatorMonitor.CancelCurrentAnimation();
				}
				m_frameLastOpened = Time.frameCount;
				panelState = PanelState.Opening;
				base.gameObject.SetActive(value: true);
				onOpen.Invoke();
				animatorMonitor.SetTrigger(showAnimationTrigger, OnVisible, waitForShowAnimation);
				PushToPanelStack();
			}
		}

		public virtual void Close()
		{
			PopFromPanelStack();
			if (base.gameObject.activeInHierarchy)
			{
				CancelInvoke();
			}
			if (panelState != PanelState.Closed && panelState != PanelState.Closing)
			{
				panelState = PanelState.Closing;
				onClose.Invoke();
				animatorMonitor.SetTrigger(hideAnimationTrigger, OnHidden);
				if (EventSystem.current != null && selectables.Contains(EventSystem.current.currentSelectedGameObject))
				{
					EventSystem.current.SetSelectedGameObject(null);
				}
			}
		}

		public virtual void SetOpen(bool value)
		{
			if (value)
			{
				Open();
			}
			else
			{
				Close();
			}
		}

		public virtual void Toggle()
		{
			if (isOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		protected virtual void OnVisible()
		{
			panelState = PanelState.Open;
			RefreshSelectablesList();
			m_previousSelected = ((EventSystem.current != null) ? EventSystem.current.currentSelectedGameObject : null);
			if (InputDeviceManager.autoFocus && firstSelected != null && m_previousSelected != null && !selectables.Contains(m_previousSelected))
			{
				Selectable component = m_previousSelected.GetComponent<Selectable>();
				if (component != null)
				{
					component.OnDeselect(null);
				}
			}
		}

		protected virtual void OnHidden()
		{
			panelState = PanelState.Closed;
			if (deactivateOnHidden)
			{
				base.gameObject.SetActive(value: false);
			}
			onClosed.Invoke();
		}

		protected virtual void Update()
		{
			if (!isOpen || !(topPanel == this))
			{
				return;
			}
			if (InputDeviceManager.isBackButtonDown)
			{
				if (Time.frameCount != m_frameLastOpened)
				{
					onBackButtonDown.Invoke();
				}
				return;
			}
			EventSystem current = EventSystem.current;
			if (current != null)
			{
				GameObject currentSelectedGameObject = current.currentSelectedGameObject;
				if (currentSelectedGameObject != null && selectables.Contains(currentSelectedGameObject))
				{
					m_lastSelected = currentSelectedGameObject;
				}
				if (Time.realtimeSinceStartup >= m_timeNextCheck && focusCheckFrequency > 0f && topPanel == this && InputDeviceManager.autoFocus)
				{
					m_timeNextCheck = Time.realtimeSinceStartup + focusCheckFrequency;
					CheckFocus();
				}
				if (Time.realtimeSinceStartup >= m_timeNextRefresh && refreshSelectablesFrequency > 0f && topPanel == this && InputDeviceManager.autoFocus)
				{
					m_timeNextRefresh = Time.realtimeSinceStartup + refreshSelectablesFrequency;
					RefreshSelectablesList();
				}
			}
		}

		public virtual void SetFocus(GameObject selectable)
		{
			firstSelected = null;
			m_lastSelected = selectable;
			if (InputDeviceManager.autoFocus)
			{
				EventSystem.current.SetSelectedGameObject(null);
				UIUtility.Select(m_lastSelected.GetComponent<Selectable>());
				CheckFocus();
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(selectable);
			}
		}

		public virtual void CheckFocus()
		{
			if (!monitorSelection || !InputDeviceManager.autoFocus || EventSystem.current == null || topPanel != this)
			{
				return;
			}
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject == null || !selectables.Contains(currentSelectedGameObject))
			{
				GameObject gameObject = null;
				if (m_lastSelected != null && selectables.Contains(m_lastSelected))
				{
					gameObject = m_lastSelected;
				}
				else
				{
					Selectable selectable = ((firstSelected != null) ? firstSelected.GetComponent<Selectable>() : null);
					gameObject = ((selectable != null && selectable.IsActive() && selectable.IsInteractable()) ? firstSelected : GetFirstInteractableButton());
				}
				if (gameObject != null)
				{
					EventSystem.current.SetSelectedGameObject(gameObject);
				}
			}
		}

		protected GameObject GetFirstInteractableButton()
		{
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
			foreach (Selectable selectable in componentsInChildren)
			{
				if (selectable.interactable)
				{
					return selectable.gameObject;
				}
			}
			return null;
		}
	}
}
