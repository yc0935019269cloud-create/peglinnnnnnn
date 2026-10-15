using System;
using System.Collections.Generic;
using Battle.Enemies;
using Challenges;
using Loading;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUI.Encirclepedia;
using Relics;
using Stats;
using ToolBox.Serialization;
using UnityEngine;

namespace Saving
{
	public class PersistentPlayerData
	{
		public delegate void PersistentPlayerDataEvent();

		private const string DAGGER_NAME = "dagger";

		private const string STONE_NAME = "stone";

		private const string EVASIVE_MANEUVORB_NAME = "evasive_maneuvorb";

		private const string SPLATORB_NAME = "splatorb";

		private const string SPHEAR_NAME = "sphear";

		public static readonly string SAVE_KEY = "PersistentPlayerSaveData";

		public static readonly int BALLADIN_PROGRESS_MAX = 2500;

		public static readonly int ROUNDREL_PROGRESS_MAX = 2500;

		public static readonly int SPINVENTOR_PROGRESS_MAX = 1250;

		public static PersistentPlayerDataEvent OnAchievementsLoaded;

		public Dictionary<Class, int> CruciballLevels = new Dictionary<Class, int>();

		public List<string> UnlockedOrbs = new List<string>();

		public List<string> QueuedOrbUnlocks = new List<string>();

		public List<RelicEffect> UnlockedRelics = new List<RelicEffect>();

		public List<RelicEffect> QueuedRelicUnlocks = new List<RelicEffect>();

		public List<ChallengeEffect> UnlockedChallenges = new List<ChallengeEffect>();

		public List<Class> QueuedClassUnlocks = new List<Class>();

		public bool HasPurchasedFullGameUnlock;

		public bool AchievementsLoaded;

		public bool CruciballActive;

		public int BalladinProgress;

		public int RoundrelProgress;

		public int SpinventorProgress;

		private EnemyData.BestiarySaveData _bestiarySaveData;

		private PersistentSaveData _cachedData;

		private PermanentStats.PermanentStatsSaveData _cachedStats;

		private Dictionary<Class, int> _selectedCruciballLevels = new Dictionary<Class, int>();

		private PermanentStats _permanentStats;

		private static PersistentPlayerData _instance;

		public PermanentStats PermanentStats => _permanentStats;

