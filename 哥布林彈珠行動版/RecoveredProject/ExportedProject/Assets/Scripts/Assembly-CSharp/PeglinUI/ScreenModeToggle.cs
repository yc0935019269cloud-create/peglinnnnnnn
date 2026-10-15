using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class ScreenModeToggle : MonoBehaviour
	{
		public FullScreenMode ScreenMode;

		public ResolutionModeEnumerator ResolutionModeEnumerator;

		private Toggle _toggle;

		private void OnEnable()
		{
			if (_toggle == null)
			{
				_toggle = GetComponent<Toggle>();
			}
			if (SettingsManager.Instance.ScreenMode == ScreenMode)
			{
				_toggle.isOn = true;
			}
			else
			{
				_toggle.isOn = false;
			}
			_toggle.onValueChanged.AddListener(SetFullscreenMode);
		}

		private void SetFullscreenMode(bool status)
		{
			if (status)
			{
				SettingsManager.Instance.ScreenMode = ScreenMode;
				ResolutionModeEnumerator.SetAvailableResolutionOptions(SettingsManager.Instance.ScreenMode);
			}
		}

		private void OnDisable()
		{
			_toggle.onValueChanged.RemoveAllListeners();
		}
	}
}
