using System;
using PeglinUI.ControllerSupport;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class TabManager : MonoBehaviour
	{
		public Tab[] tabs;

		public Color enabledColor;

		public Color disabledColor;

		public GameObject leftBumperIcon;

		public GameObject rightBumperIcon;

		private int _selectedTabIndex;

		private Player _player;

		private RewiredEventSystem _rewiredEventSystem;

		private void Awake()
		{
			_player = ReInput.players.GetPlayer(0);
			Tab[] array = tabs;
			foreach (Tab t in array)
			{
				t.TabButton.onClick.AddListener(() =>
				{
					SwapTab(t);
				});
			}
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			InputTypeChanged();
			SwapTab(tabs[0]);
		}

		private void OnEnable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
		}

		private void OnDisable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
		}

		private void Update()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				if (_player.GetButtonDown("MenuTabRight"))
				{
					_selectedTabIndex = ((_selectedTabIndex + 1 < tabs.Length) ? (_selectedTabIndex + 1) : 0);
					SwapTab(tabs[_selectedTabIndex]);
				}
				else if (_player.GetButtonDown("MenuTabLeft"))
				{
					_selectedTabIndex = ((_selectedTabIndex - 1 < 0) ? (tabs.Length - 1) : (_selectedTabIndex - 1));
					SwapTab(tabs[_selectedTabIndex]);
				}
			}
		}

		private void SwapTab(Tab targetTab)
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				Tab tab = tabs[i];
				if (tab == targetTab)
				{
					tab.TabContent.SetActive(value: true);
					_rewiredEventSystem.SetSelectedGameObject(FindFirstSelectableInChildren(tab.TabContent));
					tab.TabContentBackground.enabled = true;
					tab.TabBackground.color = enabledColor;
					_selectedTabIndex = i;
				}
				else
				{
					tab.TabContent.SetActive(value: false);
					tab.TabContentBackground.enabled = false;
					tab.TabBackground.color = disabledColor;
				}
			}
		}

		private void InputTypeChanged()
		{
			if (leftBumperIcon != null)
			{
				leftBumperIcon.SetActive(!ControllerInputManager.IsPointerBased);
			}
			if (leftBumperIcon != null)
			{
				rightBumperIcon.SetActive(!ControllerInputManager.IsPointerBased);
			}
		}

		private static GameObject FindFirstSelectableInChildren(GameObject parent)
		{
			Selectable[] componentsInChildren = parent.GetComponentsInChildren<Selectable>();
			foreach (Selectable selectable in componentsInChildren)
			{
				if (selectable.gameObject != parent)
				{
					return selectable.gameObject;
				}
			}
			Debug.LogWarning("Did not find a selectable in the tab children");
			return null;
		}
	}
}
