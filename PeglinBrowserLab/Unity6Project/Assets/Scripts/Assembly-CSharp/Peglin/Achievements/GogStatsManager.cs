using UnityEngine;

namespace Peglin.Achievements
{
	public class GogStatsManager : MonoBehaviour, IAchievementUnlocker
	{
		public void UnlockAchievement(Achievement a)
		{
			Debug.LogWarning("Steamworks Achievement interface called even though Steamworks is disabled!");
		}

		public void SetupAchievementData()
		{
			Debug.LogWarning("Steamworks Achievement interface called even though Steamworks is disabled!");
		}
	}
}
