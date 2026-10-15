using System;
using Battle;
using Battle.Enemies;
using UnityEngine;

namespace Peglin.Achievements
{
	[RequireComponent(typeof(Enemy))]
	public class UnlockAchievementOnFirstTurnDeath : MonoBehaviour
	{
		private bool canUnlockAchievement;

		private Enemy thisEnemy;

		private void OnEnable()
		{
			if (thisEnemy == null)
			{
				thisEnemy = GetComponent<Enemy>();
			}
			Enemy.OnEnemySpawned = (Enemy.EnemySpawned)Delegate.Combine(Enemy.OnEnemySpawned, new Enemy.EnemySpawned(EnableAchievement));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(DisableAchievement));
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(UnlockAchievement));
			if (AchievementData.Achievements[AchievementData.AchievementId.DEFEAT_ANY_BOSS_FIRST_TURN].Unlocked)
			{
				base.enabled = false;
			}
		}

		private void OnDisable()
		{
			Enemy.OnEnemySpawned = (Enemy.EnemySpawned)Delegate.Remove(Enemy.OnEnemySpawned, new Enemy.EnemySpawned(EnableAchievement));
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(DisableAchievement));
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(UnlockAchievement));
		}

		private void EnableAchievement(Enemy e)
		{
			if (e == thisEnemy)
			{
				canUnlockAchievement = true;
			}
		}

		private void DisableAchievement()
		{
			if (canUnlockAchievement)
			{
				canUnlockAchievement = false;
			}
		}

		private void UnlockAchievement(Enemy e, long damage, Enemy.EnemyDamageSource damageSource)
		{
			if (canUnlockAchievement && e == thisEnemy && e.CurrentHealth <= 0f)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.DEFEAT_ANY_BOSS_FIRST_TURN]);
			}
		}
	}
}
