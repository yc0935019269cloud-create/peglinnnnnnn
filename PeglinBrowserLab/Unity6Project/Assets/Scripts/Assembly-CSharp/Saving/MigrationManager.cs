using System;
using System.Collections.Generic;
using Stats;
using ToolBox.Serialization;
using UnityEngine;

namespace Saving
{
	public static class MigrationManager
	{
		public static Dictionary<Version, Action> versionActionMap = new Dictionary<Version, Action>
		{
			{
				new Version(1, 3),
				ForceTwoTimesSpeed
			},
			{
				new Version(1, 2),
				CleanupRunStatistics
			},
			{
				new Version(1, 0),
				DeleteRunDataFromBaseFile
			}
		};

		public static List<Version> orderedVersionList = new List<Version>
		{
			new Version(1, 0),
			new Version(1, 2),
			new Version(1, 3)
		};

		public static void CheckAndRunMigrations(Version from)
		{
			List<Version> list = new List<Version>();
			for (int i = 0; i < orderedVersionList.Count; i++)
			{
				if (from.CompareTo(orderedVersionList[i]) <= 0)
				{
					list.Add(orderedVersionList[i]);
				}
			}
			foreach (Version item in list)
			{
				Debug.Log($"Found migration {versionActionMap[item].Method.Name} for version {item}");
				versionActionMap[item]();
			}
		}

		private static void DeleteRunDataFromBaseFile()
		{
			Debug.Log("Fetching run data in base file for deletion...");
			SaveObjectData[] array = DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.BASE);
			int num = 0;
			SaveObjectData[] array2 = array;
			foreach (SaveObjectData saveObjectData in array2)
			{
				if (saveObjectData != null && saveObjectData.DeleteOnLose)
				{
					num++;
					saveObjectData.Delete();
				}
			}
			Debug.Log($"Deleted {num} objects from the base save file!");
		}

		private static void CleanupRunStatistics()
		{
			SaveObjectData saveObjectData = DataSerializer.Load<SaveObjectData>(PermanentStats.PermanentStatsSaveData.KEY, DataSerializer.SaveType.BASE);
			if (saveObjectData != null)
			{
				saveObjectData.Delete();
				Debug.Log("Deleted existing permanent stats");
			}
			PersistentPlayerData.Instance.PermanentStats.Clear();
			PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
		}

		private static void ForceTwoTimesSpeed()
		{
			PlayerPrefs.SetFloat("SpeedupLevel", 2f);
			SettingsManager.Instance.SpeedupLevel = 2f;
		}
	}
}
