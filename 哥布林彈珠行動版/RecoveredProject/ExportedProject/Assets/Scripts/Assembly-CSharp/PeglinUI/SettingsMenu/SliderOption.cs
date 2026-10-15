using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.SettingsMenu
{
	public class SliderOption : MonoBehaviour
	{
		public float maxValue = 1f;

		public float minValue;

		[Header("Prefab References")]
		public Slider slider;

		private Player _player;

		protected float _slidingTimer;

		protected float _slidingSkipTimer;

		private void OnEnable()
		{
			slider.minValue = minValue;
			slider.maxValue = maxValue;
			_player = ReInput.players.GetPlayer(0);
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
					slider.value += 0.05f * axis;
					_slidingTimer += Time.unscaledDeltaTime;
				}
				else if (_slidingTimer >= 0.5f && _slidingSkipTimer >= 0.1f)
				{
					_slidingTimer += Time.unscaledDeltaTime;
					slider.value += 0.05f * axis;
					_slidingSkipTimer = 0f;
				}
				else if (_slidingTimer >= 0.5f && _slidingSkipTimer < 0.1f)
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
	}
}
