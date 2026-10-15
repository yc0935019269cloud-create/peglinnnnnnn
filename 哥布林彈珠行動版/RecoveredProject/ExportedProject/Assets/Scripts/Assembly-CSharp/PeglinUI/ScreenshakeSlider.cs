using UnityEngine;

namespace PeglinUI
{
	public class ScreenshakeSlider : SettingsSlider
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateValue(SettingsManager.Instance.ScreenshakeInstensity);
			_slider.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(float value)
		{
			SettingsManager.Instance.ScreenshakeInstensity = value;
			CurrentValueText.text = $"{Mathf.RoundToInt(_slider.value * 100f)}%";
		}

		private void OnDisable()
		{
			_slider.onValueChanged.RemoveAllListeners();
		}
	}
}
