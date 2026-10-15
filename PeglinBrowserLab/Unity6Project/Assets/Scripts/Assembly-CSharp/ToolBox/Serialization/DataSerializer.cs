using System;
using System.Collections.Generic;
using System.IO;
using Battle.Enemies;
using Loading;
using Map;
using Newtonsoft.Json;
using Peglin.ClassSystem;
using Peglin.OdinSerializer;
using Saving;
using Stats;
using ToolBox.Serializer.Platforms;
using UnityEngine;

namespace ToolBox.Serialization
{
	public static class DataSerializer
	{
		public enum SaveType
		{
			BASE = 0,
			RUN = 1,
			BACKUP = 2,
			OLD_BASE = 3,
			STATS = 4
		}

		private static Dictionary<string, ISerializable> _baseData;

		private static Dictionary<string, ISerializable> _runData;

		private static Dictionary<string, ISerializable> _backupData;

		private static Dictionary<string, ISerializable> _statsData;

		private static int _currentProfileIndex;

		private static IFileSystemInterface _saveInterface;

		private const string FILE_NAME = "Save";

		private const DataFormat DATA_FORMAT = DataFormat.Binary;

		private const int INITIAL_SIZE = 64;

		private static IFileSystemInterface saveInterface
		{
			get
			{
				if (_saveInterface == null)
				{
					_saveInterface = new StandardFileSystemInterface();
				}
				return _saveInterface;
			}
		}

		public static int CurrentProfileIndex => _currentProfileIndex;

		public static void Save<T>(string key, T dataToSave, SaveType saveType)
		{
			Dictionary<string, ISerializable> data = GetData(saveType);
			if (data.TryGetValue(key, out var value))
			{
				((Item<T>)value).Value = dataToSave;
				return;
			}
			Item<T> value2 = new Item<T>
			{
				Value = dataToSave
			};
			data.Add(key, value2);
		}

		public static T Load<T>(string key, SaveType saveType)
		{
			GetData(saveType).TryGetValue(key, out var value);
			Item<T> item = (Item<T>)value;
			if (item != null)
			{
				return item.Value;
			}
			return default;
		}

		public static T Load<T>(string key, Dictionary<string, ISerializable> saveFile)
		{
			saveFile.TryGetValue(key, out var value);
			Item<T> item = (Item<T>)value;
			if (item != null)
			{
				return item.Value;
			}
			return default;
		}

		public static Dictionary<string, ISerializable> GetData(SaveType type)
		{
			return type switch
			{
				SaveType.RUN => _runData, 
				SaveType.BACKUP => _backupData, 
				SaveType.STATS => _statsData, 
				_ => _baseData, 
			};
		}

		public static bool HasKey(string key, SaveType saveType)
		{
			Dictionary<string, ISerializable> data = GetData(saveType);
			if (!string.IsNullOrEmpty(key))
			{
				return data.ContainsKey(key);
			}
			return false;
		}

		public static void DeleteKey(string key, SaveType saveType)
		{
			Dictionary<string, ISerializable> data = GetData(saveType);
			if (HasKey(key, saveType))
			{
				data.Remove(key);
			}
		}

		public static void DeleteAll(SaveType saveType)
		{
			GetData(saveType).Clear();
		}

		public static T[] FetchAllData<T>(SaveType saveType) where T : ISerializable
		{
			List<ISerializable> list = new List<ISerializable>();
			Dictionary<string, ISerializable> data = GetData(saveType);
			foreach (string key in data.Keys)
			{
				if (data[key].GetType() == typeof(Item<T>))
				{
					list.Add(((Item<T>)data[key]).Value);
				}
			}
			T[] array = new T[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (T)list[i];
			}
			return array;
		}

		public static void ChangeProfile(int profileIndex)
		{
			if (CurrentProfileIndex != profileIndex)
			{
				SaveFile(SaveType.RUN);
				SaveFile(SaveType.BASE);
				_currentProfileIndex = profileIndex;
				LoadFile(SaveType.BASE);
				LoadFile(SaveType.RUN);
			}
		}

		public static void SaveFile(SaveType saveType)
		{
			string filePath = GetFilePath(CurrentProfileIndex, saveType);
			if (GetData(saveType) != null)
			{
				byte[] bytes = SerializationUtility.SerializeValue(GetData(saveType), DataFormat.Binary);
				saveInterface.SaveFile(filePath, bytes);
			}
		}

		public static void SaveFileReplace(byte[] bytes, SaveType saveType)
		{
			string filePath = GetFilePath(CurrentProfileIndex, saveType);
			saveInterface.SaveFile(filePath, bytes);
		}

