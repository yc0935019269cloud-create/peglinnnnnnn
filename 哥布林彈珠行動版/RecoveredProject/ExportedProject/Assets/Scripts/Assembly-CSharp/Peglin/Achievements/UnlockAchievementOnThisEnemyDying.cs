using System;
using Battle.Enemies;
using Map;
using UnityEngine;

namespace Peglin.Achievements
{
	public class UnlockAchievementOnThisEnemyDying : MonoBehaviour
	{
		public int ActRequirement = -1;

		public AchievementData.AchievementId AchToUnlock;

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckAndUnlockAchievement));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckAndUnlockAchievement));
		}

		private void CheckAndUnlockAchievement(Enemy enemy)
		{
			if (ActRequirement <= 0 || MapController.instance.Act == ActRequirement)
			{
				if (AchievementData.Achievements[AchToUnlock].Unlocked)
				{
					base.enabled = false;
				}
				else if (enemy == GetComponent<Enemy>() && enemy.CurrentHealth <= 0f)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchToUnlock]);
				}
			}
		}
	}
}