		public static PersistentPlayerData Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = InitFromSaveFile();
				}
				return _instance;
			}
		}

		private PersistentPlayerData(PersistentSaveData loadData)
		{
			if (loadData == null)
			{
				Debug.LogWarning("Error loading saved data, cannot init loaded data.");
				return;
			}
			Initialize(loadData);
			PermanentStats.PermanentStatsSaveData permanentStatsSaveData = (PermanentStats.PermanentStatsSaveData)DataSerializer.Load<SaveObjectData>(PermanentStats.PermanentStatsSaveData.KEY, DataSerializer.SaveType.BASE);
			if (permanentStatsSaveData != null)
			{
				_cachedStats = permanentStatsSaveData;
				_permanentStats = new PermanentStats(permanentStatsSaveData);
			}
			else
			{
				_permanentStats = new PermanentStats();
				_cachedStats = new PermanentStats.PermanentStatsSaveData(_permanentStats);
			}
		}

		private PersistentPlayerData()
		{
			CruciballLevels.Clear();
			AchievementData.InitializeAchievements();
			Class[] array = (Class[])Enum.GetValues(typeof(Class));
			for (int i = 0; i < array.Length; i++)
			{
				CruciballLevels.Add(array[i], -1);
			}
			Debug.Log("Setting up one time selected cruciball levels");
			_selectedCruciballLevels.Clear();
			for (int j = 0; j < Enum.GetValues(typeof(Class)).Length; j++)
			{
				Class key = (Class)j;
				_selectedCruciballLevels[key] = 1;
			}
			AchievementsLoaded = true;
			OnAchievementsLoaded?.Invoke();
			_permanentStats = new PermanentStats();
			_ = new EnemyData.EnemySaveData[AssetLoading.Instance.EnemyDataTable.Count];
			ClobberBestiaryDataWithMemoryValues();
			_cachedStats = new PermanentStats.PermanentStatsSaveData(_permanentStats);
			SavePersistentData(shouldSaveFile: false);
		}

		private void Initialize(PersistentSaveData loadData)
		{
			AchievementData.InitializeAchievements();
			CruciballLevels.Clear();
			LoadCruciball(loadData);
			UnlockedOrbs.Clear();
			if (loadData.UnlockedOrbs != null)
			{
				UnlockedOrbs.AddRange(loadData.UnlockedOrbs);
			}
			UnlockedRelics.Clear();
			BalladinProgress = loadData._balladinProgress;
			RoundrelProgress = loadData._roundrelProgress;
			SpinventorProgress = loadData._spinventorProgress;
			if (loadData.UnlockedRelics != null)
			{
				for (int i = 0; i < loadData.UnlockedRelics.Length; i++)
				{
					UnlockedRelics.Add((RelicEffect)loadData.UnlockedRelics[i]);
				}
			}
			if (loadData.QueuedRelicUnlocks != null)
			{
				for (int j = 0; j < loadData.QueuedRelicUnlocks.Length; j++)
				{
					QueuedRelicUnlocks.Add((RelicEffect)loadData.QueuedRelicUnlocks[j]);
				}
			}
			if (loadData.QueuedOrbUnlocks != null)
			{
				QueuedOrbUnlocks.AddRange(loadData.QueuedOrbUnlocks);
			}
			UnlockedChallenges.Clear();
			if (loadData.UnlockedChallenges != null)
			{
				for (int k = 0; k < loadData.UnlockedChallenges.Length; k++)
				{
					UnlockedChallenges.Add((ChallengeEffect)loadData.UnlockedChallenges[k]);
				}
			}
			HasPurchasedFullGameUnlock = loadData.HasPurchasedFullGameUnlock || IAPManager.FullGamePurchased;
			IAPManager.FullGamePurchased = IAPManager.FullGamePurchased || HasPurchasedFullGameUnlock;
			if (loadData.AchievementData == null)
			{
				Debug.Log("Setting up one time achievement data");
				loadData.AchievementData = GetAchievementData();
				loadData.Save();
				SaveManager.SaveBase();
				AchievementsLoaded = true;
				OnAchievementsLoaded?.Invoke();
			}
			else
			{
				VerifyAchievementData(loadData.AchievementData);
				SaveManager.SaveBase();
			}
			if (!UnlockedOrbs.Contains("stone"))
			{
				UnlockedOrbs.Add("stone");
			}
			if (!UnlockedOrbs.Contains("dagger"))
			{
				UnlockedOrbs.Add("dagger");
			}
			if (AchievementData.Achievements[AchievementData.AchievementId.SHIELD_AMOUNT_AT_END_OF_BATTLE].Unlocked || BalladinProgress >= BALLADIN_PROGRESS_MAX)
			{
				if (!UnlockedOrbs.Contains("sphear"))
				{
					UnlockedOrbs.Add("sphear");
				}
				if (!UnlockedRelics.Contains(RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK))
				{
					UnlockedRelics.Add(RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK);
				}
			}
			if (AchievementData.Achievements[AchievementData.AchievementId.POISON_AMOUNT_ON_ENEMY].Unlocked || RoundrelProgress >= ROUNDREL_PROGRESS_MAX)
			{
				if (!UnlockedOrbs.Contains("evasive_maneuvorb"))
				{
					UnlockedOrbs.Add("evasive_maneuvorb");
				}
				if (!UnlockedRelics.Contains(RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT))
				{
					UnlockedRelics.Add(RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT);
				}
			}
			if (AchievementData.Achievements[AchievementData.AchievementId.MULTIPLE_SLIME_BOUNCES].Unlocked || SpinventorProgress >= SPINVENTOR_PROGRESS_MAX)
			{
				if (!UnlockedOrbs.Contains("splatorb"))
				{
					UnlockedOrbs.Add("splatorb");
				}
				if (!UnlockedRelics.Contains(RelicEffect.UNIQUE_ORBS_BUFF))
				{
					UnlockedRelics.Add(RelicEffect.UNIQUE_ORBS_BUFF);
				}
			}
			if (!UnlockedRelics.Contains(RelicEffect.ADDITIONAL_PEGLIN_CHOICES))
			{
				UnlockedRelics.Add(RelicEffect.ADDITIONAL_PEGLIN_CHOICES);
			}
			if (!UnlockedRelics.Contains(RelicEffect.IMMORTAL))
			{
				UnlockedRelics.Add(RelicEffect.IMMORTAL);
			}
			if (loadData.SelectedCruciballLevels == null)
			{
				Debug.Log("Setting up one time selected cruciball levels");
				for (int l = 0; l < Enum.GetValues(typeof(Class)).Length; l++)
				{
					Class key = (Class)l;
					_selectedCruciballLevels[key] = Mathf.Min(CruciballLevels[key] + 1, 20);
				}
				loadData.SelectedCruciballLevels = GetSelectedCruciballLevelsSaveFormat();
			}
			else
			{
				for (int m = 0; m < loadData.SelectedCruciballLevels.Length; m++)
				{
					int b = Mathf.Min(CruciballLevels[(Class)m] + 1, 20);
					int value = Mathf.Min(loadData.SelectedCruciballLevels[m], b);
					_selectedCruciballLevels[(Class)m] = value;
				}
			}
			if (loadData.BestiarySaveData != null)
			{
				_bestiarySaveData = loadData.BestiarySaveData;
				if (AssetLoading.Instance.EnemyDataTable.Count == 0)
				{
					AssetLoading.Instance.LoadBestiary();
				}
				if (_bestiarySaveData.EnemyData.Length < AssetLoading.Instance.EnemyDataTable.Count)
				{
					List<EnemyData.EnemySaveData> list = new List<EnemyData.EnemySaveData>(_bestiarySaveData.EnemyData);
					foreach (EnemyData value3 in AssetLoading.Instance.EnemyDataTable.Values)
					{
						bool flag = false;
						foreach (EnemyData.EnemySaveData item in list)
						{
							if (item.LocKey == value3.LocKey)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							list.Add(new EnemyData.EnemySaveData(value3));
						}
					}
					_bestiarySaveData.EnemyData = list.ToArray();
				}
				EnemyData.EnemySaveData[] enemyData = _bestiarySaveData.EnemyData;
				foreach (EnemyData.EnemySaveData enemySaveData in enemyData)
				{
					if (AssetLoading.Instance.EnemyDataTable.TryGetValue(enemySaveData.TableKey, out var value2))
					{
						value2.ImportSaveData(enemySaveData);
					}
				}
			}
			else
			{
				ClobberBestiaryDataWithMemoryValues();
			}
			CheckCruciballAchievements();
			if (UnlockedRelics.Contains(RelicEffect.MAX_HEALTH_LARGE) && !AchievementData.Achievements[AchievementData.AchievementId.BRING_EGG_TO_PEGLIN_CHEF].Unlocked)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.BRING_EGG_TO_PEGLIN_CHEF);
			}
			_cachedData = loadData;
		}

		public void DeleteBestiaryHistory()
		{
			_bestiarySaveData = null;
		}

		public void DeleteSaveData()
		{
			SaveManager.RequestDelete();
			_cachedData = null;
			_cachedStats = null;
			_permanentStats = new PermanentStats();
			DataSerializer.DeleteKey(BestiaryManager.BestiaryHistorySaveData.KEY, DataSerializer.SaveType.BASE);
			DataSerializer.DeleteKey(EnemyData.BestiarySaveData.KEY, DataSerializer.SaveType.BASE);
			DeleteBestiaryHistory();
			UnlockedOrbs.Clear();
			UnlockedRelics.Clear();
			UnlockedChallenges.Clear();
			QueuedOrbUnlocks.Clear();
			QueuedRelicUnlocks.Clear();
			List<Class> list = new List<Class>();
			foreach (Class key in CruciballLevels.Keys)
			{
				list.Add(key);
			}
			foreach (Class item in list)
			{
				CruciballLevels[item] = -1;
			}
			DataSerializer.DeleteBackupFile();
			AssetLoading.Instance.DeleteRunStats();
			AssetLoading.Instance.LoadClearBestiary();
			SavePersistentData(shouldSaveFile: true);
		}

		private void LoadCruciball(PersistentSaveData loadData)
		{
			for (int i = 0; i < loadData.CruciballLevels.Length; i++)
			{
				CruciballLevels.Add((Class)i, loadData.CruciballLevels[i]);
			}
			int length = Enum.GetValues(typeof(Class)).Length;
			for (int j = loadData.CruciballLevels.Length; j < length; j++)
			{
				CruciballLevels.Add((Class)j, -1);
			}
		}

		public void ClobberBestiaryDataWithMemoryValues()
		{
			EnemyData.EnemySaveData[] array = new EnemyData.EnemySaveData[AssetLoading.Instance.EnemyDataTable.Count];
			int num = 0;
			foreach (EnemyData value2 in AssetLoading.Instance.EnemyDataTable.Values)
			{
				EnemyData.EnemySaveData enemySaveData = (array[num] = new EnemyData.EnemySaveData(value2));
				if (AssetLoading.Instance.EnemyDataTable.TryGetValue(enemySaveData.TableKey, out var value))
				{
					value.ImportSaveData(enemySaveData);
				}
				num++;
			}
			_bestiarySaveData = new EnemyData.BestiarySaveData(array);
		}

		public void CheckCruciballAchievements()
		{
			for (int i = 0; i < CruciballLevels.Count; i++)
			{
				int num = CruciballLevels[(Class)i];
				if (num >= 5)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.REACH_CRUCIBALL_5, updatePlayerData: false);
				}
				if (num >= 10)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.REACH_CRUCIBALL_10, updatePlayerData: false);
				}
				if (num >= 15)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.REACH_CRUCIBALL_15, updatePlayerData: false);
				}
				if (num >= 20)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.REACH_CRUCIBALL_20, updatePlayerData: false);
				}
			}
		}

		public void SetLastSelectedCruciball(Class cl, int level)
		{
			_selectedCruciballLevels[cl] = level;
		}

		public int GetSelectedCruciballLevel(Class cl)
		{
			return _selectedCruciballLevels[cl];
		}

		public void SaveBestiaryData()
		{
			EnemyData.EnemySaveData[] enemyData = _bestiarySaveData.EnemyData;
			foreach (EnemyData.EnemySaveData enemySaveData in enemyData)
			{
				if (AssetLoading.Instance.EnemyDataTable.TryGetValue(enemySaveData.TableKey, out var value))
				{
					enemySaveData.Update(value);
				}
			}
			_bestiarySaveData.Save();
		}

		private static PersistentPlayerData InitFromSaveFile()
		{
			try
			{
				DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.BASE);
				DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.RUN);
				if (!DataSerializer.HasKey(SAVE_KEY, DataSerializer.SaveType.BASE))
				{
					if (ValidateBaseSave((PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BASE)))
					{
						DataSerializer.RestoreBaseFile();
						return LoadFromBaseFile();
					}
					return new PersistentPlayerData();
				}
				PersistentSaveData persistentSaveData = (PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BASE);
				if (persistentSaveData == null)
				{
					if (ValidateBaseSave((PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BASE)))
					{
						DataSerializer.RestoreBaseFile();
						return LoadFromBaseFile();
					}
					return new PersistentPlayerData();
				}
				if (ValidateBaseSave((PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BASE)))
				{
					Debug.Log("Merging backup data with local data");
					DataSerializer.CompareAndMergeBaseFileWithBackup();
				}
				return new PersistentPlayerData(persistentSaveData);
			}
			catch (Exception ex)
			{
				Debug.LogError($"We found an error reading a save file: {ex} - Stack Trace: {ex.StackTrace}");
				SaveManager.SaveFileCorruptionDetector.Instance.CloudSavesDisabled = true;
				DataSerializer.RestoreBaseFile();
				return LoadFromBaseFile();
			}
		}

		private static PersistentPlayerData LoadFromBaseFile()
		{
			Debug.Log("PersistentPlayerData::LoadFromBaseFile()");
			DataSerializer.LoadFile(DataSerializer.SaveType.BASE);
			DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.BASE);
			DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.RUN);
			return new PersistentPlayerData((PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BASE));
		}

		public void SavePersistentData(bool shouldSaveFile, bool validateBackup = false)
		{
			if (shouldSaveFile)
			{
				Debug.Log("Saving file");
			}
			int[] array = new int[Enum.GetValues(typeof(Class)).Length];
			for (int i = 0; i < CruciballLevels.Values.Count; i++)
			{
				array[i] = CruciballLevels[(Class)i];
			}
			int[] unlockedRelics = RelicEffectsToInt(UnlockedRelics);
			int[] queuedRelicUnlocks = RelicEffectsToInt(QueuedRelicUnlocks);
			int[] unlockedChallenges = ChallengeEffectsToInt(UnlockedChallenges);
			bool hasPurchasedFullGameUnlock = IAPManager.FullGamePurchased || HasPurchasedFullGameUnlock;
			_cachedData = new PersistentSaveData(array, unlockedRelics, UnlockedOrbs.ToArray(), queuedRelicUnlocks, QueuedOrbUnlocks.ToArray(), unlockedChallenges, GetAchievementData(), hasPurchasedFullGameUnlock, GetSelectedCruciballLevelsSaveFormat(), _bestiarySaveData, BalladinProgress, RoundrelProgress, SpinventorProgress);
			if (validateBackup)
			{
				if (ValidateBaseSave(_cachedData))
				{
					Debug.Log("Found inconsistent data with the backup. Merging backup data with local data");
					DataSerializer.CompareAndMergeBaseFileWithCache(_cachedData);
					Initialize(_cachedData);
				}
				else
				{
					DataSerializer.BackupBaseFile();
				}
			}
			_cachedData.Save();
			_cachedStats = new PermanentStats.PermanentStatsSaveData(_permanentStats);
			_cachedStats.Save();
			if (shouldSaveFile)
			{
				SaveManager.SaveBase();
			}
		}

		public void UpdateAchievements()
		{
			if (_cachedData != null)
			{
				_cachedData.AchievementData = GetAchievementData();
				_cachedData.Save();
				SaveManager.SaveBase();
			}
		}

		private int[] RelicEffectsToInt(List<RelicEffect> effects)
		{
			int[] array = new int[effects.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (int)effects[i];
			}
			return array;
		}

		private int[] ChallengeEffectsToInt(List<ChallengeEffect> effects)
		{
			int[] array = new int[effects.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (int)effects[i];
			}
			return array;
		}

		private bool[] GetAchievementData()
		{
			bool[] array = new bool[Enum.GetNames(typeof(AchievementData.AchievementId)).Length];
			for (int i = 0; i < array.Length; i++)
			{
				AchievementData.AchievementId key = (AchievementData.AchievementId)i;
				array[i] = AchievementData.Achievements[key].Unlocked;
			}
			return array;
		}

		private void VerifyAchievementData(bool[] loadedAchievementData)
		{
			if (loadedAchievementData == null)
			{
				return;
			}
			try
			{
				for (int i = 0; i < loadedAchievementData.Length; i++)
				{
					AchievementData.AchievementId achievementId = (AchievementData.AchievementId)i;
					if (!AchievementData.Achievements.ContainsKey(achievementId))
					{
						Debug.LogWarning($"Did not find achievement {achievementId} in dictionary");
						continue;
					}
					bool unlocked = AchievementData.Achievements[achievementId].Unlocked;
					if (loadedAchievementData[i] && !unlocked)
					{
						AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[achievementId], updatePlayerData: false);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Exception found loading achievements: " + ex.Message + " " + ex.StackTrace);
				Debug.Log("Unlocking local achievements");
				for (int j = 0; j < loadedAchievementData.Length; j++)
				{
					AchievementData.AchievementId achievementId2 = (AchievementData.AchievementId)j;
					if (!AchievementData.Achievements.ContainsKey(achievementId2))
					{
						Debug.LogWarning($"Did not find achievement {achievementId2} in dictionary");
					}
					else
					{
						AchievementData.Achievements[achievementId2].Unlocked = loadedAchievementData[j];
					}
				}
			}
			AchievementsLoaded = true;
			OnAchievementsLoaded?.Invoke();
		}

		public void SetChallengeUnlocked(ChallengeEffect challenge)
		{
			if (!UnlockedChallenges.Contains(challenge))
			{
				UnlockedChallenges.Add(challenge);
			}
		}

		public void SetRelicUnlocked(RelicEffect relic)
		{
			if (!UnlockedRelics.Contains(relic))
			{
				UnlockedRelics.Add(relic);
				QueuedRelicUnlocks.Add(relic);
			}
		}

		public void SetOrbUnlocked(string orbName)
		{
			if (!UnlockedOrbs.Contains(orbName))
			{
				UnlockedOrbs.Add(orbName);
				QueuedOrbUnlocks.Add(orbName);
			}
		}

		public bool AnyCruciballUnlocked()
		{
			foreach (Class key in CruciballLevels.Keys)
			{
				if (AnyCruciballUnlockedForClass(key))
				{
					return true;
				}
			}
			return false;
		}

		public bool AnyCruciballUnlockedForClass(Class c)
		{
			return CruciballLevels[c] >= 0;
		}

		public bool IsCruciballLevelUnlockedForAnyClass(int level)
		{
			foreach (Class key in CruciballLevels.Keys)
			{
				if (CruciballLevels[key] >= level)
				{
					return true;
				}
			}
			return false;
		}

		public int[] GetSelectedCruciballLevelsSaveFormat()
		{
			int length = Enum.GetValues(typeof(Class)).Length;
			int[] array = new int[length];
			for (int i = 0; i < length; i++)
			{
				if (_selectedCruciballLevels.ContainsKey((Class)i))
				{
					array[i] = _selectedCruciballLevels[(Class)i];
				}
				else
				{
					array[i] = 0;
				}
			}
			return array;
		}

		public bool IsClassUnlockedByProgress(Class cl)
		{
			switch (cl)
			{
			case Class.Balladin:
				return BalladinProgress >= BALLADIN_PROGRESS_MAX;
			case Class.Roundrel:
				return BalladinProgress >= ROUNDREL_PROGRESS_MAX;
			case Class.Spinventor:
				return SpinventorProgress >= SPINVENTOR_PROGRESS_MAX;
			case Class.Peglin:
				return true;
			default:
				Debug.LogError($"Tried to query unlock condition for unknown class {cl}");
				return false;
			}
		}

		private static bool ValidateBaseSave(PersistentSaveData saveData)
		{
			DataSerializer.LoadFile(DataSerializer.SaveType.BACKUP);
			DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.BACKUP);
			if (DataSerializer.HasKey(SAVE_KEY, DataSerializer.SaveType.BACKUP))
			{
				PersistentSaveData persistentSaveData = (PersistentSaveData)DataSerializer.Load<SaveObjectData>(SAVE_KEY, DataSerializer.SaveType.BACKUP);
				bool flag = false;
				bool flag2 = false;
				if (persistentSaveData == null)
				{
					return false;
				}
				if (saveData == null)
				{
					return true;
				}
				if (persistentSaveData.CruciballLevels.Length < saveData.CruciballLevels.Length)
				{
					return false;
				}
				for (int i = 0; i < saveData.CruciballLevels.Length; i++)
				{
					int num = saveData.CruciballLevels[i];
					if (persistentSaveData.CruciballLevels[i] < num)
					{
						flag = true;
					}
					if (persistentSaveData.CruciballLevels[i] > num)
					{
						flag2 = true;
					}
				}
				if (flag & flag2)
				{
					Debug.LogError("We've found conflicting data in the backup and the base. We're not going to touch the backup OR the save file.");
					return false;
				}
				if (flag)
				{
					Debug.Log("Backing up the main save");
					DataSerializer.BackupBaseFile();
					return false;
				}
				if (flag2)
				{
					Debug.Log("We've found backup data with newer data.");
					return true;
				}
			}
			return false;
		}
	}
}
