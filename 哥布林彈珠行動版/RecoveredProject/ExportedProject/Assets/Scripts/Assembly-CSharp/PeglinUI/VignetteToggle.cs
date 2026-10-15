using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class VignetteToggle : MonoBehaviour
	{
		public SettingsManager.IntensityLevel VignetteLevel;

		private Toggle _toggle;

		private void OnEnable()
		{
			if (_toggle == null)
			{
				_toggle = GetComponent<Toggle>();
			}
			if (SettingsManager.Instance.VignetteIntensity == VignetteLevel)
			{
				_toggle.isOn = true;
			}
			else
			{
				_toggle.isOn = false;
			}
			_toggle.onValueChanged.AddListener(SetVignetteLevel);
		}

		private void SetVignetteLevel(bool status)
		{
			if (status)
			{
				SettingsManager.Instance.VignetteIntensity = VignetteLevel;
			}
		}

		private void OnDisable()
		{
			_toggle.onValueChanged.RemoveAllListeners();
		}
	}
}
