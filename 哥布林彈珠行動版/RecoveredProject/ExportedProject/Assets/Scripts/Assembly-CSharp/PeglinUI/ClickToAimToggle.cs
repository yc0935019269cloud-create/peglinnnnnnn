using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class ClickToAimToggle : MonoBehaviour
	{
		private Toggle _toggle;

		private void OnEnable()
		{
			if (_toggle == null)
			{
				_toggle = GetComponent<Toggle>();
			}
			_toggle.isOn = SettingsManager.Instance.ClickToAim;
		}

		public void ToggleClickToAim(bool value)
		{
			SettingsManager.Instance.ClickToAim = value;
		}
	}
}