		public static void SetData(SaveType saveType, Dictionary<string, ISerializable> data)
		{
			switch (saveType)
			{
			case SaveType.RUN:
				_runData = data;
				break;
			case SaveType.BACKUP:
				_backupData = data;
				break;
			case SaveType.BASE:
				_baseData = data;
				break;
			case SaveType.STATS:
				_statsData = data;
				break;
			default:
				Debug.LogError($"Tried to replace unsupported save type: {saveType}");
				break;
			}
		}

		public static string SerializeToJson(SaveType saveType)
		{
			return JsonConvert.SerializeObject(GetData(saveType), Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}

		public static Dictionary<string, ISerializable> DeserializeJson(string json)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, ISerializable>>(json, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}

		public static void LoadFile(SaveType saveType)
		{
			string filePath = GetFilePath(CurrentProfileIndex, saveType);
			byte[] bytes = Array.Empty<byte>();
			saveInterface.LoadFile(filePath, ref bytes);
			bool flag = true;
			try
			{
				Debug.Log($"DataSerializer: Loading file of type {saveType}");
				Debug.Log($"DataSerializer: Loading bytes of size {bytes.Length}");
				switch (saveType)
				{
				case SaveType.BASE:
					if (_baseData == null)
					{
						_baseData = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
					}
					if (_baseData == null)
					{
						_baseData = new Dictionary<string, ISerializable>(64);
					}
					break;
				case SaveType.RUN:
					_runData = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
					if (_runData == null)
					{
						_runData = new Dictionary<string, ISerializable>(64);
					}
					break;
				case SaveType.BACKUP:
					_backupData = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
					if (_backupData == null)
					{
						_backupData = new Dictionary<string, ISerializable>(64);
					}
					break;
				case SaveType.STATS:
					_statsData = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
					if (_statsData == null)
					{
						_statsData = new Dictionary<string, ISerializable>(64);
					}
					break;
				case SaveType.OLD_BASE:
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex.Message);
				switch (saveType)
				{
				case SaveType.BASE:
					Debug.LogWarning("Save Data corrupted - Restoring backup");
					try
					{
						string filePath4 = GetFilePath(_currentProfileIndex, SaveType.BACKUP);
						byte[] bytes2 = Array.Empty<byte>();
						saveInterface.LoadFile(filePath4, ref bytes2);
						SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes2, DataFormat.Binary);
						saveInterface.RestoreBackupSave(GetFilePath(_currentProfileIndex, SaveType.BASE));
					}
					catch (Exception ex2)
					{
						Debug.LogWarning(ex2.Message);
						Debug.LogWarning("Backup save also corrupted!");
						flag = false;
					}
					SaveManager.SaveFileCorruptionDetector.Instance.SaveFileHasBeenCorrupted = true;
					break;
				case SaveType.BACKUP:
					Debug.LogWarning("Backup Save corrupted");
					BackupBaseFile();
					saveInterface.DeleteFile(GetFilePath(CurrentProfileIndex, SaveType.BACKUP));
					flag = false;
					break;
				case SaveType.STATS:
				{
					Debug.Log("Stats data corrupted");
					string filePath3 = GetFilePath(CurrentProfileIndex, SaveType.STATS);
					saveInterface.DeleteFile(filePath3);
					break;
				}
				default:
				{
					Debug.Log("Run data corrupted");
					string filePath2 = GetFilePath(CurrentProfileIndex, SaveType.RUN);
					saveInterface.DeleteFile(filePath2);
					SaveManager.SaveFileCorruptionDetector.Instance.RunFileHasBeenCorrupted = true;
					break;
				}
				}
				if (flag)
				{
					LoadFile(saveType);
				}
			}
		}

		public static void AttemptLoadArbitraryData(byte[] data)
		{
			SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(data, DataFormat.Binary);
		}

		public static DateTime GetFileTimestamp(SaveType saveType)
		{
			string filePath = GetFilePath(CurrentProfileIndex, saveType);
			if (File.Exists(filePath))
			{
				return File.GetLastWriteTimeUtc(filePath);
			}
			return DateTime.MinValue;
		}

		public static bool FileMissingOrZeroLength(SaveType saveType)
		{
			string filePath = GetFilePath(CurrentProfileIndex, saveType);
			if (File.Exists(filePath))
			{
				return File.ReadAllBytes(filePath).Length != 0;
			}
			return true;
		}

