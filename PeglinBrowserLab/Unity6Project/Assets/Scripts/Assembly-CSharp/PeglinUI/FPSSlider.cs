using System;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class FPSSlider : MonoBehaviour
	{
		public TextMeshProUGUI MaxFPSText;

		public TextMeshProUGUI CurrentFPSText;

		public Toggle VsyncToggle;

		private Slider _slider;

		private float _slidingTimer;

		private float _slidingSkipTimer;

		private Player _player;

		private void OnEnable()
		{
			if (_slider == null)
			{
				_slider = GetComponent<Slider>();
			}
			_slider.minValue = 30f;
			_slider.maxValue = SettingsManager.GetReportedRefreshRate(Screen.currentResolution);
			_slider.value = SettingsManager.Instance.TargetFramerate;
			MaxFPSText.text = _slider.maxValue.ToString();
			CurrentFPSText.text = _slider.value.ToString();
			VsyncToggle.isOn = SettingsManager.Instance.Vsync;
			_slider.interactable = !VsyncToggle.isOn;
			_player = ReInput.players.GetPlayer(0);
			DisplayManager.OnDisplayChanged = (DisplayManager.DisplayChanged)Delegate.Combine(DisplayManager.OnDisplayChanged, new DisplayManager.DisplayChanged(UpdateSlider));
		}

		private void Update()
		{
			float axis = _player.GetAxis("UIHorizontal");
			if (axis != 0f)
			{
				if (_slidingTimer >= 1f && _slidingSkipTimer >= 0.1f)
				{
					_slidingTimer += Time.unscaledDeltaTime;
					_slider.value += 5f * axis;
					_slidingSkipTimer = 0f;
				}
				else if (_slidingTimer >= 1f && _slidingSkipTimer < 0.1f)
				{
					_slidingSkipTimer += Time.unscaledDeltaTime;
				}
				else
				{
					_slidingTimer += Time.unscaledDeltaTime;
				}
			}
			else
			{
				_slidingTimer = 0f;
			}
		}

		public void UpdateTargetFramerate()
		{
			SettingsManager.Instance.TargetFramerate = Mathf.Clamp((int)_slider.value, 30, SettingsManager.GetReportedRefreshRate(Screen.currentResolution));
			CurrentFPSText.text = _slider.value.ToString();
		}

		public void ToggleVsync()
		{
			SettingsManager.Instance.Vsync = VsyncToggle.isOn;
			_slider.interactable = !VsyncToggle.isOn;
		}

		public void UpdateMaxFramerate(int framerate)
		{
			_slider.maxValue = framerate;
			MaxFPSText.text = framerate.ToString();
		}

		private void UpdateSlider()
		{
			UpdateTargetFramerate();
			UpdateMaxFramerate(SettingsManager.GetReportedRefreshRate(Screen.currentResolution));
		}
	}
}
