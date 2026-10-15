using System.Collections.Generic;
using UnityEngine;

namespace Peglin.Achievements
{
	public class DebugAchievementUnlocker : MonoBehaviour, IAchievementUnlocker
	{
		public void SetupAchievementData()
		{
			Debug.Log("Setting up achievement data... Debug achievements set to default.");
			Debug.Log("Testing static achievement data...");
			foreach (KeyValuePair<AchievementData.AchievementId, Achievement> achievement in AchievementData.Achievements)
			{
				if (achievement.Key != achievement.Value.Id)
				{
					Debug.LogError($"Achievement key {achievement.Key} does not match value {achievement.Value.Id} in achievement data!");
				}
			}
		}

		public void UnlockAchievement(Achievement a)
		{
			Debug.Log($"Requested Achievement {a.Id} unlock...");
			if (!a.Unlocked)
			{
				Debug.Log($"Achievement {a.Id} unlocked!");
				a.Unlocked = true;
			}
		}
	}
}
