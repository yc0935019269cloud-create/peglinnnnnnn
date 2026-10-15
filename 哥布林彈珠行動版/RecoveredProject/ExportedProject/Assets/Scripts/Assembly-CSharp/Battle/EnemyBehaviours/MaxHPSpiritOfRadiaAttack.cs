using System;
using System.Collections;
using Battle.Enemies;
using Battle.Enemies.Attacks;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class MaxHPSpiritOfRadiaAttack : AlternateEnemyAction
	{
		public string animOverride = "";

		public float midWaitTime = 2f;

		public string animOverrideEnd = "";

		public string effectPegString = "";

		public int forSpiritOfRadiaPhase = 2;

		public Projectile maxHPDrainProjectile;

		public int maxHpAttackDamage = 5;

		public float postAttackWaitTime = 2f;

		private Animator _anim;

		private SpiritOfRadiaBoss _spiritOfRadiaBoss;

		private EffectPeg _effectPeg;

		private ApplyMaxHpDrainOnSpawn _applyMaxHpDrainOnSpawnComponent;

		private float _normalRangedAttackDamage;

		private void Start()
		{
			_anim = _enemy.GetComponent<Animator>();
			_spiritOfRadiaBoss = _enemy as SpiritOfRadiaBoss;
			_applyMaxHpDrainOnSpawnComponent = GetComponent<ApplyMaxHpDrainOnSpawn>();
			_normalRangedAttackDamage = _enemy.DamagePerRangedAttack;
		}

		private void OnEnable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
		}

		private void OnDisable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
		}

		private void OnEffectPegCreated(EffectPeg effectPeg)
		{
			if (_spiritOfRadiaBoss != null && _effectPeg == null && effectPeg.effectName == effectPegString)
			{
				_effectPeg = effectPeg;
			}
		}

		public override bool ConditionsMet()
		{
			if (_spiritOfRadiaBoss != null)
			{
				if (SpiritOfRadiaBoss.CurrentPhase == forSpiritOfRadiaPhase)
				{
					if (!(_effectPeg == null))
					{
						return _effectPeg.pegActive;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			PlaySFX();
			int num = Mathf.Max(maxHpAttackDamage + _enemy.GetEffectIntensity(StatusEffectType.Strength), 1);
			_enemy.SetDamageToMaxHp(newValue: true, num, num);
			_enemy.UnblockableAttacks = true;
			_enemy.postAttackWaitTime = postAttackWaitTime;
			StartCoroutine(WaitAndEndAnimation());
			yield return _enemy.RangedAttack(target, num, maxHPDrainProjectile.gameObject, animOverride);
			_spiritOfRadiaBoss.ReduceEffectIntensity(StatusEffectType.Blind, Mathf.RoundToInt((float)_spiritOfRadiaBoss.GetEffectIntensity(StatusEffectType.Blind) * _spiritOfRadiaBoss.blindReductionPercentPerMaxHPAttack));
			_enemy.postAttackWaitTime = 0f;
			_enemy.SetDamageToMaxHp(newValue: false);
			_enemy.UnblockableAttacks = false;
		}

		private IEnumerator WaitAndEndAnimation()
		{
			yield return new WaitForSeconds(midWaitTime);
			_anim.SetTrigger(animOverrideEnd);
		}

		public override bool IncrementAndCheckCharge()
		{
			if (SpiritOfRadiaBoss.CurrentPhase != forSpiritOfRadiaPhase)
			{
				return false;
			}
			return base.IncrementAndCheckCharge();
		}
	}
}
