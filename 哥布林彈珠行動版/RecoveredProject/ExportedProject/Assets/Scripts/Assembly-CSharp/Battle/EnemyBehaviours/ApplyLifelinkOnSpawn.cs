using System;
using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ApplyLifelinkOnSpawn : MonoBehaviour
	{
		public int lifelinkIntensity = 10;

		public string effectPegString = "";

		private Enemy _enemy;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private EffectPeg _effectPeg;

		private bool _toggled;

		private IEnumerator Start()
		{
			_enemy = GetComponent<Enemy>();
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (effectPegString == "")
			{
				EnableLifelink();
			}
		}

		private void OnEnable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(EnableLifelink));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Combine(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
		}

		private void OnDisable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(EnableLifelink));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Remove(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
			DisableLifelink();
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
				EnableLifelink();
			}
			else
			{
				DisableLifelink();
			}
		}

		private void EnableLifelink()
		{
			_toggled = true;
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Lifelink, lifelinkIntensity), StatusEffectSource.ENEMY);
				enemy.CreateLifelinkRenderer();
			}
		}

		private void DisableLifelink()
		{
			_toggled = false;
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.RemoveEffect(StatusEffectType.Lifelink);
				enemy.DestroyLifelinkRenderer();
			}
		}

		private void OnEnemySpawned(Enemy enemy)
		{
			if (_toggled && enemy.GetEffectIntensity(StatusEffectType.Lifelink) == 0)
			{
				enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Lifelink, lifelinkIntensity), StatusEffectSource.ENEMY);
			}
		}
	}
}
