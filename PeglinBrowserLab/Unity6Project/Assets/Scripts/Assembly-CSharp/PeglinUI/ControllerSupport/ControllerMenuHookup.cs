using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;

namespace PeglinUI.ControllerSupport
{
	public class ControllerMenuHookup : MonoBehaviour
	{
		public bool firstSelectedWithMouse;

		private const int REWIRED_PLAYER_ID = 0;

		private Player _player;

		private RewiredEventSystem _rewired;

		private GameObject _lastSelected;

		private bool _lastSelectedChangedThisTurn;

		public GameObject LastSelected
		{
			set
			{
				_lastSelected = value;
				_lastSelectedChangedThisTurn = true;
			}
		}

		private void Start()
		{
			_rewired = GetComponent<RewiredEventSystem>();
			_player = ReInput.players.GetPlayer(0);
			if (ControllerInputManager.primaryInputType != InputType.Controller && !firstSelectedWithMouse)
			{
				_lastSelected = _rewired.firstSelectedGameObject;
				_rewired.firstSelectedGameObject = null;
				_rewired.SetSelectedGameObject(null);
			}
		}

		private void LateUpdate()
		{
			if (_lastSelectedChangedThisTurn)
			{
				_lastSelectedChangedThisTurn = false;
				return;
			}
			if (_rewired.currentSelectedGameObject != null)
			{
				_lastSelected = _rewired.currentSelectedGameObject;
			}
			if (!Mathf.Approximately(_player.GetAxis("UIHorizontal"), 0f) || (!Mathf.Approximately(_player.GetAxis("UIVertical"), 0f) && _rewired.currentSelectedGameObject == null))
			{
				_rewired.SetSelectedGameObject(_lastSelected);
			}
		}
	}
}
