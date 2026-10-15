using System;
using Battle.Enemies;
using PeglinUI.MainMenu;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	[RequireComponent(typeof(PachinkoBall))]
	public class RefreshBoardOnDrawAndDamageSelf : MonoBehaviour
	{
		public int minusTotal;

		public float damageReductionPercent;

		public float enemyDamageMod;

		private BattleController _battleController;

		private PegManager _pegManager;

		private PachinkoBall _pBall;

		private PlayerHealthController _healthController;

		private PredictionManager _predictionManager;

		private void OnEnable()
		{
			if (_battleController == null)
			{
				_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			}
			if (_battleController == null)
			{
				MainMenuRandomOrbDrop.RequestBoardReset?.Invoke();
				base.enabled = false;
				return;
			}
			_pegManager = _battleController.pegManager;
			_healthController = _battleController.PlayerHealthController;
			_pBall = GetComponent<PachinkoBall>();
			PachinkoBall.OnPachinkoBallArmed = (PachinkoBall.PachinkoBallArmed)Delegate.Combine(PachinkoBall.OnPachinkoBallArmed, new PachinkoBall.PachinkoBallArmed(RefreshBoard));
		}

		private void OnDisable()
		{
			PachinkoBall.OnPachinkoBallArmed = (PachinkoBall.PachinkoBallArmed)Delegate.Remove(PachinkoBall.OnPachinkoBallArmed, new PachinkoBall.PachinkoBallArmed(RefreshBoard));
			if (_pegManager != null)
			{
				PegManager pegManager = _pegManager;
				pegManager.OnPegsRefreshed = (PegManager.RefreshedPegsEvent)Delegate.Remove(pegManager.OnPegsRefreshed, new PegManager.RefreshedPegsEvent(HandlePegsRefreshed));
			}
		}

		private void RefreshBoard(PachinkoBall pBallArmed)
		{
			if (pBallArmed != _pBall)
			{
				return;
			}
			PersistentOrb component = GetComponent<PersistentOrb>();
			if (component.remainingPersistence >= component.modifiedPersistLevel - 1)
			{
				PegManager pegManager = _pegManager;
				pegManager.OnPegsRefreshed = (PegManager.RefreshedPegsEvent)Delegate.Combine(pegManager.OnPegsRefreshed, new PegManager.RefreshedPegsEvent(HandlePegsRefreshed));
				if (_battleController != null)
				{
					_battleController.ResetField();
				}
				PegManager pegManager2 = _pegManager;
				pegManager2.OnPegsRefreshed = (PegManager.RefreshedPegsEvent)Delegate.Remove(pegManager2.OnPegsRefreshed, new PegManager.RefreshedPegsEvent(HandlePegsRefreshed));
			}
		}

		private void HandlePegsRefreshed(int numPegs)
		{
			_healthController.DealSelfDamage((float)numPegs * (1f - damageReductionPercent) - (float)minusTotal, blockable: true, PlayerHealthController.DamageSource.ORB_SELF);
			if (enemyDamageMod > 0f)
			{
				Enemy.OnAllEnemiesDamaged?.Invoke(Mathf.RoundToInt((float)numPegs * enemyDamageMod));
			}
		}
	}
}