		public static string GetFilePath(int profileIndex, SaveType saveType)
		{
			string persistentDataPath = Application.persistentDataPath;
			return saveType switch
			{
				SaveType.RUN => Path.Combine(persistentDataPath, string.Format("{0}_{1}r.data", "Save", profileIndex)), 
				SaveType.BACKUP => Path.Combine(persistentDataPath, string.Format("{0}_{1}.data.bak", "Save", profileIndex)), 
				SaveType.OLD_BASE => Path.Combine(persistentDataPath, string.Format("{0}_{1}.data.bak.base", "Save", profileIndex)), 
				SaveType.STATS => Path.Combine(persistentDataPath, $"Stats_{profileIndex}.data"), 
				_ => Path.Combine(persistentDataPath, string.Format("{0}_{1}.data", "Save", profileIndex)), 
			};
		}

		public static void DeleteFile(SaveType saveType)
		{
			string filePath = GetFilePath(_currentProfileIndex, saveType);
			saveInterface.DeleteFile(filePath);
		}

		public static void BackupBaseFile()
		{
			string filePath = GetFilePath(CurrentProfileIndex, SaveType.BASE);
			try
			{
				byte[] bytes = Array.Empty<byte>();
				saveInterface.LoadFile(filePath, ref bytes);
				SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
				saveInterface.CreateBackupSave(filePath);
			}
			catch (Exception ex)
			{
				Debug.LogError("The main save data has been corrupted. We are going to skip backing this data up. Exception message: " + ex.Message);
			}
		}

		public static void DeleteBackupFile()
		{
			string filePath = GetFilePath(_currentProfileIndex, SaveType.BASE);
			saveInterface.DeleteFile(filePath + ".bak");
		}

		public static void RestoreBaseFile()
		{
			string filePath = GetFilePath(CurrentProfileIndex, SaveType.BASE);
			string filePath2 = GetFilePath(CurrentProfileIndex, SaveType.BACKUP);
			string filePath3 = GetFilePath(CurrentProfileIndex, SaveType.OLD_BASE);
			if (File.Exists(filePath3))
			{
				File.Delete(filePath3);
			}
			File.Move(filePath, filePath3);
			File.Copy(filePath2, filePath);
		}

		public static string CompareTimestampsAndMerge(string json)
		{
			Debug.Log("Comparing and merging base files...");
			Dictionary<string, ISerializable> dictionary = DeserializeJson(json);
			if (dictionary != null)
			{
				CompareTimestampsAndMergeWithBase(dictionary);
			}
			return SerializeToJson(SaveType.BASE);
		}

		public static bool IsFileTheCorrectType(SaveType saveType, byte[] bytes)
		{
			Dictionary<string, ISerializable> dictionary = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(bytes, DataFormat.Binary);
			if (dictionary == null)
			{
				return false;
			}
			return saveType switch
			{
				SaveType.BASE => dictionary.ContainsKey(PersistentPlayerData.SAVE_KEY), 
				SaveType.RUN => dictionary.ContainsKey(MapControllerSaveData.MAP_CONTROLLER_SAVE_KEY), 
				SaveType.STATS => dictionary.ContainsKey(RunStats.RunStatsHistory.KEY), 
				_ => false, 
			};
		}

		public static bool CompareTimestampsAndMerge(byte[] remote)
		{
			Dictionary<string, ISerializable> dictionary = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(remote, DataFormat.Binary);
			if (dictionary != null)
			{
				return CompareTimestampsAndMergeWithBase(dictionary);
			}
			return false;
		}

