using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI
{
	public class SettingsSlider : MonoBehaviour
	{
		public TextMeshProUGUI MaxValueText;

		public TextMeshProUGUI CurrentValueText;

		protected Slider _slider;

		protected float _slidingTimer;

		protected float _slidingSkipTimer;

		protected Player _player;

		protected virtual void OnEnable()
		{
			if (_slider == null)
			{
				_slider = GetComponent<Slider>();
			}
			_slider.minValue = 0f;
			_slider.maxValue = 1f;
			_player = ReInput.players.GetPlayer(0);
			MaxValueText.text = "100%";
		}

		private void Update()
		{
			if (EventSystem.current.currentSelectedGameObject != base.gameObject)
			{
				_slidingTimer = 0f;
				return;
			}
			float axis = _player.GetAxis("UIHorizontal");
			if (axis == 0f)
			{
				axis = _player.GetAxis("Horizontal");
			}
			if (axis != 0f)
			{
				if (_slidingTimer == 0f)
				{
					_slider.value += 0.05f * axis;
					_slidingTimer += Time.unscaledDeltaTime;
				}
				else if (_slidingTimer >= 1f && _slidingSkipTimer >= 0.1f)
				{
					_slidingTimer += Time.unscaledDeltaTime;
					_slider.value += 0.05f * axis;
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

		public void UpdateValue(float value)
		{
			_slider.value = value;
			CurrentValueText.text = $"{Mathf.RoundToInt(_slider.value * 100f)}%";
		}
	}
}
