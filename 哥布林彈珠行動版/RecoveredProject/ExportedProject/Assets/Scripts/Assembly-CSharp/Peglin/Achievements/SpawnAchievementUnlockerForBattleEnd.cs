using UnityEngine;

namespace Peglin.Achievements
{
	public class SpawnAchievementUnlockerForBattleEnd : MonoBehaviour
	{
		public AchievementData.AchievementId AchievementToUnlock;

		private bool _spawned;

		private void OnEnable()
		{
			if (!_spawned)
			{
				new GameObject("BattleEndAchievementUnlocker").AddComponent<UnlockAchievementOnBattleWin>().AchievementToUnlock = AchievementToUnlock;
				_spawned = true;
			}
		}
	}
}
