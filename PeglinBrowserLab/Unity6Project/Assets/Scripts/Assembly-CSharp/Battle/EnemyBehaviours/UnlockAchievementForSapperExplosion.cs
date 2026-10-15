using System;
using Battle.Enemies;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class UnlockAchievementForSapperExplosion : MonoBehaviour
	{
		public float DamageThreshold = 1000f;

		private Enemy _thisEnemy;

		private float _runningTotal;

		private void OnEnable()
		{
			_thisEnemy = GetComponent<Enemy>();
			_runningTotal = 0f;
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(OnEnemyDamaged));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(OnEnemyDamaged));
		}

		private void OnEnemyDamaged(Enemy enemy, long damage, Enemy.EnemyDamageSource damageSource)
		{
			if (!(enemy != _thisEnemy))
			{
				if (damageSource == Enemy.EnemyDamageSource.OtherEnemies)
				{
					_runningTotal += damage;
				}
				if (_runningTotal > DamageThreshold)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DEAL_1000_DAMAGE_TO_SAPPER_WITH_SAPPERS);
				}
			}
		}
	}
}
