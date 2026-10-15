using System;
using Battle.Attacks;
using Battle.Enemies;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class UnlockRuinsBossOneShotAchievement : MonoBehaviour
	{
		private int enemiesDeadInOneTurn;

		private int totalEnemiesDead;

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CountEnemies));
			AttackManager.OnAttackEnded = (AttackManager.AttackEnded)Delegate.Combine(AttackManager.OnAttackEnded, new AttackManager.AttackEnded(ResetCount));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CountEnemies));
			AttackManager.OnAttackEnded = (AttackManager.AttackEnded)Delegate.Remove(AttackManager.OnAttackEnded, new AttackManager.AttackEnded(ResetCount));
		}

		private void ResetCount()
		{
			enemiesDeadInOneTurn = 0;
		}

		private void CountEnemies(Enemy enemy)
		{
			enemiesDeadInOneTurn++;
			totalEnemiesDead++;
			if (enemiesDeadInOneTurn >= 4)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.KILL_ALL_RUINS_IN_ONE_ATTACK);
			}
			if (totalEnemiesDead >= 4 && UnityEngine.Object.FindObjectOfType<PlayerHealthController>().CurrentHealth <= 0f)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DIE_WHILE_KILLING_MINES_BOSS);
			}
		}
	}
}
