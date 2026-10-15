using System;
using System.Collections;
using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeglinUI.ControllerSupport
{
	public class ConditionalFireButton : MonoBehaviour
	{
		[SerializeField]
		private PlayfieldMouseDetector _mouseDetector;

		private bool _isShotEndable;

		public PachinkoFireButton pachinkoFireButton;

		private Canvas _canvas;

		private void Awake()
		{
			_canvas = GetComponent<Canvas>();
			UpdateInputType();
		}

		private void OnEnable()
		{
			PlayfieldMouseDetector mouseDetector = _mouseDetector;
			mouseDetector.OnPlayfieldMouseDetectorToggled = (Action)Delegate.Combine(mouseDetector.OnPlayfieldMouseDetectorToggled, new Action(UpdateInputType));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Combine(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(UpdateStatusOnUnpause));
			BattleController.OnShotTimeout = (BattleController.ShotTimeoutEvent)Delegate.Combine(BattleController.OnShotTimeout, new BattleController.ShotTimeoutEvent(OnShotTimeout));
			BattleController.OnShotForcefullyEnded = (BattleController.ShotTimeoutEvent)Delegate.Combine(BattleController.OnShotForcefullyEnded, new BattleController.ShotTimeoutEvent(OnShotForcefullyEnded));
			BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Combine(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
			BattleController.OnStartedAwaitingShot = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnStartedAwaitingShot, new BattleController.BattleEvent(OnStartedAwaitingShot));
			PachinkoBall.OnPachinkoBallArmed = (PachinkoBall.PachinkoBallArmed)Delegate.Combine(PachinkoBall.OnPachinkoBallArmed, new PachinkoBall.PachinkoBallArmed(OnPachinkoBallArmed));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(OnPachinkoBallDestroyed));
		}

		private void OnDisable()
		{
			PlayfieldMouseDetector mouseDetector = _mouseDetector;
			mouseDetector.OnPlayfieldMouseDetectorToggled = (Action)Delegate.Remove(mouseDetector.OnPlayfieldMouseDetectorToggled, new Action(UpdateInputType));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Remove(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(UpdateStatusOnUnpause));
			BattleController.OnShotTimeout = (BattleController.ShotTimeoutEvent)Delegate.Remove(BattleController.OnShotTimeout, new BattleController.ShotTimeoutEvent(OnShotTimeout));
			BattleController.OnShotForcefullyEnded = (BattleController.ShotTimeoutEvent)Delegate.Remove(BattleController.OnShotForcefullyEnded, new BattleController.ShotTimeoutEvent(OnShotForcefullyEnded));
			BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Remove(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
			BattleController.OnStartedAwaitingShot = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnStartedAwaitingShot, new BattleController.BattleEvent(OnStartedAwaitingShot));
			PachinkoBall.OnPachinkoBallArmed = (PachinkoBall.PachinkoBallArmed)Delegate.Remove(PachinkoBall.OnPachinkoBallArmed, new PachinkoBall.PachinkoBallArmed(OnPachinkoBallArmed));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(OnPachinkoBallDestroyed));
		}

		private void UpdateInputType()
		{
			UpdateFireButtonVisibility();
		}

		private IEnumerator WaitAndUpdateFireButtonVisibility()
		{
			yield return null;
			UpdateFireButtonVisibility();
		}

		private void UpdateFireButtonVisibility()
		{
			if (_canvas != null)
			{
				if (SceneManager.GetActiveScene().name != "Battle")
				{
					_canvas.enabled = ControllerInputManager.RequireConfirmationToFire && _mouseDetector.isActiveAndEnabled;
				}
				else if ((BattleController.CurrentBattleState == BattleController.BattleState.AWAITING_SHOT || BattleController.CurrentBattleState == BattleController.BattleState.NAVIGATION) && ControllerInputManager.RequireConfirmationToFire && _mouseDetector.isActiveAndEnabled)
				{
					pachinkoFireButton.SetToFireMode();
					_canvas.enabled = true;
				}
				else if (BattleController.CurrentBattleState == BattleController.BattleState.AWAITING_SHOT_COMPLETION && _isShotEndable)
				{
					pachinkoFireButton.SetToEndShotMode();
					_canvas.enabled = true;
				}
				else
				{
					_canvas.enabled = false;
				}
			}
		}

		private void UpdateStatusOnUnpause(bool pause)
		{
			if (!pause)
			{
				UpdateFireButtonVisibility();
			}
		}

		private void ShotFired(Vector2 aimVector)
		{
			StartCoroutine(WaitAndUpdateFireButtonVisibility());
		}

		private void OnPachinkoBallDestroyed(PachinkoBall pball)
		{
			UpdateFireButtonVisibility();
		}

		private void OnShotTimeout()
		{
			_isShotEndable = true;
			UpdateFireButtonVisibility();
		}

		private void OnStartedAwaitingShot()
		{
			_isShotEndable = false;
			UpdateFireButtonVisibility();
		}

		private void OnPachinkoBallArmed(PachinkoBall pBall)
		{
			_isShotEndable = false;
			UpdateFireButtonVisibility();
		}

		private void OnShotForcefullyEnded()
		{
			_isShotEndable = false;
			UpdateFireButtonVisibility();
		}

		private void OnShotComplete()
		{
			_isShotEndable = false;
			UpdateFireButtonVisibility();
		}
	}
}
