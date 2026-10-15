using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rewired.Internal;
using Rewired.Utils.Interfaces;
using Rewired.Utils.Platforms.Windows;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace Rewired.Utils
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ExternalTools : IExternalTools
	{
		private static Func<object> _getPlatformInitializerDelegate;

		private bool _isEditorPaused;

		private Action<bool> _EditorPausedStateChangedEvent;

		public static Func<object> getPlatformInitializerDelegate
		{
			get
			{
				return _getPlatformInitializerDelegate;
			}
			set
			{
				_getPlatformInitializerDelegate = value;
			}
		}

		public bool isEditorPaused => _isEditorPaused;

		public bool UnityInput_IsTouchPressureSupported => UnityEngine.Input.touchPressureSupported;

		public event Action<bool> EditorPausedStateChangedEvent
		{
			add
			{
				_EditorPausedStateChangedEvent = (Action<bool>)Delegate.Combine(_EditorPausedStateChangedEvent, value);
			}
			remove
			{
				_EditorPausedStateChangedEvent = (Action<bool>)Delegate.Remove(_EditorPausedStateChangedEvent, value);
			}
		}

		public event Action<uint, bool> XboxOneInput_OnGamepadStateChange;

		public void Destroy()
		{
		}

		public object GetPlatformInitializer()
		{
			return Main.GetPlatformInitializer();
		}

		public string GetFocusedEditorWindowTitle()
		{
			return string.Empty;
		}

		public bool IsEditorSceneViewFocused()
		{
			return false;
		}

		public bool LinuxInput_IsJoystickPreconfigured(string name)
		{
			return false;
		}

		public int XboxOneInput_GetUserIdForGamepad(uint id)
		{
			return 0;
		}

		public ulong XboxOneInput_GetControllerId(uint unityJoystickId)
		{
			return 0uL;
		}

		public bool XboxOneInput_IsGamepadActive(uint unityJoystickId)
		{
			return false;
		}

		public string XboxOneInput_GetControllerType(ulong xboxControllerId)
		{
			return string.Empty;
		}

		public uint XboxOneInput_GetJoystickId(ulong xboxControllerId)
		{
			return 0u;
		}

		public void XboxOne_Gamepad_UpdatePlugin()
		{
		}

		public bool XboxOne_Gamepad_SetGamepadVibration(ulong xboxOneJoystickId, float leftMotor, float rightMotor, float leftTriggerLevel, float rightTriggerLevel)
		{
			return false;
		}

		public void XboxOne_Gamepad_PulseVibrateMotor(ulong xboxOneJoystickId, int motorInt, float startLevel, float endLevel, ulong durationMS)
		{
		}

		public void GetDeviceVIDPIDs(out List<int> vids, out List<int> pids)
		{
			vids = new List<int>();
			pids = new List<int>();
		}

		public int GetAndroidAPILevel()
		{
			return -1;
		}

		public void WindowsStandalone_ForwardRawInput(IntPtr rawInputHeaderIndices, IntPtr rawInputDataIndices, uint indicesCount, IntPtr rawInputData, uint rawInputDataSize)
		{
			UnityEngine.Windows.Input.ForwardRawInput(rawInputHeaderIndices, rawInputDataIndices, indicesCount, rawInputData, rawInputDataSize);
		}

		public bool UnityUI_Graphic_GetRaycastTarget(object graphic)
		{
			if (graphic as Graphic == null)
			{
				return false;
			}
			return (graphic as Graphic).raycastTarget;
		}

		public void UnityUI_Graphic_SetRaycastTarget(object graphic, bool value)
		{
			if (!(graphic as Graphic == null))
			{
				(graphic as Graphic).raycastTarget = value;
			}
		}

		public float UnityInput_GetTouchPressure(ref Touch touch)
		{
			return touch.pressure;
		}

		public float UnityInput_GetTouchMaximumPossiblePressure(ref Touch touch)
		{
			return touch.maximumPossiblePressure;
		}

		public IControllerTemplate CreateControllerTemplate(Guid typeGuid, object payload)
		{
			return ControllerTemplateFactory.Create(typeGuid, payload);
		}

		public Type[] GetControllerTemplateTypes()
		{
			return ControllerTemplateFactory.templateTypes;
		}

		public Type[] GetControllerTemplateInterfaceTypes()
		{
			return ControllerTemplateFactory.templateInterfaceTypes;
		}
	}
}
