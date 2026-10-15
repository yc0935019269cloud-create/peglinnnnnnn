using System;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using Relics;

namespace Battle.Attacks
{
	public class AoeAttack : TargetedAttack
	{
		public enum AoeType
		{
			SIDE = 0,
			BEHIND = 1,
			ALL = 2
		}

		public AoeType aoeType;

		public int range = 1;

		protected override void SpawnSpell(SpellBehavior spell, Enemy target, int critCount, bool isExtraAttack = false)
		{
			_critCount = critCount;
			if (target == null || !target.gameObject.activeSelf)
			{
				_attackManager.AttackAnimationEnded();
				return;
			}
			spell.gameObject.transform.position = target.GetComponentInChildren<TargetingUI>(includeInactive: true).transform.position;
			spell.gameObject.SetActive(value: true);
			spell.OnSpellHit = (SpellBehavior.SpellHit)Delegate.Combine(spell.OnSpellHit, new SpellBehavior.SpellHit(HandleSpellHit));
			spell.OnSpellEnded = (SpellBehavior.SpellEnded)Delegate.Combine(spell.OnSpellEnded, new SpellBehavior.SpellEnded(HandleSpellEnded));
		}

		private void HandleSpellHit(Enemy target)
		{
			if (target == null || !target.gameObject.activeSelf)
			{
				return;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS))
			{
				range++;
			}
			AoeType aoeType = this.aoeType;
			if (_relicManager.AttemptUseRelic(RelicEffect.TARGETED_ATTACKS_HIT_ALL))
			{
				aoeType = AoeType.ALL;
			}
			Enemy[] splashRangeEnemies = _attackManager.enemyManager.GetSplashRangeEnemies(_attackManager.enemyManager.GetSlotIndexForEnemy(target, out var _), _attackManager.enemyManager.GetSlotForEnemy(target), range, aoeType);
			foreach (Enemy enemy in splashRangeEnemies)
			{
				if (enemy == null)
				{
					continue;
				}
				enemy.DealTargetedAttackDamage(_hitDamage, GetDamageMod(enemy.spellAttackDamageMod), _critCount);
				IAffectEnemyOnHit[] components = GetComponents<IAffectEnemyOnHit>();
				if (components != null && components.Length != 0)
				{
					IAffectEnemyOnHit[] array = components;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].AffectEnemy(enemy, _relicManager, _critCount);
					}
				}
				foreach (StatusEffect statusEffect in GetStatusEffects(_critCount))
				{
					enemy.ApplyStatusEffect(statusEffect, StatusEffectSource.PLAYER);
				}
				_attackManager.WaitForEnemyDamageAnimationToEnd(enemy);
			}
		}

		private void HandleSpellEnded()
		{
			_attackManager.AttackAnimationEnded();
		}
	}
}
