using System;
using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class GainStatusEffectOnRefresh : MonoBehaviour
	{
		public StatusEffectType statusEffect;

		public int intensity;

		public int numberOfRefreshToTrigger;

		private int _currentRefreshCounter;

		private PachinkoBall _pBall;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("NUMBER_OF_REFRESH", numberOfRefreshToTrigger.ToString());
		}

		private void OnEnable()
		{
			_pBall = GetComponent<PachinkoBall>();
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Combine(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckAndApplyEffect));
		}

		private void OnDisable()
		{
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckAndApplyEffect));
		}

		private void CheckAndApplyEffect()
		{
			if (_pBall.CurrentState == PachinkoBall.FireballState.FIRING && !_pBall.IsDummy && ++_currentRefreshCounter >= numberOfRefreshToTrigger)
			{
				_currentRefreshCounter = 0;
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(new StatusEffect[1]
				{
					new StatusEffect(statusEffect, intensity)
				}, StatusEffectSource.PLAYER);
			}
		}
	}
}
