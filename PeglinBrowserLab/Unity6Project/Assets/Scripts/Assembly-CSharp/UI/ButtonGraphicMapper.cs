using System;
using PeglinUI.ControllerSupport;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ButtonGraphicMapper : MonoBehaviour
	{
		[SerializeField]
		private bool _hideIfUsingMouse;

		[Tooltip("If we want to have text in place of this button")]
		[SerializeField]
		private TMP_Text _mouseText;

		[Tooltip("Is there a Mouse graphic we want to hide if this is controller?")]
		[SerializeField]
		private Image _mouseIcon;

		[Tooltip("Effectively what type of button this is. Accept, Back, etc.")]
		[SerializeField]
		public ControllerIconMapping _controllerIconMapping;

		[Tooltip("How long to toggle the up & down state for anim. -1 for no anim.")]
		[SerializeField]
		private float _animationFrameTime = 0.65f;

		private Image _image;

		private Sprite _primarySprite;

		private Sprite _secondarySprite;

		private float _toggleTimer;

		private bool _isPrimary = true;

		private void OnEnable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
			_image = GetComponent<Image>();
			UpdateInputType();
		}

		private void OnDisable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
		}

		private void Start()
		{
			UpdateInputType();
		}

		public void SetIconMapping(ControllerIconMapping mapping)
		{
			_controllerIconMapping = mapping;
			UpdateInputType();
		}

		private void Update()
		{
			if (_image.enabled && SettingsManager.Instance != null && SettingsManager.Instance.AnimateButtonPrompts == SettingsManager.ButtonPromptsStyle.Off)
			{
				_image.enabled = false;
			}
			else if (_hideIfUsingMouse && ControllerInputManager.primaryInputType == InputType.Mouse)
			{
				_image.enabled = false;
			}
			else if (!_image.enabled && SettingsManager.Instance != null && SettingsManager.Instance.AnimateButtonPrompts != SettingsManager.ButtonPromptsStyle.Off && ControllerInputManager.primaryInputType != InputType.Touch)
			{
				_image.enabled = true;
			}
			if ((SettingsManager.Instance == null || SettingsManager.Instance.AnimateButtonPrompts == SettingsManager.ButtonPromptsStyle.Animated) && _image.enabled && _animationFrameTime > 0f && _secondarySprite != null)
			{
				_toggleTimer -= Time.unscaledDeltaTime;
				if (_toggleTimer < 0f)
				{
					if (_isPrimary)
					{
						_image.sprite = _secondarySprite;
					}
					else
					{
						_image.sprite = _primarySprite;
					}
					_isPrimary = !_isPrimary;
					_toggleTimer = _animationFrameTime;
				}
			}
			else if ((SettingsManager.Instance == null || SettingsManager.Instance.AnimateButtonPrompts == SettingsManager.ButtonPromptsStyle.Static) && !_isPrimary)
			{
				_image.sprite = _primarySprite;
				_isPrimary = true;
			}
		}

		private void UpdateInputType()
		{
			switch (ControllerInputManager.primaryInputType)
			{
			case InputType.Controller:
				SetButtonsForController();
				_image.enabled = true;
				break;
			case InputType.Mouse:
			case InputType.Keyboard:
				_primarySprite = _controllerIconMapping.keyboardIcon;
				_secondarySprite = _controllerIconMapping.keyboardIconPressed;
				_image.enabled = true;
				break;
			case InputType.Touch:
				_image.enabled = false;
				break;
			}
			_image.sprite = _primarySprite;
			_isPrimary = true;
			_toggleTimer = _animationFrameTime;
			if (_mouseText != null)
			{
				_image.enabled = false;
				_mouseText.gameObject.SetActive(ControllerInputManager.primaryInputType == InputType.Mouse);
			}
			if (_mouseIcon != null)
			{
				_mouseIcon.enabled = ControllerInputManager.primaryInputType == InputType.Mouse;
			}
			if (_image.enabled && SettingsManager.Instance != null && SettingsManager.Instance.AnimateButtonPrompts == SettingsManager.ButtonPromptsStyle.Off)
			{
				_image.enabled = false;
			}
		}

		private void SetButtonsForController()
		{
			switch (Application.platform)
			{
			case RuntimePlatform.Switch:
				_primarySprite = _controllerIconMapping.switchIcon;
				_secondarySprite = _controllerIconMapping.switchIconPressed;
				break;
			case RuntimePlatform.XboxOne:
				_primarySprite = _controllerIconMapping.xboxIcon;
				_secondarySprite = _controllerIconMapping.xboxIconPressed;
				break;
			case RuntimePlatform.PS4:
				_primarySprite = _controllerIconMapping.psIcon;
				_secondarySprite = _controllerIconMapping.psIconPressed;
				break;
			default:
				switch (SettingsManager.Instance.ControllerInput)
				{
				case SettingsManager.PCControllerInputTypes.Xbox:
					_primarySprite = _controllerIconMapping.xboxIcon;
					_secondarySprite = _controllerIconMapping.xboxIconPressed;
					break;
				case SettingsManager.PCControllerInputTypes.Playstation:
					_primarySprite = _controllerIconMapping.psIcon;
					_secondarySprite = _controllerIconMapping.psIconPressed;
					break;
				case SettingsManager.PCControllerInputTypes.Switch:
					_primarySprite = _controllerIconMapping.switchIcon;
					_secondarySprite = _controllerIconMapping.switchIconPressed;
					break;
				default:
					SetButtonsFromJoystickDetection();
					break;
				}
				break;
			}
			if (_mouseIcon != null)
			{
				_mouseIcon.enabled = false;
			}
		}

		private void SetButtonsFromJoystickDetection()
		{
			switch (ReInput.controllers.GetJoystick(0)?.hardwareTypeGuid.ToString() ?? "")
			{
			case "c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb":
			case "71dfe6c8-9e81-428f-a58e-c7e664b7fbed":
			case "cd9718bf-a87a-44bc-8716-60a0def28a9f":
			case "5286706d-19b4-4a45-b635-207ce78d8394":
			case "d2aef070-7caa-42ff-b2dc-daac7e4a62b4":
			case "a75d195b-27a8-41ac-97db-2bd5a649a817":
			case "1b5a521b-6833-4c54-ab6c-bac653c93e9c":
			case "7c338d42-ec21-4402-84ed-7ab547343c19":
			case "274096a0-b4d5-413f-bb4c-7dd68cae6f0f":
			case "65ea105c-6390-4d11-a49b-13a402b1f2d9":
				_primarySprite = _controllerIconMapping.psIcon;
				_secondarySprite = _controllerIconMapping.psIconPressed;
				break;
			case "3eb01142-da0e-4a86-8ae8-a15c2b1f2a04":
			case "605dc720-1b38-473d-a459-67d5857aa6ea":
			case "521b808c-0248-4526-bc10-f1d16ee76bf1":
			case "1fbdd13b-0795-4173-8a95-a2a75de9d204":
			case "7bf3154b-9db8-4d52-950f-cd0eed8a5819":
				_primarySprite = _controllerIconMapping.switchIcon;
				_secondarySprite = _controllerIconMapping.switchIconPressed;
				break;
			default:
				_primarySprite = _controllerIconMapping.xboxIcon;
				_secondarySprite = _controllerIconMapping.xboxIconPressed;
				break;
			}
		}
	}
}
