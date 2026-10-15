using System;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks
{
	public class SimpleAttack : Attack
	{
		public GameObject shotPrefab;

		private SimpleShotAll _shotAll;

		protected long _hitDamage;

		protected int _critCount;

		public override void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long bonus, int critCount = 0)
		{
			_critCount = critCount;
			_attackManager = attackManager;
			_shotAll = UnityEngine.Object.Instantiate(shotPrefab).GetComponentInChildren<SimpleShotAll>();
			_shotAll.Initialize(attackManager.SfxAudioSource);
			SimpleShotAll shotAll = _shotAll;
			shotAll.OnHitPointReached = (SimpleShotAll.AttackEvent)Delegate.Combine(shotAll.OnHitPointReached, new SimpleShotAll.AttackEvent(DealDamageAll));
			SimpleShotAll shotAll2 = _shotAll;
			shotAll2.OnAttackAnimationEnded = (SimpleShotAll.AttackEvent)Delegate.Combine(shotAll2.OnAttackAnimationEnded, new SimpleShotAll.AttackEvent(HandleAttackAnimationEnded));
			_critCount = critCount;
			_hitDamage = GetDamage(attackManager, pegMultipliersTally, dmgMult, bonus, critCount);
		}

		private void DealDamageAll()
		{
			IAffectEnemyOnHit[] components = GetComponents<IAffectEnemyOnHit>();
			for (int i = 0; i < _attackManager.enemyManager.Enemies.Count; i++)
			{
				Enemy enemy = _attackManager.enemyManager.Enemies[i];
				if (enemy == null)
				{
					continue;
				}
				enemy.DamageWithTypeMods(_hitDamage, 1f, _critCount, Enemy.EnemyDamageSource.AOE);
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
					if (statusEffect.EffectType != StatusEffectType.None)
					{
						enemy.ApplyStatusEffect(statusEffect, StatusEffectSource.PLAYER);
					}
				}
				_attackManager.WaitForEnemyDamageAnimationToEnd(enemy);
			}
		}

		private void HandleAttackAnimationEnded()
		{
			_attackManager.AttackAnimationEnded();
		}
	}
}
