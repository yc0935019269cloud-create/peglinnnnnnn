using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Enemies;
using Challenges;
using Cruciball;
using Currency;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using Data.Scenarios;
using I2.Loc;
using Loading;
using Loading.Pegboards;
using Map.MinimapView;
using NaughtyAttributes;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUtils;
using Relics;
using Saving;
using Scenarios;
using Seeding;
using Stats;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Worldmap;

namespace Map
{
	public class MapController : MonoBehaviour
	{
		public delegate void NodeSelectionEvent(string mapName, int floor, int cruciballLevel);

		public delegate void MapReturnEvent(int backgroundIndex);

		public static NodeSelectionEvent OnNodeSelectionEvent;

		public static MapReturnEvent OnMapReturnEvent;

		private const string MIMIC_MAP_DATA_NAME = "MimicMinibossMapData";

		public PeglinSceneLoader.Scene thisScene = PeglinSceneLoader.Scene.FOREST_MAP;

		public PeglinSceneLoader.Scene nextScene = PeglinSceneLoader.Scene.FOREST_WIN;

		public const int MAX_ORBS_PER_BATTLE = 5;

		private const int EASY_BATTLES_PER_AREA = 2;

		private const PeglinSceneLoader.Scene FINAL_WIN_SCENE = PeglinSceneLoader.Scene.MINES_WIN;

		private const PeglinSceneLoader.Scene CORE_WIN_SCENE = PeglinSceneLoader.Scene.CORE_WIN;

		public int Act = 1;

		public string mapNameLocKey = "";

		[Header("Scriptable Object Vars")]
		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private DeckManager _deckManager;

		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private MapDataTreasure _treasureMapData;

		[SerializeField]
		private MapDataShop _shopMapData;

		[SerializeField]
		private LoadMapData _loadMapData;

		[SerializeField]
		private FloatVariable _playerHealth;

		[SerializeField]
		private FloatVariable _playerMaxHealth;

		[SerializeField]
		private FloatVariable _reductionToMaxHealth;

		[SerializeField]
		private SecretSeedManager _secretSeedManager;

		[SerializeField]
		private ChallengeSet _globalChallenges;

		public ChallengesPerAct debugChallenges;

		[Header("Debug parameters")]
		[SerializeField]
		public Class debugClass;

		[Header("SceneNames")]
		private readonly PeglinSceneLoader.Scene battleSceneName = PeglinSceneLoader.Scene.BATTLE;

		private readonly PeglinSceneLoader.Scene textScenarioName = PeglinSceneLoader.Scene.TEXT_SCENARIO;

		private readonly PeglinSceneLoader.Scene treasureScenarioName = PeglinSceneLoader.Scene.TREASURE;

		private readonly PeglinSceneLoader.Scene pegMinigameScenarioName = PeglinSceneLoader.Scene.PEG_MINIGAME;

		private readonly PeglinSceneLoader.Scene shopScenarioName = PeglinSceneLoader.Scene.SHOP_SCENARIO;

		[Header("AnimationParams")]
		public RuntimeAnimatorController[] animationControllers;

		public AnimationCurve walkCurve;

		public float prePanWaitTime = 1f;

		public float initialCameraPanTime = 8f;

		public float preWalkWaitTime = 1.25f;

		public float postWalkWaitTime = 1f;

		public float nextLineColorTime = 1f;

		public float walkTime = 1f;

		public float fadeInTime = 0.4f;

		public float fadeOutTime = 0.8f;

		private float _cameraYOffset = 0.5f;

		private int _numEasyBattles;

		[Header("Backgrounds and Pegboard Frame")]
		public BackgroundData[] backgroundData;

		public GameObject[] battlePegboardFrame;

		public GameObject[] scenarioPegboardFrame;

		public GameObject[] pegMinigamePegboardFrame;

		private int currentRoomCount;

		private int backgroundIndex;

		private int floorCount;

		private int mimicsBattledThisAct;

		[Header("Mapgen Params")]
		[Tooltip("This number includes the treasure nodes that are on the map by default.")]
		public int minTreasureNodes = 2;

		public int minEliteNodes = 2;

		public int minShopNodes = 2;

		[HideInInspector]
		public bool inputBlocked;

		public MapNode rootNode;

		private MapNode _nextNode;

		private MapNode _previousNode;

		[Header("Scenario Data")]
		[SerializeField]
		private List<MapDataBattle> _potentialEasyBattles;

		[SerializeField]
		private List<MapDataBattle> _potentialRandomBattles;

		[SerializeField]
		private List<MapDataBattle> _potentialEliteBattles;

		[SerializeField]
		private MapDataBattle _mimicBatteData;

		[SerializeField]
		private List<MapDataScenario> _potentialRandomScenarios;

		[SerializeField]
		private List<MapDataScenario> _visitedScenarios;

		[SerializeField]
		private List<MapDataPegMinigame> _potentialPegMinigameScenarios;

		private Queue<MapDataBattle> _easyBattlesRandomQueue = new Queue<MapDataBattle>();

		private Queue<MapDataBattle> _regularBattlesRandomQueue = new Queue<MapDataBattle>();

		private Queue<MapDataScenario> _scenariosRandomQueue = new Queue<MapDataScenario>();

		private List<string> _seededVisitedScenarioNames = new List<string>();

		public bool allowRepeatScenarios;

		private List<MapDataBattle> _remainingEasyBattles;

		private List<MapDataBattle> _remainingRandomBattles;

		private List<MapDataScenario> _remainingRandomScenarios;

		private List<MapDataBattle> _allEasyBattles;

		private List<MapDataBattle> _allRandomBattles;

		private List<MapDataScenario> _allRandomScenarios;

		private Dictionary<string, int> _enemiesKilledDuringCombat = new Dictionary<string, int>();

		private Dictionary<string, int> _enemiesWithLootDropDuringCombat = new Dictionary<string, int>();

		private Dictionary<string, int> _enemiesWithCardDropDuringCombat = new Dictionary<string, int>();

		private static readonly string EASY_BATTLES_SAVE_KEY = "EasyBattles";

		private static readonly string RANDOM_BATTLES_SAVE_KEY = "RandomBattles";

		private static readonly string SCENARIOS_SAVE_KEY = "Scenarios";

		private static readonly string VISITED_SCENARIOS_SAVE_KEY = "VisitedScenarios";

		private static readonly string SEEDED_VISITED_SCENARIOS_SAVE_KEY = "SeededVisitedScenarios";

		public static MapController instance = null;

		private bool _firstLoad = true;

		private GameObject _player;

		private Vector3 _playerInitialPosition;

		private Vector3 _playerDefaultScale;

		[SerializeField]
		private MapNode extraChallengeFight;

		private SeededMapNodeData[] _nodeChangesFromCruciballSeededData = new SeededMapNodeData[0];

		private CameraFollowPoint _cameraFollowPoint;

		private MapNode[] _nodes;

		private string _mapName;

		private PegLayoutLoader pegLayoutLoader;

		public MapDataBattle MimicBattleData
		{
			get
			{
				return _mimicBatteData;
			}
			protected set
			{
				_mimicBatteData = value;
			}
		}

		public MapNode bossNode
		{
			get
			{
				MapNode[] nodes = _nodes;
				foreach (MapNode mapNode in nodes)
				{
					if (mapNode.RoomType == RoomType.BOSS)
					{
						return mapNode;
					}
				}
				return null;
			}
		}

		[Button(null, EButtonEnableMode.Always)]
		private void PrintSeededQueues()
		{
			Debug.Log("Relic Queues");
			PrintRelicQueue("\tCurrentCommonRelicsQueue", _relicManager.CurrentCommonRelicsQueue);
			PrintRelicQueue("\tCurrentRareRelicsQueue", _relicManager.CurrentRareRelicsQueue);
			PrintRelicQueue("\tCurrentBossRelicsQueue", _relicManager.CurrentBossRelicsQueue);
			PrintQueue("\tCurrentShopRelicsQueue", _relicManager.CurrentShopRelicsQueue);
			Debug.Log("Orb Queues");
			PrintQueue("\tCurrentCommonOrbQueue", _deckManager.CurrentCommonOrbQueue);
			PrintQueue("\tCurrentUncommonOrbQueue", _deckManager.CurrentUncommonOrbQueue);
			PrintQueue("\tCurrentRareOrbQueue", _deckManager.CurrentRareOrbQueue);
			Debug.Log("MapData Queues");
			PrintQueue("\tCurrentEasyBattlesRandomQueue", _easyBattlesRandomQueue);
			PrintQueue("\tCurrentRegularBattlesRandomQueue", _regularBattlesRandomQueue);
			PrintQueue("\tCurrentScenariosRandomQueue", _scenariosRandomQueue);
			_relicManager.PrintOrbertsStorySeededData();
			if (_secretSeedManager.currentSecretSeed != null)
			{
				CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
				if ((bool)_secretSeedManager.currentSecretSeed)
				{
					PrintQueue("\tSecret Seed commonOrbQueue", customPoolsSecretSeed.commonOrbQueue);
					PrintQueue("\tSecret Seed uncommonOrbQueue", customPoolsSecretSeed.uncommonOrbQueue);
					PrintQueue("\tSecret Seed rareOrbQueue", customPoolsSecretSeed.rareOrbQueue);
					PrintRelicQueue("\tSecret Seed commonRelicQueue", customPoolsSecretSeed.commonRelicQueue);
					PrintRelicQueue("\tSecret Seed rareRelicQueue", customPoolsSecretSeed.rareRelicQueue);
					PrintRelicQueue("\tSecret Seed bossRelicQueue", customPoolsSecretSeed.bossRelicQueue);
				}
			}
		}

		private void PrintQueue<T>(string queueName, Queue<T> queueToPrint)
		{
			string text = queueName + ": ";
			for (int i = 0; i < queueToPrint.Count; i++)
			{
				T item = queueToPrint.Dequeue();
				queueToPrint.Enqueue(item);
				text = text + item.ToString() + ", ";
			}
			Debug.Log(text);
		}

		private void PrintRelicQueue(string queueName, Queue<RelicManager.RelicClassRarityOverrides> queueToPrint)
		{
			string text = queueName + ": ";
			for (int i = 0; i < queueToPrint.Count; i++)
			{
				RelicManager.RelicClassRarityOverrides item = queueToPrint.Dequeue();
				queueToPrint.Enqueue(item);
				text = text + item.relic.ToString() + ", ";
			}
			Debug.Log(text);
		}

