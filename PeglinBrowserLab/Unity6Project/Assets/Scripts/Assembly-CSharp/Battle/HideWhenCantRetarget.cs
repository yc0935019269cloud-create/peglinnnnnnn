using System;
using Battle.Enemies;
using PeglinUI.ControllerSupport;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
	public class HideWhenCantRetarget : MonoBehaviour
	{
		public delegate void ExternalUpdateAllTargetingUIButtons();

		public static ExternalUpdateAllTargetingUIButtons ExternalUpdateCurrentTargetingUI = () =>
		{
		};

		public TargetingUI targetingUI;

		public Image _lockImage;

		private bool _shotInFlight;

		private void Awake()
		{
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OnShotFired));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(OnTurnComplete));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Combine(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(UpdateStatusOnUnpause));
			ExternalUpdateCurrentTargetingUI = (ExternalUpdateAllTargetingUIButtons)Delegate.Combine(ExternalUpdateCurrentTargetingUI, new ExternalUpdateAllTargetingUIButtons(UpdateCurrentTargetingUI));
		}

		private void OnEnable()
		{
			UpdateVisibility();
		}

		private void OnDestroy()
		{
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OnShotFired));
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(OnTurnComplete));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(UpdateInputType));
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Remove(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(UpdateStatusOnUnpause));
			ExternalUpdateCurrentTargetingUI = (ExternalUpdateAllTargetingUIButtons)Delegate.Remove(ExternalUpdateCurrentTargetingUI, new ExternalUpdateAllTargetingUIButtons(UpdateCurrentTargetingUI));
		}

		private void OnShotFired(Vector2 aimvector)
		{
			_shotInFlight = true;
			UpdateVisibility();
		}

		private void OnTurnComplete()
		{
			_shotInFlight = false;
			UpdateVisibility();
		}

		private void UpdateInputType()
		{
			UpdateVisibility();
		}

		private void UpdateCurrentTargetingUI()
		{
			if (base.gameObject.activeInHierarchy)
			{
				UpdateVisibility();
			}
		}

		private void UpdateVisibility()
		{
			base.gameObject.SetActive(!_shotInFlight);
			_lockImage.enabled = targetingUI.Enemy.targetLock || targetingUI.Enemy.untargetable;
		}

		private void UpdateStatusOnUnpause(bool pause)
		{
			if (!pause)
			{
				UpdateVisibility();
			}
		}
	}
}
