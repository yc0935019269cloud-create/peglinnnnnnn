using System;
using System.Collections.Generic;
using Peglin.Achievements;
using UnityEngine;

namespace Achievements
{
	[Serializable]
	[CreateAssetMenu(menuName = "Achievements/Achievement Database")]
	public class AchievementDatabase : ScriptableObject
	{
		[SerializeField]
		public List<Achievement> achievementList = new List<Achievement>(1);

		private Dictionary<AchievementData.AchievementId, Achievement> achievementLookupDictionary;

		public Achievement GetAchievement(AchievementData.AchievementId id)
		{
			if (achievementLookupDictionary == null)
			{
				LoadAchievementDictionary();
			}
			if (achievementLookupDictionary.TryGetValue(id, out var value))
			{
				value.Unlocked = AchievementData.Achievements[value.Id].Unlocked;
				return value;
			}
			return null;
		}

		private void LoadAchievementDictionary()
		{
			if (achievementLookupDictionary == null)
			{
				achievementLookupDictionary = new Dictionary<AchievementData.AchievementId, Achievement>();
			}
			else
			{
				achievementLookupDictionary.Clear();
			}
			foreach (Achievement achievement in achievementList)
			{
				if (!AchievementData.Achievements.ContainsKey(achievement.Id))
				{
					Debug.LogWarning($"Did not find achievement in Database {achievement.Id} in achievements in code");
					continue;
				}
				achievement.Unlocked = AchievementData.Achievements[achievement.Id].Unlocked;
				achievementLookupDictionary.TryAdd(achievement.Id, achievement);
			}
		}

		public (int, int) GetProgression()
		{
			int count = achievementList.Count;
			int num = 0;
			foreach (Achievement achievement in achievementList)
			{
				if (AchievementData.Achievements[achievement.Id].Unlocked)
				{
					num++;
				}
			}
			return (num, count);
		}
	}
}
