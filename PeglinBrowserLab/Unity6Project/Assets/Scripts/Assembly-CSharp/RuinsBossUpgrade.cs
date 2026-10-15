using System;
using System.Collections.Generic;
using Battle.Enemies;
using Battle.Enemies.C20Upgrades;
using Battle.StatusEffects;
using UnityEngine;

public class RuinsBossUpgrade : BossUpgrade
{
	private RotatingSquareEnemy _myRuinEnemy;

	private void Start()
	{
		_myRuinEnemy = _enemy.GetComponent<RotatingSquareEnemy>();
	}

	private void OnEnable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
	}

	private void OnDisable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
	}

	public override void Upgrade()
	{
	}

	private void OnEnemyKilled(Enemy enemy)
	{
		if (enemy != _enemy)
		{
			return;
		}
		RotatingSquareEnemy[] array = UnityEngine.Object.FindObjectsOfType<RotatingSquareEnemy>();
		Dictionary<int, RotatingSquareEnemy> dictionary = new Dictionary<int, RotatingSquareEnemy>();
		RotatingSquareEnemy[] array2 = array;
		foreach (RotatingSquareEnemy rotatingSquareEnemy in array2)
		{
			dictionary.Add(rotatingSquareEnemy.currentRotation, rotatingSquareEnemy);
		}
		int currentRotation = _myRuinEnemy.currentRotation;
		for (int j = 0; j < 3; j++)
		{
			int key = (currentRotation + 1 + j) % 4;
			if (!dictionary.ContainsKey(key))
			{
				continue;
			}
			RotatingSquareEnemy rotatingSquareEnemy2 = dictionary[key];
			StatusEffect[] statusEffects = _enemy.StatusEffects;
			foreach (StatusEffect statusEffect in statusEffects)
			{
				if (isUpgraded || StatusEffect.IsStatusEffectDebuff(statusEffect.EffectType))
				{
					rotatingSquareEnemy2.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, Mathf.CeilToInt((float)statusEffect.Intensity * 0.2f)), StatusEffectSource.ENEMY);
				}
			}
			break;
		}
	}
}
