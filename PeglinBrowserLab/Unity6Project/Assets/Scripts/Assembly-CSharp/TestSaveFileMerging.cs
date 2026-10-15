using System;
using Battle.Enemies;
using Peglin.Achievements;
using Peglin.ClassSystem;
using Saving;
using ToolBox.Serialization;
using UnityEngine;

public class TestSaveFileMerging : MonoBehaviour
{
	public Bestiary bestiary;

	private void OnEnable()
	{
		PersistentSaveData persistentSaveData = CreateRandomizedSaveFile(0);
		PersistentSaveData persistentSaveData2 = CreateRandomizedSaveFile(2);
		PersistentSaveData persistentSaveData3 = CreateRandomizedSaveFile(0);
		PersistentSaveData persistentSaveData4 = CreateRandomizedSaveFile(0);
		PersistentSaveData merged = DataSerializer.CompareAndMergeBaseFiles(persistentSaveData, persistentSaveData2);
		CompareAchievements(persistentSaveData, merged);
		CompareAchievements(persistentSaveData2, merged);
		CompareCruciballLevels(persistentSaveData, merged);
		CompareCruciballLevels(persistentSaveData2, merged);
		CompareBestiaryData(persistentSaveData, merged);
		CompareBestiaryData(persistentSaveData2, merged);
		persistentSaveData4.CruciballLevels = null;
		PersistentSaveData merged2 = DataSerializer.CompareAndMergeBaseFiles(persistentSaveData3, persistentSaveData4);
		CompareAchievements(persistentSaveData3, merged2);
		CompareAchievements(persistentSaveData4, merged2);
		CompareCruciballLevels(persistentSaveData3, merged2);
		CompareCruciballLevels(persistentSaveData4, merged2);
		CompareBestiaryData(persistentSaveData3, merged2);
		CompareBestiaryData(persistentSaveData4, merged2);
	}

	private PersistentSaveData CreateRandomizedSaveFile(int minusBestiary)
	{
		int[] array = new int[Enum.GetNames(typeof(Class)).Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = UnityEngine.Random.Range(0, 20);
		}
		bool[] array2 = new bool[Enum.GetNames(typeof(AchievementData.AchievementId)).Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = UnityEngine.Random.Range(0, 1) == 0;
		}
		int[] unlockedRelics = new int[0];
		string[] unlockedOrbs = new string[0];
		int[] unlockedChallenges = new int[0];
		string[] queuedOrbUnlocks = new string[0];
		int[] queuedRelicUnlocks = new int[0];
		int[] selectedCruciballLevels = new int[0];
		bool hasPurchasedFullGameUnlock = UnityEngine.Random.Range(0, 1) == 0;
		if (minusBestiary >= 0)
		{
			EnemyData.EnemySaveData[] array3 = new EnemyData.EnemySaveData[bestiary.enemyDataList.Length - minusBestiary];
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k] = GenerateEnemySaveData(bestiary.enemyDataList[k]);
			}
			EnemyData.BestiarySaveData bestiarySaveData = new EnemyData.BestiarySaveData(array3);
			return new PersistentSaveData(array, unlockedRelics, unlockedOrbs, queuedRelicUnlocks, queuedOrbUnlocks, unlockedChallenges, array2, hasPurchasedFullGameUnlock, selectedCruciballLevels, bestiarySaveData, 0, 0, 0);
		}
		return new PersistentSaveData(array, unlockedRelics, unlockedOrbs, queuedRelicUnlocks, queuedOrbUnlocks, unlockedChallenges, array2, hasPurchasedFullGameUnlock, selectedCruciballLevels, null, 0, 0, 0);
	}

	private EnemyData.EnemySaveData GenerateEnemySaveData(EnemyData enemyData)
	{
		EnemyData.EnemySaveData enemySaveData = new EnemyData.EnemySaveData();
		enemySaveData.LocKey = enemyData.LocKey;
		enemySaveData.Seen = UnityEngine.Random.Range(0, 1) == 0;
		enemySaveData.Killed = UnityEngine.Random.Range(0, 1000);
		enemySaveData.CardDrop = UnityEngine.Random.Range(0, 1000);
		enemySaveData.LootDrop = UnityEngine.Random.Range(0, 1000);
		enemySaveData.ResearchProgress = Mathf.Clamp(enemySaveData.Killed * 10 + enemySaveData.LootDrop * 10 + enemySaveData.CardDrop * 100, 0, 1000);
		return enemySaveData;
	}

	private void CompareAchievements(PersistentSaveData source, PersistentSaveData merged)
	{
		bool flag = false;
		if (source.AchievementData.Length != merged.AchievementData.Length)
		{
			Debug.LogError("Different sized achievement data");
			return;
		}
		for (int i = 0; i < source.AchievementData.Length; i++)
		{
			if (merged.AchievementData[i] && !source.AchievementData[i])
			{
				Debug.LogError($"Source achievement {i} missing in merged dataset");
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.Log("Achievement test finished successfully");
		}
	}

	private void CompareCruciballLevels(PersistentSaveData source, PersistentSaveData merged)
	{
		bool flag = false;
		if (source.CruciballLevels.Length != merged.CruciballLevels.Length)
		{
			Debug.LogError("Different sized max cruciball levels");
		}
		for (int i = 0; i < source.CruciballLevels.Length; i++)
		{
			if (merged.CruciballLevels[i] < source.CruciballLevels[i])
			{
				Debug.LogError($"Source cruciball level {i} higher than merged");
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.Log("Cruciball test finished successfully");
		}
	}

	private void CompareBestiaryData(PersistentSaveData source, PersistentSaveData merged)
	{
		bool flag = false;
		if (source.BestiarySaveData.EnemyData.Length != merged.BestiarySaveData.EnemyData.Length)
		{
			if (merged.BestiarySaveData.EnemyData.Length < source.BestiarySaveData.EnemyData.Length)
			{
				Debug.LogError("Loss of bestiary entries in merged file");
			}
			return;
		}
		for (int i = 0; i < source.BestiarySaveData.EnemyData.Length; i++)
		{
			EnemyData.EnemySaveData enemySaveData = source.BestiarySaveData.EnemyData[i];
			EnemyData.EnemySaveData enemySaveData2 = merged.BestiarySaveData.EnemyData[i];
			if (enemySaveData2.LocKey != enemySaveData.LocKey)
			{
				Debug.LogError("Mismatched enemy data " + enemySaveData2.LocKey);
				flag = true;
			}
			if (enemySaveData.SeenInBestiary && !enemySaveData2.SeenInBestiary)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " not marked as seen in bestiary in merged data");
				flag = true;
			}
			if (enemySaveData.Seen && !enemySaveData2.Seen)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " not marked as seen in merged data");
				flag = true;
			}
			if (enemySaveData.CardDrop > enemySaveData2.CardDrop)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " has more card drops than merged data");
				flag = true;
			}
			if (enemySaveData.Killed > enemySaveData2.Killed)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " has more kills than merged data");
				flag = true;
			}
			if (enemySaveData.LootDrop > enemySaveData2.LootDrop)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " has more loot drops than merged data");
				flag = true;
			}
			if (enemySaveData.ResearchProgress > enemySaveData2.ResearchProgress)
			{
				Debug.LogError("Source enemy " + enemySaveData.LocKey + " has more research progress than merged data");
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.Log("Bestiary test finished successfully");
		}
	}
}
