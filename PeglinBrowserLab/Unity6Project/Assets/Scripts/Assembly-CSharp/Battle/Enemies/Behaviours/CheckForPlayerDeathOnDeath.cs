using System;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class CheckForPlayerDeathOnDeath : MonoBehaviour
	{
		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
		}

		private void OnEnemyDestroyed(Enemy enemy)
		{
			if (enemy == GetComponent<Enemy>() && UnityEngine.Object.FindObjectOfType<PlayerHealthController>().CurrentHealth <= 0f)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DIE_WHILE_KILLING_MINES_BOSS);
			}
		}
	}
}
