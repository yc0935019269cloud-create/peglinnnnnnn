using System;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.ControllerSupport
{
	public class MapToggleToInput : MonoBehaviour
	{
		[SerializeField]
		private string _rewiredAction = "UICancel";

		[SerializeField]
		private bool _isAxisNegative;

		[SerializeField]
		private bool _isAxisPositive;

		[SerializeField]
		private bool _isBlockingWindow;

		[SerializeField]
		private bool _buttonDown;

		private Player _player;

		private bool _waitForZero;

		private Toggle _toggle;

		private void Awake()
		{
			_toggle = GetComponent<Toggle>();
		}

		private void Start()
		{
			_player = ReInput.players.GetPlayer(0);
		}

		private void OnEnable()
		{
			UIControlsSelf.OnClosePressed = (UIControlsSelf.ClosePressed)Delegate.Combine(UIControlsSelf.OnClosePressed, new UIControlsSelf.ClosePressed(SelectControl));
		}

		private void OnDisable()
		{
			UIControlsSelf.OnClosePressed = (UIControlsSelf.ClosePressed)Delegate.Remove(UIControlsSelf.OnClosePressed, new UIControlsSelf.ClosePressed(SelectControl));
		}

		private void Update()
		{
			if (_waitForZero)
			{
				if (Math.Abs(_player.GetAxis(_rewiredAction)) < 0.01f)
				{
					_waitForZero = false;
				}
			}
			else if ((_isBlockingWindow || !GameBlockingWindow.wasOpenThisFrame) && _toggle.enabled)
			{
				if (_isAxisNegative && _player.GetAxis(_rewiredAction) < -0.01f)
				{
					Submit();
					_waitForZero = true;
				}
				else if (_isAxisPositive && _player.GetAxis(_rewiredAction) > 0.01f)
				{
					Submit();
					_waitForZero = true;
				}
				else if (_buttonDown && !_isAxisNegative && !_isAxisPositive && _player.GetButtonDown(_rewiredAction))
				{
					Submit();
				}
				else if (!_buttonDown && !_isAxisNegative && !_isAxisPositive && _player.GetButtonUp(_rewiredAction))
				{
					Submit();
				}
			}
		}

		private void SelectControl()
		{
			if (base.gameObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
		}

		private void Submit()
		{
			_toggle.isOn = !_toggle.isOn;
			_toggle.onValueChanged.Invoke(_toggle.isOn);
		}
	}
}
