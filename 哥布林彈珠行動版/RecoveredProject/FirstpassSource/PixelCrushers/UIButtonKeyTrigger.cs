using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(Selectable))]
	public class UIButtonKeyTrigger : MonoBehaviour
	{
		[Tooltip("Trigger the selectable when this key is pressed.")]
		public KeyCode key;

		[Tooltip("Trigger the selectable when this input button is pressed.")]
		public string buttonName = string.Empty;

		[Tooltip("Trigger if any key, input button, or mouse button is pressed.")]
		public bool anyKeyOrButton;

		[Tooltip("Ignore trigger key/button if UI button is being clicked Event System's Submit input. Prevents unintentional double clicks.")]
		public bool skipIfBeingClickedBySubmit = true;

		[Tooltip("Visually show UI Button in pressed state when triggered.")]
		public bool simulateButtonClick = true;

		[Tooltip("Show pressed state for this duration in seconds.")]
		public float simulateButtonDownDuration = 0.1f;

		private Selectable m_selectable;

		public static bool monitorInput = true;

		protected Selectable selectable
		{
			get
			{
				return m_selectable;
			}
			set
			{
				m_selectable = value;
			}
		}

		protected virtual void Awake()
		{
			m_selectable = GetComponent<Selectable>();
			if (m_selectable == null)
			{
				base.enabled = false;
			}
		}

		protected void Update()
		{
			if (monitorInput && m_selectable.enabled && m_selectable.interactable && m_selectable.gameObject.activeInHierarchy && (InputDeviceManager.IsKeyDown(key) || (!string.IsNullOrEmpty(buttonName) && InputDeviceManager.IsButtonDown(buttonName)) || (anyKeyOrButton && InputDeviceManager.IsAnyKeyDown())) && (!skipIfBeingClickedBySubmit || !IsBeingClickedBySubmit()))
			{
				Click();
			}
		}

		protected virtual bool IsBeingClickedBySubmit()
		{
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == m_selectable.gameObject && InputDeviceManager.instance != null)
			{
				return InputDeviceManager.IsButtonDown(InputDeviceManager.instance.submitButton);
			}
			return false;
		}

		protected virtual void Click()
		{
			if (simulateButtonClick)
			{
				StartCoroutine(SimulateButtonClick());
			}
			else
			{
				ExecuteEvents.Execute(m_selectable.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
			}
		}

		protected IEnumerator SimulateButtonClick()
		{
			m_selectable.OnPointerDown(new PointerEventData(EventSystem.current));
			for (float timeLeft = simulateButtonDownDuration; timeLeft > 0f; timeLeft -= Time.unscaledDeltaTime)
			{
				yield return null;
			}
			m_selectable.OnPointerUp(new PointerEventData(EventSystem.current));
			m_selectable.OnDeselect(null);
			ExecuteEvents.Execute(m_selectable.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
		}
	}
}
