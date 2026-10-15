using System;
using Battle;
using Battle.Enemies;
using Map.MinimapView;
using UnityEngine;

namespace PeglinUI
{
	public class CursorManagement : MonoBehaviour
	{
		public enum CursorState
		{
			NONE = 0,
			REGULAR = 1,
			AWAITING_SHOT = 2,
			TARGETING = 3,
			DURING_SHOT = 4
		}

		public Texture2D normalCursor;

		public Texture2D targetCursor;

		public Texture2D readyToShootCursor;

		public Texture2D duringShotCursor;

		private static CursorManagement _instance;

		private TargetingManager _targetingManager;

		private BattleController _battleContoller;

		private Vector2 _hotspot = new Vector2(16f, 4f);

		private bool _hoveringOnEnemy;

		private bool _shotInFlight;

		private bool _overlayToggled;

		private bool _useHardwareCursor;

		private CursorState _state;

		private CursorState _previousState;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void Start()
		{
			_state = ((!SettingsManager.Instance.UseHardwareCursor) ? CursorState.REGULAR : CursorState.NONE);
			Enemy.OnMouseHoverEnterEnemy = (Enemy.MouseHoverEnemy)Delegate.Combine(Enemy.OnMouseHoverEnterEnemy, new Enemy.MouseHoverEnemy(OnMouseHoverEnemy));
			Enemy.OnMouseHoverExitEnemy = (Enemy.MouseHoverEnemy)Delegate.Combine(Enemy.OnMouseHoverExitEnemy, new Enemy.MouseHoverEnemy(OnMouseHoverExitEnemy));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OnShotFired));
			BattleController.OnBallCreationComplete = (Action)Delegate.Combine(BattleController.OnBallCreationComplete, new Action(OnBallCreationComplete));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(ForceReturnToRegular));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(OnTurnComplete));
			PlayfieldMouseDetector.OnPlayfieldMouseHoverEnter = (Action)Delegate.Combine(PlayfieldMouseDetector.OnPlayfieldMouseHoverEnter, new Action(SwitchToShoot));
			PlayfieldMouseDetector.OnPlayfieldMouseHoverExit = (Action)Delegate.Combine(PlayfieldMouseDetector.OnPlayfieldMouseHoverExit, new Action(ReturnToRegular));
			NavOnlyController.NavigationStarted = (Action)Delegate.Combine(NavOnlyController.NavigationStarted, new Action(SwitchToShoot));
			NavOnlyController.NavigationTriggered = (Action)Delegate.Combine(NavOnlyController.NavigationTriggered, new Action(ForceReturnToRegular));
			DestroyMapController.OnLoadMainMenu = (Action)Delegate.Combine(DestroyMapController.OnLoadMainMenu, new Action(ForceReturnToRegular));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Combine(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(OnOverlayToggled));
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Combine(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(OnOverlayToggled));
			PostBattleController.OnMisNavigationEvent = (PostBattleController.MisNavigationEvent)Delegate.Combine(PostBattleController.OnMisNavigationEvent, new PostBattleController.MisNavigationEvent(ForceAwaitingShot));
		}

		private void OnDisable()
		{
			Enemy.OnMouseHoverEnterEnemy = (Enemy.MouseHoverEnemy)Delegate.Remove(Enemy.OnMouseHoverEnterEnemy, new Enemy.MouseHoverEnemy(OnMouseHoverEnemy));
			Enemy.OnMouseHoverExitEnemy = (Enemy.MouseHoverEnemy)Delegate.Remove(Enemy.OnMouseHoverExitEnemy, new Enemy.MouseHoverEnemy(OnMouseHoverExitEnemy));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OnShotFired));
			BattleController.OnBallCreationComplete = (Action)Delegate.Remove(BattleController.OnBallCreationComplete, new Action(OnBallCreationComplete));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(ForceReturnToRegular));
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(OnTurnComplete));
			PlayfieldMouseDetector.OnPlayfieldMouseHoverEnter = (Action)Delegate.Remove(PlayfieldMouseDetector.OnPlayfieldMouseHoverEnter, new Action(SwitchToShoot));
			PlayfieldMouseDetector.OnPlayfieldMouseHoverExit = (Action)Delegate.Remove(PlayfieldMouseDetector.OnPlayfieldMouseHoverExit, new Action(ReturnToRegular));
			NavOnlyController.NavigationStarted = (Action)Delegate.Remove(NavOnlyController.NavigationStarted, new Action(SwitchToShoot));
			NavOnlyController.NavigationTriggered = (Action)Delegate.Remove(NavOnlyController.NavigationTriggered, new Action(ForceReturnToRegular));
			DestroyMapController.OnLoadMainMenu = (Action)Delegate.Remove(DestroyMapController.OnLoadMainMenu, new Action(ForceReturnToRegular));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Remove(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(OnOverlayToggled));
			PostBattleController.OnMisNavigationEvent = (PostBattleController.MisNavigationEvent)Delegate.Remove(PostBattleController.OnMisNavigationEvent, new PostBattleController.MisNavigationEvent(ForceAwaitingShot));
		}

		private void Update()
		{
			if (_useHardwareCursor != SettingsManager.Instance.UseHardwareCursor)
			{
				_useHardwareCursor = SettingsManager.Instance.UseHardwareCursor;
				_state = ((!_useHardwareCursor) ? CursorState.REGULAR : CursorState.NONE);
				SetCursorForState(_state);
			}
			else if (_state != _previousState)
			{
				SetCursorForState(_state);
				_previousState = _state;
			}
		}

		public void SetCursorForState(CursorState state)
		{
			if (SettingsManager.Instance.UseHardwareCursor)
			{
				state = CursorState.NONE;
			}
			Texture2D texture = null;
			switch (state)
			{
			case CursorState.REGULAR:
				texture = normalCursor;
				break;
			case CursorState.TARGETING:
				texture = targetCursor;
				break;
			case CursorState.AWAITING_SHOT:
				texture = readyToShootCursor;
				break;
			case CursorState.DURING_SHOT:
				texture = duringShotCursor;
				break;
			case CursorState.NONE:
				texture = null;
				break;
			}
			Cursor.SetCursor(texture, _hotspot, CursorMode.Auto);
		}

		private void OnOverlayToggled(bool open)
		{
			_overlayToggled = open;
			if (open)
			{
				_state = CursorState.REGULAR;
			}
			else if (_shotInFlight)
			{
				_state = CursorState.DURING_SHOT;
			}
			else if (_hoveringOnEnemy)
			{
				_state = CursorState.TARGETING;
			}
			else if (GameBlockingWindow.cursorChangeWindowOpen)
			{
				_state = CursorState.REGULAR;
			}
			else
			{
				_state = CursorState.AWAITING_SHOT;
			}
		}

		private void SwitchToShoot()
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				_state = (_shotInFlight ? CursorState.DURING_SHOT : CursorState.AWAITING_SHOT);
			}
		}

		private void ReturnToRegular()
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				if (!_shotInFlight)
				{
					_state = ((!_hoveringOnEnemy) ? CursorState.REGULAR : CursorState.TARGETING);
				}
				else if (_shotInFlight && !_hoveringOnEnemy)
				{
					_state = CursorState.REGULAR;
				}
			}
		}

		private void ForceReturnToRegular()
		{
			_state = CursorState.REGULAR;
			_hoveringOnEnemy = false;
			_shotInFlight = false;
		}

		private void ForceAwaitingShot()
		{
			_state = CursorState.AWAITING_SHOT;
			_hoveringOnEnemy = false;
			_shotInFlight = false;
		}

		private void OnBallCreationComplete()
		{
			if (!SettingsManager.Instance.UseHardwareCursor && !_shotInFlight && !_overlayToggled)
			{
				_state = (_hoveringOnEnemy ? CursorState.TARGETING : CursorState.AWAITING_SHOT);
			}
		}

		private void OnShotFired(Vector2 aimvector)
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				_shotInFlight = true;
				_state = CursorState.DURING_SHOT;
			}
		}

		private void OnTurnComplete()
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				if (_hoveringOnEnemy)
				{
					_state = CursorState.TARGETING;
				}
				_shotInFlight = false;
			}
		}

		private void OnMouseHoverEnemy()
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				_hoveringOnEnemy = true;
				if (_targetingManager == null)
				{
					_targetingManager = UnityEngine.Object.FindObjectOfType<TargetingManager>();
				}
				if (!_shotInFlight && _targetingManager != null && _targetingManager.canTarget && _state != CursorState.DURING_SHOT)
				{
					_state = CursorState.TARGETING;
				}
				else if (_shotInFlight)
				{
					_state = CursorState.DURING_SHOT;
				}
			}
		}

		private void OnMouseHoverExitEnemy()
		{
			if (!SettingsManager.Instance.UseHardwareCursor)
			{
				if (_hoveringOnEnemy && !_shotInFlight && !_overlayToggled)
				{
					_state = ((BattleController.CurrentBattleState != BattleController.BattleState.AWAITING_SHOT) ? CursorState.REGULAR : CursorState.AWAITING_SHOT);
				}
				_hoveringOnEnemy = false;
			}
		}
	}
}