		private void Awake()
		{
			if (instance != null)
			{
				instance.transform.GetChild(0).gameObject.SetActive(value: true);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				instance = this;
				UnityEngine.Object.DontDestroyOnLoad(this);
				_player = GameObject.FindGameObjectWithTag("Player");
				_playerInitialPosition = _player.transform.position;
				_playerDefaultScale = _player.transform.localScale;
				if (animationControllers != null && animationControllers.Length - 1 >= (int)_cruciballManager.currentClass)
				{
					_player.GetComponent<Animator>().runtimeAnimatorController = animationControllers[(int)_cruciballManager.currentClass];
				}
				else
				{
					Debug.LogWarning("Animator data does not match the class selected in MapController");
				}
				_nodes = GetSortedMapNodesArray();
				MapNode[] nodes = _nodes;
				foreach (MapNode obj in nodes)
				{
					obj.relicManager = _relicManager;
					obj.deckManager = _deckManager;
				}
				_mapName = LocalizationManager.GetTranslation(mapNameLocKey);
			}
			if (_playerHealth == null)
			{
				Debug.LogError("MapController::Player health ScriptableObject is null");
			}
			if (_playerMaxHealth == null)
			{
				Debug.LogError("MapController::Player max health ScriptableObject is null");
			}
		}

		private MapNode[] GetSortedMapNodesArray()
		{
			List<MapNode> list = new List<MapNode>();
			Queue<MapNode> queue = new Queue<MapNode>();
			queue.Enqueue(rootNode);
			while (queue.Count > 0)
			{
				MapNode mapNode = queue.Dequeue();
				if (mapNode == null)
				{
					continue;
				}
				MapNode[] childNodes = mapNode.ChildNodes;
				for (int i = 0; i < childNodes.Length; i++)
				{
					if (!queue.Contains(childNodes[i]))
					{
						queue.Enqueue(childNodes[i]);
					}
				}
				if (!list.Contains(mapNode))
				{
					list.Add(mapNode);
				}
			}
			return list.ToArray();
		}

		private void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SaveManager.OnSaveRequested = (SaveManager.SaveRequested)Delegate.Combine(SaveManager.OnSaveRequested, new SaveManager.SaveRequested(SaveData));
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Combine(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(RecordRunStatsOnLoss));
			Enemy.OnEnemyKilled = (Enemy.EnemyDataEvent)Delegate.Combine(Enemy.OnEnemyKilled, new Enemy.EnemyDataEvent(OnEnemyKilled));
			Enemy.OnEnemyCardDropped = (Enemy.EnemyDataEvent)Delegate.Combine(Enemy.OnEnemyCardDropped, new Enemy.EnemyDataEvent(OnEnemyCardDropped));
			Enemy.OnEnemyLootDropped = (Enemy.EnemyDataEvent)Delegate.Combine(Enemy.OnEnemyLootDropped, new Enemy.EnemyDataEvent(OnEnemyLootDropped));
			MimicEnemy.OnMimicOpened = (MimicEnemy.MimicOpened)Delegate.Combine(MimicEnemy.OnMimicOpened, new MimicEnemy.MimicOpened(HandleMimicOpened));
		}

		private void OnEnemyCardDropped(string lockey)
		{
			OnEnemyKilled(lockey);
			if (!_enemiesWithCardDropDuringCombat.TryAdd(lockey, 1))
			{
				_enemiesWithCardDropDuringCombat[lockey]++;
			}
		}

		private void OnEnemyLootDropped(string locKey)
		{
			OnEnemyKilled(locKey);
			if (!_enemiesWithLootDropDuringCombat.TryAdd(locKey, 1))
			{
				_enemiesWithLootDropDuringCombat[locKey]++;
			}
		}

		private void OnEnemyKilled(string locKey)
		{
			if (!_enemiesKilledDuringCombat.TryAdd(locKey, 1))
			{
				_enemiesKilledDuringCombat[locKey]++;
			}
		}

		public bool HasEnemyDroppedCardThisBattle(string lockey)
		{
			return _enemiesWithCardDropDuringCombat.ContainsKey(lockey);
		}

