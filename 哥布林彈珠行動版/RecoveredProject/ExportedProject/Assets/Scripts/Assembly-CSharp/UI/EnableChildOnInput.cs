using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class EnableChildOnInput : MonoBehaviour
	{
		public GameObject childObject;

		public float fadeOutAfterTime = 3f;

		private Player _player;

		private float _skipButtonFadeTimer;

		private void Awake()
		{
			_player = ReInput.players.GetPlayer(0);
		}

		private void Update()
		{
			if (childObject.activeInHierarchy)
			{
				_skipButtonFadeTimer -= Time.unscaledDeltaTime;
				if (_skipButtonFadeTimer <= 0f)
				{
					childObject.SetActive(value: false);
				}
			}
			else
			{
				_skipButtonFadeTimer = fadeOutAfterTime;
			}
			if (_player.GetAnyButtonDown())
			{
				childObject.SetActive(value: true);
				EventSystem.current.SetSelectedGameObject(childObject);
				_skipButtonFadeTimer = fadeOutAfterTime;
			}
		}
	}
}
