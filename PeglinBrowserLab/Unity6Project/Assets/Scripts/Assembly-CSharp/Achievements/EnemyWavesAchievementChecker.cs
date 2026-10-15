using Peglin.Achievements;
using UnityEngine;

namespace Achievements
{
	public class EnemyWavesAchievementChecker : MonoBehaviour
	{
		public AchievementData.AchievementId achievementToUnlock = AchievementData.AchievementId.SPAWN_ALL_MINIONS;

		private Spawner _spawner;

		private bool achievementUnlocked;

		private void OnEnable()
		{
			_spawner = Object.FindObjectOfType<Spawner>();
			achievementUnlocked = AchievementData.Achievements[achievementToUnlock].Unlocked;
		}

		private void Update()
		{
			if (!_spawner.HasSpawnsLeft() && !achievementUnlocked)
			{
				AchievementManager.Instance.UnlockAchievement(achievementToUnlock);
			}
		}
	}
}
