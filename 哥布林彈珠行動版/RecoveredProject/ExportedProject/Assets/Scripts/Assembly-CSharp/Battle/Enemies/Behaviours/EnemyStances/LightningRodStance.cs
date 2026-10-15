using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Behaviours.EnemyStances
{
	public class LightningRodStance : EnemyStance
	{
		private string _animationOverride = "stance_change";

		public float damagePerMeleeAttack = 14f;

		public float projectileAttackDamageMod = 1f;

		public float spellAttackDamageMod = 0.5f;

		public StatusEffect applyWhenHitByTargeted;

		private float _regulardamagePerMeleeAttack;

		private float _regularProjectileAttackDamageMod = 1f;

		private float _regularSpellAttackDamageMod = 1f;

		private Animator _animator;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		public override void ApplyStanceToEnemy(Enemy enemy, bool initial = false)
		{
			if (!initial)
			{
				_animator.SetTrigger(_animationOverride);
			}
			_regulardamagePerMeleeAttack = enemy.DamagePerMeleeAttack;
			_regularProjectileAttackDamageMod = enemy.projectileAttackDamageMod;
			_regularSpellAttackDamageMod = enemy.spellAttackDamageMod;
			enemy.canAttack = true;
			enemy.DamagePerMeleeAttack = damagePerMeleeAttack;
			enemy.projectileAttackDamageMod = projectileAttackDamageMod;
			enemy.spellAttackDamageMod = spellAttackDamageMod;
			enemy.applyStatWhenTargetedAttacked = applyWhenHitByTargeted;
		}

		public override void RemoveStanceOnEnemy(Enemy enemy)
		{
			enemy.DamagePerMeleeAttack = _regulardamagePerMeleeAttack;
			enemy.projectileAttackDamageMod = _regularProjectileAttackDamageMod;
			enemy.spellAttackDamageMod = _regularSpellAttackDamageMod;
			enemy.applyStatWhenTargetedAttacked = null;
		}
	}
}
