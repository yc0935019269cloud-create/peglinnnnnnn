using System;
using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class GainStatusEffectOnCrit : MonoBehaviour
	{
		public StatusEffectType statusEffect;

		public int intensity;

		public int numberOfCritsToTrigger;

		private int _currentCritCounter;

		private PachinkoBall _pBall;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("NUMBER_OF_CRITS", numberOfCritsToTrigger.ToString());
		}

		private void OnEnable()
		{
			_pBall = GetComponent<PachinkoBall>();
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(CheckAndApplyEffect));
		}

		private void OnDisable()
		{
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(CheckAndApplyEffect));
		}

		private void CheckAndApplyEffect()
		{
			if (_pBall.CurrentState == PachinkoBall.FireballState.FIRING && !_pBall.IsDummy && ++_currentCritCounter >= numberOfCritsToTrigger)
			{
				_currentCritCounter = 0;
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(new StatusEffect[1]
				{
					new StatusEffect(statusEffect, intensity)
				}, StatusEffectSource.PLAYER);
			}
		}
	}
}
