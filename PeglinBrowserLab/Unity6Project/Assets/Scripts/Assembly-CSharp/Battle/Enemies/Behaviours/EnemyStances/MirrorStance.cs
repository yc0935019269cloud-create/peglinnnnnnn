using System.Collections;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies.Behaviours.EnemyStances
{
	public class MirrorStance : EnemyStance
	{
		private string _animationOverride = "stance_change";

		public float damagePerMeleeAttack = 7f;

		public float projectileAttackDamageMod = 0.5f;

		public float spellAttackDamageMod = 1f;

		public StatusEffect applyWhenHitByProjectile;

		public int startingReflectPercent = 4;

		public int reflectIncrease = 2;

		private float _regulardamagePerMeleeAttack;

		private float _regularProjectileAttackDamageMod = 1f;

		private float _regularSpellAttackDamageMod = 1f;

		private int _currentReflectionIntensity;

		private Animator _animator;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_currentReflectionIntensity = startingReflectPercent;
		}

		public override void ApplyStanceToEnemy(Enemy enemy, bool initial = false)
		{
			if (!initial)
			{
				_animator.SetTrigger(_animationOverride);
			}
			StartCoroutine(WaitAndApplyStatuses(enemy));
		}

		private IEnumerator WaitAndApplyStatuses(Enemy enemy)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_regulardamagePerMeleeAttack = enemy.DamagePerMeleeAttack;
			_regularProjectileAttackDamageMod = enemy.projectileAttackDamageMod;
			_regularSpellAttackDamageMod = enemy.spellAttackDamageMod;
			enemy.canAttack = false;
			enemy.DamagePerMeleeAttack = damagePerMeleeAttack;
			enemy.projectileAttackDamageMod = projectileAttackDamageMod;
			enemy.spellAttackDamageMod = spellAttackDamageMod;
			enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Reflect, _currentReflectionIntensity), StatusEffectSource.ENEMY);
			enemy.applyStatWhenProjectileAttacked = applyWhenHitByProjectile;
			_currentReflectionIntensity += reflectIncrease;
		}

		public override void RemoveStanceOnEnemy(Enemy enemy)
		{
			enemy.DamagePerMeleeAttack = _regulardamagePerMeleeAttack;
			enemy.projectileAttackDamageMod = _regularProjectileAttackDamageMod;
			enemy.spellAttackDamageMod = _regularSpellAttackDamageMod;
			enemy.RemoveEffect(StatusEffectType.Reflect);
			enemy.applyStatWhenProjectileAttacked = null;
		}
	}
}
