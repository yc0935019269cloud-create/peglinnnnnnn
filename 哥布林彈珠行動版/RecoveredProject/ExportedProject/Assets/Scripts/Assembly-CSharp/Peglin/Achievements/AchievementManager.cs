using System.Collections.Generic;
using Saving;
using UnityEngine;

namespace Peglin.Achievements
{
	public class AchievementManager : MonoBehaviour
	{
		public delegate void AchievementUnlockedEvent(Achievement a);

		private IAchievementUnlocker _unlocker;

		public static bool AchievementsOn = true;

		public static AchievementUnlockedEvent OnAchievementUnlocked = (Achievement achievement) =>
		{
		};

		private static AchievementManager _instance;

		public IAchievementUnlocker Unlocker => _unlocker;

		public static AchievementManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("AchievementManager").AddComponent<AchievementManager>();
					_instance.InternalInitialize();
				}
				return _instance;
			}
		}

		private void Awake()
		{
			_instance = this;
			InternalInitialize();
		}

		public void Initialize()
		{
			if (_unlocker == null)
			{
				Debug.LogError("We failed initializing the AchievementsManager");
			}
		}

		public void UnlockAchievement(Achievement a, bool updatePlayerData = true)
		{
			if (AchievementsOn && !a.Unlocked)
			{
				_unlocker.UnlockAchievement(a);
				if (updatePlayerData)
				{
					OnAchievementUnlocked(a);
					PersistentPlayerData.Instance.UpdateAchievements();
				}
			}
		}

		public void UnlockAchievement(AchievementData.AchievementId id, bool updatePlayerData = true)
		{
			UnlockAchievement(AchievementData.Achievements[id], updatePlayerData);
		}

		public void InternalInitialize()
		{
			Object.DontDestroyOnLoad(this);
			_unlocker = base.gameObject.AddComponent<SteamStatsManager>();
			_unlocker.SetupAchievementData();
		}

		public void SetupAchievementData()
		{
			_unlocker.SetupAchievementData();
		}

		public int GetNumberOfAchievementsUnlocked()
		{
			int num = 0;
			foreach (KeyValuePair<AchievementData.AchievementId, Achievement> achievement in AchievementData.Achievements)
			{
				if (achievement.Value.Unlocked)
				{
					num++;
				}
			}
			return num;
		}
	}
}
