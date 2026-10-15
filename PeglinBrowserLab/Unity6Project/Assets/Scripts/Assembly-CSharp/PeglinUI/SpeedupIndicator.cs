using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class SpeedupIndicator : MonoBehaviour
	{
		public Sprite SpeedupNotPressed;

		public Sprite SpeedupPressed;

		public TextMeshProUGUI Text;

		private Image _img;

		private void Start()
		{
			if (_img == null)
			{
				_img = GetComponent<Image>();
			}
			UpdateText();
			UpdateSpeedupState();
		}

		private void OnEnable()
		{
			SettingsManager.OnSettingsChangedEvent = (SettingsManager.SettingsChangedEvent)Delegate.Combine(SettingsManager.OnSettingsChangedEvent, new SettingsManager.SettingsChangedEvent(UpdateText));
			TimescaleManager.OnSpeedupStateChanged = (TimescaleManager.SpeedupStateChanged)Delegate.Combine(TimescaleManager.OnSpeedupStateChanged, new TimescaleManager.SpeedupStateChanged(UpdateSpeedupState));
		}

		private void OnDisable()
		{
			SettingsManager.OnSettingsChangedEvent = (SettingsManager.SettingsChangedEvent)Delegate.Remove(SettingsManager.OnSettingsChangedEvent, new SettingsManager.SettingsChangedEvent(UpdateText));
			TimescaleManager.OnSpeedupStateChanged = (TimescaleManager.SpeedupStateChanged)Delegate.Remove(TimescaleManager.OnSpeedupStateChanged, new TimescaleManager.SpeedupStateChanged(UpdateSpeedupState));
		}

		public void UpdateSpeedupState()
		{
			if (!(TimescaleManager.Instance == null))
			{
				if (TimescaleManager.Instance.isSpedUp)
				{
					_img.sprite = SpeedupPressed;
					Text.enabled = true;
				}
				else
				{
					_img.sprite = SpeedupNotPressed;
					Text.enabled = false;
				}
			}
		}

		private void UpdateText()
		{
			if (SettingsManager.Instance != null)
			{
				Text.text = $"x{SettingsManager.Instance.SpeedupLevel}";
			}
		}

		public void ToggleSpeedup()
		{
			TimescaleManager.Instance.ManualSpeedupToggle();
		}
	}
}
