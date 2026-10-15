using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Attacks;
using Battle.Enemies;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using Challenges;
using Data;
using Loading;
using Map;
using PeglinUtils;
using Relics;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyManager : MonoBehaviour
{
	public delegate void EnemyMoved(int fromSlot, int toSlot, Enemy enemy);

	public delegate void EnemySpanwed(Enemy enemy);

	public enum SlotType
	{
		GROUND = 1,
		FLYING = 2,
		STATIONARY = 4
	}

	public class EnemySlot
	{
		public Vector3 Position = Vector3.zero;

		public Enemy Enemy;

		public int Index;

		public EnemySlot(Vector3 position, Enemy enemy, int index)
		{
			Position = position;
			Enemy = enemy;
			Index = index;
		}
	}

	[Serializable]
	public class ActExtraSpawn
	{
		public WaveGroup.WaveType slotType;

		public AssetReferenceGameObject enemyAssetReference;

		[Tooltip("How many will be added to the list of enemies. Which is picked from uniformly.")]
		public int rartiy = 1;

		public int difficulty = 1;
	}

	public class c19EnemyAndDifficulty
	{
		public GameObject enemyObject;

		public int difficulty = 1;

		public c19EnemyAndDifficulty(GameObject _enemyObject, int _difficulty)
		{
			enemyObject = _enemyObject;
			difficulty = _difficulty;
		}
	}

	public static EnemyMoved OnEnemyMoved;

	public static EnemySpanwed OnEnemySpawned = (Enemy enemy) =>
	{
	};

	public static readonly int[] MAX_EXTRA_ENEMIES_ON_C19_PER_ACT = new int[4] { 1, 2, 3, 4 };

	public static readonly int[] MAX_C19_TOTAL_SPAWN_DIFFIULTY_PER_ACT = new int[4] { 2, 3, 4, 5 };

	public static readonly int EASY_BATTLE_MAX_EXTRA_ENEMIES_ON_C19 = 1;

	public static readonly int EASY_BATTLE_MAX_DIFFICULTY_ON_C19 = 1;

	public List<Enemy> Enemies = new List<Enemy>();

	[SerializeField]
	private GameObject _rainbowSlimePrefab;

	[SerializeField]
	private GameObject _painbowSlimePrefab;

	[SerializeField]
	private bool _forceRainbowSpawnInEditor;

	[SerializeField]
	private int _debugNumberOfSlots = 7;

	[SerializeField]
	private float _groundEnemiesHeight;

	[SerializeField]
	private float _flyingEnemiesHeight = 1f;

	[SerializeField]
	private float _meleeRangeOffset = 1f;

	private float _stationaryOffset = 1.3f;

	private EnemySlot[] _groundSlots;

	private EnemySlot[] _flyingSlots;

	private EnemySlot[] _stationarySlots;

	private List<EnemySlot[]> _allSlots = new List<EnemySlot[]>();

	private List<SpecialMovementEnemy> _specialMovementEnemies = new List<SpecialMovementEnemy>();

	private Transform _playerTransform;

	private Vector3 _playerTransformOffset;

	public GameObject cardDropAnimation;

	private Vector2 _spawnPosition;

	private int _ongoingVictoryBlockingOperations;

	private List<c19EnemyAndDifficulty> _extraSpawnEnemyDifficultiesForThisAct = new List<c19EnemyAndDifficulty>();

	public List<ActExtraSpawn> c19ForestDefaultExtraSpawnEnemies;

	public List<ActExtraSpawn> c19CastleDefaultExtraSpawnEnemies;

	public List<ActExtraSpawn> c19MinesDefaultExtraSpawnEnemies;

	public List<ActExtraSpawn> c19CoreDefaultExtraSpawnEnemies;

	public List<GameObject> c19SpawnBanList;

	public GameObject spinfectionPegCreationEffect;

	private int _statusEffectIntensityAppliedToEnemiesThisBattle;

	public int FlyingEnemyCount { get; protected set; }

	public int GroundEnemyCount { get; protected set; }

	public bool EnemiesActing { get; protected set; }

	public float groundEnemiesHeight => _groundEnemiesHeight;

	public float flyingEnemiesHeight => _flyingEnemiesHeight;

	public Vector2 playerPosition => _playerTransform.position + _playerTransformOffset;

	public int ongoingVictoryBlockingOperations => _ongoingVictoryBlockingOperations;

	public int statusEffectIntensityAppliedToEnemiesThisBattle => _statusEffectIntensityAppliedToEnemiesThisBattle;

	public void Initialize(Transform playerTransform, Vector3 playerTransformOffset, Vector2 spawnPosition, int numberOfSlots, List<StarterSpawn> starterSpawns, StatusEffectData statusEffectData, RelicManager relicManager, float seededRainbowSlimeRoll, float[] seededExtraEnemyRolls, WaveGroup[] waveGroups, C19BattleData c19BattleData, bool canSpawnRainbowSlime = true, bool canSpawnCruciballEnemies = true, bool canSpawnPainbowSlime = true, bool skipBattleAndGoToCore = false)
	{
		int num = ((numberOfSlots > 0) ? numberOfSlots : _debugNumberOfSlots);
		_groundSlots = new EnemySlot[num];
		_flyingSlots = new EnemySlot[num];
		_stationarySlots = new EnemySlot[num];
		_allSlots.Add(_groundSlots);
		_allSlots.Add(_flyingSlots);
		_allSlots.Add(_stationarySlots);
		_playerTransform = playerTransform;
		_playerTransformOffset = playerTransformOffset;
		_spawnPosition = spawnPosition;
		if (canSpawnCruciballEnemies)
		{
			InitC19ExtraEnemieDificultiesList();
		}
		GenerateSlots(playerPosition, _spawnPosition, num);
		if (!skipBattleAndGoToCore)
		{
			AddStarterEnemies(starterSpawns, statusEffectData, relicManager, seededRainbowSlimeRoll, seededExtraEnemyRolls, waveGroups, c19BattleData, canSpawnRainbowSlime, canSpawnCruciballEnemies, canSpawnPainbowSlime);
		}
	}

	private void AddStarterEnemies(List<StarterSpawn> starterSpawns, StatusEffectData statusEffectData, RelicManager relicManager, float seededRainbowSlimeRoll, float[] seededExtraEnemyRolls, WaveGroup[] waveGroups, C19BattleData c19BattleData, bool canSpawnRainbowSlime = true, bool canSpawnCruciballEnemies = true, bool canSpawnPainbowSlime = true)
	{
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		List<GameObject> list3 = new List<GameObject>();
		int num = 0;
		foreach (StarterSpawn starterSpawn in starterSpawns)
		{
			bool flag = false;
			Vector3 position;
			switch (starterSpawn.spawnerType)
			{
			case WaveGroup.WaveType.STATIONARY:
				if (starterSpawn.slot >= _stationarySlots.Length)
				{
					Debug.LogWarning("Starter enemy was setup to spawn outside of slot index bounds, skipping...");
					continue;
				}
				position = GetSpawnPointForStationarySlot(starterSpawn.slot);
				flag = true;
				break;
			case WaveGroup.WaveType.FLYING:
				if (starterSpawn.slot >= _flyingSlots.Length)
				{
					Debug.LogWarning("Starter enemy was setup to spawn outside of slot index bounds, skipping...");
					continue;
				}
				position = GetSpawnPointForFlyingSlot(starterSpawn.slot);
				break;
			case WaveGroup.WaveType.GROUND:
				if (starterSpawn.slot >= _groundSlots.Length)
				{
					Debug.LogWarning("Starter enemy was setup to spawn outside of slot index bounds, skipping...");
					continue;
				}
				position = GetSpawnPointForGroundSlot(starterSpawn.slot);
				break;
			default:
				position = GetSpawnPointForGroundSlot(starterSpawn.slot);
				break;
			}
			if (starterSpawn.spawnData == null || starterSpawn?.spawnData?.enemyAssetReference == null)
			{
				Debug.LogError("Incorrectly formatted starter spawns causing issues. Verify this battle's spawn config. Spawn data: " + starterSpawn);
				return;
			}
			AssetReference assetReference;
			if (starterSpawn.spawnData.randomizedAssets != null && starterSpawn.spawnData.randomizedAssets.Length != 0)
			{
				int num2 = UnityEngine.Random.Range(0, starterSpawn.spawnData.randomizedAssets.Length);
				assetReference = starterSpawn.spawnData.randomizedAssets[num2];
			}
			else
			{
				assetReference = starterSpawn.spawnData.enemyAssetReference;
			}
			string enemyName;
			Enemy enemy;
			if (AssetLoading.Instance.EnemyPrefabs.TryGetValue(assetReference.RuntimeKey.ToString(), out var value))
			{
				enemyName = value.name;
				enemy = UnityEngine.Object.Instantiate(value, position, Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
			}
			else
			{
#if UNITY_WEBGL && !UNITY_EDITOR
				// Web port: an instantiate operation can never complete synchronously in the browser.
				// The prefab itself is preloaded (WebAddressablesBoot), so load it and instantiate directly.
				value = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<GameObject>(assetReference.RuntimeKey).WaitForCompletion(), position, Quaternion.identity, base.transform);
#else
				value = assetReference.InstantiateAsync(position, Quaternion.identity, base.transform).WaitForCompletion();
#endif
				enemyName = value.name;
				enemy = value.GetComponent<Enemy>();
			}
			if (enemy == null)
			{
				Debug.LogError("Incorrectly formatted starter spawns causing issues. Verify this battle's spawn config." + starterSpawn);
				return;
			}
			if (enemy.HealthBarObj == null)
			{
				enemy.currentBossIconIndex = num++;
			}
			enemy.Initialize(statusEffectData, this, relicManager, enemyName);
			if (flag)
			{
				AddStationaryEnemy(enemy, starterSpawn.slot);
				list.Add(value);
				continue;
			}
			AddEnemy(enemy, starterSpawn.slot, starterSpawn.spawnerType == WaveGroup.WaveType.FLYING);
			if (enemy.IsFlying)
			{
				list3.Add(value);
			}
			else if (!enemy.IsFlying)
			{
				list2.Add(value);
			}
		}
		if (canSpawnRainbowSlime && (seededRainbowSlimeRoll <= 0.02f || (Application.isEditor && _forceRainbowSpawnInEditor)))
		{
			int num3 = 1;
			if (!Application.isEditor)
			{
				num3 = MapController.instance.Act switch
				{
					2 => 3, 
					3 => 5, 
					4 => 6, 
					_ => 1, 
				};
			}
			for (int i = num3; i < _groundSlots.Length; i++)
			{
				if (_groundSlots[i].Enemy == null)
				{
					Enemy enemy2 = UnityEngine.Object.Instantiate(_rainbowSlimePrefab, GetSpawnPointForGroundSlot(i), Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
					if (enemy2 != null)
					{
						enemy2.Initialize(statusEffectData, this, relicManager, _rainbowSlimePrefab.name);
						AddEnemy(enemy2, i);
						break;
					}
				}
			}
		}
		if (canSpawnPainbowSlime && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.TASTE_THE_PAINBOW) && (ChallengeManager.Instance.currentPainbowSlimeHealth > 0 || ChallengeManager.Instance.currentPainbowSlimeHealth == -1))
		{
			int value2 = 1;
			if (!Application.isEditor)
			{
				value2 = MapController.instance.Act switch
				{
					1 => 2, 
					2 => 1, 
					3 => 0, 
					4 => 0, 
					_ => 0, 
				};
			}
			value2 = Mathf.Clamp(value2, 0, _groundSlots.Length - 1);
			int num4 = 0;
			int num5 = value2;
			while (num4 < _groundSlots.Length)
			{
				num4++;
				if (_groundSlots[num5].Enemy == null)
				{
					Enemy enemy3 = UnityEngine.Object.Instantiate(_painbowSlimePrefab, GetSpawnPointForGroundSlot(num5), Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
					if (enemy3 != null)
					{
						enemy3.Initialize(statusEffectData, this, relicManager, _painbowSlimePrefab.name);
						AddEnemy(enemy3, num5);
						break;
					}
				}
				num5 = (num5 + 1) % _groundSlots.Length;
			}
		}
		if (!canSpawnCruciballEnemies)
		{
			return;
		}
		int maxExtraEnemies = 1;
		int maxDifficulty = 1;
		if (MapController.instance != null)
		{
			maxExtraEnemies = MAX_EXTRA_ENEMIES_ON_C19_PER_ACT[MapController.instance.Act - 1];
			if (c19BattleData != null && c19BattleData.maxExtraEnemies != -1)
			{
				maxExtraEnemies = c19BattleData.maxExtraEnemies;
			}
			if (StaticGameData.currentNode.MapDataType == MapDataType.EASY_BATTLE)
			{
				maxExtraEnemies = EASY_BATTLE_MAX_EXTRA_ENEMIES_ON_C19;
			}
			maxDifficulty = MAX_C19_TOTAL_SPAWN_DIFFIULTY_PER_ACT[MapController.instance.Act - 1];
			if (c19BattleData != null && c19BattleData.maxDifficulty != -1)
			{
				maxDifficulty = c19BattleData.maxDifficulty;
			}
			if (StaticGameData.currentNode.MapDataType == MapDataType.EASY_BATTLE)
			{
				maxDifficulty = EASY_BATTLE_MAX_DIFFICULTY_ON_C19;
			}
		}
		List<c19EnemyAndDifficulty> list4 = new List<c19EnemyAndDifficulty>();
		foreach (GameObject item in list2)
		{
			list4.Add(new c19EnemyAndDifficulty(item, GetDifficultyForC19EnemySpawn(item)));
		}
		List<c19EnemyAndDifficulty> list5 = new List<c19EnemyAndDifficulty>();
		foreach (GameObject item2 in list3)
		{
			list5.Add(new c19EnemyAndDifficulty(item2, GetDifficultyForC19EnemySpawn(item2)));
		}
		List<c19EnemyAndDifficulty> list6 = new List<c19EnemyAndDifficulty>();
		foreach (GameObject item3 in list)
		{
			list6.Add(new c19EnemyAndDifficulty(item3, GetDifficultyForC19EnemySpawn(item3)));
		}
		foreach (WaveGroup waveGroup in waveGroups)
		{
			switch (waveGroup.waveType)
			{
			case WaveGroup.WaveType.GROUND:
			{
				WaveData[] waveData = waveGroup.waveData;
				foreach (WaveData waveData3 in waveData)
				{
					GameObject enemyObjectFromAsset2 = GetEnemyObjectFromAsset(waveData3.spawnData.enemyAssetReference);
					list4.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset2, GetDifficultyForC19EnemySpawn(enemyObjectFromAsset2)));
				}
				break;
			}
			case WaveGroup.WaveType.FLYING:
			{
				WaveData[] waveData = waveGroup.waveData;
				foreach (WaveData waveData4 in waveData)
				{
					GameObject enemyObjectFromAsset3 = GetEnemyObjectFromAsset(waveData4.spawnData.enemyAssetReference);
					list5.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset3, GetDifficultyForC19EnemySpawn(enemyObjectFromAsset3)));
				}
				break;
			}
			case WaveGroup.WaveType.STATIONARY:
			{
				WaveData[] waveData = waveGroup.waveData;
				foreach (WaveData waveData2 in waveData)
				{
					GameObject enemyObjectFromAsset = GetEnemyObjectFromAsset(waveData2.spawnData.enemyAssetReference);
					list6.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset, GetDifficultyForC19EnemySpawn(enemyObjectFromAsset)));
				}
				break;
			}
			}
		}
		List<ActExtraSpawn> list7 = new List<ActExtraSpawn>();
		if (MapController.instance != null && c19BattleData.canAddEnemiesFromOutsideBattle)
		{
			list7 = MapController.instance.Act switch
			{
				1 => c19ForestDefaultExtraSpawnEnemies, 
				2 => c19CastleDefaultExtraSpawnEnemies, 
				3 => c19MinesDefaultExtraSpawnEnemies, 
				4 => c19CoreDefaultExtraSpawnEnemies, 
				_ => new List<ActExtraSpawn>(), 
			};
		}
		foreach (ActExtraSpawn item4 in list7)
		{
			GameObject enemyObjectFromAsset4 = GetEnemyObjectFromAsset(item4.enemyAssetReference);
			switch (item4.slotType)
			{
			case WaveGroup.WaveType.GROUND:
			{
				for (int m = 0; m < item4.rartiy; m++)
				{
					list4.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset4, item4.difficulty));
				}
				break;
			}
			case WaveGroup.WaveType.FLYING:
			{
				for (int n = 0; n < item4.rartiy; n++)
				{
					list5.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset4, item4.difficulty));
				}
				break;
			}
			case WaveGroup.WaveType.STATIONARY:
			{
				for (int l = 0; l < item4.rartiy; l++)
				{
					list6.Add(new c19EnemyAndDifficulty(enemyObjectFromAsset4, item4.difficulty));
				}
				break;
			}
			}
		}
		for (int num6 = list4.Count - 1; num6 >= 0; num6--)
		{
			if (c19SpawnBanList.Contains(list4[num6].enemyObject))
			{
				list4.RemoveAt(num6);
			}
		}
		for (int num7 = list5.Count - 1; num7 >= 0; num7--)
		{
			if (c19SpawnBanList.Contains(list5[num7].enemyObject))
			{
				list5.RemoveAt(num7);
			}
		}
		for (int num8 = list6.Count - 1; num8 >= 0; num8--)
		{
			if (c19SpawnBanList.Contains(list6[num8].enemyObject))
			{
				list6.RemoveAt(num8);
			}
		}
		AttemptCruciballBonusSpawnOnEnemyList(list4, list5, list6, statusEffectData, relicManager, _groundSlots.Length, maxExtraEnemies, maxDifficulty, seededExtraEnemyRolls);
	}

	private void AttemptCruciballBonusSpawnOnEnemyList(List<c19EnemyAndDifficulty> posibleGroundEnemies, List<c19EnemyAndDifficulty> posibleFlyingEnemies, List<c19EnemyAndDifficulty> posibleStationaryEnemies, StatusEffectData statusEffectData, RelicManager relicManager, int numSlots, int maxExtraEnemies, int maxDifficulty, float[] seededExtraEnemyRolls = null)
	{
		bool flag = false;
		if (seededExtraEnemyRolls != null)
		{
			flag = true;
		}
		int num = 0;
		int num2 = (numSlots - 3) * 3;
		float num3 = 1.5f / (float)num2;
		float num4 = posibleGroundEnemies.Count + posibleFlyingEnemies.Count + posibleStationaryEnemies.Count;
		float num5 = num3 * 3f * ((float)posibleGroundEnemies.Count / num4);
		float num6 = num3 * 3f * ((float)posibleFlyingEnemies.Count / num4);
		float num7 = num3 * 3f * ((float)posibleStationaryEnemies.Count / num4);
		int num8 = 0;
		int num9 = 0;
		for (int num10 = numSlots - 2; num10 >= 1; num10--)
		{
			SlotType[] array = new SlotType[3]
			{
				SlotType.GROUND,
				SlotType.FLYING,
				SlotType.STATIONARY
			};
			foreach (SlotType slotType in array)
			{
				if (num8 >= maxExtraEnemies || num9 >= maxDifficulty)
				{
					break;
				}
				float num11 = num3;
				bool flag2 = true;
				switch (slotType)
				{
				case SlotType.GROUND:
					num11 = num5;
					flag2 = _groundSlots[num10].Enemy != null;
					break;
				case SlotType.FLYING:
					num11 = num6;
					flag2 = _flyingSlots[num10].Enemy != null;
					break;
				case SlotType.STATIONARY:
					num11 = num7;
					flag2 = _stationarySlots[num10].Enemy != null;
					break;
				default:
					continue;
				}
				if (flag2)
				{
					continue;
				}
				float num12;
				if (flag)
				{
					num12 = seededExtraEnemyRolls[num];
					num = (num + 1) % seededExtraEnemyRolls.Length;
				}
				else
				{
					num12 = UnityEngine.Random.value;
				}
				if (num12 >= num11)
				{
					continue;
				}
				List<c19EnemyAndDifficulty> list = null;
				switch (slotType)
				{
				case SlotType.GROUND:
					list = new List<c19EnemyAndDifficulty>(posibleGroundEnemies.ToArray());
					break;
				case SlotType.FLYING:
					list = new List<c19EnemyAndDifficulty>(posibleFlyingEnemies.ToArray());
					break;
				case SlotType.STATIONARY:
					list = new List<c19EnemyAndDifficulty>(posibleStationaryEnemies.ToArray());
					break;
				default:
					continue;
				}
				for (int num13 = list.Count - 1; num13 >= 0; num13--)
				{
					if (num9 + list[num13].difficulty > maxDifficulty)
					{
						list.RemoveAt(num13);
					}
				}
				if (list.Count <= 0)
				{
					continue;
				}
				int index;
				if (flag)
				{
					index = ListExtensions.ConvertPercentageToListIndex(seededExtraEnemyRolls[num], list.Count);
					num = (num + 1) % seededExtraEnemyRolls.Length;
				}
				else
				{
					index = UnityEngine.Random.Range(0, list.Count);
				}
				GameObject enemyObject = list[index].enemyObject;
				int difficulty = list[index].difficulty;
				switch (slotType)
				{
				case SlotType.GROUND:
				{
					Enemy enemy = UnityEngine.Object.Instantiate(enemyObject, GetSpawnPointForGroundSlot(num10), Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
					if (enemy != null)
					{
						enemy.Initialize(statusEffectData, this, relicManager, enemyObject.name);
						enemy.HealthBarBarSprite.SetIsC19ExtraEnemy();
						AddEnemy(enemy, num10);
						num8++;
						num9 += difficulty;
					}
					break;
				}
				case SlotType.FLYING:
				{
					Enemy enemy = UnityEngine.Object.Instantiate(enemyObject, GetSpawnPointForFlyingSlot(num10), Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
					if (enemy != null)
					{
						enemy.Initialize(statusEffectData, this, relicManager, enemyObject.name);
						enemy.HealthBarBarSprite.SetIsC19ExtraEnemy();
						AddEnemy(enemy, num10, isFlyingOverride: true);
						num8++;
						num9 += difficulty;
					}
					break;
				}
				case SlotType.STATIONARY:
				{
					Enemy enemy = UnityEngine.Object.Instantiate(enemyObject, GetSpawnPointForStationarySlot(num10), Quaternion.identity, base.transform)?.GetComponentInChildren<Enemy>();
					if (enemy != null)
					{
						enemy.Initialize(statusEffectData, this, relicManager, enemyObject.name);
						enemy.HealthBarBarSprite.SetIsC19ExtraEnemy();
						AddStationaryEnemy(enemy, num10);
						num8++;
						num9 += difficulty;
					}
					break;
				}
				}
			}
		}
	}

	private void InitC19ExtraEnemieDificultiesList()
	{
		List<ActExtraSpawn> list = null;
		if (MapController.instance != null)
		{
			list = MapController.instance.Act switch
			{
				1 => c19ForestDefaultExtraSpawnEnemies, 
				2 => c19CastleDefaultExtraSpawnEnemies, 
				3 => c19MinesDefaultExtraSpawnEnemies, 
				4 => c19CoreDefaultExtraSpawnEnemies, 
				_ => null, 
			};
		}
		if (list == null)
		{
			return;
		}
		foreach (ActExtraSpawn item in list)
		{
			_extraSpawnEnemyDifficultiesForThisAct.Add(new c19EnemyAndDifficulty(GetEnemyObjectFromAsset(item.enemyAssetReference), item.difficulty));
		}
	}

	private int GetDifficultyForC19EnemySpawn(GameObject enemy)
	{
		if (_extraSpawnEnemyDifficultiesForThisAct == null)
		{
			return 1;
		}
		foreach (c19EnemyAndDifficulty item in _extraSpawnEnemyDifficultiesForThisAct)
		{
			if (item.enemyObject == enemy)
			{
				return item.difficulty;
			}
		}
		return 1;
	}

	private GameObject GetEnemyObjectFromAsset(AssetReferenceGameObject reference)
	{
		string key = reference.RuntimeKey.ToString();
		if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key))
		{
			Debug.LogWarning($"Did not find cached prefab for enemy {reference}");
			GameObject value = reference.LoadAssetAsync<GameObject>().WaitForCompletion();
			AssetLoading.Instance.EnemyPrefabs.Add(key, value);
		}
		return AssetLoading.Instance.EnemyPrefabs[key];
	}

	private void GenerateSlots(Vector3 playerPos, Vector2 spawnPosition, int numberOfSlots)
	{
		float num = Mathf.Abs(playerPos.x - spawnPosition.x) / (float)numberOfSlots;
		int num2 = ((playerPos.x < spawnPosition.x) ? 1 : (-1));
		for (int i = 0; i < numberOfSlots; i++)
		{
			float num3 = playerPos.x + _meleeRangeOffset * (float)num2 + num * (float)i * (float)num2;
			Vector2 vector = new Vector2(num3, _groundEnemiesHeight);
			Vector2 vector2 = new Vector2(num3, _flyingEnemiesHeight);
			Vector2 vector3 = new Vector2(num3 + _stationaryOffset, _groundEnemiesHeight);
			_groundSlots[i] = new EnemySlot(vector, null, i);
			_flyingSlots[i] = new EnemySlot(vector2, null, i);
			_stationarySlots[i] = new EnemySlot(vector3, null, i);
		}
	}

	private void OnDrawGizmos()
	{
		if (_groundSlots != null && _groundSlots.Length != 0)
		{
			for (int i = 0; i < _groundSlots.Length; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(_groundSlots[i].Position, Vector2.one * 0.5f);
			}
		}
		Gizmos.color = Color.red;
		if (_playerTransform != null)
		{
			Gizmos.DrawCube(playerPosition + new Vector2(_meleeRangeOffset, 0f), Vector2.one * 0.5f);
		}
	}

	public void AddEnemy(Enemy enemy, int atSlot = -1, bool isFlyingOverride = false)
	{
		Vector3 vector = Vector3.zero;
		StationaryEnemy stationaryEnemy = enemy as StationaryEnemy;
		if (stationaryEnemy != null)
		{
			vector = stationaryEnemy.spawnOffset;
		}
		vector.z -= (float)atSlot * 0.1f;
		enemy.transform.localPosition += vector;
		if (enemy is SpecialMovementEnemy)
		{
			_specialMovementEnemies.Add(enemy as SpecialMovementEnemy);
		}
		EnemySlot enemySlot;
		if (enemy.IsFlying | isFlyingOverride)
		{
			enemy.enemyTypes |= Enemy.EnemyType.Flying;
			enemy.IsFlying = true;
			enemySlot = ((atSlot >= 0) ? _flyingSlots[atSlot] : _flyingSlots[_flyingSlots.Length - 1]);
		}
		else
		{
			enemySlot = ((atSlot >= 0) ? _groundSlots[atSlot] : _groundSlots[_groundSlots.Length - 1]);
		}
		if (enemySlot.Enemy != null)
		{
			Debug.LogError("EnemyManager::AddEnemy(): Attempted to overwrite existing enemy! " + enemySlot.Enemy.name);
		}
		else
		{
			enemySlot.Enemy = enemy;
			Enemies.Add(enemy);
			if (enemy.IsFlying)
			{
				FlyingEnemyCount++;
			}
			else
			{
				GroundEnemyCount++;
			}
		}
		if (!enemy.enemyTypes.HasFlag(Enemy.EnemyType.Minion) && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.SPINFECTION_PEGS))
		{
			CreateSpinfectionPegsOnDeath createSpinfectionPegsOnDeath = enemy.gameObject.AddComponent<CreateSpinfectionPegsOnDeath>();
			createSpinfectionPegsOnDeath.spinfectionPegCreationEffect = spinfectionPegCreationEffect;
			createSpinfectionPegsOnDeath.amount = ChallengeManager.SPINFECTION_PEG_AMOUNT;
		}
		OnEnemySpawned?.Invoke(enemy);
	}

	public Enemy[] GetColumnSplashRangeEnemies(int slot, SlotType slotType, int range, AoeAttack.AoeType aoeType)
	{
		List<Enemy> list = new List<Enemy>();
		list.AddRange(GetSplashRangeEnemies(slot, slotType, range, aoeType));
		Enemy[] array = slotType switch
		{
			SlotType.STATIONARY => GetSplashRangeEnemies((float)slot + 0.5f, SlotType.FLYING, range, aoeType), 
			SlotType.GROUND => GetSplashRangeEnemies(slot, SlotType.FLYING, range, aoeType), 
			_ => GetSplashRangeEnemies(slot, SlotType.GROUND, range, aoeType), 
		};
		foreach (Enemy item in array)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public Enemy[] GetSplashRangeEnemies(float inputSlot, SlotType slotType, int range, AoeAttack.AoeType aoeType)
	{
		if (aoeType == AoeAttack.AoeType.ALL)
		{
			return Enemies.ToArray();
		}
		int num = Mathf.FloorToInt(inputSlot);
		bool flag = false;
		bool flag2 = false;
		if (inputSlot != 0f && Mathf.FloorToInt(inputSlot) == (int)(inputSlot - 0.5f))
		{
			switch (slotType)
			{
			case SlotType.GROUND:
				Debug.LogError("Grounded enemies at 0.5 slot intervals should be treated as stationary enemies instead.");
				break;
			case SlotType.FLYING:
				flag = true;
				flag2 = true;
				break;
			}
		}
		Enemy enemy = null;
		if (!flag)
		{
			switch (slotType)
			{
			case SlotType.GROUND:
				enemy = GetGroundEnemyAtSlot(num);
				break;
			case SlotType.STATIONARY:
				enemy = GetStationaryEnemyAtSlot(num);
				flag2 = true;
				break;
			case SlotType.FLYING:
				enemy = GetFlyingEnemyAtSlot(num);
				break;
			}
		}
		float num2 = num;
		if (flag2)
		{
			num2 += 0.5f;
		}
		List<Enemy> list = new List<Enemy>();
		if (enemy != null && enemy.CurrentHealth > 0f)
		{
			list.Add(enemy);
		}
		if (aoeType == AoeAttack.AoeType.SIDE)
		{
			list.AddRange(GetSplashRangeEnemiesOnSide(num2, slotType, range, -1));
			if (GetVerticalSplashEnemy(num, slotType) != null)
			{
				list.Add(GetVerticalSplashEnemy(num, slotType));
			}
		}
		if (aoeType == AoeAttack.AoeType.SIDE || aoeType == AoeAttack.AoeType.BEHIND)
		{
			list.AddRange(GetSplashRangeEnemiesOnSide(num2, slotType, range, 1));
		}
		return list.ToArray();
	}

	private Enemy[] GetSplashRangeEnemiesOnSide(float targetEnemyIndex, SlotType targetSlotType, int range, int direction)
	{
		List<Enemy> list = new List<Enemy>();
		float num = Mathf.Sign(direction) * 0.5f;
		float num2 = 0.5f;
		float num3 = targetEnemyIndex + num;
		while (list.Count < range && num2 <= (float)range)
		{
			int num4 = Mathf.FloorToInt(num3);
			if (num4 >= _groundSlots.Length || num4 < 0)
			{
				break;
			}
			Enemy enemy = null;
			enemy = ((num3 - Mathf.Floor(num3) == 0.5f) ? GetStationaryEnemyAtSlot(num4) : ((targetSlotType != SlotType.FLYING) ? GetGroundEnemyAtSlot(num4) : GetFlyingEnemyAtSlot(num4)));
			if (enemy != null && enemy.CurrentHealth > 0f)
			{
				list.Add(enemy);
			}
			num2 += 0.5f;
			num3 += num;
		}
		return list.ToArray();
	}

	public Enemy GetVerticalSplashEnemy(int slot, SlotType targetSlotType)
	{
		Enemy result = null;
		switch (targetSlotType)
		{
		case SlotType.GROUND:
			result = GetFlyingEnemyAtSlot(slot);
			break;
		case SlotType.FLYING:
			result = GetGroundEnemyAtSlot(slot);
			break;
		}
		return result;
	}

	public int GetSlotIndexForGroundEnemy(Enemy enemy)
	{
		for (int i = 0; i < _groundSlots.Length; i++)
		{
			if (_groundSlots[i].Enemy == enemy)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetSlotIndexForFlyingEnemy(Enemy enemy)
	{
		for (int i = 0; i < _flyingSlots.Length; i++)
		{
			if (_flyingSlots[i] != null && _flyingSlots[i].Enemy == enemy)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetSlotIndexForEnemy(Enemy enemy, out bool isStationary)
	{
		isStationary = false;
		for (int i = 0; i < _groundSlots.Length; i++)
		{
			if (_groundSlots[i].Enemy == enemy)
			{
				return i;
			}
		}
		for (int j = 0; j < _flyingSlots.Length; j++)
		{
			if (_flyingSlots[j].Enemy == enemy)
			{
				return j;
			}
		}
		for (int k = 0; k < _stationarySlots.Length; k++)
		{
			if (_stationarySlots[k].Enemy == enemy)
			{
				isStationary = true;
				return k;
			}
		}
		return -1;
	}

	public Enemy GetFlyingEnemyAtSlot(int index)
	{
		return _flyingSlots[index].Enemy;
	}

	public Enemy GetGroundEnemyAtSlot(int index)
	{
		return _groundSlots[index].Enemy;
	}

	public Enemy GetStationaryEnemyAtSlot(int index)
	{
		return _stationarySlots[index].Enemy;
	}

	public void AddStationaryEnemy(Enemy enemy, int slot)
	{
		Vector3 vector = Vector3.zero;
		StationaryEnemy stationaryEnemy = enemy as StationaryEnemy;
		if (stationaryEnemy != null)
		{
			vector = stationaryEnemy.spawnOffset;
		}
		enemy.transform.localPosition += vector;
		if (enemy is SpecialMovementEnemy)
		{
			_specialMovementEnemies.Add(enemy as SpecialMovementEnemy);
		}
		if (_stationarySlots[slot].Enemy != null)
		{
			Debug.LogError("EnemyManager::AddEnemy(): Attempted to overwrite enemy in stationary slot " + slot);
		}
		else
		{
			_stationarySlots[slot].Enemy = enemy;
			Enemies.Add(enemy);
			GroundEnemyCount++;
		}
		if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.SPINFECTION_PEGS))
		{
			CreateSpinfectionPegsOnDeath createSpinfectionPegsOnDeath = enemy.gameObject.AddComponent<CreateSpinfectionPegsOnDeath>();
			createSpinfectionPegsOnDeath.spinfectionPegCreationEffect = spinfectionPegCreationEffect;
			createSpinfectionPegsOnDeath.amount = ChallengeManager.SPINFECTION_PEG_AMOUNT;
		}
	}

	public void RemoveEnemy(Enemy enemy)
	{
		if (enemy is SpecialMovementEnemy)
		{
			_specialMovementEnemies.Remove(enemy as SpecialMovementEnemy);
		}
		foreach (EnemySlot[] allSlot in _allSlots)
		{
			if (RemoveEnemyFromSlots(enemy, allSlot))
			{
				break;
			}
		}
	}

	private bool RemoveEnemyFromSlots(Enemy enemy, EnemySlot[] slots)
	{
		SpawnEnemyOnDeath component = enemy.GetComponent<SpawnEnemyOnDeath>();
		EffectPlayerOnDeath component2 = enemy.GetComponent<EffectPlayerOnDeath>();
		foreach (EnemySlot enemySlot in slots)
		{
			if (!(enemySlot.Enemy == enemy))
			{
				continue;
			}
			enemySlot.Enemy = null;
			Enemies.Remove(enemy);
			if (component2 != null)
			{
				UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>().ApplyStatusEffects(component2.effects, StatusEffectSource.ENEMY);
			}
			if (component != null)
			{
				if (!GetStationaryEnemyAtSlot(enemySlot.Index))
				{
					Enemy component3 = UnityEngine.Object.Instantiate(component.enemyPrefabToSpawn, enemySlot.Position, Quaternion.identity).GetComponent<Enemy>();
					component3.Initialize(enemy.GetComponentInChildren<StatusEffectIconManager>().EffectData, this, enemy.relicManager, component.enemyPrefabToSpawn.name);
					if (component.passStatusEffects)
					{
						StatusEffect[] statusEffects = enemy.StatusEffects;
						foreach (StatusEffect statusEffect in statusEffects)
						{
							if (statusEffect.Intensity > 0)
							{
								component3.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, statusEffect.Intensity), StatusEffectSource.ENEMY);
							}
						}
					}
					AddStationaryEnemy(component3, enemySlot.Index);
				}
				return true;
			}
			if (enemy.IsFlying)
			{
				FlyingEnemyCount--;
			}
			else
			{
				GroundEnemyCount--;
			}
			return true;
		}
		return false;
	}

	public bool IsGroundSpawnPointClear()
	{
		return _groundSlots[_groundSlots.Length - 1].Enemy == null;
	}

	public bool IsFlyingSpawnPointClear()
	{
		return _flyingSlots[_flyingSlots.Length - 1].Enemy == null;
	}

	public bool IsFlyingSlotClear(int slot)
	{
		if (slot >= 0 && slot < _flyingSlots.Length)
		{
			return _flyingSlots[slot].Enemy == null;
		}
		return false;
	}

	public bool IsGroundSlotClear(int slot)
	{
		if (slot >= 0 && slot < _groundSlots.Length)
		{
			return _groundSlots[slot].Enemy == null;
		}
		return false;
	}

	public bool IsStationarySlotClear(int slot)
	{
		if (slot >= _stationarySlots.Length)
		{
			Debug.LogError($"Attempting to place a stationary enemy in slot {slot} when array is only size {_stationarySlots}.");
			return false;
		}
		return _stationarySlots[slot].Enemy == null;
	}

	public Vector3 GetGroundSpawnPoint()
	{
		return _groundSlots[_groundSlots.Length - 1].Position;
	}

	public Vector3 GetFlyingSpawnPoint()
	{
		return _flyingSlots[_flyingSlots.Length - 1].Position;
	}

	public Vector3 GetSpawnPointForGroundSlot(int slot)
	{
		return _groundSlots[slot].Position;
	}

	public Vector3 GetSpawnPointForStationarySlot(int slot)
	{
		if (slot >= 0 && slot < _stationarySlots.Length)
		{
			return _stationarySlots[slot].Position;
		}
		Debug.LogError("EnemyManager::GetSpawnPointForStationarySlot(): Slot requested was out of bounds, returning last slot position");
		return _stationarySlots[_stationarySlots.Length - 1].Position;
	}

	public Vector3 GetSpawnPointForFlyingSlot(int slot)
	{
		return _flyingSlots[slot].Position;
	}

	public void TakeTurns()
	{
		StartCoroutine(ResolveEnemyTurns());
	}

	public void TickStatusEffects()
	{
		StartCoroutine(ResolveStatusEffects());
	}

	public SlotType GetSlotForEnemy(Enemy enemy)
	{
		EnemySlot[] groundSlots = _groundSlots;
		for (int i = 0; i < groundSlots.Length; i++)
		{
			if (groundSlots[i].Enemy == enemy)
			{
				return SlotType.GROUND;
			}
		}
		groundSlots = _flyingSlots;
		for (int i = 0; i < groundSlots.Length; i++)
		{
			if (groundSlots[i].Enemy == enemy)
			{
				return SlotType.FLYING;
			}
		}
		groundSlots = _stationarySlots;
		for (int i = 0; i < groundSlots.Length; i++)
		{
			if (groundSlots[i].Enemy == enemy)
			{
				return SlotType.STATIONARY;
			}
		}
		return SlotType.GROUND;
	}

	public Vector3 MoveEnemyToFarthestEmptyGroundSlot(Enemy enemy)
	{
		int num = -1;
		for (int i = 0; i < _groundSlots.Length; i++)
		{
			if (enemy == _groundSlots[i].Enemy)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogWarning("EnemyManager::MoveEnemyToFarthestEmptyGroundSlot(): Asked to move enemy that is not in our slots");
			return Vector3.zero;
		}
		int farthestEmptyGroundSlot = GetFarthestEmptyGroundSlot(num);
		if (farthestEmptyGroundSlot >= 0)
		{
			_groundSlots[num].Enemy = null;
			_groundSlots[farthestEmptyGroundSlot].Enemy = enemy;
			return _groundSlots[farthestEmptyGroundSlot].Position;
		}
		return _groundSlots[num].Position;
	}

	public bool EnemiesAnimating()
	{
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.CurrentlyActing || enemy.queuedAnimations.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool AllValidEnemiesDead()
	{
		CleanupDeadEnemies();
		for (int i = 0; i < Enemies.Count; i++)
		{
			if (!Enemies[i].ignoreForVictoryCheck)
			{
				return false;
			}
		}
		return true;
	}

	public void CleanupDeadEnemies()
	{
		List<Enemy> list = new List<Enemy>();
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.gameObject.activeInHierarchy && enemy.CurrentHealth == 0f)
			{
				list.Add(enemy);
			}
		}
		foreach (Enemy item in list)
		{
			item.ForceDestruction();
		}
	}

	private int GetFarthestEmptyGroundSlot(int minSlot)
	{
		for (int num = _groundSlots.Length - 2; num >= minSlot; num--)
		{
			if (_groundSlots[num].Enemy == null)
			{
				return num;
			}
		}
		return -1;
	}

	public Enemy GetFarthestEnemyFromPlayer()
	{
		Enemy enemy = null;
		float num = -1f;
		foreach (Enemy enemy2 in Enemies)
		{
			float num2 = GetSlotIndexForEnemy(enemy2, out var isStationary);
			if (isStationary)
			{
				num2 += 0.5f;
			}
			if (enemy2.CurrentHealth > 0f && (enemy == null || num2 > num || (num2 == num && enemy2.IsFlying && !enemy.IsFlying)))
			{
				num = num2;
				enemy = enemy2;
			}
		}
		return enemy;
	}

	public List<Enemy> GetOrderedTargetingList()
	{
		List<Enemy> list = new List<Enemy>();
		for (int i = 0; i < _groundSlots.Length; i++)
		{
			if (_flyingSlots[i].Enemy != null)
			{
				list.Add(_flyingSlots[i].Enemy);
			}
			if (_groundSlots[i].Enemy != null)
			{
				list.Add(_groundSlots[i].Enemy);
			}
			if (_stationarySlots[i].Enemy != null)
			{
				list.Add(_stationarySlots[i].Enemy);
			}
		}
		return list;
	}

	private IEnumerator ResolveEnemyTurns()
	{
		EnemiesActing = true;
		foreach (SpecialMovementEnemy specialMovementEnemy in _specialMovementEnemies)
		{
			specialMovementEnemy.StartSpecialMove(_groundSlots, _flyingSlots);
		}
		bool anySpecialEnemiesMoving = true;
		if (_specialMovementEnemies.Count > 0)
		{
			while (anySpecialEnemiesMoving)
			{
				anySpecialEnemiesMoving = false;
				foreach (SpecialMovementEnemy specialMovementEnemy2 in _specialMovementEnemies)
				{
					if (specialMovementEnemy2.isMoving)
					{
						anySpecialEnemiesMoving = true;
						break;
					}
				}
				yield return null;
			}
		}
		for (int i = 0; i < _groundSlots.Length; i++)
		{
			Enemy groundEnemy = TellEnemyToTakeAction(_groundSlots, i);
			while (groundEnemy != null && groundEnemy.CurrentlyActing)
			{
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			Enemy flyingEnemy = TellEnemyToTakeAction(_flyingSlots, i);
			while (flyingEnemy != null && flyingEnemy.CurrentlyActing)
			{
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			Enemy stationaryEnemy = TellEnemyToTakeAction(_stationarySlots, i);
			while (stationaryEnemy != null && stationaryEnemy.CurrentlyActing)
			{
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
		}
		EnemiesActing = false;
	}

	private IEnumerator ResolveStatusEffects()
	{
		EnemiesActing = true;
		List<Enemy> enemiesActing = new List<Enemy>();
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.ResolveStatusEffects())
			{
				enemiesActing.Add(enemy);
			}
		}
		while (EnemiesActing)
		{
			bool enemiesActing2 = false;
			foreach (Enemy item in enemiesActing)
			{
				if (item.CurrentlyActing)
				{
					enemiesActing2 = true;
				}
			}
			EnemiesActing = enemiesActing2;
			yield return WaitObjectAllocator.waitForEndOfFrame;
		}
	}

	public void NegativeStatusEffectAddedToEnemy(int intensity)
	{
		_statusEffectIntensityAppliedToEnemiesThisBattle += intensity;
	}

	private Enemy TellEnemyToTakeAction(EnemySlot[] slots, int slotIndex)
	{
		Enemy enemy = slots[slotIndex].Enemy;
		if (enemy == null)
		{
			return null;
		}
		enemy.PreMoveTurnAction();
		int range = slotIndex + 1;
		if (slotIndex == 0 && !enemy.IsOnlyRanged)
		{
			enemy.Attack(range, playerPosition);
		}
		else if (IsInAttackRange(slotIndex, enemy.AttackRange) && (slots[0].Enemy != null || enemy.RangeOverMelee))
		{
			enemy.Attack(range, playerPosition);
		}
		else if (enemy.IsOnlyRanged)
		{
			enemy.Attack(range, playerPosition);
		}
		else
		{
			int num = ((!enemy.FreeMovement) ? CalculateMovement(slots, slotIndex, enemy.defaultMovement, enemy.MaxMovement) : CalculateFreeMovement(slots, slotIndex, enemy.MaxMovement));
			if (num != slotIndex)
			{
				if (enemy.Move(slots[num].Position, num))
				{
					slots[num].Enemy = enemy;
					slots[slotIndex].Enemy = null;
					OnEnemyMoved?.Invoke(slotIndex, num, enemy);
				}
			}
			else
			{
				bool flag = false;
				WalkEnemy walkEnemy = enemy as WalkEnemy;
				if (walkEnemy != null)
				{
					flag = walkEnemy.MoveAndShoot;
				}
				if (flag)
				{
					StartCoroutine(walkEnemy.DoMoveAndShootAttack());
				}
			}
		}
		return enemy;
	}

	private int CalculateMovement(EnemySlot[] slots, int slot, int defaultMove, int maxMove)
	{
		if (slot == 0)
		{
			return 0;
		}
		for (int num = Math.Min(slot, defaultMove); num > 0; num--)
		{
			if (slots[slot - num].Enemy == null)
			{
				return slot - num;
			}
		}
		maxMove = Math.Min(slot, maxMove);
		if (maxMove > defaultMove)
		{
			for (int i = defaultMove + 1; i <= maxMove; i++)
			{
				if (slots[slot - i].Enemy == null)
				{
					return slot - i;
				}
			}
		}
		return slot;
	}

	private int CalculateFreeMovement(EnemySlot[] slots, int currentSlot, int enemyMovementSpeed)
	{
		if (currentSlot == 0)
		{
			return 0;
		}
		for (int i = 0; i <= currentSlot && currentSlot - i <= enemyMovementSpeed; i++)
		{
			if (slots[i].Enemy == null)
			{
				return i;
			}
		}
		return currentSlot;
	}

	private bool IsInAttackRange(int slot, int attackRange)
	{
		return slot + 1 <= attackRange;
	}

	public int NumDamagedEnemies()
	{
		int num = 0;
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.CurrentHealth < enemy.StartingHealth && enemy.CanBeHealed)
			{
				num++;
			}
		}
		return num;
	}

	public void CreateVictoryBlockingOperation()
	{
		_ongoingVictoryBlockingOperations++;
	}

	public void ResolveVictoryBlockingOperation()
	{
		_ongoingVictoryBlockingOperations--;
	}

	public IEnumerator WaitAndResolveVictoryBlocker(float delay)
	{
		yield return new WaitForSeconds(delay);
		_ongoingVictoryBlockingOperations--;
	}
}
