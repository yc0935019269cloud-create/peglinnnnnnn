using UnityEngine;

namespace PeglinUI
{
	public class RumbleSlider : SettingsSlider
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateValue(SettingsManager.Instance.RumbleInstensity);
			_slider.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(float value)
		{
			SettingsManager.Instance.RumbleInstensity = value;
			CurrentValueText.text = $"{Mathf.RoundToInt(_slider.value * 100f)}%";
		}

		private void OnDisable()
		{
			_slider.onValueChanged.RemoveAllListeners();
		}
	}
}
