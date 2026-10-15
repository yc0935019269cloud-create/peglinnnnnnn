using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ApplyResistancesOnSpawn : MonoBehaviour
	{
		public string effectPegString = "";

		public bool removeIntangiball = true;

		public int initialIntangiballToGive = -1;

		public bool multiplyIntangiballInsteadOfRemove;

		public float multiplyIntangiballFactor = 2f;

		public bool halveDamage;

		private Enemy _enemy;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private EffectPeg _effectPeg;

		private bool _toggledState;

		private List<Enemy> _enemiesGivenResistance = new List<Enemy>();

		private Dictionary<Enemy, int> _enemiesWithRemovedIntangiball = new Dictionary<Enemy, int>();

		public bool toggledState => _toggledState;

		private IEnumerator Start()
		{
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			_enemy = GetComponent<Enemy>();
			yield return WaitObjectAllocator.waitForEndOfFrame;
			EnableEffects();
		}

		private void OnEnable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Combine(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
		}

		private void OnDisable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Remove(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
			DisableEffects();
		}

		private void OnEffectPegCreated(EffectPeg effectPeg)
		{
			if (_spiritOfRadia != null && _effectPeg == null && effectPeg.effectName == effectPegString)
			{
				_effectPeg = effectPeg;
				EffectPeg effectPeg2 = _effectPeg;
				effectPeg2.OnEffectPegToggled = (EffectPeg.EffectPegToggled)Delegate.Combine(effectPeg2.OnEffectPegToggled, new EffectPeg.EffectPegToggled(OnEffectPegToggled));
			}
		}

		private void OnEffectPegToggled(bool state)
		{
			if (state)
			{
				EnableEffects();
			}
			else
			{
				DisableEffects();
			}
		}

		private void EnableEffects()
		{
			_toggledState = true;
			if (halveDamage)
			{
				EnableResistance();
			}
			if (removeIntangiball)
			{
				ReapplyIntangiball();
			}
		}

		private void DisableEffects()
		{
			_toggledState = false;
			if (halveDamage)
			{
				DisableResistance();
			}
			if (removeIntangiball)
			{
				RemoveIntangiball();
			}
		}

		private void EnableResistance()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				if (!_enemiesGivenResistance.Contains(enemy))
				{
					enemy.bombDamageMod = 0.5f;
					enemy.projectileAttackDamageMod = 0.5f;
					enemy.spellAttackDamageMod = 0.5f;
					_enemiesGivenResistance.Add(enemy);
				}
			}
		}

		private void DisableResistance()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				if (_enemiesGivenResistance.Contains(enemy))
				{
					enemy.bombDamageMod = 1f;
					enemy.projectileAttackDamageMod = 1f;
					enemy.spellAttackDamageMod = 1f;
					_enemiesGivenResistance.Remove(enemy);
				}
			}
		}

		private void RemoveIntangiball()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				if (!_enemiesWithRemovedIntangiball.Keys.Contains(enemy))
				{
					_enemiesWithRemovedIntangiball[enemy] = enemy.GetEffectIntensity(StatusEffectType.Intangiball);
					if (multiplyIntangiballInsteadOfRemove)
					{
						int effectIntensity = enemy.GetEffectIntensity(StatusEffectType.Intangiball);
						int intensity = Mathf.RoundToInt((float)effectIntensity * multiplyIntangiballFactor) - effectIntensity;
						enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, intensity), StatusEffectSource.ENEMY);
					}
					else
					{
						enemy.RemoveEffect(StatusEffectType.Intangiball);
					}
				}
			}
		}

		private void ReapplyIntangiball()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				if (!_enemiesWithRemovedIntangiball.Keys.Contains(enemy))
				{
					if (enemy.GetEffectIntensity(StatusEffectType.Intangiball) == 0)
					{
						enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, initialIntangiballToGive), StatusEffectSource.ENEMY);
					}
					continue;
				}
				if (multiplyIntangiballInsteadOfRemove)
				{
					enemy.ReduceEffectIntensity(StatusEffectType.Intangiball, _enemiesWithRemovedIntangiball[enemy]);
				}
				else
				{
					enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, _enemiesWithRemovedIntangiball[enemy]), StatusEffectSource.ENEMY);
				}
				_enemiesWithRemovedIntangiball.Remove(enemy);
			}
		}

		private void OnEnemySpawned(Enemy enemy)
		{
			if (halveDamage && _toggledState && !_enemiesGivenResistance.Contains(enemy))
			{
				enemy.bombDamageMod = 0.5f;
				enemy.projectileAttackDamageMod = 0.5f;
				enemy.spellAttackDamageMod = 0.5f;
				_enemiesGivenResistance.Add(enemy);
			}
			if (removeIntangiball && _toggledState && !_enemiesWithRemovedIntangiball.Keys.Contains(enemy) && enemy.GetEffectIntensity(StatusEffectType.Intangiball) == 0)
			{
				enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, initialIntangiballToGive), StatusEffectSource.ENEMY);
			}
		}
	}
}
