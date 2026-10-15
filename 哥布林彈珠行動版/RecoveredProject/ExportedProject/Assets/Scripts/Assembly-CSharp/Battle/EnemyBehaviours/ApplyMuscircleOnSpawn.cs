using System;
using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ApplyMuscircleOnSpawn : MonoBehaviour
	{
		public string effectPegString = "";

		public int muscircleIntensity = 4;

		private Enemy _enemy;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private bool _spiritOfRadiaMuscircleSaved;

		private EffectPeg _effectPeg;

		private bool _toggled;

		private IEnumerator Start()
		{
			_enemy = GetComponent<Enemy>();
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (effectPegString == "")
			{
				EnableMuscircle();
			}
		}

		private void OnEnable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(EnableMuscircle));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Combine(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
		}

		private void OnDisable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(EnableMuscircle));
			EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Remove(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnEnemySpawned));
			DisableMuscircle();
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
				EnableMuscircle();
			}
			else
			{
				DisableMuscircle();
			}
		}

		private void EnableMuscircle()
		{
			_toggled = true;
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, muscircleIntensity), StatusEffectSource.ENEMY);
			}
		}

		private void DisableMuscircle()
		{
			if (!_spiritOfRadiaMuscircleSaved && _spiritOfRadia != null)
			{
				muscircleIntensity = _spiritOfRadia.GetEffectIntensity(StatusEffectType.Strength);
				_spiritOfRadiaMuscircleSaved = true;
			}
			_toggled = false;
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				enemy.RemoveEffect(StatusEffectType.Strength);
			}
		}

		private void OnEnemySpawned(Enemy enemy)
		{
			if (_toggled && enemy.GetEffectIntensity(StatusEffectType.Strength) == 0)
			{
				enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, muscircleIntensity), StatusEffectSource.ENEMY);
			}
		}
	}
}
