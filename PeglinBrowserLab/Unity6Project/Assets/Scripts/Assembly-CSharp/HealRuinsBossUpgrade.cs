using System;
using Battle.Enemies;
using Battle.Enemies.C20Upgrades;
using UnityEngine;

public class HealRuinsBossUpgrade : BossUpgrade
{
	public float healAmount = 250f;

	private RotatingSquareEnemy _myRuinEnemy;

	private void Start()
	{
		_myRuinEnemy = _enemy.GetComponent<RotatingSquareEnemy>();
	}

	public override void Upgrade()
	{
	}

	private void OnEnable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
	}

	private void OnDisable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
	}

	private void OnEnemyKilled(Enemy enemy)
	{
		if (!isUpgraded || enemy != _enemy)
		{
			return;
		}
		RotatingSquareEnemy[] array = UnityEngine.Object.FindObjectsOfType<RotatingSquareEnemy>();
		foreach (RotatingSquareEnemy rotatingSquareEnemy in array)
		{
			if (!(rotatingSquareEnemy.gameObject == base.gameObject))
			{
				rotatingSquareEnemy.Heal(healAmount);
			}
		}
	}
}
