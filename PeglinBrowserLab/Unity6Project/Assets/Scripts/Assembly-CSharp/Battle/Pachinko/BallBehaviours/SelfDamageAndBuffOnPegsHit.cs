using System;
using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class SelfDamageAndBuffOnPegsHit : MonoBehaviour
	{
		public float selfDamage = 3f;

		public float numberOfPegs = 20f;

		public StatusEffectType effectType = StatusEffectType.Strength;

		public int intensity = 1;

		private BattleController _battleController;

		private PachinkoBall _pachinkoBall;

		private PlayerHealthController _playerHealthController;

		private PlayerStatusEffectController _playerStatusEffectController;

		private int _currentHits;

		private void OnEnable()
		{
			if (_battleController == null)
			{
				_battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
			}
			if (_pachinkoBall == null)
			{
				_pachinkoBall = GetComponent<PachinkoBall>();
			}
			if (_playerHealthController == null)
			{
				_playerHealthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
			}
			if (_playerStatusEffectController == null)
			{
				_playerStatusEffectController = UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>();
			}
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void OnDisable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue("SELF_DAMAGE_AMOUNT", selfDamage.ToString());
			component.SetParameterValue("NUMBER_OF_PEGS", numberOfPegs.ToString());
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (!_pachinkoBall.IsDummy && peg.pegType == Peg.PegType.REGULAR)
			{
				_currentHits++;
				if ((float)_currentHits >= numberOfPegs)
				{
					_currentHits = 0;
					_playerHealthController.DealSelfDamage(selfDamage, blockable: true, PlayerHealthController.DamageSource.ORB_SELF);
					_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(effectType, intensity), StatusEffectSource.PLAYER);
				}
			}
		}
	}
}
