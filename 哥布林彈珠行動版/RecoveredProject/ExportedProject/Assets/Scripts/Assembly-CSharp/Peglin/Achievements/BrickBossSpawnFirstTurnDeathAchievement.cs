using UnityEngine;

namespace Peglin.Achievements
{
	public class BrickBossSpawnFirstTurnDeathAchievement : MonoBehaviour
	{
		private void Awake()
		{
			new GameObject("BrickBossFirstTurnDeathUnlocker").AddComponent<BrickBossFirstTurnDeathAchievementUnlocker>();
		}
	}
}
