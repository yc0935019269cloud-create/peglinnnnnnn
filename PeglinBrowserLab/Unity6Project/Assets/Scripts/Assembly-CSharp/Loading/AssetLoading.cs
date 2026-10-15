using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Battle.Enemies;
using Peglin.OdinSerializer;
using Peglin.OdinSerializer.Utilities;
using Saving;
using Stats;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Loading
{
	public class AssetLoading : MonoBehaviour
	{
		public Dictionary<string, GameObject> OrbPrefabs = new Dictionary<string, GameObject>();

		public Dictionary<string, GameObject> EnemyPrefabs = new Dictionary<string, GameObject>();

		public Dictionary<string, PegboardParser.PegboardData> PegLayouts = new Dictionary<string, PegboardParser.PegboardData>();

		public Queue<RunStats.RunStatsSaveData> RunStats = new Queue<RunStats.RunStatsSaveData>();

		public Dictionary<string, EnemyData> EnemyDataTable = new Dictionary<string, EnemyData>();

		public TMP_Text categoryText;

		public TMP_Text progressText;

		public Slider slider;

		public RectTransform peglinAnimation;

		private int _counter;

		private int _allAssetsCounter;

		public static readonly float ORB_COUNT = 325f;

		public static readonly float ENEMY_COUNT = 134f;

		public static readonly float LAYOUT_COUNT = 134f;

		private Bestiary _bestiary;

		private static AssetLoading _instance;

		public Bestiary Bestiary
		{
			get
			{
				if (_bestiary == null)
				{
					LoadBestiary();
				}
				return _bestiary;
			}
		}

		public static AssetLoading Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("AssetLoading").AddComponent<AssetLoading>();
				}
				return _instance;
			}
		}

		private void OnEnable()
		{
			if (slider != null)
			{
				slider.enabled = false;
			}
			if (_instance == null)
			{
				_instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void LoadAllAssets(Action onLoadComplete)
		{
			LoadAssets(onLoadComplete);
			LoadRunStats();
		}

		public void AddRunStats(RunStats stats)
		{
			foreach (RunStats.RunStatsSaveData runStat in RunStats)
			{
				if (runStat.runId.Equals(stats.runId.ToString()))
				{
					return;
				}
			}
			if (RunStats.Count >= 20)
			{
				RunStats.Dequeue();
			}
			PersistentPlayerData.Instance.PermanentStats.AddStatsToPermanent(stats);
			RunStats.Enqueue(new RunStats.RunStatsSaveData(stats));
			SaveRunStats();
		}

		private void SaveRunStats()
		{
			Queue<RunStats.RunStatsSaveData> queue = new Queue<RunStats.RunStatsSaveData>(RunStats);
			RunStats.RunStatsSaveData[] array = new RunStats.RunStatsSaveData[queue.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = queue.Dequeue();
			}
			new RunStats.RunStatsHistory(array).Save();
			SaveManager.SaveStats();
		}

		public void LoadRunStats()
		{
			DataSerializer.LoadFile(DataSerializer.SaveType.STATS);
			RunStats.RunStatsHistory runStatsHistory = (RunStats.RunStatsHistory)DataSerializer.Load<SaveObjectData>(Stats.RunStats.RunStatsHistory.KEY, DataSerializer.SaveType.STATS);
			if (runStatsHistory == null)
			{
				return;
			}
			RunStats.RunStatsSaveData[] runsHistory = runStatsHistory.runsHistory;
			foreach (RunStats.RunStatsSaveData runStatsSaveData in runsHistory)
			{
				if (!runStatsSaveData.runId.IsNullOrWhitespace())
				{
					RunStats.Enqueue(runStatsSaveData);
				}
			}
		}

		public void DeleteRunStats()
		{
			DataSerializer.DeleteFile(DataSerializer.SaveType.STATS);
			RunStats.Clear();
			SaveRunStats();
		}

		public GameObject GetOrbPrefab(string prefabName)
		{
			if (Regex.Match(prefabName, "\\([(Clone)]+\\)").Success)
			{
				prefabName = Regex.Replace(prefabName, "\\([(Clone)]+\\)", "");
			}
			if (OrbPrefabs.TryGetValue(prefabName, out var value))
			{
				return value;
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Orbs/" + prefabName + ".prefab");
			if (!asyncOperationHandle.IsValid())
			{
				Debug.LogError(prefabName + " not found when loading deck!");
			}
			return asyncOperationHandle.WaitForCompletion();
		}

		private void LoadAssets(Action onLoadComplete)
		{
			LoadBestiary();
			slider.maxValue = ORB_COUNT + ENEMY_COUNT + LAYOUT_COUNT * 2f;
			_allAssetsCounter = 0;
			AsyncOperationHandle<IList<GameObject>> asyncOperationHandle = LoadAssetGroup("Orbs", OrbPrefabs, ORB_COUNT);
			asyncOperationHandle.Completed += (AsyncOperationHandle<IList<GameObject>> _) =>
			{
				UnityEngine.Object.Destroy(categoryText.transform.parent.gameObject);
				onLoadComplete();
			};
		}

		private AsyncOperationHandle<IList<GameObject>> LoadAssetGroup(string assetGroup, Dictionary<string, GameObject> prefabDict, float totalEntries)
		{
			categoryText.text = "Loading " + assetGroup;
			progressText.text = "";
			_counter = 0;
			return Addressables.LoadAssetsAsync(assetGroup, (GameObject go) =>
			{
				AddProgress(totalEntries);
				prefabDict.TryAdd(go.name, go);
			});
		}

		private IEnumerator LoadPegboards(string assetGroup, Dictionary<string, PegboardParser.PegboardData> pegboardDict, float totalEntries, Action onLoadComplete)
		{
			categoryText.text = "Loading " + assetGroup;
			progressText.text = "";
			_counter = 0;
			AsyncOperationHandle<IList<TextAsset>> operation = Addressables.LoadAssetsAsync(assetGroup, (TextAsset asset) =>
			{
				AddProgress(totalEntries);
			});
			yield return operation;
			IList<TextAsset> result = operation.Result;
			categoryText.text = "Deserializing Pegboards";
			progressText.text = "";
			_counter = 0;
			foreach (TextAsset item in result)
			{
				PegboardParser.PegboardData pegboardData = SerializationUtility.DeserializeValue<PegboardParser.PegboardData>(new MemoryStream(item.bytes), DataFormat.Binary);
				pegboardDict.Add(pegboardData.name, pegboardData);
				AddProgress(totalEntries);
			}
			onLoadComplete?.Invoke();
		}

		private void AddProgress(float totalEntries)
		{
			_counter++;
			_allAssetsCounter++;
			slider.value = _allAssetsCounter;
			progressText.text = $"{_counter.ToString()} / {totalEntries}";
		}

		public void LoadBestiary()
		{
			_bestiary = Addressables.LoadAssetAsync<Bestiary>("Bestiary").WaitForCompletion();
			EnemyDataTable.Clear();
			EnemyData[] enemyDataList = _bestiary.enemyDataList;
			foreach (EnemyData enemyData in enemyDataList)
			{
				if (enemyData.LocKey.IsNullOrWhitespace())
				{
					Debug.Log($"Did not find loc key in data file for {enemyData.enemyPrefabAssetReference}");
				}
				EnemyDataTable.Add(enemyData.LocKey + "_name", enemyData);
			}
		}

		public void LoadClearBestiary()
		{
			_bestiary = Addressables.LoadAssetAsync<Bestiary>("Bestiary").WaitForCompletion();
			EnemyDataTable.Clear();
			EnemyData[] enemyDataList = _bestiary.enemyDataList;
			foreach (EnemyData enemyData in enemyDataList)
			{
				if (enemyData.LocKey.IsNullOrWhitespace())
				{
					Debug.Log($"Did not find loc key in data file for {enemyData.enemyPrefabAssetReference}");
				}
				enemyData.Seen = false;
				enemyData.SeenInBestiary = false;
				enemyData.Killed = 0;
				enemyData.KilledBy = 0;
				enemyData.CardDrop = 0;
				enemyData.LootDrop = 0;
				EnemyDataTable.Add(enemyData.LocKey + "_name", enemyData);
			}
		}
	}
}
