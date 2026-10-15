using System;
using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScenarioAnswerButton : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[HideInInspector]
	public int index = -1;

	public TextMeshProUGUI buttonTextContainer;

	public Image pointerImage;

	public Image controllerButtonImage;

	private Button _button;

	private RewiredEventSystem _rewiredEventSystem;

	private void OnEnable()
	{
		_button = GetComponent<Button>();
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(HandleInputTypeChange));
		HandleInputTypeChange();
	}

	private void OnDisable()
	{
		ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(HandleInputTypeChange));
	}

	private void HandleInputTypeChange()
	{
		if (!ControllerInputManager.IsPointerBased && _rewiredEventSystem.currentSelectedGameObject == base.gameObject)
		{
			controllerButtonImage.gameObject.SetActive(value: true);
		}
		pointerImage.enabled = ControllerInputManager.IsPointerBased;
	}

	public void OnSelect(BaseEventData eventData)
	{
		controllerButtonImage.gameObject.SetActive(ControllerInputManager.primaryInputType != InputType.Mouse);
		pointerImage.enabled = ControllerInputManager.primaryInputType == InputType.Mouse;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		controllerButtonImage.gameObject.SetActive(value: false);
		pointerImage.enabled = ControllerInputManager.primaryInputType == InputType.Mouse;
	}
}
