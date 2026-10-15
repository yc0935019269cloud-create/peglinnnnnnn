using System;
using System.Collections.Generic;
using Battle.Enemies;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class UnlockCoinCollectingAchievement : MonoBehaviour
	{
		private PegManager _pegManager;

		private bool _shouldCheckAchievement = true;

		private List<Peg> _allCoinPegs;

		private Enemy _enemy;

		private void OnEnable()
		{
			if (AchievementData.Achievements[AchievementData.AchievementId.COLLECT_ALL_DRAGON_COINS].Unlocked)
			{
				_shouldCheckAchievement = false;
				return;
			}
			_pegManager = UnityEngine.Object.FindObjectOfType<BattleController>().pegManager;
			_enemy = GetComponent<Enemy>();
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckForCoins));
		}

		private void OnDisable()
		{
			if (_shouldCheckAchievement)
			{
				Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckForCoins));
			}
		}

		private void CheckForCoins(Enemy enemy)
		{
			if (_enemy != enemy)
			{
				return;
			}
			bool flag = true;
			foreach (Peg allPeg in _pegManager.allPegs)
			{
				if (allPeg.pegType == Peg.PegType.REGULAR && !allPeg.IsDisabled() && allPeg.NumCoins() > 0)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.COLLECT_ALL_DRAGON_COINS);
			}
		}
	}
}
