using System;
using System.Collections;
using Battle.Enemies;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ApplyMaxHpDrainOnSpawn : MonoBehaviour
	{
		public string effectPegString = "";

		public float damageToMaxHp = 2f;

		public float normalDamage = 8f;

		public bool applyOnSpawn = true;

		private Enemy _enemy;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private EffectPeg _effectPeg;

		private IEnumerator Start()
		{
			_enemy = GetComponent<Enemy>();
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (applyOnSpawn)
			{
				EnableMaxHpAttacks();
			}
		}

		private void OnEnemySpawned(Enemy enemy)
		{
			enemy.SetDamageToMaxHp(newValue: true, Mathf.RoundToInt(enemy.DamagePerMeleeAttack / 4f), Mathf.RoundToInt(enemy.DamagePerRangedAttack / 4f));
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
			DisableMaxHpAttacks();
		}

		private void EnableMaxHpAttacks()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.SetDamageToMaxHp(newValue: true, Mathf.RoundToInt(enemy.DamagePerMeleeAttack / 4f), Mathf.RoundToInt(enemy.DamagePerRangedAttack / 4f));
			}
		}

		private void DisableMaxHpAttacks()
		{
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.SetDamageToMaxHp(newValue: false);
			}
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
				EnableMaxHpAttacks();
			}
			else
			{
				DisableMaxHpAttacks();
			}
		}
	}
}