		private void Start()
		{
			_cameraFollowPoint = GetComponent<CameraFollowPoint>();
			ChallengeManager.Instance.AttemptClearChallengeForAct(Act - 1);
			if (Act == 4 && !ChallengeManager.Instance.chosenToGoToCore)
			{
				ChallengeManager.Instance.chosenToGoToCore = true;
				ChallengeManager.Instance.chosenToSkipCore = false;
				ChallengeManager.Instance.beatenMinesBossAndCanGoToCore = true;
			}
			bool flag = !_loadMapData.NewGame && !_loadMapData.ContinueGame;
			if (_loadMapData.NewGame | flag)
			{
				if (StaticGameData.seedSet)
				{
					UnityEngine.Random.state = StaticGameData.nextMapSeedState;
					CheckAndFixRandomState();
				}
				_firstLoad = true;
				CreateMapDataLists();
				rootNode.SetActiveState(RoomState.NEXT);
				ChestScenarioController.bombChestsOpened = 0;
				StaticGameData.mimicsBattled = 0;
				PostProcessMap();
				MapNode[] nodes;
				if (_relicManager.AttemptUseRelic(RelicEffect.ONLY_MINIBOSSES))
				{
					nodes = _nodes;
					foreach (MapNode mapNode in nodes)
					{
						if (mapNode.RoomType != RoomType.BOSS)
						{
							mapNode.RoomType = RoomType.MINI_BOSS;
							mapNode.GenerateIcon();
						}
					}
				}
				ChangeMapSecretSeed changeMapSecretSeed = _secretSeedManager.currentSecretSeed as ChangeMapSecretSeed;
				if (changeMapSecretSeed != null)
				{
					changeMapSecretSeed.ChangeMap(this, _nodes);
				}
				if (StaticGameData.seedSet)
				{
					SeedMapContents();
				}
				List<SeededMapNodeData> list = new List<SeededMapNodeData>();
				nodes = _nodes;
				for (int i = 0; i < nodes.Length; i++)
				{
					if (nodes[i].TryGetComponent<ChangePotentialMapDataBasedOnCruciball>(out var component))
					{
						if (component.newMapData.name == "MimicMinibossMapData")
						{
							SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
							seededBattleNodeData.GenerateMinibossData(_deckManager, _relicManager, this, allowMimic: true, forceMimic: true);
							list.Add(seededBattleNodeData);
						}
						else
						{
							Debug.LogError("ChangePotentialMapDataBasedOnCruciball not implemented for anything other than a mimic in the core chest.");
						}
					}
				}
				_nodeChangesFromCruciballSeededData = list.ToArray();
				SetChangeFromCruciballNodeSeededData();
				if (_playerMaxHealth.Value == 0f)
				{
					_playerHealth.Reset();
					_playerMaxHealth.Reset();
					_reductionToMaxHealth.Reset();
				}
				if (flag)
				{
					RemoveVisitedScenariosFromPreviousMaps();
				}
				if (_loadMapData.NewGame)
				{
					CurrencyManager.Instance.ClearGold();
					int num = _cruciballManager.GetStartingGold();
					if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.ADD_ORBS_AND_UPGRADE))
					{
						num += 100;
					}
					CurrencyManager.Instance.AddGold(num, silent: true);
					StaticGameData.totalFloorCount = 0;
					StaticGameData.StoreNumberOfOrbsRemoved = 0;
					ChallengeManager.Instance.Init(_globalChallenges, this, _deckManager, _relicManager, _cruciballManager);
				}
				OnNodeSelectionEvent?.Invoke(_mapName, 1, _cruciballManager.currentCruciballLevel);
			}
			else if (_loadMapData.ContinueGame)
			{
				MapControllerSaveData mapControllerSaveData = (MapControllerSaveData)DataSerializer.Load<SaveObjectData>(MapControllerSaveData.MAP_CONTROLLER_SAVE_KEY, DataSerializer.SaveType.RUN);
				if (mapControllerSaveData != null)
				{
					StaticGameData.nextMapSeedState = mapControllerSaveData.NextMapSeedState;
					RebuildDataFromMapControllerSave(mapControllerSaveData);
					if (StaticGameData.CurrentRunStats != null)
					{
						StaticGameData.CurrentRunStats = LoadRunStats();
						StaticGameData.CurrentRunStats.StartRunTimer();
					}
					SaveManager.RequestLoad();
					MapNode[] nodes = _nodes;
					for (int i = 0; i < nodes.Length; i++)
					{
						nodes[i].LoadNode();
					}
				}
				else
				{
					Debug.LogError("MapController::Start(): Attempted to load saved data but there was none. Starting the scene from the top.");
					UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
					StaticGameData.nextMapSeedState = UnityEngine.Random.state;
					rootNode.SetActiveState(RoomState.NEXT);
					_firstLoad = true;
				}
			}
			SetUpPreviousAndNextNodes();
			extraChallengeFight.gameObject.SetActive(value: false);
			if (ChallengeManager.Instance.challengesOfEachAct[Act] == ChallengeEffect.FIGHT_MINIBOSS_FROM_NEXT_ACT)
			{
				extraChallengeFight.gameObject.SetActive(value: true);
				extraChallengeFight.RoomType = RoomType.MINI_BOSS;
				extraChallengeFight.GenerateIcon();
			}
			else if (ChallengeManager.Instance.challengesOfEachAct[Act] == ChallengeEffect.FIGHT_EXTRA_BOSS)
			{
				extraChallengeFight.gameObject.SetActive(value: true);
				extraChallengeFight.RoomType = RoomType.BOSS;
				List<MapData> list2 = new List<MapData>(extraChallengeFight.potentialMapData);
				if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.visitedBosses.Count > 0)
				{
					_ = StaticGameData.CurrentRunStats.visitedBosses.ToArray()[StaticGameData.CurrentRunStats.visitedBosses.Count - 1];
					MapData[] potentialMapData = extraChallengeFight.potentialMapData;
					foreach (MapData mapData in potentialMapData)
					{
						if (StaticGameData.CurrentRunStats.visitedBosses.Contains(((MapDataBattle)mapData).bossType))
						{
							list2.Remove(mapData);
						}
					}
				}
				if (list2.Contains(bossNode.potentialMapData[bossNode.selectedBossIndex]))
				{
					list2.Remove(bossNode.potentialMapData[bossNode.selectedBossIndex]);
				}
				extraChallengeFight.ForceSetPotentialMapData(list2.ToArray());
				extraChallengeFight.GenerateIcon(ChallengeManager.Instance.GetExtraChallengeBossRoll(Act));
			}
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.cruciballLevel = _cruciballManager.currentCruciballLevel;
				StaticGameData.CurrentRunStats.selectedClass = _cruciballManager.currentClass;
			}
			if (!_loadMapData.ContinueGame)
			{
				MapNode[] nodes = _nodes;
				for (int i = 0; i < nodes.Length; i++)
				{
					nodes[i].SaveNode();
				}
				StaticGameData.nextMapSeedState = UnityEngine.Random.state;
				SaveManager.RequestSave();
			}
			IntroFade();
			_loadMapData.NewGame = false;
			_loadMapData.ContinueGame = false;
		}

		public static void CheckAndFixRandomState()
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			bool flag = true;
			for (int i = 0; i < 200; i++)
			{
				if ((double)UnityEngine.Random.value > 1E-05)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Debug.LogError("Random number generator only giving zeros. Reseting RNG.");
				UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			}
			else
			{
				UnityEngine.Random.state = state;
			}
		}

		private void RemoveVisitedScenariosFromPreviousMaps()
		{
			RemainingNodesSaveData remainingNodesSaveData = (RemainingNodesSaveData)DataSerializer.Load<SaveObjectData>(RemainingNodesSaveData.REMAINING_NODES_SAVE_KEY + VISITED_SCENARIOS_SAVE_KEY, DataSerializer.SaveType.RUN);
			if (remainingNodesSaveData == null)
			{
				_visitedScenarios = new List<MapDataScenario>();
				return;
			}
			_visitedScenarios = FindMapsAndPopulate(_potentialRandomScenarios, remainingNodesSaveData.MapNameList);
			List<MapDataScenario> list = new List<MapDataScenario>();
			foreach (MapDataScenario remainingRandomScenario in _remainingRandomScenarios)
			{
				if (_visitedScenarios.Contains(remainingRandomScenario))
				{
					list.Add(remainingRandomScenario);
				}
			}
			foreach (MapDataScenario item in list)
			{
				_remainingRandomScenarios.Remove(item);
			}
			RemainingNodesSaveData remainingNodesSaveData2 = (RemainingNodesSaveData)DataSerializer.Load<SaveObjectData>(RemainingNodesSaveData.REMAINING_NODES_SAVE_KEY + SEEDED_VISITED_SCENARIOS_SAVE_KEY, DataSerializer.SaveType.RUN);
			_seededVisitedScenarioNames = remainingNodesSaveData2.MapNameList;
		}

		private void RebuildDataFromMapControllerSave(MapControllerSaveData mapControllerSaveData)
		{
			LoadMapControllerData(mapControllerSaveData);
			_relicManager.LoadRelicData();
			ChallengeManager.Instance.LoadChallengeData(_globalChallenges, this, _deckManager, _relicManager, _cruciballManager);
			PlayerHealthController.PlayerStatsSaveData saveData = (PlayerHealthController.PlayerStatsSaveData)DataSerializer.Load<SaveObjectData>(PlayerHealthController.PlayerStatsSaveData.KEY, DataSerializer.SaveType.RUN);
			LoadPlayerData(saveData);
			RebuildScenarioData();
			_deckManager.LoadDeckData();
			_cruciballManager.LoadCruciballData();
			_secretSeedManager.LoadSecretSeedData();
			if (animationControllers != null && animationControllers.Length - 1 >= (int)_cruciballManager.currentClass)
			{
				_player.GetComponent<Animator>().runtimeAnimatorController = animationControllers[(int)_cruciballManager.currentClass];
			}
			else
			{
				Debug.LogWarning("Animator data does not match the class selected in MapController");
			}
			floorCount = mapControllerSaveData.FloorCount;
			StaticGameData.totalFloorCount = mapControllerSaveData.TotalFloorCount;
			OnNodeSelectionEvent?.Invoke(_mapName, floorCount, _cruciballManager.currentCruciballLevel);
		}

		private RunStats LoadRunStats()
		{
			CurrentRunStatsSaveData currentRunStatsSaveData = (CurrentRunStatsSaveData)DataSerializer.Load<SaveObjectData>(CurrentRunStatsSaveData.KEY, DataSerializer.SaveType.RUN);
			if (currentRunStatsSaveData == null)
			{
				Debug.LogWarning("No runstats save data found for run, creating new one from scratch...");
				return new RunStats();
			}
			return new RunStats(currentRunStatsSaveData.runStatsSaveData);
		}

		private void RebuildScenarioData()
		{
			RemainingNodesSaveData remainingNodesSaveData = (RemainingNodesSaveData)DataSerializer.Load<SaveObjectData>(RemainingNodesSaveData.REMAINING_NODES_SAVE_KEY + EASY_BATTLES_SAVE_KEY, DataSerializer.SaveType.RUN);
			RemainingNodesSaveData remainingNodesSaveData2 = (RemainingNodesSaveData)DataSerializer.Load<SaveObjectData>(RemainingNodesSaveData.REMAINING_NODES_SAVE_KEY + RANDOM_BATTLES_SAVE_KEY, DataSerializer.SaveType.RUN);
			RemainingNodesSaveData remainingNodesSaveData3 = (RemainingNodesSaveData)DataSerializer.Load<SaveObjectData>(RemainingNodesSaveData.REMAINING_NODES_SAVE_KEY + SCENARIOS_SAVE_KEY, DataSerializer.SaveType.RUN);
			LoadRemaningScenarios(new RemainingNodesSaveData[3] { remainingNodesSaveData, remainingNodesSaveData2, remainingNodesSaveData3 });
			RemoveVisitedScenariosFromPreviousMaps();
		}

		private void CreateMapDataLists()
		{
			_remainingEasyBattles = new List<MapDataBattle>(_potentialEasyBattles);
			_remainingRandomBattles = new List<MapDataBattle>(_potentialRandomBattles);
			_remainingRandomScenarios = new List<MapDataScenario>(_potentialRandomScenarios);
			_allEasyBattles = new List<MapDataBattle>(_potentialEasyBattles.ToArray());
			_allEasyBattles.Shuffle();
			_easyBattlesRandomQueue = new Queue<MapDataBattle>(_allEasyBattles);
			_allRandomBattles = new List<MapDataBattle>(_potentialRandomBattles.ToArray());
			_allRandomBattles.Shuffle();
			_regularBattlesRandomQueue = new Queue<MapDataBattle>(_allRandomBattles);
			_allRandomScenarios = new List<MapDataScenario>(_potentialRandomScenarios.ToArray());
			_allRandomScenarios.Shuffle();
			_scenariosRandomQueue = new Queue<MapDataScenario>(_allRandomScenarios);
		}

		private void PopulateRemainingMapDataFromSave(List<string> maps, MapDataType type)
		{
			switch (type)
			{
			case MapDataType.EASY_BATTLE:
				_remainingEasyBattles = FindMapsAndPopulate(_potentialEasyBattles, maps);
				break;
			case MapDataType.RANDOM_BATTLE:
				_remainingRandomBattles = FindMapsAndPopulate(_potentialRandomBattles, maps);
				break;
			case MapDataType.RANDOM_SCENARIO:
				_remainingRandomScenarios = FindMapsAndPopulate(_potentialRandomScenarios, maps);
				PrintRemainingScenarios();
				break;
			default:
				Debug.LogError($"Requested populate {type} but we don't have any lists for that");
				break;
			}
		}

		private List<T> FindMapsAndPopulate<T>(List<T> mapDataList, List<string> mapNames) where T : MapData
		{
			List<T> list = new List<T>();
			foreach (string mapName in mapNames)
			{
				foreach (T mapData in mapDataList)
				{
					if (mapData.name == mapName)
					{
						list.Add(mapData);
					}
				}
			}
			return list;
		}

		private List<string> GetMapNames<T>(List<T> mapDataList) where T : MapData
		{
			List<string> list = new List<string>();
			foreach (T mapData in mapDataList)
			{
				list.Add(mapData.name);
			}
			return list;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (!scene.isLoaded)
			{
				return;
			}
			if (scene.name.Contains("Map"))
			{
				inputBlocked = true;
				if (!_firstLoad)
				{
					StaticGameData.loadNextScene = false;
					GetComponent<MapPreviewOpenClose>()?.CloseMapUI();
					if (backgroundData != null)
					{
						OnMapReturnEvent?.Invoke(backgroundIndex);
					}
					Vector3 position = _player.transform.position;
					position.z = Camera.main.transform.position.z;
					Camera.main.transform.position = position;
					_cameraFollowPoint.enabled = true;
					SetUpPreviousAndNextNodes();
					IntroFade();
					SaveManager.RequestSave();
					MapNode[] nodes = _nodes;
					for (int i = 0; i < nodes.Length; i++)
					{
						nodes[i].SaveNode();
					}
					SaveManager.SaveRun();
				}
			}
			else
			{
				base.gameObject.transform.GetChild(0).gameObject.SetActive(value: false);
				_cameraFollowPoint.enabled = false;
			}
			OnNodeSelectionEvent?.Invoke(_mapName, floorCount, _cruciballManager.currentCruciballLevel);
		}

		private void RecordRunStatsOnLoss()
		{
			SaveBestiary();
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.defeatedOnLevel = Act;
				StaticGameData.CurrentRunStats.defeatedOnRoom = floorCount;
			}
		}

		private void SetUpPreviousAndNextNodes()
		{
			MapNode mapNode = null;
			if (_previousNode != null)
			{
				if (StaticGameData.specificNodeOverride == null)
				{
					int num = Mathf.Min(_previousNode.ChildNodes.Length - 1, StaticGameData.chosenNextNodeIndex);
					_nextNode = _previousNode.ChildNodes[num];
				}
				else
				{
					_nextNode = StaticGameData.specificNodeOverride;
				}
				_previousNode.SetActiveState(RoomState.PREVIOUS, recursive: true, setIcon: false);
				_nextNode.SetActiveState(RoomState.NEXT, recursive: true, setIcon: false);
				_nextNode.GenerateMapData();
				mapNode = _nextNode;
			}
			else
			{
				rootNode.GenerateMapData();
				mapNode = rootNode;
			}
			StartInstancingNextNodesObjects(mapNode);
		}

		private void StartInstancingNextNodesObjects(MapNode nextNode)
		{
			MapDataBattle mapDataBattle = nextNode.MapData as MapDataBattle;
			if (!(mapDataBattle == null))
			{
				if (pegLayoutLoader == null)
				{
					pegLayoutLoader = base.gameObject.AddComponent<PegLayoutLoader>();
				}
				float num = preWalkWaitTime + walkTime + postWalkWaitTime + fadeOutTime - 0.2f;
				if (GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>() != null)
				{
					num += fadeInTime;
				}
				StaticGameData.preInstancedPegboardData = pegLayoutLoader.StartPreInstancingPegboard(mapDataBattle.pegLayout.PegLayoutData, mapDataBattle.pegLayout.layoutDataFilename, num);
				PegLayoutLoader obj = pegLayoutLoader;
				obj.OnPegboardPreInstancingFinished = (PegLayoutLoader.PegboardPreInstancingFinished)Delegate.Combine(obj.OnPegboardPreInstancingFinished, new PegLayoutLoader.PegboardPreInstancingFinished(OnPegboardPreInstancingFinished));
			}
		}

		private void OnPegboardPreInstancingFinished(PegLayoutLoader.PreInstancedPegboardData preInstancedPegboardData)
		{
			Debug.Log("Pre Instance Successful");
			PegLayoutLoader obj = pegLayoutLoader;
			obj.OnPegboardPreInstancingFinished = (PegLayoutLoader.PegboardPreInstancingFinished)Delegate.Remove(obj.OnPegboardPreInstancingFinished, new PegLayoutLoader.PegboardPreInstancingFinished(OnPegboardPreInstancingFinished));
		}

		private void IntroFade()
		{
			if (_previousNode != null)
			{
				_player.transform.position = _previousNode.transform.position;
			}
			Image component = GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>();
			if (component == null)
			{
				IntroCameraPan();
				return;
			}
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = component.DOFade(0f, fadeInTime).From(1f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(IntroCameraPan));
		}

		private void IntroCameraPan()
		{
			PauseMenu.PauseBlock = false;
			if (_firstLoad)
			{
				_cameraFollowPoint.enabled = false;
				_cameraYOffset = _cameraFollowPoint.yOffset;
				StartCoroutine(PrePanWait(_firstLoad));
				_firstLoad = false;
			}
			else
			{
				PostFadeInit();
			}
		}

		private IEnumerator PrePanWait(bool isFirstLoad = false)
		{
			float num = prePanWaitTime;
			if (Act == 4 && _previousNode == null && SceneManager.GetActiveScene().name != "DebugMapCore")
			{
				num += 1.5f;
			}
			yield return new WaitForSeconds(num);
			Vector3 position = _player.transform.position;
			position.y = Mathf.Max(bossNode.transform.position.y, position.y + _cameraYOffset);
			position.z = Camera.main.transform.position.z;
			float y = Camera.main.transform.position.y;
			float num2 = _playerInitialPosition.y - y;
			float num3 = position.y - y;
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			float num4 = initialCameraPanTime * Mathf.Max(0.2f, num3 / num2);
			if (Act == 4 && _previousNode == null && SceneManager.GetActiveScene().name != "DebugMapCore")
			{
				num4 += 6f;
			}
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = Camera.main.transform.DOMove(position, num4).SetEase(Ease.InOutSine);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(PostFadeInit));
		}

		private void PostFadeInit()
		{
			if (_previousNode != null)
			{
				if (_previousNode.ChildNodes.Length > 1)
				{
					int index = Mathf.Min(_previousNode.ChildNodes.Length - 1, StaticGameData.chosenNextNodeIndex);
					_previousNode.AnimateNextLine(index, nextLineColorTime);
				}
			}
			else
			{
				_nextNode = rootNode;
			}
			GetComponent<CameraFollowPoint>().enabled = true;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.FIGHT_MINIBOSS_FROM_NEXT_ACT) || ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.FIGHT_EXTRA_BOSS))
			{
				StartCoroutine(StartExtraChallengeNodeSelection());
			}
			else
			{
				StartCoroutine(StartGoblinWalk());
			}
		}

		private IEnumerator StartExtraChallengeNodeSelection()
		{
			yield return new WaitForSeconds(postWalkWaitTime);
			PauseMenu.PauseBlock = true;
			yield return StartCoroutine(DoNodeSelectionFadeOut());
			PauseMenu.PauseBlock = false;
			MapData mapData = null;
			SeededMapNodeData seededNodeData = null;
			RoomType roomType = RoomType.NONE;
			if (ChallengeManager.Instance.challengesOfEachAct[Act] == ChallengeEffect.FIGHT_MINIBOSS_FROM_NEXT_ACT)
			{
				(MapData, SeededMapNodeData) extraChallengeMiniboss = ChallengeManager.Instance.GetExtraChallengeMiniboss(this, _potentialEliteBattles);
				mapData = extraChallengeMiniboss.Item1;
				seededNodeData = extraChallengeMiniboss.Item2;
				roomType = RoomType.MINI_BOSS;
			}
			else if (ChallengeManager.Instance.challengesOfEachAct[Act] == ChallengeEffect.FIGHT_EXTRA_BOSS)
			{
				(MapData, SeededMapNodeData) extraChallengeBoss = ChallengeManager.Instance.GetExtraChallengeBoss(this, extraChallengeFight.potentialMapData);
				mapData = extraChallengeBoss.Item1;
				seededNodeData = extraChallengeBoss.Item2;
				roomType = RoomType.BOSS;
			}
			StaticGameData.battleVictorySceneOverride = thisScene;
			StaticGameData.seededNodeData = seededNodeData;
			LoadSceneFromMapData(mapData);
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.visitedRooms.Enqueue(roomType);
			}
			if (StaticGameData.CurrentRunStats != null && roomType == RoomType.BOSS)
			{
				StaticGameData.CurrentRunStats.visitedBosses.Enqueue(((MapDataBattle)mapData).bossType);
			}
		}

		private IEnumerator StartGoblinWalk()
		{
			yield return new WaitForSeconds(preWalkWaitTime);
			if (StaticGameData.specificNodeOverride != null)
			{
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _player.transform.DOScale(Vector3.zero, 0.3f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(SecretTunnelMovement));
				yield break;
			}
			float num = walkTime;
			if (Act == 4 && _previousNode == null)
			{
				num += 1.5f;
			}
			_player.GetComponent<SpriteRenderer>().flipX = _nextNode.transform.position.x < _player.transform.position.x;
			_player.GetComponent<Animator>().SetBool("moving", value: true);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore2 = _player.transform.DOMove(_nextNode.transform.position, num).SetEase(walkCurve);
			tweenerCore2.onComplete = (TweenCallback)Delegate.Combine(tweenerCore2.onComplete, new TweenCallback(WalkFinished));
		}

		private void SecretTunnelMovement()
		{
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _player.transform.DOMove(_nextNode.transform.position, walkTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(WalkFinished));
		}

		private void WalkFinished()
		{
			if (StaticGameData.specificNodeOverride != null)
			{
				_player.transform.DOScale(_playerDefaultScale, 0.3f);
				StaticGameData.specificNodeOverride = null;
			}
			_nextNode.ActivateNode();
		}

		public void ResolveNode(MapNode node)
		{
			MapDataScenario mapDataScenario = node.MapData as MapDataScenario;
			if (mapDataScenario != null)
			{
				mapDataScenario.Init(_deckManager);
			}
			StaticGameData.seededNodeData = node.seededNodeData;
			StartCoroutine(NodeSelected(node));
		}

		private IEnumerator NodeSelected(MapNode node)
		{
			floorCount++;
			StaticGameData.totalFloorCount++;
			OnNodeSelectionEvent?.Invoke(LocalizationManager.GetTranslation(mapNameLocKey), floorCount, _cruciballManager.currentCruciballLevel);
			if ((node.RoomType == RoomType.UNKNOWN || node.RoomType == RoomType.SCENARIO || node.RoomType == RoomType.PEG_MINIGAME) && _relicManager.AttemptUseRelic(RelicEffect.GAIN_PERCENTAGE_OF_GOLD_EACH_EVENT))
			{
				int num = Mathf.CeilToInt((float)CurrencyManager.Instance.GoldAmount * 0.2f);
				CurrencyManager.Instance.AddGold(num);
				_relicManager.DoCoinsProvideHealingRelicsEffect(num);
			}
			if ((node.RoomType == RoomType.UNKNOWN || node.RoomType == RoomType.SCENARIO || node.RoomType == RoomType.PEG_MINIGAME) && _relicManager.AttemptUseRelic(RelicEffect.MORE_TREASURE_NODES))
			{
				node.MapData = _treasureMapData;
				node.MapDataType = MapDataType.TREASURE;
				node.seededNodeData = _relicManager.GetNextOrbertsStorySeededTreasureData();
				StaticGameData.seededNodeData = node.seededNodeData;
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.MIMICS_ARE_MORE_COMMON))
			{
				float num2 = 1f;
				float num3 = 0f;
				SeededMapNodeData seededNodeData = node.seededNodeData;
				if (!(seededNodeData is SeededTreasureNodeData))
				{
					if (!(seededNodeData is SeededUnknownNodeData))
					{
						if (!(seededNodeData is SeededTextScenarioNodeData))
						{
							if (seededNodeData is SeededPegMinigameNodeData)
							{
								num2 = ((SeededPegMinigameNodeData)node.seededNodeData).mimicChallengeChanceRoll;
								num3 = 0.3f;
							}
						}
						else
						{
							num2 = ((SeededTextScenarioNodeData)node.seededNodeData).mimicChallengeChanceRoll;
							num3 = 0.3f;
						}
					}
					else
					{
						num2 = ((SeededUnknownNodeData)node.seededNodeData).mimicChallengeChanceRoll;
						num3 = 0.3f;
					}
				}
				else
				{
					num2 = ((SeededTreasureNodeData)node.seededNodeData).mimicChallengeChanceRoll;
					num3 = 0.4f;
				}
				if (num2 < num3)
				{
					node.MapData = _mimicBatteData;
					node.MapDataType = MapDataType.ELITE_BATTLE;
					node.seededNodeData = ChallengeManager.Instance.GetNextSeededMimicBattle();
					StaticGameData.seededNodeData = node.seededNodeData;
					StaticGameData.battleVictorySceneOverride = thisScene;
				}
			}
			if (node.TryGetComponent<ChangePotentialMapDataBasedOnCruciball>(out var component) && _cruciballManager.currentCruciballLevel >= component.cruciallLevel)
			{
				node.MapData = component.newMapData;
				node.MapDataType = component.newMapDataType;
				node.seededNodeData = component.seededMapNodeData;
				StaticGameData.seededNodeData = node.seededNodeData;
				StaticGameData.battleVictorySceneOverride = thisScene;
			}
			_player.GetComponent<Animator>().SetBool("moving", value: false);
			yield return new WaitForSeconds(postWalkWaitTime);
			inputBlocked = true;
			PauseMenu.PauseBlock = true;
			yield return StartCoroutine(DoNodeSelectionFadeOut());
			PauseMenu.PauseBlock = false;
			if (_previousNode != null)
			{
				_previousNode.SetActiveState(RoomState.TRAVERSED, recursive: true, setIcon: false);
			}
			_previousNode = node;
			LoadSceneFromMapData(node.MapData);
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.visitedRooms.Enqueue(node.RoomType);
			}
		}

		private IEnumerator DoNodeSelectionFadeOut()
		{
			Image component = GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>();
			Color color = component.color;
			color.a = 0f;
			Color color2 = component.color;
			color2.a = 1f;
			component.color = color;
			component.enabled = true;
			PauseMenu.PauseBlock = true;
			component.DOColor(color2, fadeOutTime);
			yield return new WaitForSeconds(fadeOutTime);
		}

		public MapData GetMapDataByName(string mapDataName, MapDataType type)
		{
			MapData mapData = null;
			switch (type)
			{
			case MapDataType.EASY_BATTLE:
				mapData = _potentialEasyBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				if (mapData != null && !mapData.ignoreRemove)
				{
					_potentialEasyBattles.Remove(mapData as MapDataBattle);
				}
				break;
			case MapDataType.ELITE_BATTLE:
				mapData = _potentialEliteBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				if (mapData == null && mapDataName == _mimicBatteData.name)
				{
					mapData = _mimicBatteData;
				}
				break;
			case MapDataType.RANDOM_BATTLE:
				mapData = _potentialRandomBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				if (mapData != null && !mapData.ignoreRemove)
				{
					_potentialRandomBattles.Remove(mapData as MapDataBattle);
				}
				break;
			case MapDataType.RANDOM_SCENARIO:
				mapData = _potentialRandomScenarios.Find((MapDataScenario x) => x.name.Equals(mapDataName));
				if (mapData != null && (!allowRepeatScenarios || !mapData.ignoreRemove))
				{
					_potentialRandomScenarios.Remove(mapData as MapDataScenario);
				}
				break;
			case MapDataType.TREASURE:
				_treasureMapData.background = GetBackgroundNoTick();
				mapData = _treasureMapData;
				break;
			case MapDataType.STORE:
				_shopMapData.background = GetBackgroundNoTick();
				mapData = _shopMapData;
				break;
			case MapDataType.PEG_MINIGAME:
				mapData = _potentialPegMinigameScenarios.Find((MapDataPegMinigame x) => x.name.Equals(mapDataName));
				break;
			default:
				Debug.LogError("Unknown MapDataType when loading data!");
				break;
			}
			if (mapData == null)
			{
				Debug.LogError("Did not find MapData by name: " + mapDataName);
			}
			return mapData;
		}

		public MapData LoadMapDataByNameFromAllData(string mapDataName, MapDataType type)
		{
			MapData mapData = null;
			switch (type)
			{
			case MapDataType.EASY_BATTLE:
				mapData = _allEasyBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				break;
			case MapDataType.ELITE_BATTLE:
				mapData = _potentialEliteBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				if (mapData == null && mapDataName == _mimicBatteData.name)
				{
					mapData = _mimicBatteData;
				}
				break;
			case MapDataType.RANDOM_BATTLE:
				mapData = _allRandomBattles.Find((MapDataBattle x) => x.name.Equals(mapDataName));
				break;
			case MapDataType.RANDOM_SCENARIO:
				mapData = _allRandomScenarios.Find((MapDataScenario x) => x.name.Equals(mapDataName));
				break;
			}
			if (mapData == null)
			{
				Debug.LogError("Did not find MapData by name: " + mapDataName);
			}
			return mapData;
		}

		public void AssignTreasureDataToStatic()
		{
			_treasureMapData.background = GetBackgroundNoTick();
			_treasureMapData.pegboardFrame = scenarioPegboardFrame.GetFromLengthSafeIndex(backgroundIndex);
			StaticGameData.dataToLoad = _treasureMapData;
		}

		private (MapData, MapDataType) AttemptGetTextScenario(MapNode node)
		{
			MapData mapData = null;
			SeededTextScenarioNodeData seededTextScenarioNodeData = node.seededNodeData as SeededTextScenarioNodeData;
			if (seededTextScenarioNodeData == null && node.seededNodeData is SeededUnknownNodeData seededUnknownNodeData)
			{
				seededTextScenarioNodeData = seededUnknownNodeData.textScenerioData;
			}
			if (seededTextScenarioNodeData != null)
			{
				mapData = CheckAndUpdateSeededScenario(seededTextScenarioNodeData.textScenario, node);
			}
			else
			{
				int num = 0;
				int num2 = UnityEngine.Random.Range(0, _remainingRandomScenarios.Count);
				while (_remainingRandomScenarios.Count > num)
				{
					mapData = _remainingRandomScenarios[num2];
					MapDataScenario mapDataScenario = mapData as MapDataScenario;
					mapDataScenario.background = GetBackgroundNoTick();
					if (mapDataScenario != null && mapDataScenario.ValidatePreReqs(this, _deckManager, _relicManager, node, _playerHealth.Value, _playerMaxHealth.Value))
					{
						mapDataScenario.Init(_deckManager);
						if (!allowRepeatScenarios)
						{
							_remainingRandomScenarios.Remove(mapDataScenario);
							_visitedScenarios.Add(mapDataScenario);
						}
						Debug.Log("Adding " + mapDataScenario.name + " to visited scenarios");
						PrintRemainingScenarios();
						break;
					}
					Debug.Log("Prerequisite missed for " + mapDataScenario.name);
					mapData = null;
					num2++;
					num++;
					if (num2 >= _remainingRandomScenarios.Count)
					{
						num2 = 0;
					}
				}
			}
			return (mapData, MapDataType.RANDOM_SCENARIO);
		}

		public void CheckDebugScenerioPreReqs(MapNode node, MapDataScenario debugScenario)
		{
			if (debugScenario != null && !debugScenario.ValidatePreReqs(this, _deckManager, _relicManager, node, _playerHealth.Value, _playerMaxHealth.Value))
			{
				Debug.LogWarning($"Debug prerequisite failed for {debugScenario.name} on {node}");
			}
		}

		private (MapData, MapDataType) GetPegMinigameScenario(MapNode node)
		{
			int index = ((!(node.seededNodeData is SeededPegMinigameNodeData seededPegMinigameNodeData)) ? UnityEngine.Random.Range(0, _potentialPegMinigameScenarios.Count) : ListExtensions.ConvertPercentageToListIndex(seededPegMinigameNodeData.pegMinigameChoiceRoll, _potentialPegMinigameScenarios.Count));
			MapDataPegMinigame mapDataPegMinigame = _potentialPegMinigameScenarios[index];
			mapDataPegMinigame.background = GetBackgroundNoTick();
			mapDataPegMinigame.bouncerPrefab = backgroundData[backgroundIndex].BouncerPrefab;
			return (mapDataPegMinigame, MapDataType.PEG_MINIGAME);
		}

		public void GenerateRandomMapData(MapNode node)
		{
			bool flag = node.seededNodeData != null;
			MapData mapData;
			MapDataType mapDataType;
			switch (node.RoomType)
			{
			case RoomType.BATTLE:
			{
				SeededBattleNodeData seededBattleData = null;
				if (flag)
				{
					seededBattleData = node.seededNodeData as SeededBattleNodeData;
				}
				(mapData, mapDataType) = ChooseRandomBattle(seededBattleData);
				break;
			}
			case RoomType.MINI_BOSS:
			{
				SeededBattleNodeData seededBattleNodeData = null;
				if (flag)
				{
					seededBattleNodeData = node.seededNodeData as SeededBattleNodeData;
				}
				(mapData, mapDataType) = ChooseRandomMiniboss(seededBattleNodeData?.minibossBattle);
				break;
			}
			case RoomType.SCENARIO:
				(mapData, mapDataType) = AttemptGetTextScenario(node);
				if (mapData == null)
				{
					SeededTextScenarioNodeData seededTextScenarioNodeData = node.seededNodeData as SeededTextScenarioNodeData;
					SeededUnknownNodeData seededUnknownNodeData2 = null;
					if (seededTextScenarioNodeData != null)
					{
						seededUnknownNodeData2 = seededTextScenarioNodeData.unknownNodeData;
					}
					(MapData, MapDataType, SeededMapNodeData) randomScenario2 = GetRandomScenario(node, seededUnknownNodeData2);
					mapData = randomScenario2.Item1;
					mapDataType = randomScenario2.Item2;
					SeededMapNodeData item2 = randomScenario2.Item3;
					node.seededNodeData = item2;
				}
				break;
			case RoomType.TREASURE:
				_treasureMapData.background = GetBackgroundNoTick();
				mapData = _treasureMapData;
				mapDataType = MapDataType.TREASURE;
				break;
			case RoomType.STORE:
				_shopMapData.background = GetBackgroundNoTick();
				mapData = _shopMapData;
				mapDataType = MapDataType.STORE;
				break;
			case RoomType.PEG_MINIGAME:
				(mapData, mapDataType) = GetPegMinigameScenario(node);
				break;
			default:
			{
				SeededUnknownNodeData seededUnknownNodeData = node.seededNodeData as SeededUnknownNodeData;
				(MapData, MapDataType, SeededMapNodeData) randomScenario = GetRandomScenario(node, seededUnknownNodeData);
				mapData = randomScenario.Item1;
				mapDataType = randomScenario.Item2;
				SeededMapNodeData item = randomScenario.Item3;
				node.seededNodeData = item;
				break;
			}
			}
			if (mapData == null || mapDataType == MapDataType.UNKNOWN)
			{
				Debug.LogError("Attempted to generate Unknown or bad map data type");
			}
			node.MapData = mapData;
			node.MapDataType = mapDataType;
		}

		private (MapData, MapDataType, SeededMapNodeData) GetRandomScenario(MapNode node, SeededUnknownNodeData seededUnknownNodeData)
		{
			bool flag = seededUnknownNodeData != null;
			float num = 0.05f;
			float num2 = 0.56f;
			float num3 = (node.canBeMiniboss ? _cruciballManager.GetUnknownMapEliteChance() : 0f);
			num2 -= num3 / 2f;
			float num4 = ((!flag) ? UnityEngine.Random.value : seededUnknownNodeData.typeRoll);
			if (num4 < num)
			{
				_treasureMapData.background = GetBackgroundNoTick();
				SeededTreasureNodeData item = null;
				if (flag)
				{
					item = seededUnknownNodeData.treasureData;
				}
				return (_treasureMapData, MapDataType.TREASURE, item);
			}
			if (num4 < num2 + num && _remainingRandomScenarios.Count > 0)
			{
				var (mapData, item2) = AttemptGetTextScenario(node);
				if (mapData != null)
				{
					SeededMapNodeData item3 = null;
					if (flag)
					{
						item3 = seededUnknownNodeData.textScenerioData;
					}
					return (mapData, item2, item3);
				}
				num2 = 0f;
			}
			(MapDataBattle, MapDataType) tuple2;
			if (num4 < num3 + num2 + num)
			{
				MapDataBattle seededMinibossChoice = null;
				SeededMapNodeData item4 = null;
				if (flag)
				{
					seededMinibossChoice = seededUnknownNodeData.minibossData.minibossBattle;
					item4 = seededUnknownNodeData.minibossData;
				}
				tuple2 = ChooseRandomMiniboss(seededMinibossChoice);
				(MapData, MapDataType) tuple3 = (tuple2.Item1, tuple2.Item2);
				return (tuple3.Item1, tuple3.Item2, item4);
			}
			SeededBattleNodeData seededBattleNodeData = null;
			if (flag)
			{
				seededBattleNodeData = seededUnknownNodeData.battleData;
			}
			tuple2 = ChooseRandomBattle(seededBattleNodeData);
			(MapData, MapDataType) tuple4 = (tuple2.Item1, tuple2.Item2);
			return (tuple4.Item1, tuple4.Item2, seededBattleNodeData);
		}

		private (MapDataBattle, MapDataType) ChooseRandomBattle(SeededBattleNodeData seededBattleData)
		{
			MapDataBattle mapDataBattle = null;
			if (_numEasyBattles < 2 && _remainingEasyBattles.Count > 0)
			{
				_numEasyBattles++;
				mapDataBattle = ((seededBattleData == null) ? _remainingEasyBattles[UnityEngine.Random.Range(0, _remainingEasyBattles.Count)] : CheckAndUpdateSeededBattle(seededBattleData.easyBattle, MapDataType.EASY_BATTLE));
				if (!mapDataBattle.ignoreRemove)
				{
					_remainingEasyBattles.Remove(mapDataBattle);
				}
				mapDataBattle.background = GetBackground();
				return (mapDataBattle, MapDataType.EASY_BATTLE);
			}
			if (_remainingRandomBattles.Count > 0)
			{
				mapDataBattle = ((seededBattleData == null) ? _remainingRandomBattles[UnityEngine.Random.Range(0, _remainingRandomBattles.Count)] : CheckAndUpdateSeededBattle(seededBattleData.regularBattle, MapDataType.RANDOM_BATTLE));
				if (!mapDataBattle.ignoreRemove)
				{
					_remainingRandomBattles.Remove(mapDataBattle);
				}
				mapDataBattle.background = GetBackground();
				return (mapDataBattle, MapDataType.RANDOM_BATTLE);
			}
			if (seededBattleData != null)
			{
				return ChooseRandomMiniboss(seededBattleData.minibossBattle);
			}
			return ChooseRandomMiniboss(null);
		}

		private (MapDataBattle, MapDataType) ChooseRandomMiniboss(MapDataBattle seededMinibossChoice)
		{
			MapDataBattle mapDataBattle = ((!(seededMinibossChoice != null)) ? _potentialEliteBattles[UnityEngine.Random.Range(0, _potentialEliteBattles.Count)] : CheckAndUpdateSeededBattle(seededMinibossChoice, MapDataType.ELITE_BATTLE));
			mapDataBattle.background = GetBackground();
			return (mapDataBattle, MapDataType.ELITE_BATTLE);
		}

		public MapDataScenario GetSeededRandomScenario()
		{
			return _allRandomScenarios[UnityEngine.Random.Range(0, _allRandomScenarios.Count)];
		}

		private MapDataScenario CheckAndUpdateSeededScenario(MapDataScenario scenario, MapNode node)
		{
			MapDataScenario mapDataScenario = scenario;
			for (int i = 0; i < _scenariosRandomQueue.Count; i++)
			{
				if (mapDataScenario != null && mapDataScenario.ValidatePreReqs(this, _deckManager, _relicManager, node, _playerHealth.Value, _playerMaxHealth.Value) && (allowRepeatScenarios || !_seededVisitedScenarioNames.Contains(mapDataScenario.name)))
				{
					break;
				}
				mapDataScenario = _scenariosRandomQueue.Dequeue();
				_scenariosRandomQueue.Enqueue(mapDataScenario);
			}
			if (!(mapDataScenario != null) || !mapDataScenario.ValidatePreReqs(this, _deckManager, _relicManager, node, _playerHealth.Value, _playerMaxHealth.Value) || (!allowRepeatScenarios && _seededVisitedScenarioNames.Contains(mapDataScenario.name)))
			{
				return null;
			}
			_seededVisitedScenarioNames.Add(mapDataScenario.name);
			return mapDataScenario;
		}

		public MapDataBattle GetSeededRandomBattleOfType(MapDataType battleType, bool allowMimic = false)
		{
			List<MapDataBattle> list;
			switch (battleType)
			{
			case MapDataType.EASY_BATTLE:
				list = _allEasyBattles;
				break;
			case MapDataType.RANDOM_BATTLE:
				list = _allRandomBattles;
				break;
			case MapDataType.ELITE_BATTLE:
				list = new List<MapDataBattle>(_potentialEliteBattles.ToArray());
				if (allowMimic)
				{
					list.Add(_mimicBatteData);
				}
				break;
			default:
				Debug.LogError("MapController.GetSeededRandomBattleOfType() called with invalid MapDataType: " + battleType);
				list = new List<MapDataBattle>();
				break;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		private MapDataBattle CheckAndUpdateSeededBattle(MapDataBattle battle, MapDataType battleType)
		{
			List<MapDataBattle> remainingBattlesList;
			Queue<MapDataBattle> queue;
			switch (battleType)
			{
			case MapDataType.EASY_BATTLE:
				remainingBattlesList = _remainingEasyBattles;
				queue = _easyBattlesRandomQueue;
				break;
			case MapDataType.RANDOM_BATTLE:
				remainingBattlesList = _remainingRandomBattles;
				queue = _regularBattlesRandomQueue;
				break;
			case MapDataType.ELITE_BATTLE:
				return battle;
			case MapDataType.BOSS:
				return battle;
			default:
				Debug.LogError("MapController.GetSeededRandomBattleOfType() called with invalid MapDataType: " + battleType);
				remainingBattlesList = new List<MapDataBattle>();
				queue = new Queue<MapDataBattle>();
				break;
			}
			MapDataBattle mapDataBattle = battle;
			for (int i = 0; i < queue.Count; i++)
			{
				if (CheckIfBattleValid(mapDataBattle, remainingBattlesList))
				{
					break;
				}
				mapDataBattle = queue.Dequeue();
				queue.Enqueue(mapDataBattle);
			}
			if (!CheckIfBattleValid(mapDataBattle, remainingBattlesList))
			{
				Debug.LogError("Something has gone wrong when updating a seeded battle.");
			}
			return mapDataBattle;
		}

		private bool CheckIfBattleValid(MapDataBattle battle, List<MapDataBattle> remainingBattlesList)
		{
			return remainingBattlesList.Contains(battle);
		}

		public void LoadBattleFromScenario(bool elite = false)
		{
			SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
			if (!elite)
			{
				seededBattleNodeData.GenerateData(_deckManager, _relicManager, this);
			}
			else
			{
				seededBattleNodeData.GenerateMinibossData(_deckManager, _relicManager, this, allowMimic: false);
			}
			MapDataBattle data;
			if (elite)
			{
				(data, _) = ChooseRandomMiniboss(seededBattleNodeData.minibossBattle);
			}
			else
			{
				(data, _) = ChooseRandomBattle(seededBattleNodeData);
			}
			StaticGameData.battleVictorySceneOverride = thisScene;
			StaticGameData.seededNodeData = seededBattleNodeData;
			LoadSceneFromMapData(data);
		}

		public void LoadBossBattleFromScenario()
		{
			SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
			seededBattleNodeData.GenerateBossData(_deckManager, _relicManager, this);
			ScenarioBossFightData component = GetComponent<ScenarioBossFightData>();
			if (component == null)
			{
				Debug.LogWarning("No ScenarioBossFightData found on MapController when requested by a Scenario.");
				return;
			}
			StaticGameData.battleVictorySceneOverride = thisScene;
			StaticGameData.relicRarityOverride = RelicRarity.RARE;
			StaticGameData.seededNodeData = seededBattleNodeData;
			LoadSceneFromMapData(component.availableBossFights[UnityEngine.Random.Range(0, component.availableBossFights.Count)]);
		}

		public void LoadSpecificBattleFromScenario(MapDataBattle battle)
		{
			SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
			seededBattleNodeData.GenerateData(_deckManager, _relicManager, this);
			StaticGameData.battleVictorySceneOverride = thisScene;
			StaticGameData.seededNodeData = seededBattleNodeData;
			LoadSceneFromMapData(battle);
		}

		public void LoadSceneFromMapData(MapData data)
		{
			StaticGameData.currentNode = _previousNode;
			StaticGameData.dataToLoad = data;
			StaticGameData.activeMapScene = thisScene;
			MapData dataToLoad = StaticGameData.dataToLoad;
			if (dataToLoad is MapDataBattle)
			{
				dataToLoad.background = GetBackground();
				dataToLoad.pegboardFrame = battlePegboardFrame.GetFromLengthSafeIndex(backgroundIndex);
			}
			else if (dataToLoad is MapDataPegMinigame)
			{
				dataToLoad.background = GetBackground();
				dataToLoad.pegboardFrame = pegMinigamePegboardFrame.GetFromLengthSafeIndex(backgroundIndex);
			}
			else
			{
				dataToLoad.background = GetBackground();
				dataToLoad.pegboardFrame = scenarioPegboardFrame.GetFromLengthSafeIndex(backgroundIndex);
			}
			if (StaticGameData.currentNode != null && StaticGameData.currentNode.RoomType == RoomType.BOSS)
			{
				StaticGameData.hasReachedBoss = true;
				int num = PlayerPrefs.GetInt(GameInit.BOSS_STREAK_PREF_STRING, 0);
				num++;
				PlayerPrefs.SetInt(GameInit.BOSS_STREAK_PREF_STRING, num);
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.visitedBosses.Enqueue(((MapDataBattle)data).bossType);
				}
			}
			if (data == null)
			{
				Debug.LogWarning("Data is null before load, something is wrong.");
				data = ChooseRandomBattle(null).Item1;
			}
			string sceneOverride = data.GetSceneOverride();
			if (sceneOverride != null && sceneOverride != "")
			{
				PeglinSceneLoader.Instance.LoadScene(battleSceneName);
				return;
			}
			switch (data.GetSceneType())
			{
			case SceneType.BATTLE:
				PeglinSceneLoader.Instance.LoadScene(battleSceneName);
				break;
			case SceneType.SCENARIO:
				PeglinSceneLoader.Instance.LoadScene(textScenarioName);
				break;
			case SceneType.TREASURE:
				PeglinSceneLoader.Instance.LoadScene(treasureScenarioName);
				break;
			case SceneType.PEG_MINIGAME:
				PeglinSceneLoader.Instance.LoadScene(pegMinigameScenarioName);
				break;
			case SceneType.STORE:
				PeglinSceneLoader.Instance.LoadScene(shopScenarioName);
				break;
			default:
				Debug.LogWarning("MAPDATA SCENETYPE NOT MAPPED IN MAP CONTROLLER! " + data.GetSceneType());
				break;
			}
		}

		private GameObject GetBackground()
		{
			if (backgroundData == null)
			{
				return null;
			}
			int lengthOfRooms = backgroundData[backgroundIndex].lengthOfRooms;
			if (lengthOfRooms > 0 && currentRoomCount >= lengthOfRooms && backgroundIndex < backgroundData.Length - 1)
			{
				currentRoomCount = 0;
				backgroundIndex++;
			}
			else
			{
				currentRoomCount++;
			}
			return backgroundData[backgroundIndex].Background;
		}

		private GameObject GetBackgroundNoTick()
		{
			if (backgroundData == null)
			{
				return null;
			}
			return backgroundData[backgroundIndex].Background;
		}

		private void SaveData()
		{
			SaveMapControllerData();
			SavePlayerData();
			SaveRemainingNodesData();
			SaveCurrentRunStats();
			SaveBestiary();
			_deckManager.SaveDeckData();
			_relicManager.SaveRelicData();
			_cruciballManager.SaveCruciballData();
			_secretSeedManager.SaveSecretSeedData();
			ChallengeManager.Instance.SaveChallengeData();
			PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
			SaveManager.SaveRun();
		}

		private void SaveMapControllerData()
		{
			string previousNodeName = ((!(_previousNode != null)) ? MapControllerSaveData.STARTING_NODE : _previousNode.gameObject.name);
			string secretTunnelOverrideNodeName = null;
			if (StaticGameData.specificNodeOverride != null)
			{
				secretTunnelOverrideNodeName = StaticGameData.specificNodeOverride.name;
			}
			string[] mapDataQueueSaveData = GetMapDataQueueSaveData(_easyBattlesRandomQueue);
			string[] mapDataQueueSaveData2 = GetMapDataQueueSaveData(_regularBattlesRandomQueue);
			string[] mapDataQueueSaveData3 = GetMapDataQueueSaveData(_scenariosRandomQueue);
			List<SeededMapNodeSaveData> list = new List<SeededMapNodeSaveData>();
			SeededMapNodeData[] nodeChangesFromCruciballSeededData = _nodeChangesFromCruciballSeededData;
			foreach (SeededMapNodeData seededMapNodeData in nodeChangesFromCruciballSeededData)
			{
				list.Add(seededMapNodeData.GetSaveData());
			}
			new MapControllerSaveData((int)thisScene, previousNodeName, StaticGameData.chosenNextNodeIndex, secretTunnelOverrideNodeName, backgroundIndex, currentRoomCount, ChestScenarioController.bombChestsOpened, mimicsBattledThisAct, _numEasyBattles, AchievementManager.AchievementsOn, floorCount, CurrencyManager.Instance.GoldAmount, StaticGameData.totalFloorCount, StaticGameData.StoreNumberOfOrbsRemoved, mapDataQueueSaveData, mapDataQueueSaveData2, mapDataQueueSaveData3, StaticGameData.currentSeed, StaticGameData.nextMapSeedState, list.ToArray()).Save();
		}

		private void SavePlayerData()
		{
			new PlayerHealthController.PlayerStatsSaveData(_playerHealth.Value, _playerMaxHealth.Value, PlayerHealthController.DamageEvasionRandomState, _reductionToMaxHealth.Value).Save();
		}

		private void SaveRemainingNodesData()
		{
			RemainingNodesSaveData remainingNodesSaveData = new RemainingNodesSaveData(EASY_BATTLES_SAVE_KEY, GetMapNames(_remainingEasyBattles), MapDataType.EASY_BATTLE);
			RemainingNodesSaveData remainingNodesSaveData2 = new RemainingNodesSaveData(RANDOM_BATTLES_SAVE_KEY, GetMapNames(_remainingRandomBattles), MapDataType.RANDOM_BATTLE);
			RemainingNodesSaveData remainingNodesSaveData3 = new RemainingNodesSaveData(SCENARIOS_SAVE_KEY, GetMapNames(_remainingRandomScenarios), MapDataType.RANDOM_SCENARIO);
			RemainingNodesSaveData remainingNodesSaveData4 = new RemainingNodesSaveData(VISITED_SCENARIOS_SAVE_KEY, GetMapNames(_visitedScenarios), MapDataType.RANDOM_SCENARIO);
			RemainingNodesSaveData remainingNodesSaveData5 = new RemainingNodesSaveData(SEEDED_VISITED_SCENARIOS_SAVE_KEY, _seededVisitedScenarioNames, MapDataType.RANDOM_SCENARIO);
			remainingNodesSaveData.Save();
			remainingNodesSaveData2.Save();
			remainingNodesSaveData3.Save();
			remainingNodesSaveData4.Save();
			remainingNodesSaveData5.Save();
		}

		private string[] GetMapDataQueueSaveData<T>(Queue<T> mapDataQueue) where T : MapData
		{
			List<T> list = new List<T>();
			for (int i = 0; i < mapDataQueue.Count; i++)
			{
				T item = mapDataQueue.Dequeue();
				mapDataQueue.Enqueue(item);
				list.Add(item);
			}
			return GetMapNames(list).ToArray();
		}

		private void SaveCurrentRunStats()
		{
			if (StaticGameData.CurrentRunStats != null)
			{
				new CurrentRunStatsSaveData(StaticGameData.CurrentRunStats).Save();
			}
		}

		private void SaveBestiary()
		{
			Debug.Log("Updating bestiary with battle results");
			foreach (KeyValuePair<string, int> item in _enemiesKilledDuringCombat)
			{
				if (AssetLoading.Instance.EnemyDataTable.TryGetValue(item.Key, out var value))
				{
					value.Killed += item.Value;
				}
				else
				{
					Debug.LogWarning("Did not find enemy for " + item.Key + " in Enemy Data Table");
				}
			}
			foreach (KeyValuePair<string, int> item2 in _enemiesWithCardDropDuringCombat)
			{
				if (AssetLoading.Instance.EnemyDataTable.TryGetValue(item2.Key, out var value2))
				{
					value2.CardDrop += item2.Value;
				}
				else
				{
					Debug.LogWarning("Did not find enemy for " + item2.Key + " in Enemy Data Table");
				}
			}
			foreach (KeyValuePair<string, int> item3 in _enemiesWithLootDropDuringCombat)
			{
				if (AssetLoading.Instance.EnemyDataTable.TryGetValue(item3.Key, out var value3))
				{
					value3.LootDrop += item3.Value;
				}
				else
				{
					Debug.LogWarning("Did not find enemy for " + item3.Key + " in Enemy Data Table");
				}
			}
			_enemiesKilledDuringCombat.Clear();
			_enemiesWithCardDropDuringCombat.Clear();
			_enemiesWithLootDropDuringCombat.Clear();
			PersistentPlayerData.Instance.SaveBestiaryData();
		}

		private void LoadMapControllerData(MapControllerSaveData data)
		{
			if (data.PreviousNodeName == MapControllerSaveData.STARTING_NODE)
			{
				rootNode.SetActiveState(RoomState.NEXT, recursive: true, setIcon: false);
				_firstLoad = true;
			}
			else
			{
				MapNode mapNode = GameObject.Find(data.PreviousNodeName)?.GetComponent<MapNode>();
				if (mapNode == null)
				{
					Debug.LogError("We've tried to load a node called " + data.PreviousNodeName + " and failed. Starting from the top...");
				}
				else
				{
					_previousNode = mapNode;
					StaticGameData.chosenNextNodeIndex = data.ChosenNextNodeIndex;
					if (data.SecretTunnelOverrideNodeName != null)
					{
						StaticGameData.specificNodeOverride = GetNodeByName(data.SecretTunnelOverrideNodeName);
					}
				}
			}
			backgroundIndex = data.BackgroundIndex;
			currentRoomCount = data.CurrentRoomCount;
			ChestScenarioController.bombChestsOpened = data.BombChestsOpened;
			StaticGameData.mimicsBattled = (mimicsBattledThisAct = data.MimicsBattled);
			_numEasyBattles = data.NumEasyBattles;
			AchievementManager.AchievementsOn = data.AchievementsOn;
			floorCount = data.FloorCount;
			CurrencyManager.Instance.ClearGold();
			CurrencyManager.Instance.AddGold(data.GoldAmount, silent: true);
			StaticGameData.StoreNumberOfOrbsRemoved = data.ShopOrbRemovedCount;
			StaticGameData.currentSeed = data.Seed;
			if (data.NodeChangesFromCruciballSeededData != null)
			{
				List<SeededMapNodeData> list = new List<SeededMapNodeData>();
				SeededMapNodeSaveData[] nodeChangesFromCruciballSeededData = data.NodeChangesFromCruciballSeededData;
				foreach (SeededMapNodeSaveData data2 in nodeChangesFromCruciballSeededData)
				{
					SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
					seededBattleNodeData.LoadSaveData(data2, _deckManager, _relicManager, this);
					list.Add(seededBattleNodeData);
				}
				_nodeChangesFromCruciballSeededData = list.ToArray();
				SetChangeFromCruciballNodeSeededData();
			}
			else
			{
				_nodeChangesFromCruciballSeededData = new SeededMapNodeData[0];
			}
			_allEasyBattles = new List<MapDataBattle>(_potentialEasyBattles.ToArray());
			_allRandomBattles = new List<MapDataBattle>(_potentialRandomBattles.ToArray());
			_allRandomScenarios = new List<MapDataScenario>(_potentialRandomScenarios.ToArray());
			_easyBattlesRandomQueue.Clear();
			_regularBattlesRandomQueue.Clear();
			_scenariosRandomQueue.Clear();
			int j;
			for (j = 0; j < data.EasyBattleNamesQueue.Length; j++)
			{
				_easyBattlesRandomQueue.Enqueue(_potentialEasyBattles.Find((MapDataBattle x) => x.name == data.EasyBattleNamesQueue[j]));
			}
			int i2;
			for (i2 = 0; i2 < data.RegularBattleNamesQueue.Length; i2++)
			{
				_regularBattlesRandomQueue.Enqueue(_potentialRandomBattles.Find((MapDataBattle x) => x.name == data.RegularBattleNamesQueue[i2]));
			}
			int i3;
			for (i3 = 0; i3 < data.ScenarioNamesQueue.Length; i3++)
			{
				_scenariosRandomQueue.Enqueue(_potentialRandomScenarios.Find((MapDataScenario x) => x.name == data.ScenarioNamesQueue[i3]));
			}
		}

		private void SetChangeFromCruciballNodeSeededData()
		{
			if (_nodeChangesFromCruciballSeededData.Length == 0)
			{
				return;
			}
			int num = 0;
			MapNode[] nodes = _nodes;
			foreach (MapNode mapNode in nodes)
			{
				if (num >= _nodeChangesFromCruciballSeededData.Length)
				{
					break;
				}
				if (mapNode.TryGetComponent<ChangePotentialMapDataBasedOnCruciball>(out var component))
				{
					if (component.newMapData.name == "MimicMinibossMapData")
					{
						component.SetSeededData(_nodeChangesFromCruciballSeededData[num]);
						num++;
					}
					else
					{
						Debug.LogError("ChangePotentialMapDataBasedOnCruciball not implemented for anything other than a mimic in the core chest.");
					}
				}
			}
		}

		private void LoadRemaningScenarios(RemainingNodesSaveData[] saveData)
		{
			foreach (RemainingNodesSaveData remainingNodesSaveData in saveData)
			{
				PopulateRemainingMapDataFromSave(remainingNodesSaveData.MapNameList, remainingNodesSaveData.MapType);
			}
		}

		private void LoadPlayerData(PlayerHealthController.PlayerStatsSaveData saveData)
		{
			if (saveData != null)
			{
				_playerMaxHealth.Set(saveData.PlayerMaxHealth);
				_playerHealth.Set(saveData.PlayerHealth);
				_reductionToMaxHealth.Set(saveData.ReductionToPlayerMaxHealth);
				if (saveData.DamageEvasionRandomState != null && saveData.DamageEvasionRandomState != null)
				{
					PlayerHealthController.DamageEvasionRandomState = new SerializableRandomState(saveData.DamageEvasionRandomState.initializationSeed, saveData.DamageEvasionRandomState.timesUsed);
				}
				else
				{
					PlayerHealthController.DamageEvasionRandomState = new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0);
				}
			}
			else
			{
				Debug.LogWarning("Attempted to load player data and it was missing from our saves");
			}
		}

		public void ProceedAfterBattle()
		{
			if (StaticGameData.loadNextScene)
			{
				SaveBestiary();
				if (nextScene == PeglinSceneLoader.Scene.MINES_WIN || nextScene == PeglinSceneLoader.Scene.CORE_MAP)
				{
					_cruciballManager.VictoryAchieved();
					if (StaticGameData.CurrentRunStats != null)
					{
						StaticGameData.CurrentRunStats.hasWon = true;
						StaticGameData.CurrentRunStats.finalHp = Mathf.RoundToInt(_playerHealth.Value);
						StaticGameData.CurrentRunStats.maxHp = Mathf.RoundToInt(_playerMaxHealth.Value);
						StaticGameData.CurrentRunStats.endDate = DateTime.Now;
					}
					CheckForUnlockableRelics();
					PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true, validateBackup: true);
				}
				else if (nextScene == PeglinSceneLoader.Scene.CORE_WIN)
				{
					if (StaticGameData.CurrentRunStats != null)
					{
						StaticGameData.CurrentRunStats.hasWonCore = true;
						StaticGameData.CurrentRunStats.finalHp = Mathf.RoundToInt(_playerHealth.Value);
						StaticGameData.CurrentRunStats.maxHp = Mathf.RoundToInt(_playerMaxHealth.Value);
						StaticGameData.CurrentRunStats.endDate = DateTime.Now;
					}
					PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true, validateBackup: true);
				}
				PeglinSceneLoader.Instance.LoadScene(nextScene);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (StaticGameData.battleVictorySceneOverride != PeglinSceneLoader.Scene.NONE)
			{
				PeglinSceneLoader.Instance.LoadScene(StaticGameData.battleVictorySceneOverride);
			}
			else
			{
				Debug.LogError("We reached a spot we weren't planning to on the MapController scene loading logic!");
				_cruciballManager.VictoryAchieved();
				PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MINES_WIN);
				StaticGameData.dataToLoad = null;
				CheckForUnlockableRelics();
				PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
			}
		}

		private void CheckForUnlockableRelics()
		{
			if (AchievementManager.AchievementsOn)
			{
				if (!PersistentPlayerData.Instance.UnlockedRelics.Contains(RelicEffect.CRIT_PIT))
				{
					PersistentPlayerData.Instance.UnlockedRelics.Add(RelicEffect.CRIT_PIT);
				}
				foreach (RelicEffect key in RelicManager.unlockableRelicsOnBeatingTheGame.Keys)
				{
					if (_relicManager.RelicEffectActive(key))
					{
						PersistentPlayerData.Instance.SetRelicUnlocked(RelicManager.unlockableRelicsOnBeatingTheGame[key]);
					}
				}
				if (_relicManager.RelicEffectActive(RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS))
				{
					PersistentPlayerData.Instance.SetRelicUnlocked(RelicEffect.TARGETED_ATTACKS_HIT_ALL);
				}
				if (_deckManager.IsOrbInDeck("destroy_peg_orb"))
				{
					PersistentPlayerData.Instance.SetRelicUnlocked(RelicEffect.ALL_ORBS_DELETE_PEGS);
				}
			}
			foreach (RelicEffect key2 in RelicManager.unlockableJokeRelics.Keys)
			{
				if (_relicManager.RelicEffectActive(key2))
				{
					PersistentPlayerData.Instance.SetRelicUnlocked(RelicManager.unlockableJokeRelics[key2]);
				}
			}
		}

		private void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SaveManager.OnSaveRequested = (SaveManager.SaveRequested)Delegate.Remove(SaveManager.OnSaveRequested, new SaveManager.SaveRequested(SaveData));
			Enemy.OnEnemyKilled = (Enemy.EnemyDataEvent)Delegate.Remove(Enemy.OnEnemyKilled, new Enemy.EnemyDataEvent(OnEnemyKilled));
			Enemy.OnEnemyLootDropped = (Enemy.EnemyDataEvent)Delegate.Remove(Enemy.OnEnemyLootDropped, new Enemy.EnemyDataEvent(OnEnemyLootDropped));
			Enemy.OnEnemyCardDropped = (Enemy.EnemyDataEvent)Delegate.Remove(Enemy.OnEnemyCardDropped, new Enemy.EnemyDataEvent(OnEnemyCardDropped));
			MimicEnemy.OnMimicOpened = (MimicEnemy.MimicOpened)Delegate.Remove(MimicEnemy.OnMimicOpened, new MimicEnemy.MimicOpened(HandleMimicOpened));
			if (pegLayoutLoader != null)
			{
				PegLayoutLoader obj = pegLayoutLoader;
				obj.OnPegboardPreInstancingFinished = (PegLayoutLoader.PegboardPreInstancingFinished)Delegate.Remove(obj.OnPegboardPreInstancingFinished, new PegLayoutLoader.PegboardPreInstancingFinished(OnPegboardPreInstancingFinished));
			}
		}

		private void HandleMimicOpened()
		{
			mimicsBattledThisAct++;
			StaticGameData.mimicsBattled = mimicsBattledThisAct;
		}

		private void PrintRemainingScenarios()
		{
			string text = "";
			for (int i = 0; i < _remainingRandomScenarios.Count; i++)
			{
				text = text + _remainingRandomScenarios[i].name + ";";
			}
			Debug.Log("Remaining scenarios: " + text);
		}

		private void PostProcessMap()
		{
			if (minEliteNodes + minShopNodes + minTreasureNodes > _nodes.Length)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			MapNode[] nodes = _nodes;
			foreach (MapNode mapNode in nodes)
			{
				if (mapNode.isFixedNode || mapNode.isFinalNode)
				{
					continue;
				}
				if (mapNode.RoomType == RoomType.TREASURE)
				{
					num++;
				}
				else if (mapNode.RoomType == RoomType.STORE)
				{
					num3++;
				}
				else
				{
					if (mapNode.RoomType != RoomType.MINI_BOSS)
					{
						continue;
					}
					MapNode[] childNodes = mapNode.ChildNodes;
					for (int j = 0; j < childNodes.Length; j++)
					{
						if (childNodes[j].RoomType == RoomType.MINI_BOSS)
						{
							mapNode.canBeMiniboss = false;
							mapNode.GenerateRoomType();
							mapNode.GenerateIcon();
							break;
						}
					}
					if (mapNode.RoomType == RoomType.MINI_BOSS)
					{
						num2++;
					}
				}
			}
			List<RoomType> ignoreTypes = new List<RoomType>
			{
				RoomType.MINI_BOSS,
				RoomType.TREASURE,
				RoomType.STORE
			};
			GenerateMinNodeTypes(RoomType.MINI_BOSS, num2, minEliteNodes, ignoreTypes);
			GenerateMinNodeTypes(RoomType.TREASURE, num, minTreasureNodes, ignoreTypes);
			GenerateMinNodeTypes(RoomType.STORE, num3, minTreasureNodes, ignoreTypes);
		}

		private void GenerateMinNodeTypes(RoomType type, int generatedNodes, int minNodes, List<RoomType> ignoreTypes)
		{
			if (type == RoomType.TREASURE && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.NEXT_ACT_HAS_NO_TREASURE_NODES))
			{
				return;
			}
			while (generatedNodes < minNodes)
			{
				MapNode mapNode = _nodes[UnityEngine.Random.Range(0, _nodes.Length)];
				for (int i = 0; i < 15; i++)
				{
					if (mapNode.potentialMapData.Length != 0 || mapNode.isFixedNode || mapNode.isFinalNode || ignoreTypes.Contains(mapNode.RoomType) || ((type == RoomType.MINI_BOSS || type == RoomType.STORE) && !mapNode.canBeMiniboss))
					{
						mapNode = _nodes[UnityEngine.Random.Range(0, _nodes.Length)];
						continue;
					}
					mapNode.RoomType = type;
					mapNode.GenerateIcon();
					break;
				}
				generatedNodes++;
			}
		}

		public MapNode GetNodeByName(string nodeName)
		{
			MapNode[] nodes = _nodes;
			foreach (MapNode mapNode in nodes)
			{
				if (mapNode.gameObject.name == nodeName)
				{
					return mapNode;
				}
			}
			return null;
		}

		public static float[] GetMultipleSeededRolls(int numberOfRolls)
		{
			List<float> list = new List<float>();
			for (int i = 0; i < numberOfRolls; i++)
			{
				list.Add(UnityEngine.Random.value);
			}
			return list.ToArray();
		}

		private void SeedMapContents()
		{
			MapNode[] nodes = _nodes;
			foreach (MapNode mapNode in nodes)
			{
				switch (mapNode.RoomType)
				{
				case RoomType.TREASURE:
				{
					SeededTreasureNodeData seededTreasureNodeData = new SeededTreasureNodeData();
					seededTreasureNodeData.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededTreasureNodeData;
					break;
				}
				case RoomType.STORE:
				{
					SeededShopNodeData seededShopNodeData = new SeededShopNodeData();
					seededShopNodeData.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededShopNodeData;
					break;
				}
				case RoomType.BATTLE:
				{
					SeededBattleNodeData seededBattleNodeData3 = new SeededBattleNodeData();
					seededBattleNodeData3.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededBattleNodeData3;
					break;
				}
				case RoomType.MINI_BOSS:
				{
					SeededBattleNodeData seededBattleNodeData2 = new SeededBattleNodeData();
					seededBattleNodeData2.GenerateMinibossData(_deckManager, _relicManager, this, allowMimic: false);
					mapNode.seededNodeData = seededBattleNodeData2;
					break;
				}
				case RoomType.BOSS:
				{
					SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
					seededBattleNodeData.GenerateBossData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededBattleNodeData;
					break;
				}
				case RoomType.SCENARIO:
				{
					SeededTextScenarioNodeData seededTextScenarioNodeData = new SeededTextScenarioNodeData();
					seededTextScenarioNodeData.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededTextScenarioNodeData;
					break;
				}
				case RoomType.UNKNOWN:
				{
					SeededUnknownNodeData seededUnknownNodeData = new SeededUnknownNodeData();
					seededUnknownNodeData.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededUnknownNodeData;
					break;
				}
				case RoomType.PEG_MINIGAME:
				{
					SeededPegMinigameNodeData seededPegMinigameNodeData = new SeededPegMinigameNodeData();
					seededPegMinigameNodeData.GenerateData(_deckManager, _relicManager, this);
					mapNode.seededNodeData = seededPegMinigameNodeData;
					break;
				}
				case RoomType.NONE:
					Debug.LogError("Trying to seed a node that hasn't chosen it's type yet. Should never get here.");
					break;
				}
			}
		}
	}
}
