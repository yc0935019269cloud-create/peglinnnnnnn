using UnityEngine;

namespace PeglinUI
{
	public class VignetteSlider : SettingsSlider
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateValue(SettingsManager.VignetteLevelToIntensity(SettingsManager.Instance.VignetteIntensity));
			_slider.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(float value)
		{
			CurrentValueText.text = $"{Mathf.RoundToInt(_slider.value * 100f)}%";
		}

		private void OnDisable()
		{
			_slider.onValueChanged.RemoveAllListeners();
		}
	}
}
