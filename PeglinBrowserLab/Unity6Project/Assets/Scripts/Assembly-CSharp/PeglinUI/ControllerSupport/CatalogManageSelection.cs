using System;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.ControllerSupport
{
	public class CatalogManageSelection : MonoBehaviour
	{
		[Header("Scene Refs")]
		public GameObject leftPanel;

		public GameObject rightPanel;

		public GameObject leftPanelButtonImage;

		public GameObject rightPanelButtonImage;

		private GameObject _selectedPanel;

		private GameObject _selectedGameObjectLeftPanel;

		private GameObject _selectedGameObjectRightPanel;

		private Player _player;

		private void OnEnable()
		{
			_selectedGameObjectLeftPanel = null;
			_selectedGameObjectRightPanel = null;
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(0);
			}
			EventSystem.current.SetSelectedGameObject(leftPanel);
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
			InputTypeChanged();
		}

		private void OnDisable()
		{
			_selectedPanel = null;
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
		}

		private void Update()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				if (_player.GetButtonDown("CatalogPanelTabRight"))
				{
					_selectedGameObjectLeftPanel = leftPanel.GetComponent<SelectableObjectContainer>().selectableObject;
					SetSelectedPanel(rightPanel, ref _selectedGameObjectRightPanel);
					rightPanelButtonImage.SetActive(value: false);
					leftPanelButtonImage.SetActive(value: true);
				}
				else if (_player.GetButtonDown("CatalogPanelTabLeft"))
				{
					_selectedGameObjectRightPanel = rightPanel.GetComponent<SelectableObjectContainer>().selectableObject;
					SetSelectedPanel(leftPanel, ref _selectedGameObjectLeftPanel);
					rightPanelButtonImage.SetActive(value: true);
					leftPanelButtonImage.SetActive(value: false);
				}
			}
		}

		private void SetSelectedPanel(GameObject panel, ref GameObject panelSelectedGameObject)
		{
			_selectedPanel = panel;
			if (panelSelectedGameObject == null)
			{
				GameObject selectableObject = panel.GetComponent<SelectableObjectContainer>().selectableObject;
				EventSystem.current.SetSelectedGameObject(selectableObject);
				panelSelectedGameObject = selectableObject;
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(panelSelectedGameObject);
			}
		}

		private void InputTypeChanged()
		{
			if (ControllerInputManager.IsPointerBased)
			{
				leftPanelButtonImage.SetActive(value: false);
				rightPanelButtonImage.SetActive(value: false);
			}
			else if (_selectedPanel == null)
			{
				rightPanelButtonImage.SetActive(value: false);
				leftPanelButtonImage.SetActive(value: true);
				SetSelectedPanel(rightPanel, ref _selectedGameObjectRightPanel);
			}
			else
			{
				leftPanelButtonImage.SetActive(_selectedPanel == rightPanel);
				rightPanelButtonImage.SetActive(_selectedPanel == leftPanel);
			}
		}
	}
}
