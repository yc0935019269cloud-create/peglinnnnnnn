using Rewired;
using UnityEngine;

namespace PeglinUI.ControllerSupport
{
	public class ControllerInputManager : MonoBehaviour
	{
		public delegate void InputTypeChanged();

		private const string CONFIRM_TO_FIRE_SETTING_STRING = "ConfirmToFireSetting";

		public static InputTypeChanged OnInputTypeChanged = () =>
		{
		};

		public static InputType primaryInputType = InputType.Controller;

		private static bool _requireConfirmationToFireSetting = false;

		private float _buttonHoldTimeForKeyboardChange = 0.5f;

		private float _buttonHoldTimer;

		private int _consecutiveKeyPresses;

		private Player _player;

		public static bool IsPointerBased
		{
			get
			{
				if (primaryInputType != InputType.Mouse)
				{
					return primaryInputType == InputType.Touch;
				}
				return true;
			}
		}

		public static bool RequireConfirmationToFireSetting
		{
			get
			{
				return _requireConfirmationToFireSetting;
			}
			set
			{
				_requireConfirmationToFireSetting = value;
				PlayerPrefs.SetInt("ConfirmToFireSetting", value ? 1 : 0);
			}
		}

		public static bool RequireConfirmationToFire
		{
			get
			{
				if (!_requireConfirmationToFireSetting && primaryInputType != InputType.Touch && primaryInputType != InputType.Keyboard)
				{
					return primaryInputType == InputType.Controller;
				}
				return true;
			}
		}

		private void Awake()
		{
			_requireConfirmationToFireSetting = PlayerPrefs.GetInt("ConfirmToFireSetting", 0) == 1;
			_player = ReInput.players.GetPlayer(0);
			if (Application.isConsolePlatform)
			{
				primaryInputType = InputType.Controller;
			}
			else if (Application.isMobilePlatform)
			{
				primaryInputType = InputType.Touch;
			}
			else
			{
				primaryInputType = InputType.Mouse;
			}
		}

		private void Update()
		{
			if (Input.touchCount > 0 && primaryInputType != InputType.Touch)
			{
				primaryInputType = InputType.Touch;
				OnInputTypeChanged();
				return;
			}
			Controller lastActiveController = _player.controllers.GetLastActiveController();
			if (lastActiveController == null)
			{
				return;
			}
			if (lastActiveController.type == ControllerType.Joystick && primaryInputType != InputType.Controller)
			{
				primaryInputType = InputType.Controller;
				OnInputTypeChanged();
			}
			else if (lastActiveController.type == ControllerType.Keyboard && primaryInputType != InputType.Keyboard)
			{
				if (Input.anyKeyDown)
				{
					_consecutiveKeyPresses++;
				}
				if (Input.anyKey)
				{
					_buttonHoldTimer += Time.deltaTime;
				}
				if (_buttonHoldTimer >= _buttonHoldTimeForKeyboardChange || _consecutiveKeyPresses >= 2)
				{
					primaryInputType = InputType.Keyboard;
					OnInputTypeChanged();
				}
			}
			else if (lastActiveController.type == ControllerType.Mouse && primaryInputType != InputType.Mouse && primaryInputType != InputType.Touch)
			{
				primaryInputType = InputType.Mouse;
				OnInputTypeChanged();
			}
			if (lastActiveController.type != ControllerType.Keyboard)
			{
				_consecutiveKeyPresses = 0;
				_buttonHoldTimer = 0f;
			}
		}
	}
}