		public static bool CompareTimestampsAndMergeWithBase(Dictionary<string, ISerializable> remoteDict)
		{
			PersistentSaveData persistentSaveData = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, remoteDict);
			if (_baseData == null)
			{
				LoadFile(SaveType.BASE);
				if (_baseData == null || _baseData.Count == 0)
				{
					_baseData = remoteDict;
				}
			}
			PersistentSaveData persistentSaveData2 = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, _baseData);
			if (persistentSaveData == null || persistentSaveData2 == null)
			{
				return false;
			}
			if (_baseData.TryGetValue(PersistentPlayerData.SAVE_KEY, out var value))
			{
				((Item<SaveObjectData>)value).Value = CompareAndMergeBaseFiles(persistentSaveData2, persistentSaveData);
			}
			return true;
		}

		public static byte[] CompareTimestampsAndMerge(byte[] original, byte[] unmerged)
		{
			Dictionary<string, ISerializable> dictionary = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(original, DataFormat.Binary);
			Dictionary<string, ISerializable> dictionary2 = SerializationUtility.DeserializeValue<Dictionary<string, ISerializable>>(unmerged, DataFormat.Binary);
			if (dictionary == null && dictionary2 == null)
			{
				return new byte[0];
			}
			if (dictionary == null)
			{
				return unmerged;
			}
			if (dictionary2 == null)
			{
				return original;
			}
			PersistentSaveData persistentSaveData = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, dictionary);
			PersistentSaveData persistentSaveData2 = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, dictionary2);
			if (persistentSaveData == null || persistentSaveData2 == null)
			{
				return original;
			}
			return SerializationUtility.SerializeValue(CompareAndMergeBaseFiles(persistentSaveData, persistentSaveData2), DataFormat.Binary);
		}

		public static void CompareAndMergeBaseFileWithBackup()
		{
			PersistentSaveData data = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, _baseData);
			PersistentSaveData data2 = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, _backupData);
			if (_baseData.TryGetValue(PersistentPlayerData.SAVE_KEY, out var value))
			{
				((Item<SaveObjectData>)value).Value = CompareAndMergeBaseFiles(data, data2);
			}
		}

		public static void CompareAndMergeBaseFileWithCache(PersistentSaveData cachedData)
		{
			PersistentSaveData data = (PersistentSaveData)Load<SaveObjectData>(PersistentPlayerData.SAVE_KEY, _backupData);
			if (_backupData.TryGetValue(PersistentPlayerData.SAVE_KEY, out var value))
			{
				((Item<SaveObjectData>)value).Value = CompareAndMergeBaseFiles(cachedData, data);
			}
		}

		public static PersistentSaveData CompareAndMergeBaseFiles(PersistentSaveData data1, PersistentSaveData data2)
		{
			for (int i = 0; i < data1.AchievementData.Length && i < data2.AchievementData.Length; i++)
			{
				if (data2.AchievementData[i] && !data1.AchievementData[i])
				{
					data1.AchievementData[i] = true;
				}
			}
			if (data2.CruciballLevels != null)
			{
				if (data1.CruciballLevels == null)
				{
					data1.CruciballLevels = data2.CruciballLevels;
				}
				else
				{
					for (int j = 0; j < data1.CruciballLevels.Length && j < data2.CruciballLevels.Length; j++)
					{
						if (data2.CruciballLevels[j] > data1.CruciballLevels[j])
						{
							data1.CruciballLevels[j] = data2.CruciballLevels[j];
						}
					}
				}
			}
			else if (data1.CruciballLevels == null)
			{
				int[] array = new int[Enum.GetValues(typeof(Class)).Length];
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = -1;
				}
				data1.CruciballLevels = array;
			}
			if (data1.BestiarySaveData == null && data2.BestiarySaveData != null)
			{
				data1.BestiarySaveData = data2.BestiarySaveData;
			}
			else if (data1.BestiarySaveData == null || data2.BestiarySaveData != null)
			{
				if (data1.BestiarySaveData == null && data2.BestiarySaveData == null)
				{
					EnemyData.EnemySaveData[] array2 = new EnemyData.EnemySaveData[AssetLoading.Instance.Bestiary.enemyDataList.Length];
					for (int l = 0; l < AssetLoading.Instance.Bestiary.enemyDataList.Length; l++)
					{
						EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[l];
						array2[l] = new EnemyData.EnemySaveData(enemyData.LocKey);
					}
					data1.BestiarySaveData = new EnemyData.BestiarySaveData(array2);
				}
				else if (data1.BestiarySaveData != null && data2.BestiarySaveData != null && data1.BestiarySaveData.EnemyData.Length != data2.BestiarySaveData.EnemyData.Length)
				{
					Dictionary<string, EnemyData.EnemySaveData> dictionary = new Dictionary<string, EnemyData.EnemySaveData>();
					Dictionary<string, EnemyData.EnemySaveData> dictionary2 = new Dictionary<string, EnemyData.EnemySaveData>();
					int num = 0;
					int num2 = 0;
					if (data1.BestiarySaveData != null && data1.BestiarySaveData.EnemyData != null)
					{
						num = data1.BestiarySaveData.EnemyData.Length;
						EnemyData.EnemySaveData[] enemyData2 = data1.BestiarySaveData.EnemyData;
						foreach (EnemyData.EnemySaveData enemySaveData in enemyData2)
						{
							dictionary.Add(enemySaveData.TableKey, enemySaveData);
						}
					}
					if (data2.BestiarySaveData != null && data2.BestiarySaveData.EnemyData != null)
					{
						num2 = data2.BestiarySaveData.EnemyData.Length;
						EnemyData.EnemySaveData[] enemyData2 = data2.BestiarySaveData.EnemyData;
						foreach (EnemyData.EnemySaveData enemySaveData2 in enemyData2)
						{
							dictionary2.Add(enemySaveData2.TableKey, enemySaveData2);
						}
					}
					Dictionary<string, EnemyData.EnemySaveData> dictionary3 = new Dictionary<string, EnemyData.EnemySaveData>();
					if (num > num2)
					{
						EnemyData.EnemySaveData[] enemyData2 = data1.BestiarySaveData.EnemyData;
						foreach (EnemyData.EnemySaveData enemySaveData3 in enemyData2)
						{
							dictionary3.Add(enemySaveData3.TableKey, enemySaveData3);
						}
					}
					else
					{
						EnemyData.EnemySaveData[] enemyData2 = data2.BestiarySaveData.EnemyData;
						foreach (EnemyData.EnemySaveData enemySaveData4 in enemyData2)
						{
							dictionary3.Add(enemySaveData4.TableKey, enemySaveData4);
						}
					}
					foreach (string key in dictionary3.Keys)
					{
						if (!dictionary.ContainsKey(key))
						{
							MergeEnemyData(dictionary3[key], dictionary2[key]);
						}
						else if (!dictionary2.ContainsKey(key))
						{
							MergeEnemyData(dictionary3[key], dictionary[key]);
						}
						else
						{
							MergeEnemyData(dictionary3[key], dictionary[key], dictionary2[key]);
						}
					}
				}
				else
				{
					for (int n = 0; n < data1.BestiarySaveData.EnemyData.Length; n++)
					{
						EnemyData.EnemySaveData enemyData3 = data1.BestiarySaveData.EnemyData[n];
						EnemyData.EnemySaveData enemyData4 = data2.BestiarySaveData.EnemyData[n];
						MergeEnemyData(enemyData3, enemyData4);
					}
				}
			}
			return data1;
		}

		private static void MergeEnemyData(EnemyData.EnemySaveData enemyData1, EnemyData.EnemySaveData enemyData2)
		{
			if (enemyData2.SeenInBestiary && !enemyData1.SeenInBestiary)
			{
				enemyData1.SeenInBestiary = true;
			}
			if (enemyData2.Seen && !enemyData1.Seen)
			{
				enemyData1.Seen = true;
			}
			if (enemyData2.KilledBy > enemyData1.KilledBy)
			{
				enemyData1.KilledBy = enemyData2.KilledBy;
			}
			if (enemyData2.ResearchProgress > enemyData1.ResearchProgress)
			{
				enemyData1.ResearchProgress = enemyData2.ResearchProgress;
			}
			if (enemyData2.CardDrop > enemyData1.CardDrop)
			{
				enemyData1.CardDrop = enemyData2.CardDrop;
			}
			if (enemyData2.Killed > enemyData1.Killed)
			{
				enemyData1.Killed = enemyData2.Killed;
			}
			if (enemyData2.LootDrop > enemyData1.LootDrop)
			{
				enemyData1.LootDrop = enemyData2.LootDrop;
			}
		}

		private static void MergeEnemyData(EnemyData.EnemySaveData target, EnemyData.EnemySaveData enemyData1, EnemyData.EnemySaveData enemyData2)
		{
			if (enemyData2.SeenInBestiary || enemyData1.SeenInBestiary)
			{
				target.SeenInBestiary = true;
			}
			if (enemyData2.Seen || enemyData1.Seen)
			{
				target.Seen = true;
			}
			target.KilledBy = ((enemyData2.KilledBy > enemyData1.KilledBy) ? enemyData2.KilledBy : enemyData1.KilledBy);
			target.ResearchProgress = ((enemyData2.ResearchProgress > enemyData1.ResearchProgress) ? enemyData2.ResearchProgress : enemyData1.ResearchProgress);
			target.CardDrop = ((enemyData2.CardDrop > enemyData1.CardDrop) ? enemyData2.CardDrop : enemyData1.CardDrop);
			target.Killed = ((enemyData2.Killed > enemyData1.Killed) ? enemyData2.Killed : enemyData1.Killed);
			target.LootDrop = ((enemyData2.LootDrop > enemyData1.LootDrop) ? enemyData2.LootDrop : enemyData1.LootDrop);
		}

		public static void Setup()
		{
			_currentProfileIndex = 0;
			LoadFile(SaveType.BASE);
			LoadFile(SaveType.RUN);
			LoadFile(SaveType.BACKUP);
			LoadFile(SaveType.STATS);
		}
	}
}
