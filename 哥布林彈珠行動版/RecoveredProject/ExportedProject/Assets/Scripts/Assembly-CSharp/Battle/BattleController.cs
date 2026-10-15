using System;
using System.Collections;
using System.Collections.Generic;
using Achievements;
using Battle.Attacks;
using Battle.Attacks.AttackBehaviours;
using Battle.Attacks.AttackModifications;
using Battle.Attacks.DamageModifiers;
using Battle.Enemies;
using Battle.EnemyBehaviours;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.Pachinko.Obstacles;
using Battle.PegBehaviour;
using Battle.StatusEffects;
using Challenges;
using Cruciball;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using Loading;
using Loading.Pegboards;
using Map;
using Peglin.Achievements;
using PeglinUI;
using PeglinUI.PostBattle;
using PeglinUtils;
using Relics;
using Rewired;
using Stats;
using UI.OrbDisplay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace Battle
{
	[RequireComponent(typeof(AttackManager))]
	[RequireComponent(typeof(EnemyManager))]
	public class BattleController : MonoBehaviour
	{
		public delegate void BattleEvent();

		public delegate void CriticalHitActivated();

		public delegate void ExternalCritTriggerRequest();

		public delegate void RefreshPotionActivated();

		public delegate void PreRefreshActivated();

		public delegate void StartBattleWithBombs(int numRegular, int numRigged);

		public delegate void SetRelicCountdownText(RelicEffect relicEffect, string text);

		public delegate void PegboardEvent(Vector3 pos);

		public delegate void ReloadStarted();

		public delegate void PersistenceUpdate(int persistenceLevel);

		public delegate void ShotTimeoutEvent();

		public delegate void ShotComplete();

		public delegate void RoundCountIncremented(int roundCount);

		public enum BattleState
		{
			SHOULD_SPAWN = 0,
			SPAWNING = 1,
			AWAITING_SHOT = 2,
			AWAITING_SHOT_COMPLETION = 3,
			ATTACKING = 4,
			THROW_BOMBS = 5,
			AWAITING_ENEMY_MOVEMENT = 6,
			AWAITING_ENEMY_CLEANUP = 7,
			AWAITING_POST_BATTLE_CONTROLLER = 8,
			NAVIGATION = 9,
			NAVIGATION_COMPLETE = 10,
			RELOAD_EFFECTS = 11,
			RELOAD_CORE = 12,
			PRE_ATTACK_SPAWN_CHECK = 13,
			DO_ATTACK = 14,
			AWAITING_STATUS_EFFECT_ANIMATIONS = 15
		}

		public static float BALL_DRAW_ANIM_TIME = 0.4f;

		public static float MULTIHIT_DISPLAY_DELAY = 0.22f;

		private const float MAX_SHOT_LENGTH_SECONDS = 180f;

		public static BattleEvent OnBattleStarted = () =>
		{
		};

		public static BattleEvent OnBattleEnded = () =>
		{
		};

		public static BattleEvent OnNavReset = () =>
		{
		};

		public static BattleEvent OnStartedAwaitingShot = () =>
		{
		};

		public static CriticalHitActivated onCriticalHitActivated = () =>
		{
		};

		public static CriticalHitActivated onCriticalHitDeactivated = () =>
		{
		};

		public static ExternalCritTriggerRequest OnExternalCritTriggerRequest = () =>
		{
		};

		public static RefreshPotionActivated onRefreshPotionActivated = () =>
		{
		};

		public static PreRefreshActivated OnPreRefreshActivated = () =>
		{
		};

		public static StartBattleWithBombs OnStartBattleWithBombs = (int numRegular, int numRigged) =>
		{
		};

		public static SetRelicCountdownText UpdateRelicCountdownText = delegate
		{
		};

		public static PegboardEvent OnAdditionalBasicPegRequested = (Vector3 pos) =>
		{
		};

		public static ReloadStarted OnReloadStarted = () =>
		{
		};

		public static Action OnBallCreationComplete = () =>
		{
		};

		public static Action OnTurnComplete = () =>
		{
		};

		public static Action OnBombThrown = () =>
		{
		};

		public static Action OnBombDetonated = () =>
		{
		};

		public static Action<BombLob> OnBombLobFinished = delegate
		{
		};

		public static Action OnAttackStarted = () =>
		{
		};

		public static Action OnOrbDiscarded = () =>
		{
		};

		public static Action OnVictory = () =>
		{
		};

		public static Action<int> OnVictoryPlayJingle = delegate
		{
		};

		public static bool BattleActive = false;

		public static PlayerHealthController.PlayerHealthEvent OnPlayerSelfDamageTaken = (float amount) =>
		{
		};

		public static PersistenceUpdate onOrbAttackEnded = (int persistenceLevel) =>
		{
		};

		public static ShotTimeoutEvent OnShotTimeout = () =>
		{
		};

		public static ShotTimeoutEvent OnShotForcefullyEnded = () =>
		{
		};

		public static ShotComplete OnShotComplete = () =>
		{
		};

		public static RoundCountIncremented OnRoundCountIncremented = (int roundCount) =>
		{
		};

		[Header("Scene Elements")]
		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private DeckManager _deckManager;

		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private GameObject _navigationOrb;

		[SerializeField]
		private Transform _playerTransform;

		[SerializeField]
		private Vector3 _playerTransformOffset = new Vector2(1f, 0.5f);

		[SerializeField]
		private PostBattleController _postBattleController;

		[SerializeField]
		private Transform _lastSlotSpawnLocation;

		[SerializeField]
		private StatusEffectData _statusEffectData;

		private List<GameObject> _pegboardInstances = new List<GameObject>();

		private int _pegboardCount;

		[SerializeField]
		private GameObject _tossableBombPrefab;

		[SerializeField]
		private GameObject _tossableRiggedBombPrefab;

		[SerializeField]
		private AudioClip _discardOrbClip;

		[SerializeField]
		private EnemyInfoManager _enemyInfoManager;

		[SerializeField]
		private PredictionManager _predictionManager;

		private AudioSource _audioSource;

		private EnemyManager _enemyManager;

		private AttackManager _attackManager;

		private TargetingManager _targetingManager;

		private DamageCountDisplay _damageCountDisplay;

		[SerializeField]
		private PeglinBattleAnimationController _peglinAnimationController;

		private ScreenShaker _screenShaker;

		public const int BaseBombDamage = 50;

		public const int CruciballBombDamage = 40;

		public const int BaseRiggedBombDamage = 200;

		public const int CruciballRiggedBombDamage = 150;

		public const int RiggedBombSelfDamage = 3;

		public const int RiggedBombCruciballSelfDamage = 5;

		public int selfDamageCount;

		private static int _shotCount = 1;

		private static int _roundCount = 0;

		public float BombScreenshakeMod = 0.8f;

		public GameObject pegBoardBramballVine;

		public GameObject coreWinTimeline;

		[Header("Debug")]
		[SerializeField]
		private GameObject[] _debugPegBoards;

		[SerializeField]
		private GameObject _debugBackground;

		[Tooltip("Can be set manually for quickly testing specific spawns, but data isn't used when coming from the map.")]
		[SerializeField]
		private WaveGroup[] _waveGroups;

		[SerializeField]
		private List<StarterSpawn> _starterSpawns;

		[SerializeField]
		private PegLayout _pegLayout;

		[Header("Unity Events")]
		public UnityEvent OnDeckShuffled;

		private PlayerHealthController _playerHealthController;

		private PlayerStatusEffectController _playerStatusEffectController;

		private static int _criticalHitCount = 0;

		private int _bombsToThrowRegular;

		private int _bombsToThrowRigged;

		private int _bombsThrownThisBattleRegular;

		private int _bombsThrownThisBattleRigged;

		private bool _forceEarlySpawn;

		private int _remainingPachinkoBalls;

		private int _skipPlayerTurnCount;

		private bool _spawnAndAwaitShot;

		private bool _spawnAndAwaitShotFire;

		private bool _hitResetPegThisTurn;

		private bool _throwingBombs;

		private int _reloadCount;

		private bool _victoryTriggeringEnemyKilled;

		private float _pachinkoShotTimer;

		private int _inflightRelicProcCount;

		private static BattleState _battleState = BattleState.SHOULD_SPAWN;

		private BattleState _prevBattleState;

		private static bool _isMinibossBattle = false;

		public static bool IsBossBattle = false;

		private BattleState _postSpawnBattleState = BattleState.AWAITING_SHOT;

		[HideInInspector]
		public static bool PreBattlePause = false;

		private GameObject _activePachinkoBall;

		private Attack _activePachinkoBallAttack;

		private bool _activeAttackIsSummoningCircle;

		private Attack _activeSummoningCircleAttack;

		public static int currentBallPersistLevel = 0;

		private bool currentBallIsPersistBonusOrb;

		private PegManager _pegManager;

		private List<Peg> _allPegs = new List<Peg>();

		private List<BouncerPeg> _bouncerPegs = new List<BouncerPeg>();

		public PegLayoutAlternator pegLayoutAlternator;

		private int _pegMultiplierDamageTally;

		private int _numPegsHit;

		private int _cactusDamageTally;

		private int _cactusDamageBonus;

		private float _damageMultiplier = 1f;

		private List<Enemy> _destroyedEnemies = new List<Enemy>();

		private List<AssetReference> _referencesToUnload = new List<AssetReference>();

		private PegLayoutLoader _pegLayoutLoader;

		private int _damageBonus;

		private List<Spawner> _spawners = new List<Spawner>();

		[HideInInspector]
		public Vector2 pachinkoBallSpawnLocation = Vector2.zero;

		private Vector2 _previousAimVector = Vector2.zero;

		private Vector2 _previousNonRotatedAimVector = new Vector2(0.5f, -0.5f);

		private float _currentAimLimiterRotation;

		private float _aimLimiterRotationDirection = 1f;

		private Vector2 _lastShotAimVector = Vector2.zero;

		private List<AttackModifier> _nextAttackModifiers = new List<AttackModifier>();

		private bool _hasRemovedDamageReductionSlimeThisTurn;

		private Vector2 _navOverride = new Vector2(float.MinValue, float.MinValue);

		private int _ongoingMultihitAnimations;

		private int bagOfOrangePegsTextUpdateCounter;

		private const int BAG_OF_ORANGE_PEGS_TEXT_UPDATE_THRESHOLD = 4;

		private const float ADDITIONAL_PEG_TEXT_UP_AMOUNT = 0.85f;

		private GameObject _pegBoard;

		private GameObject _pegLayoutParent;

		private SeededBattleNodeData _seededBattleData;

		private bool _hasSeededData;

		public int NumShotsDiscarded;

		public GameObject flyingCoinEffect;

		[HideInInspector]
		public int blackHoleNav = -2;

		public AudioSource orbsDontAttackAudioSource;

		public AudioClip orbsDontAttackRelicSFX;

		public AudioClip orbsDontAttackRelicHealSFX;

		private bool _actuallyAttacked;

		private Player _player;

		private bool _skipBattleAndGoToCore;

		public GameObject mimicTimeline;

		public PredictionManager PredictionManager
		{
			get
			{
				return _predictionManager;
			}
			protected set
			{
				_predictionManager = value;
			}
		}

		public EnemyManager enemyManager => _enemyManager;

		public AttackManager attackManager => _attackManager;

		public static int ShotCount => _shotCount;

		public static int RoundCount => _roundCount;

		public PegLayout pegLayout => _pegLayout;

		public PlayerHealthController PlayerHealthController
		{
			get
			{
				return _playerHealthController;
			}
			protected set
			{
				_playerHealthController = value;
			}
		}

		public static int criticalHitCount => _criticalHitCount;

		public static bool criticalActive => _criticalHitCount > 0;

		public int bombsThrownThisBattleRegular => _bombsThrownThisBattleRegular;

		public int bombsThrownThisBattleRigged => _bombsThrownThisBattleRigged;

		public static BattleState CurrentBattleState
		{
			get
			{
				return _battleState;
			}
			set
			{
				_battleState = value;
			}
		}

		public static bool isMinibossBattle => _isMinibossBattle;

		public GameObject activePachinkoBall => _activePachinkoBall;

		private Attack currentAttack
		{
			get
			{
				if (!_activeAttackIsSummoningCircle)
				{
					return _activePachinkoBallAttack;
				}
				return _activeSummoningCircleAttack;
			}
		}

		public PegManager pegManager => _pegManager;

		public int numPegsHit => _numPegsHit;

		public int MaxDiscardedShots
		{
			get
			{
				int num = 1;
				if (_relicManager != null)
				{
					if (_relicManager.RelicEffectActive(RelicEffect.NO_DISCARD))
					{
						num--;
					}
					if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_DISCARD))
					{
						num++;
					}
				}
				return num;
			}
		}

		private void Awake()
		{
			_skipBattleAndGoToCore = ChallengeManager.Instance.beatenMinesBossAndCanGoToCore && MapController.instance != null && MapController.instance.Act == 3 && StaticGameData.dataToLoad != null && StaticGameData.dataToLoad.GetSceneType() == SceneType.BATTLE && ((MapDataBattle)StaticGameData.dataToLoad).grantedRelicRarity == RelicRarity.BOSS;
			BattleActive = true;
			PreBattlePause = false;
			_shotCount = 1;
			_roundCount = 0;
			_seededBattleData = StaticGameData.seededNodeData as SeededBattleNodeData;
			_postBattleController.seededBattleData = _seededBattleData;
			_hasSeededData = _seededBattleData != null;
			_battleState = BattleState.SHOULD_SPAWN;
			_aimLimiterRotationDirection = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.SOME_RELICS_DONT_WORK))
			{
				_relicManager.DisableRandomRelicsForBattle(ChallengeManager.DISABLE_RELICS_AMOUNT);
			}
			_audioSource = GetComponent<AudioSource>();
			_screenShaker = Camera.main.GetComponent<ScreenShaker>();
			_enemyManager = GetComponent<EnemyManager>();
			_pegManager = new PegManager(_relicManager, _cruciballManager, this, _predictionManager);
			if (_hasSeededData)
			{
				_deckManager.deckOrderRandomState = _seededBattleData.deckOrderRandomState;
				_pegManager.specialPegShufflingRandomState = _seededBattleData.specialPegShufflingRandomState;
				if (_seededBattleData.bossMechanicsRandomStatesPerAct != null)
				{
					_pegManager.bossMechanicsRandomStatesPerAct = _seededBattleData.bossMechanicsRandomStatesPerAct;
				}
				else
				{
					_pegManager.bossMechanicsRandomStatesPerAct = new List<SerializableRandomState>
					{
						new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
						new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
						new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
						new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0)
					};
				}
			}
			else
			{
				_deckManager.deckOrderRandomState = new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0);
				_pegManager.specialPegShufflingRandomState = new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0);
				_pegManager.bossMechanicsRandomStatesPerAct = new List<SerializableRandomState>
				{
					new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
					new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
					new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0),
					new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0)
				};
			}
			bool canSpawnRainbowSlime = true;
			bool canSpawnPainbowSlime = true;
			bool flag = _cruciballManager.CanSpawnBonusEnemies();
			C19BattleData c19BattleData = null;
			int numberOfSlots;
			if (StaticGameData.dataToLoad != null && StaticGameData.dataToLoad.GetSceneType() == SceneType.BATTLE)
			{
				MapDataBattle mapDataBattle = (MapDataBattle)StaticGameData.dataToLoad;
				_pegLayout = mapDataBattle.pegLayout;
				_waveGroups = mapDataBattle.waveGroups;
				_starterSpawns = mapDataBattle.starterSpawns;
				_pegBoard = UnityEngine.Object.Instantiate(mapDataBattle.pegboardFrame);
				canSpawnRainbowSlime = mapDataBattle.grantedRelicRarity != RelicRarity.BOSS;
				flag = flag && mapDataBattle.grantedRelicRarity == RelicRarity.NONE;
				c19BattleData = mapDataBattle.c19BattleData;
				if (mapDataBattle.background != null)
				{
					UnityEngine.Object.Instantiate(mapDataBattle.background);
				}
				numberOfSlots = mapDataBattle.NumberOfSlots;
				if (mapDataBattle.battleDoodads != null && mapDataBattle.battleDoodads.Length != 0)
				{
					GameObject[] battleDoodads = mapDataBattle.battleDoodads;
					for (int i = 0; i < battleDoodads.Length; i++)
					{
						UnityEngine.Object.Instantiate(battleDoodads[i], _pegBoard.transform);
					}
				}
				if (mapDataBattle.name == "MimicMinibossMapData")
				{
					PreBattlePause = true;
					canSpawnRainbowSlime = false;
					canSpawnPainbowSlime = false;
					flag = false;
					mimicTimeline.SetActive(value: true);
					mimicTimeline.GetComponent<MimicCutsceneController>().MakeBattleLookLikeChest();
				}
			}
			else
			{
				Debug.Log("No Static Data to load, loading defaults or debug info.");
				_pegBoard = new GameObject();
				for (int j = 0; j < _debugPegBoards.Length; j++)
				{
					_pegBoard = UnityEngine.Object.Instantiate(_debugPegBoards[j]);
					_pegboardInstances.Add(_pegBoard);
					if (j > 0)
					{
						_pegBoard.SetActive(value: false);
					}
				}
				if (_debugBackground != null)
				{
					UnityEngine.Object.Instantiate(_debugBackground);
				}
				numberOfSlots = -1;
			}
			LoadEnemyAssets();
			pachinkoBallSpawnLocation = _pegLayout.BallStartingPosition;
			_navOverride = new Vector2(_pegLayout.navXOverride, _pegLayout.navYOverride);
			_enemyInfoManager.Initialize(_waveGroups);
			if (!_skipBattleAndGoToCore)
			{
				WaveGroup[] waveGroups = _waveGroups;
				foreach (WaveGroup waveGroup in waveGroups)
				{
					_spawners.Add(SpawnerFactory.CreateSpawner(waveGroup, _enemyManager, _statusEffectData, _relicManager, _enemyInfoManager));
					if (waveGroup.spawnPoint != null)
					{
						_lastSlotSpawnLocation.position = waveGroup.spawnPoint.GetComponent<Spawner>().SpawnPointPosition.position;
					}
				}
			}
			_attackManager = GetComponent<AttackManager>();
			AttackManager.NumPegsHitThisBattle = 0;
			AttackManager.PreviousAttackDamage = 0L;
			_targetingManager = GetComponent<TargetingManager>();
			Vector3 position = _playerTransform.position;
			_targetingManager.Initialize(_enemyManager, _relicManager, position);
			_damageCountDisplay = GetComponent<DamageCountDisplay>();
			_pegLayoutLoader = base.gameObject.AddComponent<PegLayoutLoader>();
			float seededRainbowSlimeRoll = ((!_hasSeededData) ? UnityEngine.Random.value : _seededBattleData.rainbowSlimeRoll);
			float[] seededExtraEnemyRolls = null;
			if (flag && _hasSeededData)
			{
				seededExtraEnemyRolls = _seededBattleData.c19ExtraEnemyRolls;
			}
			_playerStatusEffectController = _playerTransform.GetComponentInChildren<PlayerStatusEffectController>();
			_playerStatusEffectController.Init(_pegManager);
			if (_relicManager.RelicEffectActive(RelicEffect.ADD_BALLUSION_WITH_SPAWNS))
			{
				EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Combine(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnAdditionalEnemySpawned));
			}
			_enemyManager.Initialize(_playerTransform, _playerTransformOffset, _lastSlotSpawnLocation.position, numberOfSlots, _starterSpawns, _statusEffectData, _relicManager, seededRainbowSlimeRoll, seededExtraEnemyRolls, _waveGroups, c19BattleData, canSpawnRainbowSlime, flag, canSpawnPainbowSlime, _skipBattleAndGoToCore);
			if (StaticGameData.preInstancedPegboardData != null && StaticGameData.preInstancedPegboardData.pegboardData != null && StaticGameData.preInstancedPegboardData.pegboardData.name != _pegLayout.layoutDataFilename)
			{
				Debug.LogWarning("Wrong pegboard pre-instanced! Pre-instance: " + StaticGameData.preInstancedPegboardData.pegboardData.name + " - What it should be: " + _pegLayout.layoutDataFilename + " Switching to load the correct pegboard now.");
				_pegLayoutParent = _pegLayoutLoader.TryLoadPegLayout(_pegLayout.PegLayoutData, _pegLayout.layoutDataFilename, base.transform.position);
			}
			else if (StaticGameData.preInstancedPegboardData != null && StaticGameData.preInstancedPegboardData.rootGameObject != null)
			{
				StaticGameData.preInstancedPegboardData.pegLayoutLoader.ForceStopLoading(StaticGameData.preInstancedPegboardData);
				_pegLayoutParent = _pegLayoutLoader.FinsihLoadingPegboardUsingPreInstances(StaticGameData.preInstancedPegboardData, base.transform.position);
				StaticGameData.preInstancedPegboardData = null;
			}
			else if (StaticGameData.preInstancedPegboardData != null && StaticGameData.preInstancedPegboardData.pegboardData != null)
			{
				_pegLayoutParent = _pegLayoutLoader.StartLoadingPegLayoutData(StaticGameData.preInstancedPegboardData.pegboardData, base.transform.position);
			}
			else
			{
				_pegLayoutParent = _pegLayoutLoader.TryLoadPegLayout(_pegLayout.PegLayoutData, _pegLayout.layoutDataFilename, base.transform.position);
			}
			_allPegs.AddRange(_pegLayoutParent.GetComponentsInChildren<RegularPeg>());
			_allPegs.AddRange(_pegLayoutParent.GetComponentsInChildren<LongPeg>());
			_bouncerPegs.AddRange(_pegLayoutParent.GetComponentsInChildren<BouncerPeg>());
			pegLayoutAlternator = _pegLayoutParent.GetComponent<PegLayoutAlternator>();
			_pegManager.InitializePegs(_allPegs, _pegLayoutParent.GetComponentsInChildren<Bomb>(includeInactive: true), _bouncerPegs);
			_playerStatusEffectController.UpdatePlayerBlindness();
			if (StaticGameData.dataToLoad != null && StaticGameData.dataToLoad.GetSceneType() == SceneType.BATTLE)
			{
				MapDataBattle mapDataBattle2 = (MapDataBattle)StaticGameData.dataToLoad;
				_pegManager.bossType = mapDataBattle2.bossType;
				_pegManager.isC20Leshy = false;
				if (mapDataBattle2.bossType == RunStats.BossType.LESHY && _cruciballManager.currentCruciballLevel == 20)
				{
					_pegManager.isC20Leshy = true;
				}
			}
			_player = ReInput.players.GetPlayer(0);
			Act4BossPegBoardFrameManager act4BossPegBoardFrameManager = UnityEngine.Object.FindObjectOfType<Act4BossPegBoardFrameManager>();
			if (act4BossPegBoardFrameManager != null)
			{
				act4BossPegBoardFrameManager.RegisterCallbacks();
			}
		}

		private void LoadEnemyAssets()
		{
			foreach (StarterSpawn starterSpawn in _starterSpawns)
			{
				string key = starterSpawn.spawnData.enemyAssetReference.RuntimeKey.ToString();
				if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key))
				{
					GameObject value = starterSpawn.spawnData.enemyAssetReference.LoadAssetAsync<GameObject>().WaitForCompletion();
					AssetLoading.Instance.EnemyPrefabs.Add(key, value);
				}
			}
			WaveGroup[] waveGroups = _waveGroups;
			for (int i = 0; i < waveGroups.Length; i++)
			{
				WaveData[] waveData = waveGroups[i].waveData;
				foreach (WaveData waveData2 in waveData)
				{
					string key2 = waveData2.spawnData.enemyAssetReference.RuntimeKey.ToString();
					if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key2))
					{
						GameObject value2 = waveData2.spawnData.enemyAssetReference.LoadAssetAsync<GameObject>().WaitForCompletion();
						AssetLoading.Instance.EnemyPrefabs.Add(key2, value2);
					}
					if (waveData2.spawnData.randomizedAssets == null)
					{
						continue;
					}
					AssetReferenceGameObject[] randomizedAssets = waveData2.spawnData.randomizedAssets;
					foreach (AssetReferenceGameObject assetReferenceGameObject in randomizedAssets)
					{
						string key3 = assetReferenceGameObject.RuntimeKey.ToString();
						if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key3))
						{
							GameObject value3 = assetReferenceGameObject.LoadAssetAsync<GameObject>().WaitForCompletion();
							AssetLoading.Instance.EnemyPrefabs.Add(key3, value3);
						}
					}
				}
			}
		}

		private IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			DOTween.SetTweensCapacity(500, 125);
			_predictionManager.Initialize(_pegLayoutParent, _pegBoard, _relicManager, navigation: false);
			_predictionManager.SetLowResolutionMode(_pegLayout.forceLowFrameratePrediction ? PredictionManager.PredictionResolutionMode.LOW : PredictionManager.PredictionResolutionMode.NORMAL);
			_playerHealthController = _playerTransform.GetComponentInChildren<PlayerHealthController>();
			yield return WaitObjectAllocator.waitForEndOfFrame;
			ResetField(initialAssignment: true);
			_isMinibossBattle = false;
			MapDataBattle mapDataBattle = null;
			if (StaticGameData.dataToLoad != null)
			{
				mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
				if (mapDataBattle != null)
				{
					_isMinibossBattle = mapDataBattle.grantedRelicRarity == RelicRarity.RARE;
					IsBossBattle = mapDataBattle.grantedRelicRarity == RelicRarity.BOSS;
				}
			}
			AddInitialCoinsToBoard(mapDataBattle);
			SnowballDmgAndSize.ResetStaticVariables();
			AddBlackHoleAfterAttacking.ResetStaticVariables();
			if (_relicManager.AttemptUseRelic(RelicEffect.START_WITH_BALLWARK))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, 6), StatusEffectSource.PLAYER);
			}
			if (_relicManager.RelicEffectActive(RelicEffect.PEG_CLEAR_DAMAGE_SCALING))
			{
				UpdateRelicCountdownText?.Invoke(RelicEffect.PEG_CLEAR_DAMAGE_SCALING, "x" + 1f);
			}
			_relicManager.ResetBattleRelics();
			_relicManager.UpdateHealthDependentRelics();
			_relicManager.UpdatePrimeRodRelic(_shotCount);
			_deckManager.ShuffleCompleteDeck();
			foreach (GameObject item in _deckManager.shuffledDeck)
			{
				Attack component = item.GetComponent<Attack>();
				component.Initialize(_playerTransform.position, _attackManager, _relicManager, _deckManager, _cruciballManager, _playerHealthController, _playerStatusEffectController);
				component.SetRandomRolledDamage();
			}
			_playerStatusEffectController.ApplyStartingBonuses();
			if (_relicManager.AttemptUseRelic(RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, _deckManager.GetStartWithBallwarkAmount()), StatusEffectSource.PLAYER);
			}
			OnBattleStarted?.Invoke();
			_predictionManager.CopyAllPegs();
			CheckRelicsForStartingBombCount();
			if (_skipBattleAndGoToCore)
			{
				yield return null;
				yield return null;
				yield return null;
				TriggerVictory();
			}
		}

		public void AddInitialCoinsToBoard(MapDataBattle bData)
		{
			int num = 50;
			num = ((!(bData != null)) ? _cruciballManager.GetAvailableBattleGold(RelicRarity.NONE) : (_cruciballManager.GetAvailableBattleGold(bData.grantedRelicRarity) + _cruciballManager.GetModifiedExternalGoldAdd(bData.availableGoldModifier)));
			num = Mathf.Max(0, num);
			int num2 = 0;
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_BATTLE_GOLD))
			{
				num2 += 10;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.DOUBLE_COINS_AND_PRICES))
			{
				num2 = Mathf.RoundToInt(num2 * 3);
				num = Mathf.RoundToInt(num * 3);
			}
			int coinsToAdd = num + num2;
			bool flag = false;
			ICustomCoinPegLayout[] componentsInChildren = _pegLayoutParent.GetComponentsInChildren<ICustomCoinPegLayout>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetCoinsToAdd(coinsToAdd);
				flag = true;
			}
			if (!flag)
			{
				if (num2 > 0)
				{
					_pegManager.ApplyGoldToPegs(num2);
				}
				_pegManager.ApplyGoldToPegs(num, doParticleEffect: false);
			}
		}

		private void ApplyGoldToPegs(int gold, Vector3 flyingCoinEffectStartPos = default(Vector3))
		{
			_pegManager.ApplyGoldToPegs(gold, doParticleEffect: true, flyingCoinEffectStartPos);
		}

		private void CheckRelicsForStartingBombCount()
		{
			int num = 0;
			int num2 = 0;
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_STARTING_BOMBS))
			{
				num += RelicManager.ADDITIONAL_BOMB_AMOUNT;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.DOUBLE_BOMBS_ON_MAP))
			{
				num *= 2;
				Bomb[] componentsInChildren = _pegLayoutParent.GetComponentsInChildren<Bomb>();
				num += componentsInChildren.Length;
			}
			OnStartBattleWithBombs?.Invoke(num, num2);
			if (num > 0)
			{
				_pegManager.ConvertPegsToBombs(num);
			}
			if (num2 > 0)
			{
				_pegManager.ConvertPegsToBombs(num2, rigged: true);
			}
		}

		private void OnEnable()
		{
			OnAdditionalBasicPegRequested = (PegboardEvent)Delegate.Combine(OnAdditionalBasicPegRequested, new PegboardEvent(GrantAdditionalBasicPegSimple));
			PachinkoBall.OnPachinkoBallWallBounce = (PachinkoBall.PachinkoBallWallBounce)Delegate.Combine(PachinkoBall.OnPachinkoBallWallBounce, new PachinkoBall.PachinkoBallWallBounce(HandlePachinkoBallWallBounce));
			PachinkoBall.OnPachinkoBallBouncerBounce = (PachinkoBall.PachinkoBallBouncerBounce)Delegate.Combine(PachinkoBall.OnPachinkoBallBouncerBounce, new PachinkoBall.PachinkoBallBouncerBounce(HandlePachinkoBallBouncerBounce));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Combine(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(AdditionalPachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(HandlePachinkoBallDestroyed));
			PachinkoBall.OnPachinkoBallPermanentDestruction = (PachinkoBall.PachinkoBallGameObjectEvent)Delegate.Combine(PachinkoBall.OnPachinkoBallPermanentDestruction, new PachinkoBall.PachinkoBallGameObjectEvent(RemoveOrbFromDeck));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegMultihit = (Peg.PegMultihit)Delegate.Combine(Peg.OnPegMultihit, new Peg.PegMultihit(HandlePegMultihit));
			Peg.OnPegAdded = (Peg.PegAdded)Delegate.Combine(Peg.OnPegAdded, new Peg.PegAdded(HandlePegAdded));
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Combine(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(HandleCoinCollected));
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Combine(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(EndBattle));
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(HandleEnemyDestruction));
			DeckInfoManager.onShuffleAnimComplete = (DeckInfoManager.ShuffleAnim)Delegate.Combine(DeckInfoManager.onShuffleAnimComplete, new DeckInfoManager.ShuffleAnim(HandleShuffleAnimComplete));
			OrbDiscardButton.OnOrbDiscardButtonClicked = (OrbDiscardButton.OrbDiscardEvent)Delegate.Combine(OrbDiscardButton.OnOrbDiscardButtonClicked, new OrbDiscardButton.OrbDiscardEvent(AttemptOrbDiscard));
			ActivateCritOnPegHits.OnCritOnPegHitActivated = (ActivateCritOnPegHits.CritOnPegHitActivated)Delegate.Combine(ActivateCritOnPegHits.OnCritOnPegHitActivated, new ActivateCritOnPegHits.CritOnPegHitActivated(ActivateCrit));
			PegManager.AddGoldToPegsRequest = (PegManager.ExternalPegboardAddGoldRequest)Delegate.Combine(PegManager.AddGoldToPegsRequest, new PegManager.ExternalPegboardAddGoldRequest(ApplyGoldToPegs));
			OnExternalCritTriggerRequest = (ExternalCritTriggerRequest)Delegate.Combine(OnExternalCritTriggerRequest, new ExternalCritTriggerRequest(ActivateCrit));
			PachinkoFireButton.OnFireButtonPressed = (PachinkoFireButton.FireButtonPressed)Delegate.Combine(PachinkoFireButton.OnFireButtonPressed, new PachinkoFireButton.FireButtonPressed(FireButtonPressed));
			PlayerHealthController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(HandlePlayerSelfDamageTaken));
			SpawnAchievementCheckers();
			_battleState = BattleState.SHOULD_SPAWN;
			_postSpawnBattleState = BattleState.AWAITING_SHOT;
		}

		private static void SpawnAchievementCheckers()
		{
			new GameObject("PegCountAchievementChecker").AddComponent<PegCountAchievementChecker>();
			if (AchievementData.Achievements.ContainsKey(AchievementData.AchievementId.MULTIPLE_SLIME_BOUNCES) && !AchievementData.Achievements[AchievementData.AchievementId.MULTIPLE_SLIME_BOUNCES].Unlocked)
			{
				new GameObject("SlimeHitsAchievementChecker").AddComponent<SlimeHitsAchievementChecker>();
			}
		}

		private void OnDisable()
		{
			BattleActive = false;
			_criticalHitCount = 0;
			PegCoinOverlay.ongoingCoinCollections = 0;
			OnAdditionalBasicPegRequested = (PegboardEvent)Delegate.Remove(OnAdditionalBasicPegRequested, new PegboardEvent(GrantAdditionalBasicPegSimple));
			PachinkoBall.OnPachinkoBallWallBounce = (PachinkoBall.PachinkoBallWallBounce)Delegate.Remove(PachinkoBall.OnPachinkoBallWallBounce, new PachinkoBall.PachinkoBallWallBounce(HandlePachinkoBallWallBounce));
			PachinkoBall.OnPachinkoBallBouncerBounce = (PachinkoBall.PachinkoBallBouncerBounce)Delegate.Remove(PachinkoBall.OnPachinkoBallBouncerBounce, new PachinkoBall.PachinkoBallBouncerBounce(HandlePachinkoBallBouncerBounce));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Remove(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(AdditionalPachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(HandlePachinkoBallDestroyed));
			PachinkoBall.OnPachinkoBallPermanentDestruction = (PachinkoBall.PachinkoBallGameObjectEvent)Delegate.Remove(PachinkoBall.OnPachinkoBallPermanentDestruction, new PachinkoBall.PachinkoBallGameObjectEvent(RemoveOrbFromDeck));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegMultihit = (Peg.PegMultihit)Delegate.Remove(Peg.OnPegMultihit, new Peg.PegMultihit(HandlePegMultihit));
			Peg.OnPegAdded = (Peg.PegAdded)Delegate.Remove(Peg.OnPegAdded, new Peg.PegAdded(HandlePegAdded));
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Remove(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(HandleCoinCollected));
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Remove(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(EndBattle));
			PlayerHealthController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(HandlePlayerSelfDamageTaken));
			if (_relicManager.RelicEffectActive(RelicEffect.ADD_BALLUSION_WITH_SPAWNS))
			{
				EnemyManager.OnEnemySpawned = (EnemyManager.EnemySpanwed)Delegate.Remove(EnemyManager.OnEnemySpawned, new EnemyManager.EnemySpanwed(OnAdditionalEnemySpawned));
			}
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(HandleEnemyDestruction));
			DeckInfoManager.onShuffleAnimComplete = (DeckInfoManager.ShuffleAnim)Delegate.Remove(DeckInfoManager.onShuffleAnimComplete, new DeckInfoManager.ShuffleAnim(HandleShuffleAnimComplete));
			OrbDiscardButton.OnOrbDiscardButtonClicked = (OrbDiscardButton.OrbDiscardEvent)Delegate.Remove(OrbDiscardButton.OnOrbDiscardButtonClicked, new OrbDiscardButton.OrbDiscardEvent(AttemptOrbDiscard));
			ActivateCritOnPegHits.OnCritOnPegHitActivated = (ActivateCritOnPegHits.CritOnPegHitActivated)Delegate.Remove(ActivateCritOnPegHits.OnCritOnPegHitActivated, new ActivateCritOnPegHits.CritOnPegHitActivated(ActivateCrit));
			PegManager.AddGoldToPegsRequest = (PegManager.ExternalPegboardAddGoldRequest)Delegate.Remove(PegManager.AddGoldToPegsRequest, new PegManager.ExternalPegboardAddGoldRequest(ApplyGoldToPegs));
			OnExternalCritTriggerRequest = (ExternalCritTriggerRequest)Delegate.Remove(OnExternalCritTriggerRequest, new ExternalCritTriggerRequest(ActivateCrit));
			PachinkoFireButton.OnFireButtonPressed = (PachinkoFireButton.FireButtonPressed)Delegate.Remove(PachinkoFireButton.OnFireButtonPressed, new PachinkoFireButton.FireButtonPressed(FireButtonPressed));
			_deckManager.battleDeck.Clear();
			_pegManager.UnsubscribeFromDelegates();
			_relicManager.ClearDisabledRelics();
		}

		private void Update()
		{
			if (Debug.isDebugBuild && _pegboardInstances.Count > 1 && Input.GetKeyDown(KeyCode.N))
			{
				_pegboardInstances[_pegboardCount].SetActive(value: false);
				_pegboardCount = ((_pegboardCount + 1 < _pegboardInstances.Count) ? (_pegboardCount + 1) : 0);
				_pegboardInstances[_pegboardCount].SetActive(value: true);
			}
			if (PreBattlePause)
			{
				return;
			}
			BattleState battleState = _battleState;
			switch (_battleState)
			{
			case BattleState.SHOULD_SPAWN:
			{
				bool flag3 = false;
				foreach (Spawner spawner in _spawners)
				{
					if (spawner.IsSpawning())
					{
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					break;
				}
				foreach (Spawner spawner2 in _spawners)
				{
					spawner2.Spawn(_forceEarlySpawn);
				}
				if (_forceEarlySpawn)
				{
					_forceEarlySpawn = false;
				}
				_battleState = BattleState.SPAWNING;
				break;
			}
			case BattleState.SPAWNING:
			{
				bool flag2 = false;
				foreach (Spawner spawner3 in _spawners)
				{
					if (spawner3.IsSpawning())
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
				_targetingManager.SetTargetingStatus(canTarget: true, _attackManager.GetCurrentAttackTargetingType());
				if (_skipPlayerTurnCount > 0)
				{
					_battleState = BattleState.ATTACKING;
				}
				else if (!_attackManager.isHeal && (_victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead()))
				{
					TriggerVictory();
				}
				else if (!DeckInfoManager.available || !DeckInfoManager.animating)
				{
					if (_spawnAndAwaitShot)
					{
						_battleState = _postSpawnBattleState;
						_spawnAndAwaitShot = false;
					}
					else
					{
						_battleState = _postSpawnBattleState;
					}
				}
				break;
			}
			case BattleState.AWAITING_SHOT:
				if (_prevBattleState != BattleState.AWAITING_SHOT && !_spawnAndAwaitShotFire)
				{
					OnStartedAwaitingShot?.Invoke();
					_relicManager.ResetPerShotRelics();
					_roundCount++;
					OnRoundCountIncremented?.Invoke(_roundCount);
				}
				_spawnAndAwaitShotFire = false;
				_playerHealthController.EndRelicImmunity();
				if (_victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead())
				{
					TriggerVictory();
				}
				if (!_enemyManager.EnemiesAnimating() && _enemyManager.Enemies.Count == 0)
				{
					_spawnAndAwaitShotFire = true;
					_battleState = BattleState.SHOULD_SPAWN;
					_postSpawnBattleState = BattleState.AWAITING_SHOT;
				}
				if (DeckInfoManager.available && !DeckInfoManager.animating && !HasOrbBeenDrawn() && _deckManager.shuffledDeck.Count == 0)
				{
					if (_reloadCount > 0 || !_relicManager.AttemptUseRelic(RelicEffect.FREE_RELOAD))
					{
						_skipPlayerTurnCount = ((!_cruciballManager.EnemiesGetExtraReloadTurn()) ? 1 : 2);
					}
					StartReloading();
				}
				break;
			case BattleState.AWAITING_SHOT_COMPLETION:
			{
				if (_remainingPachinkoBalls <= 0 && _ongoingMultihitAnimations <= 0 && !pegManager.IsThereAnOngoingLightningSlimeChain())
				{
					bool flag4 = _victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead();
					_pachinkoShotTimer = 0f;
					if (!_attackManager.isHeal & flag4)
					{
						TriggerVictory();
					}
					else if (_attackManager.isHeal & flag4)
					{
						_battleState = BattleState.DO_ATTACK;
					}
					else
					{
						NumShotsDiscarded = 0;
						_battleState = BattleState.PRE_ATTACK_SPAWN_CHECK;
					}
					OnShotComplete?.Invoke();
					break;
				}
				float pachinkoShotTimer = _pachinkoShotTimer;
				_pachinkoShotTimer += Time.deltaTime;
				if (_relicManager.RelicEffectActive(RelicEffect.INFLIGHT_DAMAGE) && Mathf.Floor(_pachinkoShotTimer / 1.5f) > (float)_inflightRelicProcCount)
				{
					_relicManager.AttemptUseRelic(RelicEffect.INFLIGHT_DAMAGE);
					_inflightRelicProcCount++;
					GrantAdditionalBasicPeg(Vector3.zero);
				}
				if (_pachinkoShotTimer > 180f && pachinkoShotTimer <= 180f && _activePachinkoBall != null)
				{
					OnShotTimeout?.Invoke();
				}
				if (!_enemyManager.EnemiesAnimating() && _enemyManager.Enemies.Count == 0)
				{
					_spawnAndAwaitShot = true;
					_battleState = BattleState.SHOULD_SPAWN;
					_postSpawnBattleState = BattleState.AWAITING_SHOT_COMPLETION;
				}
				break;
			}
			case BattleState.PRE_ATTACK_SPAWN_CHECK:
				if (!_enemyManager.EnemiesActing && _enemyManager.Enemies.Count == 0)
				{
					if (_bombsToThrowRegular > 0 || _bombsToThrowRigged > 0)
					{
						_postSpawnBattleState = BattleState.THROW_BOMBS;
					}
					else
					{
						_postSpawnBattleState = BattleState.DO_ATTACK;
					}
					_battleState = BattleState.SHOULD_SPAWN;
				}
				else if (_bombsToThrowRegular > 0 || _bombsToThrowRigged > 0)
				{
					_battleState = BattleState.THROW_BOMBS;
				}
				else
				{
					_battleState = BattleState.DO_ATTACK;
				}
				break;
			case BattleState.THROW_BOMBS:
				if (_bombsToThrowRegular == 0 && _bombsToThrowRigged == 0 && !_throwingBombs)
				{
					_battleState = BattleState.AWAITING_SHOT_COMPLETION;
				}
				else if (!_throwingBombs)
				{
					_throwingBombs = true;
					StartCoroutine(ThrowAllBombs());
				}
				else if (_throwingBombs && _bombsToThrowRegular == 0 && _bombsToThrowRigged == 0 && !_enemyManager.EnemiesAnimating())
				{
					_throwingBombs = false;
					if (_enemyManager.Enemies.Count > 0)
					{
						_battleState = BattleState.AWAITING_SHOT_COMPLETION;
						break;
					}
					_spawnAndAwaitShot = true;
					_battleState = BattleState.SHOULD_SPAWN;
					_postSpawnBattleState = BattleState.DO_ATTACK;
				}
				break;
			case BattleState.DO_ATTACK:
				DoAttack();
				StartAttacking();
				_nextAttackModifiers.Clear();
				HandleNextAttackModifiers(AttackModifier.AttackModifierPhase.ON_ATTACK);
				break;
			case BattleState.RELOAD_EFFECTS:
				_reloadCount++;
				if (_reloadCount > 0 && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_ENEMIES_ON_RELOAD))
				{
					DealCheeseDamage();
				}
				_battleState = BattleState.RELOAD_CORE;
				break;
			case BattleState.RELOAD_CORE:
				if (!_enemyManager.EnemiesAnimating())
				{
					ShuffleDeck();
					if (_skipPlayerTurnCount > 0)
					{
						_postSpawnBattleState = BattleState.ATTACKING;
						_battleState = BattleState.SHOULD_SPAWN;
					}
					else if (_enemyManager.Enemies.Count == 0)
					{
						_postSpawnBattleState = BattleState.AWAITING_SHOT;
						_battleState = BattleState.SHOULD_SPAWN;
					}
					else
					{
						_targetingManager.SetTargetingStatus(canTarget: true, _attackManager.GetCurrentAttackTargetingType());
						_battleState = BattleState.AWAITING_SHOT;
					}
				}
				break;
			case BattleState.ATTACKING:
				if (_attackManager.IsAttacking() || _enemyManager.EnemiesAnimating())
				{
					break;
				}
				if (_victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead())
				{
					TriggerVictory();
					break;
				}
				_skipPlayerTurnCount--;
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.ENEMIES_DO_MORE_DAMAGE) && _shotCount % 2 == 1)
				{
					foreach (Enemy enemy2 in _enemyManager.Enemies)
					{
						if (enemy2 != null && enemy2.CurrentHealth > 0f)
						{
							enemy2.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.PLAYER);
						}
					}
				}
				_enemyManager.TakeTurns();
				_battleState = BattleState.AWAITING_ENEMY_MOVEMENT;
				if (_actuallyAttacked)
				{
					_actuallyAttacked = false;
					ResolvePlayerSpinfection();
				}
				break;
			case BattleState.AWAITING_ENEMY_MOVEMENT:
				if (!_enemyManager.EnemiesActing)
				{
					if (_victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead())
					{
						TriggerVictory();
						break;
					}
					_battleState = BattleState.AWAITING_STATUS_EFFECT_ANIMATIONS;
					DoPoisonSlimeEffects();
					_enemyManager.TickStatusEffects();
				}
				break;
			case BattleState.AWAITING_STATUS_EFFECT_ANIMATIONS:
				if (_enemyManager.EnemiesActing)
				{
					break;
				}
				foreach (Enemy enemy3 in _enemyManager.Enemies)
				{
					enemy3.DoEndOfTurnCleanup();
				}
				_battleState = BattleState.AWAITING_ENEMY_CLEANUP;
				break;
			case BattleState.AWAITING_ENEMY_CLEANUP:
			{
				bool flag = true;
				foreach (Enemy enemy4 in _enemyManager.Enemies)
				{
					if (!enemy4.CleanupHasFinished)
					{
						flag = false;
					}
				}
				if (flag && !_enemyManager.EnemiesAnimating())
				{
					if (_victoryTriggeringEnemyKilled || CheckIfAllEnemiesAreDead())
					{
						TriggerVictory();
					}
					DoEndOfTurnBattleCleanup();
					if (_skipPlayerTurnCount <= 0)
					{
						ChooseShuffleOrDrawAtEndOfTurn();
						ResolvePlayerStatusEffects();
					}
					_spawnAndAwaitShot = false;
					if (_battleState == BattleState.AWAITING_ENEMY_CLEANUP)
					{
						_postSpawnBattleState = BattleState.AWAITING_SHOT;
						_battleState = BattleState.SHOULD_SPAWN;
					}
				}
				break;
			}
			}
			_prevBattleState = battleState;
			if (!_enemyManager.EnemiesActing)
			{
				bool flag5 = false;
				for (int i = 0; i < _enemyManager.Enemies.Count; i++)
				{
					Enemy enemy = _enemyManager.Enemies[i];
					if (!enemy.CurrentlyActing && enemy.CurrentHealth == 0f && enemy.gameObject.activeInHierarchy)
					{
						flag5 = true;
						break;
					}
				}
				if (flag5)
				{
					_enemyManager.CleanupDeadEnemies();
				}
			}
			if (_player.GetButtonDown("Confirm") && CurrentBattleState == BattleState.AWAITING_SHOT_COMPLETION && !GameBlockingWindow.wasOpenThisFrame && !PauseMenu.WasPausedAtStartOfFrame && !PreBattlePause && _pachinkoShotTimer > 180f && _activePachinkoBall != null)
			{
				ForceEndShotDestroyAllCurrentBalls();
			}
		}

		private void HandleNextAttackModifiers(AttackModifier.AttackModifierPhase phase)
		{
			NextAttackModifier component = _activePachinkoBall.GetComponent<NextAttackModifier>();
			if (component != null && component.attackModifier.modifierPhase.HasFlag(phase))
			{
				component.UpdateAttackModifier();
				_nextAttackModifiers.Add(component.attackModifier);
			}
		}

		private void AdditionalPachinkoBallCreated()
		{
			_remainingPachinkoBalls++;
			if (_remainingPachinkoBalls >= 16 && currentAttack != null && currentAttack.locNameString == "multiball")
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.MULTIPLE_MULTIBALLS);
			}
		}

		private void HandleCoinCollected(Vector3 position)
		{
			if (CurrentBattleState == BattleState.NAVIGATION)
			{
				return;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.CONVERT_COIN_TO_DAMAGE))
			{
				int pegMultipliersTally = _pegMultiplierDamageTally;
				if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
				{
					pegMultipliersTally = _cactusDamageTally;
				}
				int num = _damageBonus;
				if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
				{
					num = _cactusDamageBonus;
				}
				TargetingManager.OnDamageTargetedEnemy?.Invoke(_attackManager.GetCurrentDamage(pegMultipliersTally, _damageMultiplier, num, _criticalHitCount), Enemy.EnemyDamageSource.RelicChip);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.COINS_TO_CRITS))
			{
				ActivateCrit();
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.COINS_PROVIDE_BALLUSION))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion), StatusEffectSource.PLAYER);
			}
		}

		private void ShotFired(Vector2 aimVector)
		{
			_shotCount++;
			if (_battleState == BattleState.NAVIGATION)
			{
				return;
			}
			_hasRemovedDamageReductionSlimeThisTurn = false;
			_playerHealthController.HideSelfDamagePreview();
			if (_relicManager.AttemptUseRelic(RelicEffect.MINIMUM_PEGS))
			{
				for (int i = 0; i < 5; i++)
				{
					AddPeg(1);
				}
			}
			if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
			{
				_damageBonus += 2;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.MULTIBALL_EVERY_X_SHOTS))
			{
				SummoningCirclePachinkoBall component = _activePachinkoBall.GetComponent<SummoningCirclePachinkoBall>();
				if (component != null)
				{
					component.addDuplicationStationMultiball = true;
				}
				else
				{
					activePachinkoBall.GetComponent<PachinkoBall>().multiballLevel++;
				}
			}
			_relicManager.AttemptUseRelic(RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM);
			_battleState = BattleState.AWAITING_SHOT_COMPLETION;
			_previousAimVector = aimVector;
			_lastShotAimVector = aimVector;
			if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER) || _relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL))
			{
				PachinkoBall component2 = activePachinkoBall.GetComponent<PachinkoBall>();
				_previousNonRotatedAimVector = component2.preRotatedAimVector;
				IncreaseAimLimiterRotation();
			}
			_pachinkoShotTimer = 0f;
			_inflightRelicProcCount = 0;
			_remainingPachinkoBalls = 1;
			PersistentOrb component3 = _activePachinkoBall.GetComponent<PersistentOrb>();
			if (_activePachinkoBallAttack != null && (component3 == null || component3.remainingPersistence == component3.modifiedPersistLevel - 1 || component3.modifiedPersistLevel <= 0))
			{
				PachinkoBall component4 = _activePachinkoBall.GetComponent<PachinkoBall>();
				if (_activePachinkoBallAttack.ExhaustOnFire && !component4.isMirrorb)
				{
					_deckManager.RemoveOrbFromBattleDeck(_activePachinkoBall);
				}
				else if (_activePachinkoBallAttack.DestroyOnFire && !component4.isMirrorb)
				{
					RemoveOrbFromDeck(_activePachinkoBall);
				}
			}
			AdjustMaxHPOnFire component5 = _activePachinkoBall.GetComponent<AdjustMaxHPOnFire>();
			if (component5 != null)
			{
				_playerHealthController.AdjustMaxHealth(component5.maxHPChange);
			}
			AttackDamagesSelf component6 = _activePachinkoBall.GetComponent<AttackDamagesSelf>();
			if (component6 != null)
			{
				if (component6.selfDamage > 0f)
				{
					_playerHealthController.DealSelfDamage(component6.selfDamage, component6.blockable, PlayerHealthController.DamageSource.ORB_SELF);
				}
				else if (component6.selfDamage < 0f)
				{
					_playerHealthController.Heal(0f - component6.selfDamage);
				}
			}
			if (_activePachinkoBall != null)
			{
				if (_activePachinkoBall.GetComponent<RefreshBoardOnFired>() != null)
				{
					ResetField();
				}
				if (_playerHealthController.playerHealthPercent <= RelicManager.GUARANTEED_CRIT_HEALTH_PERCENT && _relicManager.AttemptUseRelic(RelicEffect.LOW_HEALTH_GUARANTEED_CRIT))
				{
					ActivateCrit();
					_damageCountDisplay.DisplayCritText(_criticalHitCount, _activePachinkoBall.transform.position + Vector3.up * 0.85f, _criticalHitCount > 1 && _relicManager.AttemptUseRelic(RelicEffect.CRITS_STACK));
				}
				if (UnityEngine.Random.value <= (float)_playerStatusEffectController.EffectStrength(StatusEffectType.Ballusion) * 0.02f && _relicManager.AttemptUseRelic(RelicEffect.BALLUSION_GUARANTEED_CRIT))
				{
					ActivateCrit();
					_damageCountDisplay.DisplayCritText(_criticalHitCount, _activePachinkoBall.transform.position + Vector3.up * 0.85f, _criticalHitCount > 1 && _relicManager.AttemptUseRelic(RelicEffect.CRITS_STACK));
				}
				_deckManager.ModifyStatsOnFire(_activePachinkoBall);
			}
		}

		private void ActivateCrit()
		{
			_criticalHitCount++;
			onCriticalHitActivated();
			if (_relicManager.AttemptUseRelic(RelicEffect.CRIT_DAMAGES_ENEMIES) && _activePachinkoBall != null)
			{
				Enemy.OnAllEnemiesDamaged?.Invoke(currentAttack.GetModifiedDamagePerPeg(_criticalHitCount, calculateStaticDamageModificersInPlace: false) * 2 * _criticalHitCount);
			}
			if (_criticalHitCount >= 5 && _relicManager.RelicEffectActive(RelicEffect.CRITS_STACK))
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.MULTIPLE_CRITS);
			}
		}

		private void IncreaseAimLimiterRotation()
		{
			_currentAimLimiterRotation += 8f * _aimLimiterRotationDirection;
			if (_currentAimLimiterRotation >= 45f)
			{
				_currentAimLimiterRotation -= 90f;
			}
			else if (_currentAimLimiterRotation <= -45f)
			{
				_currentAimLimiterRotation += 90f;
			}
		}

		private void RemoveOrbFromDeck(GameObject orb)
		{
			Attack component = orb.GetComponent<Attack>();
			if (!currentBallIsPersistBonusOrb && Attack.IsEquivalent(component, _activePachinkoBallAttack) && orb.GetInstanceID() == _activePachinkoBall.GetInstanceID())
			{
				_deckManager.RemoveOrbFromBattleDeck(_activePachinkoBall);
				_deckManager.RemoveSpecifiedOrbFromDeck(_activePachinkoBall);
			}
		}

		public int CountActiveSlimeOnBoard(Peg.SlimeType sType, int increaseCountByBuffAmount = -1, int increaseCountByDebuffAmount = -1)
		{
			int num = 0;
			List<Peg> list = new List<Peg>(_pegManager.allPegs.ToArray());
			list.AddRange(_bouncerPegs);
			foreach (Peg item in list)
			{
				if (!(item == null) && !item.IsDisabled() && item.slimeType == sType)
				{
					num++;
					if (increaseCountByBuffAmount > 0 && item.buffAmount > 0)
					{
						num += item.buffAmount / increaseCountByBuffAmount;
					}
					else if (increaseCountByDebuffAmount > 0 && item.buffAmount < 0)
					{
						num += item.buffAmount / -increaseCountByDebuffAmount;
					}
				}
			}
			return num;
		}

		private int CountPoisonSlimeDamage()
		{
			int num = 0;
			List<Peg> list = new List<Peg>(_pegManager.allPegs.ToArray());
			list.AddRange(_bouncerPegs);
			foreach (Peg item in list)
			{
				if (!item.IsDisabled() && item.slimeType == Peg.SlimeType.PoisonSlime)
				{
					int num2 = ((item.buffAmount >= 0) ? item.buffAmount : (item.buffAmount * -2));
					num += Peg.POISON_SLIME_DAMAGE + num2;
				}
			}
			return num;
		}

		private void DoPoisonSlimeEffects()
		{
			int num = CountPoisonSlimeDamage();
			if (num > 0)
			{
				TargetingManager.OnDamageTargetedEnemy?.Invoke(num);
			}
		}

		private float CalculateBagOfOrangePegsMutliplier()
		{
			int num = 0;
			foreach (Peg allPeg in _pegManager.allPegs)
			{
				if (!allPeg.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (allPeg is LongPeg)
				{
					LongPeg longPeg = allPeg as LongPeg;
					if (!longPeg.IsDisabled() && !longPeg.hit)
					{
						num++;
					}
				}
				else if (!allPeg.IsDisabled())
				{
					num++;
				}
			}
			float num2 = _pegManager.allPegs.Count;
			if (pegLayoutAlternator != null)
			{
				num2 = ((!pegLayoutAlternator.pegboardAActive) ? (num2 - (float)(pegLayoutAlternator.pegboardAPegs.Count - pegLayoutAlternator.pegboardABombs.Count)) : (num2 - (float)(pegLayoutAlternator.pegboardBPegs.Count - pegLayoutAlternator.pegboardBBombs.Count)));
			}
			return RelicManager.GetBagOfOrangePegsMultiplier((float)num / num2);
		}

		private void DoAttack()
		{
			Enemy target = null;
			NumShotsDiscarded = 0;
			_targetingManager.SetTargetingStatus(canTarget: false, _attackManager.GetCurrentAttackTargetingType());
			if (_targetingManager.currentTarget != null)
			{
				target = _targetingManager.currentTarget;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.PEG_CLEAR_DAMAGE_SCALING))
			{
				float mult = CalculateBagOfOrangePegsMutliplier();
				AddDamageMultiplier(mult);
				UpdateRelicCountdownText?.Invoke(RelicEffect.PEG_CLEAR_DAMAGE_SCALING, "x" + mult);
			}
			_attackManager.Attack(target, _pegMultiplierDamageTally, _numPegsHit, _damageMultiplier, _damageBonus, _deckManager, _criticalHitCount);
			_pegMultiplierDamageTally = 0;
			_numPegsHit = 0;
			_cactusDamageTally = 0;
			_cactusDamageBonus = 0;
			_damageBonus = 0;
			_remainingPachinkoBalls = 0;
			_pegManager.ResetDelayedDeathPegs();
		}

		public void AddDamageMultiplier(float mult)
		{
			_damageMultiplier *= mult;
		}

		private void DoEndOfTurnBattleCleanup()
		{
			_criticalHitCount = 0;
			_damageMultiplier = 1f;
			OnTurnComplete?.Invoke();
			onOrbAttackEnded?.Invoke(currentBallPersistLevel - 1);
			Achievement achievement = AchievementData.Achievements[AchievementData.AchievementId.LEAVE_ENEMY_WITH_ONE_HEALTH];
			if (achievement.Unlocked)
			{
				return;
			}
			foreach (Enemy enemy in _enemyManager.Enemies)
			{
				if (enemy.CurrentHealth == 1f)
				{
					AchievementManager.Instance.UnlockAchievement(achievement);
				}
			}
		}

		private void EnemyTurnComplete()
		{
			_battleState = BattleState.AWAITING_STATUS_EFFECT_ANIMATIONS;
			_enemyManager.TickStatusEffects();
		}

		public void ResolvePlayerStatusEffects()
		{
			_playerStatusEffectController.ResolveStatusEffects();
		}

		public void ResolvePlayerSpinfection()
		{
			_playerStatusEffectController.ResolvePlayerSpinfection();
		}

		private bool HasOrbBeenDrawn()
		{
			if (_activePachinkoBall != null)
			{
				if (_activePachinkoBall.GetComponent<PachinkoBall>().CurrentState != PachinkoBall.FireballState.WAITING)
				{
					return _activePachinkoBall.GetComponent<PachinkoBall>().CurrentState == PachinkoBall.FireballState.AIMING;
				}
				return true;
			}
			return false;
		}

		private void ChooseShuffleOrDrawAtEndOfTurn()
		{
			if (_deckManager.shuffledDeck.Count == 0 && (!DeckInfoManager.available || !DeckInfoManager.animating) && !HasOrbBeenDrawn())
			{
				if (_reloadCount == 0 && _relicManager.AttemptUseRelic(RelicEffect.FREE_RELOAD))
				{
					_skipPlayerTurnCount = 0;
					_pegManager.ShuffleSpecialPegs();
				}
				else
				{
					_skipPlayerTurnCount = ((!_cruciballManager.EnemiesGetExtraReloadTurn()) ? 1 : 2);
				}
				StartReloading();
			}
			else
			{
				_pegManager.ShuffleSpecialPegs();
				if ((!DeckInfoManager.available || !DeckInfoManager.animating) && (_activePachinkoBall == null || !_activePachinkoBall.activeSelf || _activePachinkoBall.GetComponent<PachinkoBall>().IsAwaitingResults()))
				{
					DrawBall();
				}
			}
			if (!_hitResetPegThisTurn && !(pegLayoutAlternator != null))
			{
				return;
			}
			_hitResetPegThisTurn = false;
			if (GetResetPegIndex() >= 0)
			{
				return;
			}
			if (_pegManager.AreAllPegsPopped() && _pegManager.GetPegCount(Peg.PegType.RESET) > 0)
			{
				Debug.LogWarning("We're supposed to have a reset peg yet we have popped all pegs. Refresh the board.");
				ResetField();
				_pegManager.ShuffleCritPegs();
				return;
			}
			if (Debug.isDebugBuild && !_relicManager.RelicEffectActive(RelicEffect.UNPOPPABLE_PEGS))
			{
				Debug.LogWarning("We've somehow ended up with no reset potion this turn. Creating one randomly...");
			}
			_pegManager.FailSafeCreateRefreshPegs();
		}

		private void CheckForForcedCritical()
		{
			if (!(_activePachinkoBall == null))
			{
				bool flag = false;
				GuaranteedCrit component = _activePachinkoBall.GetComponent<GuaranteedCrit>();
				ActivateCritOnDraw component2 = _activePachinkoBall.GetComponent<ActivateCritOnDraw>();
				if (component != null)
				{
					ActivateCrit();
					flag = true;
				}
				if (component2 != null)
				{
					ActivateCrit();
					flag = true;
				}
				if (!flag)
				{
					_criticalHitCount = 0;
					onCriticalHitDeactivated();
				}
			}
		}

		private void CheckForStatusEffectAddOnDraw()
		{
			AddStatusEffectOnDraw component = _activePachinkoBall.GetComponent<AddStatusEffectOnDraw>();
			if (component != null)
			{
				_playerStatusEffectController.ApplyStatusEffects(component.effects, StatusEffectSource.PLAYER);
			}
		}

		private void ShuffleDeck()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.REFRESH_BOARD_ON_RELOAD))
			{
				ResetField();
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.STR_ON_RELOAD))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, 2), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.BAL_ON_RELOAD))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance, 2), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.NO_DISCARD))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance, RelicManager.SEALED_CONVICTION_BALLANCE), StatusEffectSource.PLAYER);
			}
			NumShotsDiscarded = 0;
			OnDeckShuffled?.Invoke();
			OnReloadStarted?.Invoke();
			_deckManager.ShuffleBattleDeck();
			ApplyStatChangesToOrbsOnReload();
		}

		private void ApplyStatChangesToOrbsOnReload()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.RANDOMLY_ROLL_DAMAGE))
			{
				foreach (GameObject item in _deckManager.shuffledDeck)
				{
					item.GetComponent<Attack>().SetRandomRolledDamage();
				}
			}
			foreach (GameObject item2 in _deckManager.shuffledDeck)
			{
				item2.GetComponent<Attack>().ResetPreviousOrbDamageModifier();
			}
		}

		private void DealCheeseDamage()
		{
			if (_reloadCount < 15)
			{
				Enemy.OnAllEnemiesDamaged?.Invoke(Mathf.RoundToInt(RelicManager.RELOAD_DAMAGE * (float)_reloadCount));
			}
			else
			{
				Enemy.OnAllEnemiesDamaged?.Invoke(Mathf.RoundToInt(24f * Mathf.Pow(1.227f, _reloadCount)));
			}
		}

		private void HandleShuffleAnimComplete()
		{
			if (!HasOrbBeenDrawn())
			{
				DrawBall();
			}
		}

		private IEnumerator ThrowAllBombs()
		{
			bool animationFinished = false;
			PeglinBattleAnimationController.LobPoint lobPointDelegate = () =>
			{
				animationFinished = true;
			};
			PeglinBattleAnimationController.OnLobPoint = (PeglinBattleAnimationController.LobPoint)Delegate.Combine(PeglinBattleAnimationController.OnLobPoint, lobPointDelegate);
			if (OnBombThrown != null)
			{
				OnBombThrown();
			}
			else
			{
				Debug.LogWarning("No listener for BattleController.OnBombThrown");
				animationFinished = true;
			}
			WaitForEndOfFrame endOfFrameWait = new WaitForEndOfFrame();
			while (!animationFinished)
			{
				yield return endOfFrameWait;
			}
			PeglinBattleAnimationController.OnLobPoint = (PeglinBattleAnimationController.LobPoint)Delegate.Remove(PeglinBattleAnimationController.OnLobPoint, lobPointDelegate);
			int totalBombCount = _bombsToThrowRegular;
			int riggedBombCount = _bombsToThrowRigged;
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.bombsThrown += totalBombCount;
				StaticGameData.CurrentRunStats.bombsThrownRigged += riggedBombCount;
			}
			WaitForSeconds secondsWait = new WaitForSeconds(0.02f);
			for (int i = 0; i < riggedBombCount; i++)
			{
				DoBombThrow(rigged: true);
				yield return secondsWait;
			}
			for (int i = 0; i < totalBombCount; i++)
			{
				DoBombThrow();
				yield return secondsWait;
			}
			_pegManager.BombsThrown();
		}

		private void DoBombThrow(bool rigged = false, int totalThrown = 1)
		{
			BombLob component = UnityEngine.Object.Instantiate(rigged ? _tossableRiggedBombPrefab : _tossableBombPrefab, _playerTransform.position + _playerTransformOffset, Quaternion.identity).GetComponent<BombLob>();
			component.Shoot(_peglinAnimationController.EatenBySlimeBoss, totalThrown);
			component.OnBombTossComplete = (BombLob.BombTossComplete)Delegate.Combine(component.OnBombTossComplete, new BombLob.BombTossComplete(OnBombDeath));
		}

		private void OnBombDeath(BombLob bomb)
		{
			bomb.OnBombTossComplete = (BombLob.BombTossComplete)Delegate.Remove(bomb.OnBombTossComplete, new BombLob.BombTossComplete(OnBombDeath));
			if (bomb.isRigged)
			{
				_bombsThrownThisBattleRigged++;
			}
			else
			{
				_bombsThrownThisBattleRegular++;
			}
			OnBombDetonated?.Invoke();
			OnBombLobFinished?.Invoke(bomb);
			float damageVol = 1f;
			if (_enemyManager.Enemies.Count > 0)
			{
				damageVol = 1f / (float)_enemyManager.Enemies.Count;
			}
			long damage = GetBombDamage(bomb.isRigged);
			List<Enemy> enemies = _enemyManager.Enemies;
			if (_relicManager.AttemptUseRelic(RelicEffect.STRONG_BOMBS_ON_ONLY_TARGET))
			{
				if (_targetingManager.currentTarget == null || _targetingManager.currentTarget.CurrentHealth <= 0f)
				{
					_targetingManager.AutoSelect();
				}
				DamageEnemyWithBomb(bomb, _targetingManager.currentTarget, damage, damageVol);
			}
			else
			{
				foreach (Enemy item in enemies)
				{
					DamageEnemyWithBomb(bomb, item, damage, damageVol);
				}
			}
			_screenShaker.BulkDamageScreenShake((float)(_enemyManager.Enemies.Count * 50) * BombScreenshakeMod);
			ControllerVibration.DoVibration(0.7f, 0.1f, 0.2f, 0.05f);
			if (bomb.isRigged)
			{
				bool blocked = _screenShaker.blocked;
				_screenShaker.blocked = true;
				_bombsToThrowRigged--;
				_playerHealthController.DealSelfDamage(GetRiggedBombSelfDamage(), blockable: true, PlayerHealthController.DamageSource.RIGGED_BOMB);
				_screenShaker.blocked = blocked || (SettingsManager.Instance != null && SettingsManager.Instance.ScreenshakeInstensity <= 0f);
			}
			else
			{
				_bombsToThrowRegular--;
			}
		}

		private void DamageEnemyWithBomb(BombLob bomb, Enemy enemy, long damage, float damageVol)
		{
			if (enemy == null)
			{
				return;
			}
			float num = enemy.bombDamageMod;
			if (num > 0f && num < 1f && _relicManager.RelicEffectActive(RelicEffect.NO_DAMAGE_REDUCTION))
			{
				num = 1f / num;
			}
			enemy.Damage(damage, screenshake: false, damageVol, num);
			if (enemy is ShieldEnemy)
			{
				BarricadeEnemy shield = (enemy as ShieldEnemy).shield;
				if (shield != null)
				{
					num = shield.bombDamageMod;
					if (num > 0f && num < 1f && _relicManager.RelicEffectActive(RelicEffect.NO_DAMAGE_REDUCTION))
					{
						num = 1f / num;
					}
					shield.Damage(damage, screenshake: false, damageVol, num);
				}
			}
			if (enemy.CurrentHealth > 0f)
			{
				if (_relicManager.AttemptUseRelic(RelicEffect.BOMBS_APPLY_BLIND))
				{
					enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 2), StatusEffectSource.PLAYER);
				}
				if (_relicManager.AttemptUseRelic(RelicEffect.BOMBS_APPLY_POISON))
				{
					enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison), StatusEffectSource.PLAYER);
				}
			}
			if (enemy.TryGetComponent<EnemyAddStrengthOnBombDetonate>(out var component) && component != null && (bomb.isRigged || !component.onlyRigged))
			{
				if (component.allAllies)
				{
					Enemy.OnAllEnemiesBuffed?.Invoke(1);
				}
				else
				{
					enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.ENEMY);
				}
			}
		}

		private float GetRiggedBombSelfDamage()
		{
			float num = (_cruciballManager.IncreasedRiggedDamage() ? 5 : 3);
			if (_relicManager.RelicEffectActive(RelicEffect.MORE_PIERCE_LESS_BOMB_DAMAGE))
			{
				num = Mathf.CeilToInt(num * 0.25f);
			}
			if (_relicManager.RelicEffectActive(RelicEffect.STRONG_BOMBS_ON_ONLY_TARGET))
			{
				num -= 2f;
			}
			if (_activePachinkoBall != null)
			{
				ReduceRiggedDamageThisTurn component = _activePachinkoBall.GetComponent<ReduceRiggedDamageThisTurn>();
				SummoningCirclePachinkoBall component2 = _activePachinkoBall.GetComponent<SummoningCirclePachinkoBall>();
				if (component2 != null)
				{
					component = component2.orbToSummon.GetComponent<ReduceRiggedDamageThisTurn>();
				}
				if (component != null)
				{
					num -= (float)component.reductionAmount;
				}
			}
			if (_deckManager.battleDeck != null)
			{
				foreach (GameObject item in _deckManager.battleDeck)
				{
					if (item.TryGetComponent<ReduceRiggedDamageWhenInDeck>(out var component3))
					{
						num -= (float)component3.reductionAmount;
					}
				}
			}
			return Mathf.Max(0f, num);
		}

		private void HandlePachinkoBallDestroyed(PachinkoBall pBall)
		{
			_remainingPachinkoBalls--;
			if (_battleState == BattleState.NAVIGATION && _remainingPachinkoBalls <= 0)
			{
				StartCoroutine(WaitAndCheckForNavRespawn());
			}
		}

		private IEnumerator WaitAndCheckForNavRespawn()
		{
			yield return new WaitForEndOfFrame();
			if (_battleState != BattleState.NAVIGATION || _remainingPachinkoBalls > 0)
			{
				yield break;
			}
			if (blackHoleNav != -2)
			{
				if (_seededBattleData != null)
				{
					StartCoroutine(_postBattleController.DoBlackHoleNavigation(blackHoleNav, _seededBattleData.randomNavRoll));
				}
				else
				{
					StartCoroutine(_postBattleController.DoBlackHoleNavigation(blackHoleNav));
				}
				blackHoleNav = -2;
			}
			else
			{
				ResetNavigationAfterFailure();
			}
		}

		public bool IsNavigating()
		{
			return _battleState == BattleState.NAVIGATION;
		}

		public void NavigationSuccessful()
		{
			NavOnlyController.NavigationTriggered?.Invoke();
			_battleState = BattleState.NAVIGATION_COMPLETE;
			if (_relicManager.RelicEffectActive(RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES))
			{
				_relicManager.SetRemainingCountdownForRelic(RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES, _playerStatusEffectController.EffectStrength(StatusEffectType.Ballusion));
			}
		}

		public void ResetNavigationAfterFailure()
		{
			if (_remainingPachinkoBalls <= 0)
			{
				_pegMultiplierDamageTally = 0;
				_numPegsHit = 0;
				RemoveClearedPegs();
				ArmNavigationBall();
				ResolvePlayerSpinfection();
				ResolvePlayerStatusEffects();
				OnNavReset?.Invoke();
			}
		}

		private void AttemptOrbDiscard()
		{
			if (_battleState != BattleState.AWAITING_SHOT)
			{
				return;
			}
			if (NumShotsDiscarded >= MaxDiscardedShots)
			{
				_relicManager.AttemptUseRelic(RelicEffect.NO_DISCARD);
			}
			if (!(_activePachinkoBall != null) || !_activePachinkoBall.GetComponent<PachinkoBall>().available || DeckInfoManager.populatingDisplayOrb || GameBlockingWindow.windowOpen || ControllerInspection.CurrentlyInspecting || NumShotsDiscarded >= MaxDiscardedShots)
			{
				return;
			}
			_audioSource.PlayOneShot(_discardOrbClip);
			if (_relicManager.AttemptUseRelic(RelicEffect.DISCARD_GAIN_CRIT_BALLUSION))
			{
				float num = _activePachinkoBallAttack.GetModifiedDamagePerPeg(1);
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, Mathf.RoundToInt(num * 2f)), StatusEffectSource.PLAYER);
			}
			if (_activePachinkoBallAttack.HasUpgrade && _relicManager.AttemptUseRelic(RelicEffect.DISCARD_TO_UPGRADE))
			{
				_deckManager.UpgradeSpecificOrbForThisBattle(_activePachinkoBall, _playerTransform.position, _attackManager, _relicManager, _deckManager, _cruciballManager, _playerHealthController, _playerStatusEffectController);
			}
			DamageSelfOnDiscard component = _activePachinkoBall.GetComponent<DamageSelfOnDiscard>();
			if (component != null)
			{
				if (component.selfDamage < 0f)
				{
					_playerHealthController.Heal(component.selfDamage * -1f);
				}
				else if (component.selfDamage > 0f)
				{
					_playerHealthController.DealSelfDamage(component.selfDamage, !component.unblockable, PlayerHealthController.DamageSource.ORB_SELF);
				}
			}
			PachinkoBall component2 = _activePachinkoBall.GetComponent<PachinkoBall>();
			_previousAimVector = component2.aimVector;
			_previousNonRotatedAimVector = component2.preRotatedAimVector;
			NumShotsDiscarded++;
			if (_activePachinkoBall.GetComponent<RefreshBoardOnDiscard>() != null)
			{
				ResetField();
			}
			if (_activePachinkoBall.GetComponent<ShuffleSpecialPegsOnDiscard>() != null)
			{
				_pegManager.ShuffleSpecialPegs(forceRefresh: true);
			}
			HandleNextAttackModifiers(AttackModifier.AttackModifierPhase.ON_DISCARD);
			OnOrbDiscarded?.Invoke();
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.AddOrbDiscard(_activePachinkoBallAttack);
			}
			_deckManager.ClearCurrentPersistCharges(_activePachinkoBall.GetComponent<PersistentOrb>());
			if (_deckManager.shuffledDeck.Count == 0)
			{
				if (_activePachinkoBall != null)
				{
					DestroyCurrentBall();
				}
				if (_reloadCount > 0 || !_relicManager.AttemptUseRelic(RelicEffect.FREE_RELOAD))
				{
					_skipPlayerTurnCount = ((!_cruciballManager.EnemiesGetExtraReloadTurn()) ? 1 : 2);
				}
				StartReloading();
			}
			else
			{
				DrawBall();
			}
		}

		private void StartAttacking()
		{
			_battleState = BattleState.ATTACKING;
			OnAttackStarted?.Invoke();
			_actuallyAttacked = true;
		}

		private void StartReloading()
		{
			_battleState = BattleState.RELOAD_EFFECTS;
		}

		private IEnumerator DelayStartAttacking()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_battleState = BattleState.ATTACKING;
			OnAttackStarted();
		}

		private void HandlePachinkoBallWallBounce(Vector3 pos)
		{
			if ((_battleState != BattleState.NAVIGATION || _relicManager.RelicEffectActive(RelicEffect.SUPER_BOOTS)) && _relicManager.AttemptUseRelic(RelicEffect.WALL_BOUNCES_COUNT))
			{
				GrantAdditionalBasicPeg(pos);
				if (_battleState == BattleState.NAVIGATION)
				{
					_relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS);
				}
			}
		}

		private void HandlePachinkoBallBouncerBounce(Vector3 pos)
		{
			if ((_battleState != BattleState.NAVIGATION || _relicManager.RelicEffectActive(RelicEffect.SUPER_BOOTS)) && _relicManager.AttemptUseRelic(RelicEffect.BOUNCERS_COUNT))
			{
				GrantAdditionalBasicPeg(pos);
				if (_battleState == BattleState.NAVIGATION)
				{
					_relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS);
				}
			}
		}

		private void HandlePegActivated(Peg.PegType type, Peg peg)
		{
			if (peg == null)
			{
				return;
			}
			if (_battleState == BattleState.AWAITING_SHOT)
			{
				if (type == Peg.PegType.RESET)
				{
					StartCoroutine(ResetFieldAtEndOfFrame());
				}
				return;
			}
			int num = Mathf.RoundToInt((float)peg.buffAmount * peg.buffDamageMultiplier);
			bool flag = false;
			bool flag2 = false;
			bool shufflePegs = false;
			switch (type)
			{
			case Peg.PegType.CRIT:
				flag = true;
				if (_relicManager.AttemptUseRelic(RelicEffect.CRIT_ALSO_REFRESH))
				{
					_hitResetPegThisTurn = true;
					flag2 = true;
					if (!peg.justDuplicated)
					{
						peg.Reset(criticalHitActive: true);
					}
				}
				else if (_relicManager.AttemptUseRelic(RelicEffect.UNPOPPABLE_PEGS))
				{
					if (!peg.justDuplicated)
					{
						peg.Reset(criticalHitActive: true);
					}
					peg.AddBounce(1);
				}
				if (_relicManager.AttemptUseRelic(RelicEffect.POTION_PEGS_COUNT))
				{
					HandlePegMultihit(Peg.PegType.REGULAR, peg, 3);
				}
				break;
			case Peg.PegType.BOMB:
			{
				Bomb bomb = peg as Bomb;
				if (bomb != null && bomb.isRigged)
				{
					_bombsToThrowRigged++;
				}
				else
				{
					_bombsToThrowRegular++;
				}
				break;
			}
			case Peg.PegType.RESET:
				if (_relicManager.AttemptUseRelic(RelicEffect.REFRESH_ALSO_CRIT))
				{
					flag = true;
				}
				_hitResetPegThisTurn = true;
				flag2 = true;
				if (_relicManager.AttemptUseRelic(RelicEffect.POTION_PEGS_COUNT))
				{
					HandlePegMultihit(Peg.PegType.REGULAR, peg, 3);
				}
				break;
			case Peg.PegType.MAX_HP:
				peg.ConvertPegToType(Peg.PegType.REGULAR);
				if (_relicManager.AttemptUseRelic(RelicEffect.GAIN_MAX_HP_ON_ENEMY_DEFEAT))
				{
					_playerHealthController.AdjustMaxHealth(1f);
					_playerHealthController.PlayMaxHPIncreaseSFX();
				}
				AddPeg(peg.damageMultiplier, num);
				break;
			case Peg.PegType.SPINFECTION:
				peg.ConvertPegToType(Peg.PegType.REGULAR);
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison), StatusEffectSource.ENEMY);
				AddPeg(peg.damageMultiplier, num);
				break;
			default:
				AddPeg(peg.damageMultiplier, num);
				break;
			}
			if (flag2 && peg.overlapActivationsThisShot < 10)
			{
				StartCoroutine(ResetFieldAtEndOfFrame(shufflePegs));
			}
			bool emptyAction = false;
			if (_activePachinkoBall != null)
			{
				emptyAction = _activePachinkoBall.GetComponent<EmptyAction>() != null || (_activePachinkoBall.TryGetComponent<SummoningCirclePachinkoBall>(out var component) && component.copiedOrbsAttack as EmptyAction != null);
			}
			if (flag)
			{
				ActivateCrit();
				_damageCountDisplay.QueueCritTextDisplay(peg, _criticalHitCount, _criticalHitCount > 1 && _relicManager.AttemptUseRelic(RelicEffect.CRITS_STACK), emptyAction);
			}
			long damage = _attackManager.GetCurrentDamage(_pegMultiplierDamageTally, _damageMultiplier, _damageBonus, _criticalHitCount, displayNegative: true);
			if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE) && currentAttack != null)
			{
				damage = currentAttack.GetModifiedDamagePerPeg(_criticalHitCount, calculateStaticDamageModificersInPlace: false) + num;
			}
			if (_battleState == BattleState.NAVIGATION)
			{
				if (_relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS))
				{
					_damageCountDisplay.QueueDamageTextDisplay(peg, _numPegsHit, _numPegsHit, DamageCountDisplay.TextType.HEAL, emptyAction);
				}
			}
			else if (_attackManager.isHeal)
			{
				_damageCountDisplay.QueueDamageTextDisplay(peg, damage, _numPegsHit, DamageCountDisplay.TextType.HEAL, emptyAction);
			}
			else
			{
				_damageCountDisplay.QueueDamageTextDisplay(peg, damage, _numPegsHit, DamageCountDisplay.TextType.DAMAGE, emptyAction);
			}
			if (_relicManager.RelicEffectActive(RelicEffect.PEG_CLEAR_DAMAGE_SCALING) && ++bagOfOrangePegsTextUpdateCounter >= 4)
			{
				bagOfOrangePegsTextUpdateCounter = 0;
				float num2 = CalculateBagOfOrangePegsMutliplier();
				UpdateRelicCountdownText?.Invoke(RelicEffect.PEG_CLEAR_DAMAGE_SCALING, "x" + num2);
			}
		}

		private void HandlePegAdded(GameObject newPeg, IDummyPeg parent)
		{
			Peg component = newPeg.GetComponent<Peg>();
			if (component.pegType == Peg.PegType.BOMB)
			{
				if (_pegManager != null)
				{
					_pegManager.AddBomb(component as Bomb);
				}
				return;
			}
			_allPegs.Add(component);
			if (_pegManager != null)
			{
				_pegManager.AddPeg(component);
			}
			_playerStatusEffectController.UpdatePlayerBlindness();
		}

		private void HandlePegMultihit(Peg.PegType pegType, Peg peg, int multihitLevel)
		{
			StartCoroutine(MultihitRoutine(pegType, peg, multihitLevel));
		}

		private IEnumerator MultihitRoutine(Peg.PegType pegType, Peg peg, int multihitLevel)
		{
			_ongoingMultihitAnimations++;
			WaitForSeconds waitForSeconds = new WaitForSeconds(MULTIHIT_DISPLAY_DELAY);
			Peg.PegType type = ((pegType == Peg.PegType.RESET) ? Peg.PegType.REGULAR : pegType);
			if (type == Peg.PegType.MAX_HP)
			{
				type = Peg.PegType.REGULAR;
			}
			for (int i = 0; i < multihitLevel; i++)
			{
				yield return waitForSeconds;
				Peg.OnPegActivated?.Invoke(type, peg);
				if (peg.slimeType == Peg.SlimeType.HealSlime)
				{
					float num = 1f;
					if (peg.buffAmount > 0)
					{
						num += Mathf.Floor(peg.buffAmount / Peg.BUFF_PER_HEAL_INCREASE);
					}
					else if (peg.buffAmount < 0)
					{
						num += Mathf.Floor(Mathf.Abs(peg.buffAmount) / Peg.DEBUFF_PER_HEAL_INCREASE);
					}
					PlayerHealthController.OnExternalHealRequested?.Invoke(num);
				}
				peg.CheckForRelicBuff();
			}
			_ongoingMultihitAnimations--;
		}

		private void AddPeg(int multiplier, int bonus = 0)
		{
			_numPegsHit++;
			if (_playerHealthController.CurrentHealth / _playerHealthController.MaxHealth <= RelicManager.LOW_HEALTH_DOUBLE_DAMAGE && _relicManager.AttemptUseRelic(RelicEffect.LOW_HEALTH_INCREASED_DAMAGE))
			{
				multiplier *= 2;
			}
			bool flag = _battleState == BattleState.NAVIGATION && _relicManager.RelicEffectActive(RelicEffect.SUPER_BOOTS);
			if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE) && !flag)
			{
				HealAction healAction = currentAttack as HealAction;
				if (healAction == null && currentAttack != null)
				{
					int num = Mathf.RoundToInt(currentAttack.GetModifiedDamagePerPeg(criticalHitCount, calculateStaticDamageModificersInPlace: false) * multiplier);
					num += bonus;
					if (num > 0)
					{
						orbsDontAttackAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
						orbsDontAttackAudioSource.volume = 1f;
						orbsDontAttackAudioSource.PlayOneShot(orbsDontAttackRelicSFX);
					}
					TargetingManager.OnDamageTargetedEnemy?.Invoke(num, Enemy.EnemyDamageSource.RelicChip);
				}
				else if (currentAttack != null)
				{
					int num2 = Mathf.RoundToInt(currentAttack.GetModifiedDamagePerPeg(criticalHitCount, calculateStaticDamageModificersInPlace: false) * multiplier);
					num2 += bonus;
					if (num2 > 0)
					{
						orbsDontAttackAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
						orbsDontAttackAudioSource.volume = 1f;
						orbsDontAttackAudioSource.PlayOneShot(orbsDontAttackRelicHealSFX);
					}
					healAction.DoHealAction(_playerHealthController, num2);
				}
				_cactusDamageTally += multiplier;
				_cactusDamageBonus += bonus;
				multiplier = 0;
				bonus = 0;
			}
			_pegMultiplierDamageTally += multiplier;
			_damageBonus += bonus;
		}

		private void GrantAdditionalBasicPegSimple(Vector3 pos)
		{
			GrantAdditionalBasicPeg(pos);
		}

		public void GrantAdditionalBasicPeg(Vector3 pos, int mult = 1, int bonus = 0)
		{
			_numPegsHit++;
			bool flag = _battleState == BattleState.NAVIGATION && _relicManager.RelicEffectActive(RelicEffect.SUPER_BOOTS);
			if (mult > 0)
			{
				if ((double)(_playerHealthController.CurrentHealth / _playerHealthController.MaxHealth) <= 0.25 && _relicManager.AttemptUseRelic(RelicEffect.LOW_HEALTH_INCREASED_DAMAGE))
				{
					mult *= 2;
				}
				if (!_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE) && !flag)
				{
					_pegMultiplierDamageTally += mult;
				}
				else
				{
					_cactusDamageTally += mult;
					_cactusDamageBonus += bonus;
				}
			}
			if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE) && !flag)
			{
				HealAction healAction = currentAttack as HealAction;
				if (healAction == null && currentAttack != null)
				{
					int num = Mathf.RoundToInt(currentAttack.GetModifiedDamagePerPeg(criticalHitCount, calculateStaticDamageModificersInPlace: false) * mult);
					if (num > 0)
					{
						orbsDontAttackAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
						orbsDontAttackAudioSource.volume = 1f;
						orbsDontAttackAudioSource.PlayOneShot(orbsDontAttackRelicSFX);
					}
					TargetingManager.OnDamageTargetedEnemy?.Invoke(num, Enemy.EnemyDamageSource.RelicChip);
				}
				else if (currentAttack != null)
				{
					int num2 = Mathf.RoundToInt(currentAttack.GetModifiedDamagePerPeg(criticalHitCount, calculateStaticDamageModificersInPlace: false) * mult);
					if (num2 > 0)
					{
						orbsDontAttackAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
						orbsDontAttackAudioSource.volume = 1f;
						orbsDontAttackAudioSource.PlayOneShot(orbsDontAttackRelicHealSFX);
					}
					healAction.DoHealAction(_playerHealthController, num2);
				}
			}
			if (!_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
			{
				_damageBonus += bonus;
			}
			long damage = (flag ? _numPegsHit : _attackManager.GetCurrentDamage(_pegMultiplierDamageTally, _damageMultiplier, _damageBonus, _criticalHitCount, displayNegative: true));
			GameObject gameObject = FindActivePachinkoBall();
			if (pos == Vector3.zero && gameObject != null)
			{
				pos = gameObject.transform.position;
			}
			if (!_activePachinkoBall.GetComponent<EmptyAction>() && (!_activePachinkoBall.TryGetComponent<SummoningCirclePachinkoBall>(out var component) || !(component.copiedOrbsAttack as EmptyAction != null)))
			{
				if (_attackManager.isHeal | flag)
				{
					_damageCountDisplay.DisplayHeal(damage, _numPegsHit, pos);
				}
				else
				{
					_damageCountDisplay.DisplayDamage(damage, _numPegsHit, pos);
				}
			}
		}

		private GameObject FindActivePachinkoBall()
		{
			if (_activePachinkoBall != null && !_activePachinkoBall.GetComponent<PachinkoBall>().IsAwaitingResults())
			{
				return _activePachinkoBall;
			}
			GameObject[] array = GameObject.FindGameObjectsWithTag("PachinkoBall");
			foreach (GameObject gameObject in array)
			{
				if (!gameObject.GetComponent<PachinkoBall>().IsAwaitingResults())
				{
					return gameObject;
				}
			}
			return null;
		}

		private void HandleEnemyDestruction(Enemy enemy)
		{
			SteamStatsManager.Instance?.StateChanged(StateChange.ENEMY_BEATEN);
			if (_destroyedEnemies.Contains(enemy))
			{
				return;
			}
			_destroyedEnemies.Add(enemy);
			if (enemy.GetComponent<WinWhenDestroyed>() != null)
			{
				_victoryTriggeringEnemyKilled = true;
			}
			if (enemy.GetComponent<EnemySubcomponent>() == null && _relicManager.AttemptUseRelic(RelicEffect.REFRESH_BOARD_ON_ENEMY_KILLED))
			{
				ResetField();
			}
			foreach (Enemy enemy2 in _enemyManager.Enemies)
			{
				if (enemy == enemy2)
				{
					_targetingManager.RemoveDeadEnemyFromTargets(enemy);
					_enemyManager.RemoveEnemy(enemy);
					enemy2.gameObject.SetActive(value: false);
					if (_enemyManager.Enemies.Count == 0)
					{
						_forceEarlySpawn = true;
					}
					break;
				}
			}
			if (!enemy.enemyTypes.HasFlag(Enemy.EnemyType.Minion) && enemy.CurrentHealth <= 0f && _playerHealthController.CurrentHealth >= 0f && _relicManager.AttemptUseRelic(RelicEffect.GAIN_MAX_HP_ON_ENEMY_DEFEAT))
			{
				_pegManager.ConvertPegToMaxHpPeg(1);
			}
			if (enemy.isActiveAndEnabled)
			{
				enemy.gameObject.SetActive(value: false);
			}
		}

		private void HandlePlayerSelfDamageTaken(float amount)
		{
			selfDamageCount++;
			OnPlayerSelfDamageTaken?.Invoke(amount);
		}

		private bool CheckIfAllEnemiesAreDead()
		{
			if (_enemyManager.AllValidEnemiesDead())
			{
				foreach (Spawner spawner in _spawners)
				{
					if (spawner.HasSpawnsLeft())
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public void EndBattle()
		{
			_criticalHitCount = 0;
			_battleState = BattleState.AWAITING_POST_BATTLE_CONTROLLER;
			DestroyCurrentBall();
			if (_playerHealthController.CurrentHealth <= 0f && _postBattleController != null && _postBattleController.gameObject.activeSelf)
			{
				_postBattleController.upgradeOptionsCanvas.GetComponent<CanvasGroup>().DOFade(0f, 0.4f);
			}
			OnBattleEnded?.Invoke();
		}

		public void TriggerVictoryWithNoRewards()
		{
			CompleteVictory(playJingle: false);
			UnityEngine.Object.FindObjectOfType<BattleUpgradeCanvas>().gameObject.SetActive(value: false);
			_postBattleController.SkipMimicNavigation();
			PreBattlePause = false;
		}

		public void TriggerVictory()
		{
			SteamStatsManager.Instance?.StateChanged(StateChange.ENCOUNTER_BEATEN);
			if (!(_playerHealthController.CurrentHealth <= 0f))
			{
				if (_enemyManager.ongoingVictoryBlockingOperations > 0)
				{
					_battleState = BattleState.AWAITING_POST_BATTLE_CONTROLLER;
					StartCoroutine(WaitForVictoryBlockers());
				}
				else
				{
					CompleteVictory();
				}
			}
		}

		private IEnumerator WaitForVictoryBlockers()
		{
			while (_enemyManager.ongoingVictoryBlockingOperations > 0)
			{
				yield return null;
			}
			CompleteVictory();
		}

		private void CompleteVictory(bool playJingle = true)
		{
			AttackManager.PreviousAttackDamage = 0L;
			EndBattle();
			if (_playerHealthController.CurrentHealth <= 0f)
			{
				return;
			}
			_playerHealthController.EndRelicImmunity();
			OnVictory?.Invoke();
			if (playJingle)
			{
				MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
				int obj = 1;
				if (MapController.instance != null)
				{
					obj = MapController.instance.Act;
				}
				if (mapDataBattle != null && mapDataBattle.bossType != RunStats.BossType.NONE)
				{
					obj = RunStats.BossTypeToActMap[mapDataBattle.bossType];
				}
				OnVictoryPlayJingle?.Invoke(obj);
			}
			bool num = StaticGameData.currentNode == null;
			MapDataBattle mapDataBattle2 = null;
			if (!num && StaticGameData.currentNode.MapData is MapDataBattle)
			{
				mapDataBattle2 = StaticGameData.currentNode.MapData as MapDataBattle;
			}
			bool flag = false;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.FIGHT_MINIBOSS_FROM_NEXT_ACT) || ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.FIGHT_EXTRA_BOSS))
			{
				ChallengeManager.Instance.AttemptClearChallengeForAct(MapController.instance.Act);
				flag = true;
				StaticGameData.loadNextScene = false;
			}
			if (!num && StaticGameData.currentNode.isFinalNode)
			{
				if (ChallengeManager.Instance.IsAct4Unlocked())
				{
					ChallengeManager.Instance.beatenMinesBossAndCanGoToCore = true;
					ChallengeManager.Instance.SaveChallengeData();
					SaveManager.SaveRun();
					ChallengeManager.Instance.chosenToGoToCore = false;
					ChallengeManager.Instance.chosenToSkipCore = false;
					_postBattleController.showGoToCoreOption = true;
					_postBattleController.gameObject.SetActive(value: true);
				}
				else
				{
					_postBattleController.winTimeline.SetActive(value: true);
				}
			}
			else if (((ReleaseDemoManager.ReleaseDemoActive && mapDataBattle2 != null && mapDataBattle2.grantedRelicRarity == RelicRarity.BOSS) || (mapDataBattle2 != null && mapDataBattle2.grantedRelicRarity == RelicRarity.BOSS && MapController.instance.Act == 4)) | flag)
			{
				if (MapController.instance.Act == 4)
				{
					coreWinTimeline.SetActive(value: true);
				}
				else
				{
					_postBattleController.winTimeline.SetActive(value: true);
				}
			}
			else
			{
				_postBattleController.gameObject.SetActive(value: true);
			}
		}

		public void RemoveClearedPegs()
		{
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.Cleared && allPeg.NumCoins() <= 0)
				{
					num++;
				}
				allPeg.RemoveIfCleared();
			}
			foreach (BouncerPeg bouncerPeg in _bouncerPegs)
			{
				bouncerPeg.RemoveIfCleared();
			}
			Achievement a = AchievementData.Achievements[AchievementData.AchievementId.CLEAR_PEGBOARD];
			if (num == _allPegs.Count)
			{
				AchievementManager.Instance.UnlockAchievement(a);
			}
		}

		public void CheckPegboardClearedAchievement()
		{
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.Cleared)
				{
					num++;
				}
			}
			Achievement a = AchievementData.Achievements[AchievementData.AchievementId.CLEAR_PEGBOARD];
			if (num == _allPegs.Count)
			{
				AchievementManager.Instance.Unlocker.UnlockAchievement(a);
			}
		}

		private IEnumerator ResetFieldAtEndOfFrame(bool shufflePegs = false)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			ResetField();
			if (shufflePegs && _allPegs.Count > 1)
			{
				_pegManager.ShuffleSpecialPegs(forceRefresh: true);
			}
		}

		public void ResetField(bool initialAssignment = false)
		{
			if (!initialAssignment)
			{
				OnPreRefreshActivated?.Invoke();
				_pegManager.ResetPegs(_criticalHitCount > 0);
				Peg.OnPegAudioRequest?.Invoke(Peg.PegType.RESET);
			}
			_pegManager.CreateRefreshPegs();
			if (initialAssignment)
			{
				if (_allPegs.Count > 1)
				{
					_pegManager.ShuffleCritPegs();
				}
			}
			else
			{
				onRefreshPotionActivated();
			}
			if (_relicManager.RelicEffectActive(RelicEffect.PEG_CLEAR_DAMAGE_SCALING))
			{
				float num = CalculateBagOfOrangePegsMutliplier();
				UpdateRelicCountdownText?.Invoke(RelicEffect.PEG_CLEAR_DAMAGE_SCALING, "x" + num);
			}
		}

		private int GetResetPegIndex()
		{
			for (int i = 0; i < _allPegs.Count; i++)
			{
				if (_allPegs[i].pegType == Peg.PegType.RESET && _allPegs[i].gameObject.activeInHierarchy && !_allPegs[i].IsDisabled())
				{
					return i;
				}
			}
			return -1;
		}

		private void DrawBall()
		{
			if (_enemyManager.EnemiesActing || _battleState == BattleState.AWAITING_POST_BATTLE_CONTROLLER)
			{
				return;
			}
			DestroyCurrentBall();
			GameObject gameObject = _deckManager.DrawBall(_nextAttackModifiers);
			if (!(gameObject != null))
			{
				return;
			}
			_activePachinkoBall = UnityEngine.Object.Instantiate(gameObject, pachinkoBallSpawnLocation, Quaternion.identity);
			_activePachinkoBallAttack = _activePachinkoBall.GetComponent<Attack>();
			if (_nextAttackModifiers.Count != 0)
			{
				List<AttackModifier> list = new List<AttackModifier>(_nextAttackModifiers.ToArray());
				foreach (AttackModifier nextAttackModifier in _nextAttackModifiers)
				{
					nextAttackModifier.ApplyOtherModificationsToOrb(_activePachinkoBall);
					if (nextAttackModifier.onlyApplyToNextOrbDrawn)
					{
						list.Remove(nextAttackModifier);
					}
				}
				_nextAttackModifiers = list;
			}
			_activePachinkoBall.GetComponent<TrajectorySimulation>().enabled = false;
			PachinkoBall component = _activePachinkoBall.GetComponent<PachinkoBall>();
			component.Init(_relicManager, _deckManager, _previousAimVector, _predictionManager, _playerStatusEffectController);
			_activeAttackIsSummoningCircle = false;
			_activeSummoningCircleAttack = null;
			SummoningCirclePachinkoBall component2 = _activePachinkoBall.GetComponent<SummoningCirclePachinkoBall>();
			if (component2 != null)
			{
				component2.InitSummoningCircle(_deckManager, pachinkoBallSpawnLocation);
				_activeAttackIsSummoningCircle = true;
				_activeSummoningCircleAttack = component2.copiedOrbsAttack;
			}
			_activePachinkoBallAttack.CopyValuesFromDeckAttack(gameObject.GetComponent<Attack>());
			if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER) || _relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL))
			{
				component.InitAimLimiterValues(_previousNonRotatedAimVector, _currentAimLimiterRotation);
			}
			UnlimitedUpgrades component3 = gameObject.GetComponent<UnlimitedUpgrades>();
			if (component3 != null)
			{
				component.gameObject.GetComponent<UnlimitedUpgrades>().timesUpgraded = component3.timesUpgraded;
			}
			if (activePachinkoBall.TryGetComponent<MultiballOnPrevFiredPath>(out var component4))
			{
				component4.multiballDirection = _lastShotAimVector;
			}
			if (activePachinkoBall.TryGetComponent<CreateSpecialPegsWhenDrawn>(out var component5))
			{
				foreach (Peg item in _pegManager.CreateBonusSpecialPegs(component5.specialPegToCreate, component5.numToCreate))
				{
					Peg.OnPegTypeConverted?.Invoke(component5.specialPegToCreate, item.GetCenterOfPeg(), _relicManager.RelicEffectActive(RelicEffect.ALL_BOMBS_RIGGED));
				}
			}
			if (activePachinkoBall.TryGetComponent<AddCoinsWhenDrawn>(out var component6))
			{
				_pegManager.ApplyGoldToPegs(component6.numToCreate);
			}
			if (activePachinkoBall.TryGetComponent<ApplyStatusEffectsToEnemiesWhenDrawn>(out var component7))
			{
				Enemy.OnAllEnemiesMultiBuff?.Invoke(new StatusEffect[1] { component7.effectToApply }, StatusEffectSource.PLAYER);
			}
			PersistentOrb component8 = _activePachinkoBall.GetComponent<PersistentOrb>();
			currentBallIsPersistBonusOrb = false;
			if (component8 != null && component8.remainingPersistence < component8.modifiedPersistLevel)
			{
				currentBallPersistLevel = component8.remainingPersistence;
				if (component8.remainingPersistence < component8.modifiedPersistLevel - 1)
				{
					currentBallIsPersistBonusOrb = true;
				}
			}
			else
			{
				currentBallPersistLevel = -1;
			}
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _activePachinkoBall.transform.DOScale(_activePachinkoBall.transform.localScale, BALL_DRAW_ANIM_TIME).From(Vector3.one * 0.05f).SetEase(Ease.InOutSine);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(ArmBallForShot));
			InitializeAttack(_activePachinkoBall);
			currentAttack.CalculateStaticDamageBuffs(saveResults: true);
			ResolveMagnetBehavior();
			CheckForForcedCritical();
			CheckForStatusEffectAddOnDraw();
			float num = 0f;
			if (_activePachinkoBall.TryGetComponent<AttackDamagesSelf>(out var component9))
			{
				num += component9.selfDamage;
				if (_relicManager.RelicEffectActive(RelicEffect.REDUCE_ORB_SELF_DAMAGE))
				{
					num--;
				}
			}
			if (num > 0f)
			{
				_playerHealthController.ShowSelfDamagePreview(num);
			}
			else
			{
				_playerHealthController.HideSelfDamagePreview();
			}
			_relicManager.UpdatePrimeRodRelic(_shotCount);
			_pegManager.ResolveCritiqualibriumRelic(currentAttack);
			OnBallCreationComplete?.Invoke();
		}

		private void ResolveMagnetBehavior()
		{
			MagnetBehavior component = _activePachinkoBall.GetComponent<MagnetBehavior>();
			if (_relicManager.RelicEffectActive(RelicEffect.PEG_MAGNET) && component == null)
			{
				_activePachinkoBall.AddComponent<MagnetBehavior>().Initialize(4f, 8f);
			}
			else if (_relicManager.RelicEffectActive(RelicEffect.PEG_MAGNET) && component != null)
			{
				component.Initialize(4f, 8f);
			}
			else if (component != null)
			{
				component.Initialize(0f, 0f);
			}
		}

		private void DestroyCurrentBall()
		{
			if (_activePachinkoBall != null)
			{
				_activePachinkoBall.GetComponent<PachinkoBall>().DestroyMultiballsAndSelf();
				_activePachinkoBall = null;
				_activePachinkoBallAttack = null;
			}
		}

		private void FireButtonPressed()
		{
			if (_pachinkoShotTimer > 180f && _activePachinkoBall != null)
			{
				ForceEndShotDestroyAllCurrentBalls();
			}
		}

		public void ForceEndShotDestroyAllCurrentBalls()
		{
			if (_activePachinkoBall != null)
			{
				PachinkoBall component = _activePachinkoBall.GetComponent<PachinkoBall>();
				_pachinkoShotTimer = 0f;
				component.ForceEndShotDestroyMultiballs();
				component.StartDestroy();
				OnShotForcefullyEnded?.Invoke();
			}
		}

		public void AddPegs(List<Peg> newPegs)
		{
			List<Peg> list = new List<Peg>(newPegs.ToArray());
			List<Peg> list2 = new List<Peg>();
			foreach (Peg item in list)
			{
				BouncerPeg bouncerPeg = item as BouncerPeg;
				if (bouncerPeg != null)
				{
					list2.Add(item);
					_bouncerPegs.Add(bouncerPeg);
					_pegManager.AddBouncerPeg(bouncerPeg);
				}
			}
			foreach (Peg item2 in list2)
			{
				list.Remove(item2);
			}
			_allPegs.AddRange(list);
			if (_pegManager != null)
			{
				_pegManager.AddPegs(list);
			}
			_playerStatusEffectController.UpdatePlayerBlindness();
		}

		public void AddBombs(List<Bomb> newBombs)
		{
			if (_pegManager != null)
			{
				_pegManager.AddBombs(newBombs);
			}
		}

		public void ClearPegLists()
		{
			_allPegs.Clear();
			_pegManager.ClearPegList();
		}

		public void ArmNavigationBall()
		{
			if (_activePachinkoBall != null)
			{
				DestroyCurrentBall();
			}
			_battleState = BattleState.NAVIGATION;
			blackHoleNav = -2;
			_criticalHitCount = 0;
			Vector2 vector = pachinkoBallSpawnLocation;
			if (_navOverride.x > -20f)
			{
				vector.x = _navOverride.x;
			}
			else
			{
				vector.x = 0f;
			}
			if (_navOverride.y > -20f)
			{
				vector.y = _navOverride.y;
			}
			_activePachinkoBall = UnityEngine.Object.Instantiate(_navigationOrb, vector, Quaternion.identity);
			_activePachinkoBallAttack = _activePachinkoBall.GetComponent<Attack>();
			_activeAttackIsSummoningCircle = false;
			_activeSummoningCircleAttack = null;
			_predictionManager.SetNavigation(navigation: true);
			PachinkoBall component = _activePachinkoBall.GetComponent<PachinkoBall>();
			component.Init(_relicManager, _deckManager, Vector2.zero, _predictionManager, _playerStatusEffectController);
			if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER) || _relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL))
			{
				IncreaseAimLimiterRotation();
				component.InitAimLimiterValues(_previousNonRotatedAimVector, _currentAimLimiterRotation);
			}
			_activePachinkoBall.GetComponent<TrajectorySimulation>().enabled = false;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _activePachinkoBall.transform.DOScale(_activePachinkoBall.transform.localScale, BALL_DRAW_ANIM_TIME).From(Vector3.one * 0.05f).SetEase(Ease.InOutSine);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(ArmNavigationBallForShot));
		}

		public void PreparePegsForNavigation()
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.pegType != Peg.PegType.MAX_HP && allPeg.pegType != Peg.PegType.SPINFECTION)
				{
					allPeg.Reset();
				}
			}
		}

		public void ArmBallForShot()
		{
			int num = 0;
			if (_relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA))
			{
				num += _relicManager.MATRYOSHKA_MULTIBALL_LEVEL;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.NEW_MATRYORBSHKA))
			{
				num++;
			}
			Multiball component = _activePachinkoBall.GetComponent<Multiball>();
			if (component != null)
			{
				num += component.multiballLevel;
			}
			PachinkoBall component2 = _activePachinkoBall.GetComponent<PachinkoBall>();
			component2.multiballLevel = num;
			component2.Arm();
			component2.SetTrajectorySimulationRadius();
			_activePachinkoBall.GetComponent<TrajectorySimulation>().enabled = true;
			RemoveDamageReductionSlime();
		}

		public void ArmNavigationBallForShot()
		{
			_activePachinkoBall.GetComponent<PachinkoBall>().Arm();
			_remainingPachinkoBalls = 1;
			_activePachinkoBall.GetComponent<TrajectorySimulation>().enabled = true;
			RemoveDamageReductionSlime();
		}

		private void RemoveDamageReductionSlime()
		{
			if (_playerHealthController.damageReductionSlimeUsedThisTurn && !_hasRemovedDamageReductionSlimeThisTurn)
			{
				_hasRemovedDamageReductionSlimeThisTurn = true;
				_playerHealthController.damageReductionSlimeUsedThisTurn = false;
				_pegManager.RemoveSlimeTypeFromAllPegs(Peg.SlimeType.DamageReductionSlime);
			}
			_pegManager.AddQueuedDamageReductionSlime();
		}

		private void InitializeAttack(GameObject ball)
		{
			Attack attack = ball.GetComponent<Attack>();
			SummoningCirclePachinkoBall component = ball.GetComponent<SummoningCirclePachinkoBall>();
			if (component != null)
			{
				attack = component.copiedOrbsAttack;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT))
			{
				attack.gameObject.AddComponent<AddRandomStatusEffectOnHit>();
			}
			_attackManager.Initialize(attack, _enemyManager, pegManager);
			Vector2 playerPosition = _playerTransform.position + _playerTransformOffset;
			attack.Initialize(playerPosition, _attackManager, _relicManager, _deckManager, _cruciballManager, _playerHealthController, _playerStatusEffectController);
		}

		private int GetBombDamage(bool rigged = false)
		{
			int num = (rigged ? 200 : 50);
			if (_cruciballManager.DecreasedBombDamage())
			{
				num = (rigged ? 150 : 40);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_BOMB_DAMAGE))
			{
				num += 10;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_BOMB_DAMAGE2))
			{
				num += 20;
			}
			if (rigged && _relicManager.AttemptUseRelic(RelicEffect.ALL_BOMBS_RIGGED))
			{
				num *= 2;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG))
			{
				num *= 2;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.STRONG_BOMBS_ON_ONLY_TARGET))
			{
				num = Mathf.RoundToInt((float)num * 4f);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.MORE_PIERCE_LESS_BOMB_DAMAGE))
			{
				num = Mathf.RoundToInt((float)num * 0.25f);
			}
			return Mathf.Max(num, 0);
		}

		private void OnAdditionalEnemySpawned(Enemy enemy)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.ADD_BALLUSION_WITH_SPAWNS))
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, 10), StatusEffectSource.PLAYER);
			}
		}

		public static bool AwaitingShotCompletion()
		{
			return _battleState == BattleState.AWAITING_SHOT_COMPLETION;
		}

		public GameObject CreateBramballVine()
		{
			GameObject obj = UnityEngine.Object.Instantiate(pegBoardBramballVine, _pegLayoutParent.transform);
			PegBoardBramballVine component = obj.GetComponent<PegBoardBramballVine>();
			component.battleController = this;
			component.playerStatusEffectController = _playerStatusEffectController;
			return obj;
		}

		public void GiveEnemiesExtraTurns(int numberOfExtraTurns)
		{
			_skipPlayerTurnCount = numberOfExtraTurns;
			if (_skipPlayerTurnCount > 0)
			{
				_postSpawnBattleState = BattleState.ATTACKING;
				_battleState = BattleState.SHOULD_SPAWN;
			}
			else if (_enemyManager.Enemies.Count == 0)
			{
				_postSpawnBattleState = BattleState.AWAITING_SHOT;
				_battleState = BattleState.SHOULD_SPAWN;
			}
			else
			{
				_targetingManager.SetTargetingStatus(canTarget: true, _attackManager.GetCurrentAttackTargetingType());
				_battleState = BattleState.AWAITING_SHOT;
			}
		}

		public void InitializeOrbAttack(Attack attack)
		{
			attack.Initialize(_playerTransform.position, _attackManager, _relicManager, _deckManager, _cruciballManager, _playerHealthController, _playerStatusEffectController);
		}

		public GameObject CreateFlyingCoinEffect()
		{
			return UnityEngine.Object.Instantiate(flyingCoinEffect);
		}

		public void ResetDamageTally()
		{
			_pegMultiplierDamageTally = 0;
			_numPegsHit = 0;
		}

		public long GetCurrentShotDamage()
		{
			return _attackManager.GetCurrentDamage(_pegMultiplierDamageTally, _damageMultiplier, _damageBonus, _criticalHitCount, displayNegative: true);
		}
	}
}
