using System;
using System.Collections.Generic;
using Battle;
using Battle.Enemies;
using UnityEngine;

namespace Peglin.Achievements
{
	public class BrickBossFirstTurnDeathAchievementUnlocker : MonoBehaviour
	{
		private List<Enemy> _enemiesToTrack = new List<Enemy>();

		private bool canUnlockAchievement = true;

		private void Start()
		{
			_enemiesToTrack.Clear();
			GameObject[] array = GameObject.FindGameObjectsWithTag("BrickBoss");
			foreach (GameObject gameObject in array)
			{
				_enemiesToTrack.Add(gameObject.GetComponent<Enemy>());
			}
			Enemy.OnEnemySpawned = (Enemy.EnemySpawned)Delegate.Combine(Enemy.OnEnemySpawned, new Enemy.EnemySpawned(AddEnemyToTrack));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(DisableAchievement));
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckAndUnlockAchievement));
		}

		private void OnDisable()
		{
			Enemy.OnEnemySpawned = (Enemy.EnemySpawned)Delegate.Remove(Enemy.OnEnemySpawned, new Enemy.EnemySpawned(AddEnemyToTrack));
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(DisableAchievement));
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckAndUnlockAchievement));
		}

		private void AddEnemyToTrack(Enemy e)
		{
			if (!_enemiesToTrack.Contains(e))
			{
				canUnlockAchievement = false;
			}
		}

		private void DisableAchievement()
		{
			if (canUnlockAchievement)
			{
				canUnlockAchievement = false;
			}
		}

		private void CheckAndUnlockAchievement(Enemy enemy)
		{
			if (!canUnlockAchievement)
			{
				return;
			}
			int num = 0;
			foreach (Enemy item in _enemiesToTrack)
			{
				if (item.CurrentHealth == 0f)
				{
					num++;
				}
			}
			if (num == _enemiesToTrack.Count)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.DEFEAT_ANY_BOSS_FIRST_TURN]);
			}
		}
	}
}
