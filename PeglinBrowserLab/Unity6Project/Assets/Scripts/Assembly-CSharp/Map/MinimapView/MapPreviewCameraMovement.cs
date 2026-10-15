using System;
using Battle;
using DG.Tweening;
using PeglinUI;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Map.MinimapView
{
	public class MapPreviewCameraMovement : MonoBehaviour
	{
		private enum MapPreviewState
		{
			Closed = 0,
			Panning = 1,
			FreeMove = 2,
			Closing = 3
		}

		private const float MAP_PREVIEW_CAMERA_SIZE = 10f;

		public Button mapButton;

		private const float DECELERATION = 9f;

		private const float BOUNCE_BACK_SPEED = 7.5f;

		private const float SCROLL_SPEED = 40f;

		private const float JOYSTICK_DEADZONE = 0.1f;

		private const float JOYSTICK_SPEED = 22f;

		public float bossIconOffset = 5f;

		public bool disabled;

		private ReturnCameraToDefaultPos _returnCamera;

		private MapPreviewState _state;

		private MapController _mapController;

		private float _parchmentXPos;

		private MapPreviewOpenClose _mapPreviewOpenClose;

		private Transform _peglinTransform;

		private Player _player;

		private float _minY;

		private float _maxY = 3f;

		private float _bouceBackSpace = 1.5f;

		private Camera _camera;

		private Rigidbody2D _cameraRigidbody;

		private ScreenShaker _screenShaker;

		private float _prevMouseYPosition;

		private Vector2 _movementVelocity;

		private bool _mouseButtonDownLastFrame;

		private float _scrollMovement;

		private float _previousCameraSize;

		private void Start()
		{
			_camera = Camera.main;
			_cameraRigidbody = _camera.GetComponent<Rigidbody2D>();
			_screenShaker = _camera.GetComponent<ScreenShaker>();
			_mapController = GameObject.FindWithTag("MapController")?.GetComponent<MapController>();
			if (_mapController == null)
			{
				if (mapButton != null)
				{
					mapButton.interactable = false;
				}
				disabled = true;
				return;
			}
			Canvas componentInChildren = _mapController.GetComponentInChildren<Canvas>();
			if (componentInChildren != null)
			{
				_parchmentXPos = componentInChildren.transform.position.x;
			}
			_mapPreviewOpenClose = _mapController.GetComponent<MapPreviewOpenClose>();
			_returnCamera = GetComponent<ReturnCameraToDefaultPos>();
			ReturnCameraToDefaultPos returnCamera = _returnCamera;
			returnCamera.onCameraFinishedLerp = (ReturnCameraToDefaultPos.CameraFinishLerp)Delegate.Combine(returnCamera.onCameraFinishedLerp, new ReturnCameraToDefaultPos.CameraFinishLerp(CloseLerpComplete));
			_player = ReInput.players.GetPlayer(0);
			CameraFollowPoint component = _mapController.GetComponent<CameraFollowPoint>();
			_minY = component.bossIconTransform.position.y + bossIconOffset;
			_maxY = _returnCamera.initialPosition.y;
			_peglinTransform = component.targetTransform;
		}

		private void OnEnable()
		{
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Combine(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(HandleGameOver));
		}

		private void OnDisable()
		{
			if (_returnCamera != null && _returnCamera.onCameraFinishedLerp != null)
			{
				ReturnCameraToDefaultPos returnCamera = _returnCamera;
				returnCamera.onCameraFinishedLerp = (ReturnCameraToDefaultPos.CameraFinishLerp)Delegate.Remove(returnCamera.onCameraFinishedLerp, new ReturnCameraToDefaultPos.CameraFinishLerp(CloseLerpComplete));
			}
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Remove(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(HandleGameOver));
		}

		private void HandleGameOver()
		{
			CloseMapAndLockCameraMovement();
		}

		public void MapButtonPressed()
		{
			if (!PauseMenu.Paused && !PauseMenu.PauseBlock && !disabled)
			{
				if (_state == MapPreviewState.Closed && !GameBlockingWindow.windowOpen)
				{
					PanToPlayer();
				}
				else if (_state == MapPreviewState.FreeMove)
				{
					CloseMapAndLockCameraMovement();
				}
			}
		}

		private void PanToPlayer()
		{
			_mapPreviewOpenClose.Open();
			_returnCamera.SetNewDefaultPosition();
			_returnCamera.freeCameraMoving = true;
			_camera.DOKill();
			if (_screenShaker != null)
			{
				_screenShaker.blocked = true;
			}
			Vector3 position = base.transform.position;
			position.x = _parchmentXPos;
			position.y = Mathf.Clamp(_peglinTransform.position.y, _minY, _maxY);
			base.transform.position = position;
			StartFreeMove();
		}

		private void StartFreeMove()
		{
			_state = MapPreviewState.FreeMove;
			_cameraRigidbody.bodyType = RigidbodyType2D.Kinematic;
			_previousCameraSize = _camera.orthographicSize;
			_camera.orthographicSize = 10f;
		}

		private void CloseMapAndLockCameraMovement()
		{
			if (_state == MapPreviewState.FreeMove)
			{
				_state = MapPreviewState.Closing;
				_cameraRigidbody.linearVelocity = Vector2.zero;
				_cameraRigidbody.bodyType = RigidbodyType2D.Kinematic;
				_camera.orthographicSize = _previousCameraSize;
				_returnCamera.ReturnToDefault(0f, force: true);
			}
		}

		private void CloseLerpComplete()
		{
			if (_state == MapPreviewState.Closing)
			{
				_mapPreviewOpenClose.Close();
				_state = MapPreviewState.Closed;
				_camera.orthographicSize = _previousCameraSize;
				if (_screenShaker != null)
				{
					_screenShaker.blocked = SettingsManager.Instance != null && SettingsManager.Instance.ScreenshakeInstensity <= 0f;
				}
			}
		}

		private void Update()
		{
			if (_state == MapPreviewState.FreeMove)
			{
				float axis = _player.GetAxis("VerticalSlow");
				if (Mathf.Abs(axis) > 0.01f)
				{
					_scrollMovement = Mathf.Sign(axis) * 40f;
				}
			}
		}

		private void FixedUpdate()
		{
			if (_state != MapPreviewState.FreeMove)
			{
				return;
			}
			float num = Time.fixedDeltaTime / Time.timeScale;
			if (!_mouseButtonDownLastFrame && Input.GetMouseButton(0))
			{
				_prevMouseYPosition = Input.mousePosition.y;
			}
			float axis = _player.GetAxis("UIVertical");
			bool flag = true;
			if (Input.GetMouseButton(0))
			{
				float num2 = _prevMouseYPosition - Input.mousePosition.y;
				_movementVelocity.y = num2 * 0.8f * Time.timeScale;
				_prevMouseYPosition = Input.mousePosition.y;
			}
			else if (Mathf.Abs(_scrollMovement) != 0f)
			{
				_movementVelocity.y = _scrollMovement;
				_scrollMovement = 0f;
			}
			else if (Mathf.Abs(axis) > 0.1f)
			{
				_movementVelocity.y = axis * 22f;
			}
			else
			{
				flag = false;
				_movementVelocity = Vector2.MoveTowards(_movementVelocity, Vector2.zero, 9f / Time.timeScale);
			}
			float num3 = _cameraRigidbody.position.y + _movementVelocity.y * num;
			if (num3 > _maxY)
			{
				float num4 = Mathf.Abs(_maxY + _bouceBackSpace - num3) / _bouceBackSpace;
				float num5 = num4 * num4;
				_movementVelocity.y *= num5;
			}
			else if (num3 < _minY)
			{
				float num6 = Mathf.Abs(_minY - _bouceBackSpace - num3) / _bouceBackSpace;
				float num7 = num6 * num6;
				_movementVelocity.y *= num7;
			}
			float num8 = _cameraRigidbody.position.y + _movementVelocity.y * num;
			if (num8 > _maxY + _bouceBackSpace)
			{
				_cameraRigidbody.position = new Vector2(_cameraRigidbody.position.x, _maxY + _bouceBackSpace);
				if (_movementVelocity.y > 0f)
				{
					_movementVelocity.y = 0f;
				}
			}
			else if (num8 < _minY - _bouceBackSpace)
			{
				_cameraRigidbody.position = new Vector2(_cameraRigidbody.position.x, _minY - _bouceBackSpace);
				if (_movementVelocity.y < 0f)
				{
					_movementVelocity.y = 0f;
				}
			}
			if (!flag)
			{
				if (_cameraRigidbody.position.y > _maxY)
				{
					_cameraRigidbody.position = Vector2.MoveTowards(_cameraRigidbody.position, new Vector2(_cameraRigidbody.position.x, _maxY), 7.5f * num);
				}
				else if (_cameraRigidbody.position.y < _minY)
				{
					_cameraRigidbody.position = Vector2.MoveTowards(_cameraRigidbody.position, new Vector2(_cameraRigidbody.position.x, _minY), 7.5f * num);
				}
			}
			_cameraRigidbody.position += _movementVelocity * num;
			_mouseButtonDownLastFrame = Input.GetMouseButton(0);
		}
	}
}
