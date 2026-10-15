using System;
using Battle.Enemies;
using UnityEngine;

public class StatsController : MonoBehaviour
{
	public int NumEnemiesKilled;

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void OnEnable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(IncrementKills));
	}

	private void OnDisable()
	{
		Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(IncrementKills));
	}

	private void IncrementKills(Enemy enemy)
	{
		NumEnemiesKilled++;
	}
}
