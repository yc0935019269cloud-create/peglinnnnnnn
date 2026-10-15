using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.StatusEffects;
using Challenges;
using Currency;
using Map;
using Newtonsoft.Json;
using Peglin.ClassSystem;
using PeglinUI.LoadoutManager;
using PeglinUtils;
using PixelCrushers.DialogueSystem;
using Saving;
using Seeding;
using ToolBox.Serialization;
using UnityEngine;

namespace Relics
{
	[CreateAssetMenu(fileName = "RelicManager", menuName = "Managers/RelicManager")]
	public class RelicManager : ScriptableObject
	{
		public delegate void RelicManagement(Relic relic);

		public delegate void CountdownRelicEvent(Relic relic, int remainingCountdown);

		private class RelicManagerSaveData : SaveObjectData
		{
			public static readonly string KEY = "RelicManager";

			[JsonProperty]
			[SerializeField]
			private RelicSaveObject[] _relics;

			[JsonProperty]
			[SerializeField]
			private int[] _startingRelics;

			[JsonProperty]
			[SerializeField]
			private RelicCountdownStatus[] _relicCountdowns;

			[JsonProperty]
			[SerializeField]
			private RelicPoolSaveObject _commonRelicPool;

			[JsonProperty]
			[SerializeField]
			private RelicPoolSaveObject _rareRelicPool;

			[JsonProperty]
			[SerializeField]
			private RelicPoolSaveObject _bossRelicPool;

			[JsonProperty]
			[SerializeField]
			private int _selectedClass;

			[JsonProperty]
			[SerializeField]
			private RelicQueueSaveObject _commonRelicQueue;

			[JsonProperty]
			[SerializeField]
			private RelicQueueSaveObject _rareRelicQueue;

			[JsonProperty]
			[SerializeField]
			private RelicQueueSaveObject _bossRelicQueue;

			[JsonProperty]
			[SerializeField]
			private RelicPoolSaveObject _shopRelicQueue;

			[JsonProperty]
			[SerializeField]
			private OrbertsStorySaveObject _orbertsStorySeededTresureData;

			public override string Name => KEY;

			[JsonIgnore]
			public RelicSaveObject[] Relics => _relics;

			[JsonIgnore]
			public int[] StartingRelics => _startingRelics;

			[JsonIgnore]
			public RelicCountdownStatus[] RelicCountdowns => _relicCountdowns;

			[JsonIgnore]
			public RelicPoolSaveObject CommonRelicPool => _commonRelicPool;

			[JsonIgnore]
			public RelicPoolSaveObject RareRelicPool => _rareRelicPool;

			[JsonIgnore]
			public RelicPoolSaveObject BossRelicPool => _bossRelicPool;

			[JsonIgnore]
			public RelicQueueSaveObject CommonRelicQueue => _commonRelicQueue;

			[JsonIgnore]
			public RelicQueueSaveObject RareRelicQueue => _rareRelicQueue;

			[JsonIgnore]
			public RelicQueueSaveObject BossRelicQueue => _bossRelicQueue;

			[JsonIgnore]
			public RelicPoolSaveObject ShopRelicQueue => _shopRelicQueue;

			[JsonIgnore]
			public OrbertsStorySaveObject OrbertsStorySeededTresureData => _orbertsStorySeededTresureData;

			[JsonIgnore]
			public Class SelectedClass
			{
				get
				{
					return (Class)_selectedClass;
				}
				set
				{
					_selectedClass = (int)value;
				}
			}

			public RelicManagerSaveData(RelicSaveObject[] relics, RelicCountdownStatus[] relicCountdowns)
				: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
			{
				_relics = relics;
				_relicCountdowns = relicCountdowns;
			}

			public RelicManagerSaveData()
				: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
			{
			}

			public RelicManagerSaveData(RelicSaveObject[] relics, RelicCountdownStatus[] relicCountdowns, RelicPoolSaveObject commonRelicPool, RelicPoolSaveObject rareRelicPool, RelicPoolSaveObject bossRelicPool, RelicQueueSaveObject commonRelicQueue, RelicQueueSaveObject rareRelicQueue, RelicQueueSaveObject bossRelicQueue, RelicPoolSaveObject shopRelicQueue, int[] startingRelics, Class selectedClass, OrbertsStorySaveObject orbertsStorySeededTresureData)
				: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
			{
				_relics = relics;
				_relicCountdowns = relicCountdowns;
				_commonRelicPool = commonRelicPool;
				_rareRelicPool = rareRelicPool;
				_bossRelicPool = bossRelicPool;
				_commonRelicQueue = commonRelicQueue;
				_rareRelicQueue = rareRelicQueue;
				_bossRelicQueue = bossRelicQueue;
				_shopRelicQueue = shopRelicQueue;
				_startingRelics = startingRelics;
				SelectedClass = selectedClass;
				_orbertsStorySeededTresureData = orbertsStorySeededTresureData;
			}
		}

		private class RelicCountdownStatus : ISerializable
		{
			public int RelicEffect;

			public int RelicCountdown;

			public RelicCountdownStatus(int relicId, int relicCountdown)
			{
				RelicEffect = relicId;
				RelicCountdown = relicCountdown;
			}
		}

		private class RelicSaveObject : ISerializable
		{
			public int RelicEffect;

			public int OrderOfAcquisition;

			public RelicSaveObject(int relicEffect, int orderOfAcquisition)
			{
				RelicEffect = relicEffect;
				OrderOfAcquisition = orderOfAcquisition;
			}
		}

		public class RelicPoolSaveObject : ISerializable
		{
			public int[] RelicEffects;

			public RelicPoolSaveObject(int[] relicEffects)
			{
				RelicEffects = relicEffects;
			}
		}

		public class RelicQueueSaveObject : ISerializable
		{
			public int[] RelicEffects;

			public RelicQueueSaveObject(int[] relicEffects)
			{
				RelicEffects = relicEffects;
			}
		}

		public struct RelicClassRarityOverrides
		{
			public Relic relic;

			public Dictionary<Class, RelicRarity> classRarities;

			public RelicClassRarityOverrides(Relic relic, Dictionary<Class, RelicRarity> classRarities)
			{
				this.relic = relic;
				this.classRarities = classRarities;
			}
		}

		public class OrbertsStorySaveObject : ISerializable
		{
			public SeededTreasureNodeSaveData[] orbertsTreasureData;

			public OrbertsStorySaveObject(SeededTreasureNodeSaveData[] _orbertsTreasureData)
			{
				orbertsTreasureData = _orbertsTreasureData;
			}
		}

		public static readonly float CHEST_RARE_CHANCE = 0.2f;

		private const int MAX_ORBERTS_CHESTS = 20;

		public const float BOMB_SPLASH_DISTANCE = 1.8f;

		public const float REFRESH_SPLASH_DISTANCE = 1.5f;

		public readonly float PEG_TO_BOMB_CHANCE = 0.05f;

		public readonly float REFRESH_POTION_HEAL_AMOUNT = 1f;

		public const float HEAL_ON_RELOAD_AMOUNT = 10f;

		public const int ADDITIONAL_BOMB_DAMAGE = 10;

		public const int ADDITIONAL_BOMB_DAMAGE2 = 20;

		private const float MAX_HP_SMALL_AMOUNT = 15f;

		private const float MAX_HP_MEDIUM_AMOUNT = 25f;

		private const float MAX_HP_LARGE_AMOUNT = 50f;

		public const float INFLIGHT_DAMAGE_INTERVAL = 1.5f;

		public readonly int MATRYOSHKA_MULTIBALL_LEVEL = 1;

		public const int NEW_MATRYOSHKA_MULTIBALL_LEVEL = 1;

		public const int NEW_MATRYOSHKA_REG_DMG_REDUCTION = 2;

		public const int NEW_MATRYOSHKA_CRIT_DMG_REDUCTION = 3;

		public const int BASIC_STONE_DAMAGE_BOOST = 1;

		public const int BASIC_STONE_CRIT_BOOST = 2;

		public const int SUPER_BOOTS_HEAL_AMOUNT = 1;

		public const float ADDITIONAL_END_BATTLE_HEAL_PER_ORB = 0.01f;

		public const float DMG_ON_HEAL_MULT = 3f;

		public static float SKULL_WAND_SELF_DMG = 3f;

		public static float ECHO_CHAMBER_PERCENT = 0.1f;

		public static float LOW_HEALTH_DOUBLE_DAMAGE = 0.25f;

		public static float GUARANTEED_CRIT_HEALTH_PERCENT = 0.5f;

		public static int ALL_ORBS_BUFF_DMG_REDUCTION = 4;

		public static int ALL_ORBS_BUFF_CRIT_REDUCTION = 4;

		public static int ALL_ORBS_BUFF_AMOUNT = 4;

		public static int ALL_ORBS_DEBUFF_AMOUNT = -3;

		public static int FIRST_PEG_HIT_BUFF_AMOUNT = 10;

		public static int ADDITIONAL_BOMB_AMOUNT = 3;

		public static readonly float RELOAD_DAMAGE = 20f;

		public static readonly int SEALED_CONVICTION_BALLANCE = 3;

		public static readonly float DAMAGE_RETURN_MODIFIER = 7f;

		public const float STRONG_SHOT_POWER = 2f;

		public const float WEAK_SHOT_POWER = 0.5f;

		public const int BALLWARK_AMOUNT_ON_SHIELD_CREATE = 4;

		public const int BALLWARK_AMOUNT_ON_SHIELD_DESTROY = 4;

		public const int PEG_SLIMED_BUFF_AMOUNT = 5;

		public const int DUPLICATION_STATION_MULTIBALL = 1;

		public const float BAG_OF_ORANGE_PEGS_DEFAULT_VALUE = 1f;

		public const int UNIQUE_ORB_DAMAGE_BUFF = 1;

		public const int UNIQUE_ORB_CRIT_BUFF = 2;

		public const int BALLUSION_ON_CRIT_AMOUNT = 10;

		public const int SQUIRREL_HITS_UNTIL_SPAWN = 30;

		[HideInInspector]
		public int currentHitsNeededToSpawnSquirrel = 30;

		public const float EXTRA_BALLUSION_LOSS = 0.5f;

		public const float LESS_BALLUSION_LOSS = 0.1f;

		public const int BALLUSION_GAIN_ON_ADDITIONAL_SPAWN = 10;

		public const int NUM_SPINESSE_GAIN_FOR_CRITS = 1;

		public const float SELF_DAMAGE_GAIN_BALLWARK_MULTIPLIER = 2f;

		public const int REFRESH_UPGRADES_PEGS_AMOUNT = 1;

		public const float COINS_PROVIDE_HEALING_PERCENT = 0.15f;

		public const int COINS_PROVIDE_BALLUSION_AMOUNT = 1;

		public const int ATTACKS_APPLY_TRANSPHERENCY_AMOUNT = 3;

		public const int ALL_ATTACKS_APPLY_EXPLOITABALL_AMOUNT = 1;

		public const int ALL_ATTACKS_APPLY_EXPLOITABALL_CRIT_AMOUNT = 2;

		public const int DAMAGE_REDUCTION_SLIME_CREATED_WHEN_DAMAGED = 1;

		public const int TEMP_CRITS_CREATED_ON_CRIT_PEG_HIT = 2;

		public const int START_WITH_EXTRA_COINS_AMOUNT = 10;

		public const float SPINFECTION_DAMAGE_MULTIPLIER = 2f;

		public const float PERCENT_OF_GOLD_GAIN_ON_ENTER_EVENT = 0.2f;

		public const float GRABBY_HAND_FLYER_DAMAGE_MOD = 1.5f;

		public const int HAGLIN_SATCHEL_ADDED_GOLD = 100;

		public const int SAFETY_PEGULATIONS_PIERCE = 2;

		public const float SAFETY_PEGULATIONS_BOMB_DAMAGE_MOD = 0.25f;

		public const float ORBERTS_EXTRA_RARE_CHANCE = 0.1f;

		private Queue<SeededTreasureNodeData> _orbertsStorySeededTreasureDataQueue;

		public const float CHAINS_SPREAD_FACTOR = 0.18f;

		public const float MATRYORBSHKA_SIZE_MULTIPLIER = 0.7071f;

		public const int CHALICE_SPINESSE_AMOUNT = 5;

		public const int DOUBLE_STATUSES_LESS_BASE_DMG = 10;

		public const float DOUBLE_STATUSES_MULT = 2f;

		public const int DIRECT_DAMAGE_ATTACK = 2;

		public const float DIRECT_DAMAGE_STATS_MOD = 2.5f;

		public const float DIRECT_DAMAGE_EXP_MOD = 1.25f;

		public const float REVERSED_GRAVITY_ROOF_FORCE = 845f;

		public const float ADDITIONAL_ATTACK_TIME_DELAY = 0.5f;

		public const float ONLY_TARGET_BOMB_DMG_MULT = 4f;

		public const int MAX_HP_ON_ORB_ADD_AMOUNT = 3;

		public const int DEBUFF_HARDER_AMOUNT = -20;

		public const int LEAF_REFRESH_ADD_COUNT = 10;

		public const float LEAF_MAX_PEGS_REFRESHED_PERCENTAGE = 0.08f;

		public const int LEAF_MIN_PEGS_REFRESHED = 5;

		private static readonly List<Peg.SlimeType> primeSlimeExclusions = new List<Peg.SlimeType>(new Peg.SlimeType[5]
		{
			Peg.SlimeType.None,
			Peg.SlimeType.Slow,
			Peg.SlimeType.Web,
			Peg.SlimeType.Yolk,
			Peg.SlimeType.FlameOverlay
		});

		public static readonly Dictionary<RelicEffect, int> relicCountdownValues = new Dictionary<RelicEffect, int>
		{
			{
				RelicEffect.APPLIES_HEALING_SLIME,
				35
			},
			{
				RelicEffect.LIFESTEAL_PEG_HITS,
				5
			},
			{
				RelicEffect.BOMB_FORCE_ALWAYS,
				3
			},
			{
				RelicEffect.HEAL_ON_PEG_HITS,
				15
			},
			{
				RelicEffect.DAMAGE_TARGETED_PEG_HITS,
				2
			},
			{
				RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG,
				30
			},
			{
				RelicEffect.SPAWN_BOMB_ON_PEG_HITS,
				30
			},
			{
				RelicEffect.ALTERNATE_SHOT_POWER,
				2
			},
			{
				RelicEffect.REFRESH_BUFF,
				4
			},
			{
				RelicEffect.ARMOUR_ON_PEGS_HIT,
				10
			},
			{
				RelicEffect.CONVERT_COIN_TO_DAMAGE,
				3
			},
			{
				RelicEffect.STRENGTH_ON_REFRESH,
				3
			},
			{
				RelicEffect.COINS_TO_CRITS,
				11
			},
			{
				RelicEffect.MULTIBALL_EVERY_X_SHOTS,
				4
			},
			{
				RelicEffect.RANDOM_SLIME_ON_PEGS_HIT,
				17
			},
			{
				RelicEffect.BALLWARK_TO_MUSCIRCLE,
				10
			},
			{
				RelicEffect.GAINING_BALLWARK_GIVES_MUSCIRCLE,
				12
			},
			{
				RelicEffect.GAMBLIN_IOU,
				0
			},
			{
				RelicEffect.CRITS_FOR_SPINESSE,
				3
			},
			{
				RelicEffect.COINS_PROVIDE_HEALING,
				100
			},
			{
				RelicEffect.MORE_TREASURE_NODES,
				4
			},
			{
				RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM,
				2
			},
			{
				RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES,
				0
			}
		};

		private static readonly Dictionary<RelicEffect, int> relicUsesPerBattleCounts = new Dictionary<RelicEffect, int>
		{
			{
				RelicEffect.FREE_RELOAD,
				1
			},
			{
				RelicEffect.PREVENT_FIRST_DAMAGE,
				1
			},
			{
				RelicEffect.START_WITH_STR,
				1
			},
			{
				RelicEffect.DODGE_PUPPET,
				1
			}
		};

		public static readonly Dictionary<RelicEffect, int> relicUsesPerRunCounts = new Dictionary<RelicEffect, int>
		{
			{
				RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT,
				100
			},
			{
				RelicEffect.PREVENT_LETHAL_DAMAGE,
				1
			}
		};

		public static readonly Dictionary<RelicEffect, int> relicUsesPerShotCounts = new Dictionary<RelicEffect, int> { 
		{
			RelicEffect.DEBUFF_SLIME_PEGS_HARDER,
			1
		} };

		public static readonly Dictionary<RelicEffect, RelicEffect> unlockableRelicsOnBeatingTheGame = new Dictionary<RelicEffect, RelicEffect>
		{
			{
				RelicEffect.BOMB_FORCE_ALWAYS,
				RelicEffect.LEGACY_METEORITE
			},
			{
				RelicEffect.CRITS_STACK,
				RelicEffect.ANCIENT_FLEECE
			}
		};

		public static readonly Dictionary<RelicEffect, RelicEffect> unlockableJokeRelics = new Dictionary<RelicEffect, RelicEffect>
		{
			{
				RelicEffect.CRIT_PIT,
				RelicEffect.RANDOM_ENEMY_HEALTH
			},
			{
				RelicEffect.RANDOM_ENEMY_HEALTH,
				RelicEffect.ALTERNATE_SHOT_POWER
			}
		};

		public static RelicManagement OnRelicsReset = delegate
		{
		};

		public static RelicManagement OnRelicAdded = delegate
		{
		};

		public static RelicManagement OnRelicRemoved = delegate
		{
		};

		public static RelicManagement OnRelicUsed = delegate
		{
		};

		public static RelicManagement OnFlashRelic = delegate
		{
		};

		public static RelicManagement OnRelicEnabled = (Relic relic) =>
		{
		};

		public static RelicManagement OnRelicDisabled = (Relic relic) =>
		{
		};

		public static RelicManagement OnRelicPaused = (Relic relic) =>
		{
		};

		public static RelicManagement OnRelicUnpaused = (Relic relic) =>
		{
		};

		public static CountdownRelicEvent OnCountdownDecremented = (Relic relic, int countdown) =>
		{
		};

		[Header("References")]
		[SerializeField]
		private DeckManager _deckManager;

		[SerializeField]
		private SecretSeedManager _secretSeedManager;

		[Header("Relic Information")]
		[Tooltip("These will be included when initialized in a debug build.")]
		[SerializeField]
		private RelicSet _debugRelics;

		[SerializeField]
		private RelicSet _globalRelics;

		[SerializeField]
		private RelicSet _introRelics;

		[SerializeField]
		private ClassLoadoutPairs[] _classLoadouts;

		[NonSerialized]
		private List<RelicClassRarityOverrides> AllCommonRelics = new List<RelicClassRarityOverrides>();

		[NonSerialized]
		private List<RelicClassRarityOverrides> AllRareRelics = new List<RelicClassRarityOverrides>();

		[NonSerialized]
		private List<RelicClassRarityOverrides> AllBossRelics = new List<RelicClassRarityOverrides>();

		[NonSerialized]
		private List<Relic> AllShopRelicsForCurrentClass = new List<Relic>();

		[NonSerialized]
		private Queue<RelicClassRarityOverrides> AllCommonRelicsRandomQueue = new Queue<RelicClassRarityOverrides>();

		[NonSerialized]
		private Queue<RelicClassRarityOverrides> AllRareRelicsRandomQueue = new Queue<RelicClassRarityOverrides>();

		[NonSerialized]
		private Queue<RelicClassRarityOverrides> AllBossRelicsRandomQueue = new Queue<RelicClassRarityOverrides>();

		[NonSerialized]
		private Queue<Relic> AllShopRelicsForCurrentClassRandomQueue = new Queue<Relic>();

		[NonSerialized]
		public List<Relic> CommonRelicPool = new List<Relic>();

		[NonSerialized]
		public List<Relic> RareRelicPool = new List<Relic>();

		[NonSerialized]
		public List<Relic> BossRelicPool = new List<Relic>();

		[NonSerialized]
		public List<Relic> RareScenarioRelicPool = new List<Relic>();

		[NonSerialized]
		public List<Relic> UnavailableRelicPool = new List<Relic>();

		[NonSerialized]
		private List<Relic> _availableCommonRelics = new List<Relic>();

		[NonSerialized]
		private List<Relic> _availableRareRelics = new List<Relic>();

		[NonSerialized]
		private List<Relic> _availableBossRelics = new List<Relic>();

		public Relic consolationPrize;

		[Header("Modifiable Stats")]
		[SerializeField]
		private FloatVariable _playerHealth;

		[SerializeField]
		private FloatVariable _maxPlayerHealth;

		[SerializeField]
		private FloatVariable _reductionToMaxPlayerHealth;

		[Header("Other")]
		public GameObject squirrelball;

		private Dictionary<RelicEffect, Relic> _ownedRelics;

		private Dictionary<RelicEffect, int> _relicRemainingCountdowns;

		private Dictionary<RelicEffect, int> _relicRemainingUsesPerBattle;

		private Dictionary<RelicEffect, int> _relicRemainingUsesPerShot;

		private Dictionary<RelicEffect, int> _relicRemainingUsesPerRun;

		private List<RelicEffect> _disabledRelics = new List<RelicEffect>();

		private int _orderCounter;

		private Dictionary<RelicEffect, int> _orderOfRelicsObtained = new Dictionary<RelicEffect, int>();

		private Class _selectedClass;

		public RelicSet globalRelics => _globalRelics;

		public RelicSet limitedIntroRelics => _introRelics;

		public Queue<RelicClassRarityOverrides> CurrentCommonRelicsQueue => AllCommonRelicsRandomQueue;

		public Queue<RelicClassRarityOverrides> CurrentRareRelicsQueue => AllRareRelicsRandomQueue;

		public Queue<RelicClassRarityOverrides> CurrentBossRelicsQueue => AllBossRelicsRandomQueue;

		public Queue<Relic> CurrentShopRelicsQueue => AllShopRelicsForCurrentClassRandomQueue;

		public List<Relic> ShopRelicPool => AllShopRelicsForCurrentClass;

		public List<RelicEffect> disabledRelics => _disabledRelics;

		public static float GetBagOfOrangePegsMultiplier(float pegsRemainingPercentage)
		{
			if ((double)pegsRemainingPercentage >= 0.6)
			{
				return 1f;
			}
			if (pegsRemainingPercentage >= 0.3f)
			{
				return 2f;
			}
			if (pegsRemainingPercentage >= 0.1f)
			{
				return 3f;
			}
			if (pegsRemainingPercentage >= 0.01f)
			{
				return 4f;
			}
			return 10f;
		}

		public static Peg.SlimeType GetRandomPrimeSlimeType()
		{
			string[] names = Enum.GetNames(typeof(Peg.SlimeType));
			List<Peg.SlimeType> list = new List<Peg.SlimeType>();
			string[] array = names;
			foreach (string value in array)
			{
				Peg.SlimeType item = (Peg.SlimeType)Enum.Parse(typeof(Peg.SlimeType), value);
				if (!primeSlimeExclusions.Contains(item))
				{
					list.Add(item);
				}
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public void DoCoinsProvideHealingRelicsEffect(int amountOfGoldAdded)
		{
			AttemptUseCountdownRelicManyTimes(RelicEffect.COINS_PROVIDE_HEALING, amountOfGoldAdded, CoinsProvideHealingRelicCallback);
		}

		private void CoinsProvideHealingRelicCallback()
		{
			PlayerHealthController.OnExternalPercentageHealRequested(0.15f);
		}

		public void ResetRelicPools()
		{
			CommonRelicPool.Clear();
			RareRelicPool.Clear();
			BossRelicPool.Clear();
			RareScenarioRelicPool.Clear();
			UnavailableRelicPool.Clear();
		}

		public void SetupInternalRelicPools()
		{
			_availableCommonRelics.Clear();
			_availableCommonRelics.AddRange(CommonRelicPool);
			_availableRareRelics.Clear();
			_availableRareRelics.AddRange(RareRelicPool);
			_availableBossRelics.Clear();
			_availableBossRelics.AddRange(BossRelicPool);
			SeedRelicDeckOrder();
		}

		public void Reset()
		{
			_ownedRelics = new Dictionary<RelicEffect, Relic>();
			_relicRemainingCountdowns = new Dictionary<RelicEffect, int>();
			_relicRemainingUsesPerBattle = new Dictionary<RelicEffect, int>();
			_relicRemainingUsesPerShot = new Dictionary<RelicEffect, int>();
			_relicRemainingUsesPerRun = new Dictionary<RelicEffect, int>();
			_orderOfRelicsObtained.Clear();
			_orderCounter = 0;
			OnRelicsReset(null);
			if (_debugRelics != null && Debug.isDebugBuild)
			{
				if (_maxPlayerHealth.Value == 0f)
				{
					_playerHealth.Reset();
					_maxPlayerHealth.Reset();
					_reductionToMaxPlayerHealth.Reset();
				}
				foreach (Relic relic in _debugRelics.relics)
				{
					AddRelic(relic);
				}
			}
			_orbertsStorySeededTreasureDataQueue = null;
			_disabledRelics.Clear();
		}

		public Relic GetRelicOfRarity(RelicRarity rarity)
		{
			return rarity switch
			{
				RelicRarity.COMMON => GetCommonRelic(fallback: true), 
				RelicRarity.RARE => GetRareRelic(fallback: true), 
				RelicRarity.BOSS => GetBossRelic(fallback: true), 
				_ => null, 
			};
		}

		private void CheckForDebugReset(List<Relic> listToCheck)
		{
			if (Debug.isDebugBuild && (listToCheck == null || listToCheck.Count == 0) && (_ownedRelics == null || _ownedRelics.Count == 0))
			{
				Debug.LogWarning("Warning: Relic Manager Resetting/Initing itself for debug!");
				Reset();
				MapController mapController = UnityEngine.Object.FindObjectOfType<MapController>();
				PopulateRelicPools(mapController.debugClass);
				SetupInternalRelicPools();
			}
		}

		public Relic GetCommonRelic(bool fallback = false)
		{
			CheckForDebugReset(_availableCommonRelics);
			if (_availableCommonRelics.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, _availableCommonRelics.Count);
				return _availableCommonRelics[index];
			}
			if (fallback)
			{
				return GetRareRelic();
			}
			return null;
		}

		public Relic GetRareRelic(bool fallback = false)
		{
			CheckForDebugReset(_availableRareRelics);
			if (_availableRareRelics.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, _availableRareRelics.Count);
				return _availableRareRelics[index];
			}
			if (fallback)
			{
				return GetCommonRelic();
			}
			return null;
		}

		public Relic GetBossRelic(bool fallback = false)
		{
			CheckForDebugReset(_availableBossRelics);
			if (_availableBossRelics.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, _availableBossRelics.Count);
				return _availableBossRelics[index];
			}
			if (fallback)
			{
				return GetRareRelic(fallback: true);
			}
			return null;
		}

		public void PopulateDialogueOwnedRelics()
		{
			DialogueLua.SetVariable("garden_gloves", RelicEffectActive(RelicEffect.REDUCE_ORB_SELF_DAMAGE));
			DialogueLua.SetVariable("infernal_ingot", RelicEffectActive(RelicEffect.LIFESTEAL_PEG_HITS));
			DialogueLua.SetVariable("collusion_detection", RelicEffectActive(RelicEffect.PRICE_FIXING));
			DialogueLua.SetVariable("suffer_sling", RelicEffectActive(RelicEffect.BASIC_STONE_BONUS_DAMAGE));
		}

		public Relic[] GetMultipleRelicsOfRarity(int number, RelicRarity rarity, bool fallback = true)
		{
			CheckForDebugReset(_availableCommonRelics);
			List<Relic> list = new List<Relic>();
			List<Relic> list2 = rarity switch
			{
				RelicRarity.COMMON => _availableCommonRelics, 
				RelicRarity.RARE => _availableRareRelics, 
				RelicRarity.NONE => new List<Relic> { consolationPrize }, 
				_ => _availableBossRelics, 
			};
			if (list2.Count == number)
			{
				return list2.ToArray();
			}
			if (rarity == RelicRarity.COMMON && list2.Count == 0)
			{
				return new Relic[1] { consolationPrize };
			}
			if ((list2.Count < number) & fallback)
			{
				List<Relic> list3 = list2.ToList();
				RelicRarity relicRarity = rarity;
				if (relicRarity > RelicRarity.NONE)
				{
					relicRarity--;
				}
				list3.AddRange(GetMultipleRelicsOfRarity(number - list2.Count, relicRarity));
				return list3.ToArray();
			}
			while (list.Count < number)
			{
				Relic relicOfRarity = GetRelicOfRarity(rarity);
				if (!list.Contains(relicOfRarity))
				{
					list.Add(relicOfRarity);
				}
			}
			return list.ToArray();
		}

		public Relic GetRelicFromLimitedList()
		{
			return limitedIntroRelics.relics[UnityEngine.Random.Range(0, limitedIntroRelics.relics.Count)];
		}

		public Relic[] GetMultipleRelicsOffOfQueue(int number, RelicRarity rarity, bool allowShopOnlyRelics = false)
		{
			List<Relic> list = new List<Relic>();
			for (int i = 0; i < number; i++)
			{
				list.Add(GetRelicOfRarityOffOfQueue(rarity, secretPoolFallback: false, allowShopOnlyRelics));
			}
			return list.ToArray();
		}

		public Relic[] CheckAndUpdateSeededRelicOffers(Relic[] relics, RelicRarity[] rarities, Relic[] extraRelicsToNotAllowInList = null, bool fallback = true, bool allowShopOnlyRelics = false)
		{
			List<Relic> list = new List<Relic>();
			if (extraRelicsToNotAllowInList != null)
			{
				list.AddRange(extraRelicsToNotAllowInList);
			}
			List<Relic> list2 = new List<Relic>();
			for (int i = 0; i < relics.Length; i++)
			{
				Relic item = CheckAndUpdateSeededRelic(relics[i], rarities[i], list, fallback, allowShopOnlyRelics);
				list2.Add(item);
				list.Add(item);
			}
			List<Relic> list3 = list2.Distinct().ToList();
			if (list3.Count == 1 && list3[0] == consolationPrize)
			{
				return new Relic[1] { consolationPrize };
			}
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				if (list2[num] == consolationPrize && list2.Count != 1)
				{
					list2.RemoveAt(num);
				}
			}
			return list2.ToArray();
		}

		private Relic CheckAndUpdateSeededRelic(Relic relic, RelicRarity rarity, List<Relic> cannotBeInList, bool fallback = true, bool allowShopOnlyRelics = false)
		{
			if (relic == consolationPrize)
			{
				return relic;
			}
			List<RelicClassRarityOverrides> allPosibleRelicsOfRarityList = GetAllPosibleRelicsOfRarityList(rarity);
			Queue<RelicClassRarityOverrides> allPosibleRelicsOfRarityQueue = GetAllPosibleRelicsOfRarityQueue(rarity);
			int index = allPosibleRelicsOfRarityList.FindIndex(0, allPosibleRelicsOfRarityList.Count, (RelicClassRarityOverrides x) => x.relic.effect == relic.effect);
			RelicClassRarityOverrides relicClassRarityOverrides = allPosibleRelicsOfRarityList[index];
			for (int num = 0; num < allPosibleRelicsOfRarityList.Count; num++)
			{
				if (CheckIfRelicValid(relicClassRarityOverrides, rarity, cannotBeInList, allowShopOnlyRelics))
				{
					break;
				}
				relicClassRarityOverrides = allPosibleRelicsOfRarityQueue.Dequeue();
				allPosibleRelicsOfRarityQueue.Enqueue(relicClassRarityOverrides);
			}
			Relic newRelic = relicClassRarityOverrides.relic;
			bool flag = false;
			if (!CheckIfRelicValid(relicClassRarityOverrides, rarity, cannotBeInList, allowShopOnlyRelics))
			{
				CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
				if (customPoolsSecretSeed != null && customPoolsSecretSeed.relicPoolFallback)
				{
					newRelic = GetRelicOfRarityOffOfQueue(rarity, secretPoolFallback: true);
					allPosibleRelicsOfRarityList = GetAllPosibleRelicsOfRarityList(rarity, forceNonSecret: true);
					index = allPosibleRelicsOfRarityList.FindIndex(0, allPosibleRelicsOfRarityList.Count, (RelicClassRarityOverrides x) => x.relic.effect == newRelic.effect);
					relicClassRarityOverrides = allPosibleRelicsOfRarityList[index];
					flag = true;
				}
			}
			bool flag2 = true;
			while (!CheckIfRelicValid(relicClassRarityOverrides, rarity, cannotBeInList, allowShopOnlyRelics) & fallback)
			{
				if (rarity > RelicRarity.NONE)
				{
					if (flag)
					{
						if (!flag2)
						{
							flag2 = true;
						}
						else
						{
							flag2 = false;
							rarity--;
						}
					}
					else
					{
						rarity--;
					}
					newRelic = GetRelicOfRarityOffOfQueue(rarity, flag2);
					allPosibleRelicsOfRarityList = GetAllPosibleRelicsOfRarityList(rarity, flag2);
					index = allPosibleRelicsOfRarityList.FindIndex(0, allPosibleRelicsOfRarityList.Count, (RelicClassRarityOverrides x) => x.relic.effect == newRelic.effect);
					relicClassRarityOverrides = allPosibleRelicsOfRarityList[index];
					continue;
				}
				return consolationPrize;
			}
			if (!CheckIfRelicValid(relicClassRarityOverrides, rarity, cannotBeInList, allowShopOnlyRelics) && !fallback)
			{
				return consolationPrize;
			}
			return newRelic;
		}

		public Relic GetRelicOfRarityOffOfQueue(RelicRarity rarity, bool secretPoolFallback = false, bool allowShopOnlyRelics = false)
		{
			Queue<RelicClassRarityOverrides> allPosibleRelicsOfRarityQueue = GetAllPosibleRelicsOfRarityQueue(rarity, secretPoolFallback);
			RelicClassRarityOverrides relicClassRarityOverrides = allPosibleRelicsOfRarityQueue.Dequeue();
			allPosibleRelicsOfRarityQueue.Enqueue(relicClassRarityOverrides);
			for (int i = 0; i < allPosibleRelicsOfRarityQueue.Count; i++)
			{
				if (CheckIfRelicValid(relicClassRarityOverrides, rarity, null, allowShopOnlyRelics))
				{
					break;
				}
				relicClassRarityOverrides = allPosibleRelicsOfRarityQueue.Dequeue();
				allPosibleRelicsOfRarityQueue.Enqueue(relicClassRarityOverrides);
			}
			return relicClassRarityOverrides.relic;
		}

		private bool CheckIfRelicValid(RelicClassRarityOverrides relicClassRarityStruct, RelicRarity rarity, List<Relic> cannotBeInList = null, bool allowShopOnlyRelics = false)
		{
			Relic relic = relicClassRarityStruct.relic;
			if (relic.effect == consolationPrize.effect)
			{
				return true;
			}
			if (cannotBeInList != null && cannotBeInList.Contains(relic))
			{
				return false;
			}
			if (relicClassRarityStruct.classRarities[_selectedClass] != rarity)
			{
				return false;
			}
			if (_ownedRelics.ContainsKey(relic.effect))
			{
				return false;
			}
			ClassLoadoutPairs[] classLoadouts = _classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class == _selectedClass)
				{
					if (allowShopOnlyRelics || !classLoadoutPairs.Loadout.ShopRelicOverrides.relics.Contains(relic))
					{
						break;
					}
					return false;
				}
			}
			return true;
		}

		public bool IsRelicShopOnlyRelic(Relic relic)
		{
			ClassLoadoutPairs[] classLoadouts = _classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class == _selectedClass)
				{
					if (!classLoadoutPairs.Loadout.ShopRelicOverrides.relics.Contains(relic))
					{
						break;
					}
					return true;
				}
			}
			return false;
		}

		public Relic GetShopOnlyRelicOffOfQueue()
		{
			Relic relic = AllShopRelicsForCurrentClassRandomQueue.Dequeue();
			AllShopRelicsForCurrentClassRandomQueue.Enqueue(relic);
			bool flag = false;
			for (int i = 0; i < AllShopRelicsForCurrentClassRandomQueue.Count; i++)
			{
				if (relic != null && !_ownedRelics.ContainsKey(relic.effect))
				{
					flag = true;
					break;
				}
				relic = AllShopRelicsForCurrentClassRandomQueue.Dequeue();
				AllShopRelicsForCurrentClassRandomQueue.Enqueue(relic);
			}
			if (!flag)
			{
				return null;
			}
			return relic;
		}

		private List<RelicClassRarityOverrides> GetAllPosibleRelicsOfRarityList(RelicRarity rarity, bool forceNonSecret = false)
		{
			CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
			if (forceNonSecret || customPoolsSecretSeed == null)
			{
				switch (rarity)
				{
				case RelicRarity.COMMON:
					return AllCommonRelics;
				case RelicRarity.RARE:
					return AllRareRelics;
				case RelicRarity.BOSS:
					return AllBossRelics;
				case RelicRarity.NONE:
				{
					Dictionary<Class, RelicRarity> dictionary = new Dictionary<Class, RelicRarity>();
					ClassLoadoutPairs[] classLoadouts = _classLoadouts;
					for (int i = 0; i < classLoadouts.Length; i++)
					{
						ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
						dictionary[classLoadoutPairs.Class] = rarity;
					}
					return new List<RelicClassRarityOverrides>
					{
						new RelicClassRarityOverrides(consolationPrize, dictionary)
					};
				}
				default:
					Debug.LogError("No relics of type " + rarity.ToString() + " found.");
					return new List<RelicClassRarityOverrides>();
				}
			}
			switch (rarity)
			{
			case RelicRarity.COMMON:
				return customPoolsSecretSeed.commonRelicPool;
			case RelicRarity.RARE:
				return customPoolsSecretSeed.rareRelicPool;
			case RelicRarity.BOSS:
				return customPoolsSecretSeed.bossRelicPool;
			case RelicRarity.NONE:
			{
				Dictionary<Class, RelicRarity> dictionary2 = new Dictionary<Class, RelicRarity>();
				ClassLoadoutPairs[] classLoadouts = _classLoadouts;
				for (int i = 0; i < classLoadouts.Length; i++)
				{
					ClassLoadoutPairs classLoadoutPairs2 = classLoadouts[i];
					dictionary2[classLoadoutPairs2.Class] = rarity;
				}
				return new List<RelicClassRarityOverrides>
				{
					new RelicClassRarityOverrides(consolationPrize, dictionary2)
				};
			}
			default:
				Debug.LogError("No relics of type " + rarity.ToString() + " found in secret seed.");
				return new List<RelicClassRarityOverrides>();
			}
		}

		private Queue<RelicClassRarityOverrides> GetAllPosibleRelicsOfRarityQueue(RelicRarity rarity, bool forceNonSecret = false)
		{
			CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
			Queue<RelicClassRarityOverrides> queue;
			if (forceNonSecret || (customPoolsSecretSeed != null && rarity == RelicRarity.NONE) || customPoolsSecretSeed == null)
			{
				switch (rarity)
				{
				case RelicRarity.COMMON:
					queue = AllCommonRelicsRandomQueue;
					break;
				case RelicRarity.RARE:
					queue = AllRareRelicsRandomQueue;
					break;
				case RelicRarity.BOSS:
					queue = AllBossRelicsRandomQueue;
					break;
				case RelicRarity.NONE:
				{
					Dictionary<Class, RelicRarity> dictionary = new Dictionary<Class, RelicRarity>();
					ClassLoadoutPairs[] classLoadouts = _classLoadouts;
					for (int i = 0; i < classLoadouts.Length; i++)
					{
						ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
						dictionary[classLoadoutPairs.Class] = rarity;
					}
					RelicClassRarityOverrides item = new RelicClassRarityOverrides(consolationPrize, dictionary);
					queue = new Queue<RelicClassRarityOverrides>();
					queue.Enqueue(item);
					break;
				}
				default:
					Debug.LogError("No relics of type " + rarity.ToString() + " found.");
					queue = new Queue<RelicClassRarityOverrides>();
					break;
				}
			}
			else
			{
				switch (rarity)
				{
				case RelicRarity.COMMON:
					queue = customPoolsSecretSeed.commonRelicQueue;
					break;
				case RelicRarity.RARE:
					queue = customPoolsSecretSeed.rareRelicQueue;
					break;
				case RelicRarity.BOSS:
					queue = customPoolsSecretSeed.bossRelicQueue;
					break;
				case RelicRarity.NONE:
				{
					Dictionary<Class, RelicRarity> dictionary2 = new Dictionary<Class, RelicRarity>();
					ClassLoadoutPairs[] classLoadouts = _classLoadouts;
					for (int i = 0; i < classLoadouts.Length; i++)
					{
						ClassLoadoutPairs classLoadoutPairs2 = classLoadouts[i];
						dictionary2[classLoadoutPairs2.Class] = rarity;
					}
					RelicClassRarityOverrides item2 = new RelicClassRarityOverrides(consolationPrize, dictionary2);
					queue = new Queue<RelicClassRarityOverrides>();
					queue.Enqueue(item2);
					break;
				}
				default:
					Debug.LogError("No relics of type " + rarity.ToString() + " found in secret seed pool.");
					queue = new Queue<RelicClassRarityOverrides>();
					break;
				}
			}
			return queue;
		}

		public Relic[] GetMultipleCommonRelicsWithRareChance(int totalRelics, float rareRelicChance)
		{
			List<Relic> list = new List<Relic>();
			if (UnityEngine.Random.Range(0f, 1f) <= rareRelicChance)
			{
				list.Add(GetRareRelic());
				list.AddRange(GetMultipleRelicsOfRarity(totalRelics - 1, RelicRarity.COMMON));
			}
			else
			{
				list.AddRange(GetMultipleRelicsOfRarity(totalRelics, RelicRarity.COMMON));
			}
			return list.ToArray();
		}

		public void AddRelic(Relic relic)
		{
			if (_ownedRelics == null)
			{
				Reset();
			}
			if (relic.effect != RelicEffect.NONE)
			{
				if (_ownedRelics.ContainsKey(relic.effect))
				{
					Debug.LogWarning($"RelicManager::AddRelic():{relic.effect} already exists in _ownedRelics, most likely because of debug relic data. Skipping...");
					return;
				}
				PersistentPlayerData.Instance.SetRelicUnlocked(relic.effect);
				_ownedRelics.Add(relic.effect, relic);
				_orderOfRelicsObtained.Add(relic.effect, _orderCounter);
				_orderCounter++;
				if (relicCountdownValues.ContainsKey(relic.effect))
				{
					_relicRemainingCountdowns[relic.effect] = relicCountdownValues[relic.effect];
				}
				if (relicUsesPerBattleCounts.ContainsKey(relic.effect))
				{
					_relicRemainingUsesPerBattle[relic.effect] = relicUsesPerBattleCounts[relic.effect];
				}
				if (relicUsesPerShotCounts.ContainsKey(relic.effect))
				{
					_relicRemainingUsesPerShot[relic.effect] = relicUsesPerShotCounts[relic.effect];
				}
				if (relicUsesPerRunCounts.ContainsKey(relic.effect))
				{
					_relicRemainingUsesPerRun[relic.effect] = relicUsesPerRunCounts[relic.effect];
				}
				_availableCommonRelics.Remove(relic);
				_availableRareRelics.Remove(relic);
				_availableBossRelics.Remove(relic);
				float num = 0f;
				switch (relic.effect)
				{
				case RelicEffect.MAX_HEALTH_SMALL:
					num = 15f;
					if (AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
					{
						num++;
					}
					_maxPlayerHealth.Add(num);
					_playerHealth.Add(num);
					break;
				case RelicEffect.MAX_HEALTH_MEDIUM:
					num = 25f;
					if (AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
					{
						num++;
					}
					_maxPlayerHealth.Add(num);
					_playerHealth.Add(num);
					break;
				case RelicEffect.MAX_HEALTH_LARGE:
					num = 50f;
					if (AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
					{
						num++;
					}
					_maxPlayerHealth.Add(num);
					_playerHealth.Add(num);
					break;
				case RelicEffect.ADD_ORBS_AND_UPGRADE:
					_deckManager.RelicModifyDeck(relic);
					CurrencyManager.Instance.AddGold(100);
					break;
				}
			}
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.relics.Add(relic.effect);
			}
			OnRelicAdded(relic);
		}

		public void RemoveRelic(Relic relic)
		{
			RemoveRelic(relic.effect);
			int num = _orderOfRelicsObtained[relic.effect];
			_orderOfRelicsObtained.Remove(relic.effect);
			List<RelicEffect> list = new List<RelicEffect>();
			foreach (RelicEffect key in _orderOfRelicsObtained.Keys)
			{
				if (_orderOfRelicsObtained[key] > num)
				{
					list.Add(key);
				}
			}
			foreach (RelicEffect item in list)
			{
				_orderOfRelicsObtained[item]--;
			}
			_orderCounter--;
		}

		public void ResetBattleRelics()
		{
			foreach (RelicEffect key in relicUsesPerBattleCounts.Keys)
			{
				_relicRemainingUsesPerBattle[key] = relicUsesPerBattleCounts[key];
				if (_ownedRelics.ContainsKey(key))
				{
					OnRelicEnabled?.Invoke(_ownedRelics[key]);
				}
			}
		}

		public void ResetPerShotRelics()
		{
			foreach (RelicEffect key in relicUsesPerShotCounts.Keys)
			{
				_relicRemainingUsesPerShot[key] = relicUsesPerShotCounts[key];
				if (_ownedRelics.ContainsKey(key))
				{
					OnRelicEnabled?.Invoke(_ownedRelics[key]);
				}
			}
		}

		public int RemainingUsesPerBattle(RelicEffect re)
		{
			if (!_relicRemainingUsesPerBattle.ContainsKey(re))
			{
				return -1;
			}
			return _relicRemainingUsesPerBattle[re];
		}

		public void UpdateHealthDependentRelics()
		{
			float num = _playerHealth.Value / _maxPlayerHealth.Value;
			if (_ownedRelics.ContainsKey(RelicEffect.LOW_HEALTH_GUARANTEED_CRIT))
			{
				if (num <= GUARANTEED_CRIT_HEALTH_PERCENT)
				{
					OnRelicEnabled?.Invoke(_ownedRelics[RelicEffect.LOW_HEALTH_GUARANTEED_CRIT]);
				}
				else
				{
					OnRelicDisabled?.Invoke(_ownedRelics[RelicEffect.LOW_HEALTH_GUARANTEED_CRIT]);
				}
			}
			if (_ownedRelics.ContainsKey(RelicEffect.LOW_HEALTH_INCREASED_DAMAGE))
			{
				if (num <= LOW_HEALTH_DOUBLE_DAMAGE)
				{
					OnRelicEnabled?.Invoke(_ownedRelics[RelicEffect.LOW_HEALTH_INCREASED_DAMAGE]);
				}
				else
				{
					OnRelicDisabled?.Invoke(_ownedRelics[RelicEffect.LOW_HEALTH_INCREASED_DAMAGE]);
				}
			}
		}

		public void UpdateStatusDependentRelics(PlayerStatusEffectController playerStatusEffectController)
		{
			if (_ownedRelics.ContainsKey(RelicEffect.CONFUSION_RELIC))
			{
				if (playerStatusEffectController.HasNegativeStatusEffect())
				{
					OnRelicEnabled?.Invoke(_ownedRelics[RelicEffect.CONFUSION_RELIC]);
				}
				else
				{
					OnRelicDisabled?.Invoke(_ownedRelics[RelicEffect.CONFUSION_RELIC]);
				}
			}
		}

		public void UpdatePrimeRodRelic(int shotCount)
		{
			if (RelicEffectActive(RelicEffect.ALL_ATTACKS_PIERCE))
			{
				BattleController.UpdateRelicCountdownText?.Invoke(RelicEffect.ALL_ATTACKS_PIERCE, shotCount.ToString());
				if (IsNumberPrime(shotCount))
				{
					OnRelicEnabled?.Invoke(_ownedRelics[RelicEffect.ALL_ATTACKS_PIERCE]);
				}
				else
				{
					OnRelicDisabled?.Invoke(_ownedRelics[RelicEffect.ALL_ATTACKS_PIERCE]);
				}
			}
		}

		private void LoadRelicFromSaveFile(Relic relic)
		{
			if (_ownedRelics == null)
			{
				Reset();
			}
			if (relic.effect != RelicEffect.NONE)
			{
				if (_ownedRelics.ContainsKey(relic.effect))
				{
					Debug.LogWarning($"RelicManager::AddRelic():{relic.effect} already exists in _ownedRelics, most likely because of debug relic data. Skipping...");
					return;
				}
				_ownedRelics.Add(relic.effect, relic);
				_orderOfRelicsObtained.Add(relic.effect, _orderCounter);
				_orderCounter++;
				_availableCommonRelics.Remove(relic);
				_availableRareRelics.Remove(relic);
				_availableBossRelics.Remove(relic);
			}
			OnRelicAdded(relic);
		}

		private void LoadRelicCountdownsFromSaveFile(RelicCountdownStatus[] countdowns)
		{
			foreach (RelicCountdownStatus relicCountdownStatus in countdowns)
			{
				RelicEffect relicEffect = (RelicEffect)relicCountdownStatus.RelicEffect;
				if (relicCountdownValues.ContainsKey(relicEffect))
				{
					_relicRemainingCountdowns[relicEffect] = relicCountdownStatus.RelicCountdown;
					if (_ownedRelics.ContainsKey(relicEffect))
					{
						OnCountdownDecremented?.Invoke(_ownedRelics[relicEffect], relicCountdownStatus.RelicCountdown);
					}
				}
				else
				{
					if (!relicUsesPerRunCounts.ContainsKey(relicEffect))
					{
						continue;
					}
					_relicRemainingUsesPerRun[relicEffect] = relicCountdownStatus.RelicCountdown;
					if (_ownedRelics.ContainsKey(relicEffect))
					{
						if (relicUsesPerRunCounts[relicEffect] > 1)
						{
							OnCountdownDecremented?.Invoke(_ownedRelics[relicEffect], relicCountdownStatus.RelicCountdown);
						}
						if (relicCountdownStatus.RelicCountdown <= 0)
						{
							OnRelicDisabled?.Invoke(_ownedRelics[relicEffect]);
						}
					}
				}
			}
		}

		public void RemoveRelic(RelicEffect re)
		{
			if (_ownedRelics.ContainsKey(re))
			{
				Relic relic = _ownedRelics[re];
				_ownedRelics.Remove(re);
				OnRelicRemoved(relic);
				switch (relic.effect)
				{
				case RelicEffect.MAX_HEALTH_SMALL:
					_maxPlayerHealth.Subtract(15f);
					_playerHealth.Add(0f);
					break;
				case RelicEffect.MAX_HEALTH_MEDIUM:
					_maxPlayerHealth.Subtract(25f);
					_playerHealth.Add(0f);
					break;
				case RelicEffect.MAX_HEALTH_LARGE:
					_maxPlayerHealth.Subtract(50f);
					_playerHealth.Add(0f);
					break;
				}
			}
		}

		public bool RelicEffectActive(RelicEffect re)
		{
			if (_ownedRelics != null && _ownedRelics.ContainsKey(re))
			{
				return !_disabledRelics.Contains(re);
			}
			return false;
		}

		public bool AttemptUseRelic(RelicEffect re)
		{
			if (_ownedRelics == null)
			{
				Reset();
			}
			if (_ownedRelics.ContainsKey(re) && !_disabledRelics.Contains(re))
			{
				if (relicCountdownValues.ContainsKey(re))
				{
					int num = (_relicRemainingCountdowns.ContainsKey(re) ? _relicRemainingCountdowns[re] : relicCountdownValues[re]);
					if (--num <= 0)
					{
						OnRelicUsed?.Invoke(_ownedRelics[re]);
						_relicRemainingCountdowns[re] = relicCountdownValues[re];
						return true;
					}
					_relicRemainingCountdowns[re] = num;
					OnCountdownDecremented(_ownedRelics[re], num);
					return false;
				}
				if (relicUsesPerBattleCounts.ContainsKey(re))
				{
					int num2 = (_relicRemainingUsesPerBattle.ContainsKey(re) ? _relicRemainingUsesPerBattle[re] : relicUsesPerBattleCounts[re]);
					if (--num2 >= 0)
					{
						OnRelicUsed?.Invoke(_ownedRelics[re]);
						_relicRemainingUsesPerBattle[re] = num2;
						if (num2 == 0)
						{
							OnRelicDisabled?.Invoke(_ownedRelics[re]);
						}
						return true;
					}
					return false;
				}
				if (relicUsesPerShotCounts.ContainsKey(re))
				{
					int num3 = (_relicRemainingUsesPerShot.ContainsKey(re) ? _relicRemainingUsesPerShot[re] : relicUsesPerShotCounts[re]);
					if (--num3 >= 0)
					{
						OnRelicUsed?.Invoke(_ownedRelics[re]);
						_relicRemainingUsesPerShot[re] = num3;
						OnCountdownDecremented?.Invoke(_ownedRelics[re], num3);
						if (num3 == 0)
						{
							OnRelicDisabled?.Invoke(_ownedRelics[re]);
						}
						return true;
					}
					return false;
				}
				if (relicUsesPerRunCounts.ContainsKey(re))
				{
					int num4 = (_relicRemainingUsesPerRun.ContainsKey(re) ? _relicRemainingUsesPerRun[re] : relicUsesPerRunCounts[re]);
					if (--num4 >= 0)
					{
						OnRelicUsed?.Invoke(_ownedRelics[re]);
						_relicRemainingUsesPerRun[re] = num4;
						OnCountdownDecremented?.Invoke(_ownedRelics[re], num4);
						if (num4 == 0)
						{
							OnRelicDisabled?.Invoke(_ownedRelics[re]);
						}
						return true;
					}
					return false;
				}
				Relic relic = _ownedRelics[re];
				OnRelicUsed(relic);
				return true;
			}
			return false;
		}

		public void FlashRelic(RelicEffect re)
		{
			if (_ownedRelics.ContainsKey(re) && !_disabledRelics.Contains(re))
			{
				Relic relic = _ownedRelics[re];
				OnFlashRelic(relic);
			}
		}

		public int GetRemainingCountdownForRelic(RelicEffect effect)
		{
			if (_relicRemainingCountdowns.ContainsKey(effect))
			{
				return _relicRemainingCountdowns[effect];
			}
			return 0;
		}

		public void SetRemainingCountdownForRelic(RelicEffect effect, int countdown)
		{
			_relicRemainingCountdowns[effect] = countdown;
			OnCountdownDecremented?.Invoke(_ownedRelics[effect], countdown);
		}

		public void AttemptUseCountdownRelicManyTimes(RelicEffect relicEffect, int numberOfTimesToUse, Action relicFunctionCallback)
		{
			if (!_ownedRelics.ContainsKey(relicEffect) || _disabledRelics.Contains(relicEffect))
			{
				return;
			}
			int num = (_relicRemainingCountdowns.ContainsKey(relicEffect) ? _relicRemainingCountdowns[relicEffect] : relicCountdownValues[relicEffect]);
			int num2 = relicCountdownValues[relicEffect];
			int num3 = num - numberOfTimesToUse;
			if (num3 > 0)
			{
				SetRemainingCountdownForRelic(relicEffect, num3);
				return;
			}
			int num4 = Mathf.FloorToInt(Math.Abs(num3) / num2) + 1;
			for (int i = 0; i < num4; i++)
			{
				OnRelicUsed?.Invoke(_ownedRelics[relicEffect]);
				relicFunctionCallback();
			}
			int countdown = num2 - Math.Abs(num3) % num2;
			SetRemainingCountdownForRelic(relicEffect, countdown);
		}

		public Relic GetRelicForEffect(RelicEffect effect)
		{
			if (_availableCommonRelics == null || _availableRareRelics == null || _availableBossRelics == null)
			{
				Reset();
			}
			foreach (Relic item in _availableCommonRelics.Union(_availableRareRelics).Union(_availableBossRelics).ToList())
			{
				if (effect == item.effect)
				{
					return item;
				}
			}
			Debug.LogWarning("RelicManager::GetRelicForEffect(): Relic for effect " + effect.ToString() + " not found.");
			return null;
		}

		public Relic GetRelicForEffectFromAllData(RelicEffect effect)
		{
			if (effect == RelicEffect.NONE)
			{
				return consolationPrize;
			}
			foreach (Relic relic in _globalRelics.relics)
			{
				if (effect == relic.effect)
				{
					return relic;
				}
			}
			Debug.LogWarning("RelicManager::GetRelicForEffectFromAllData(): Relic for effect " + effect.ToString() + " not found.");
			return null;
		}

		public List<Relic> GetRelicsForEffects(List<RelicEffect> effects)
		{
			List<Relic> results = new List<Relic>();
			if (effects.Count > 0)
			{
				GetRelicsFromPool(CommonRelicPool, effects, ref results);
			}
			if (effects.Count > 0)
			{
				GetRelicsFromPool(RareRelicPool, effects, ref results);
			}
			if (effects.Count > 0)
			{
				GetRelicsFromPool(BossRelicPool, effects, ref results);
			}
			if (effects.Count > 0)
			{
				GetRelicsFromPool(UnavailableRelicPool, effects, ref results);
			}
			if (effects.Count > 0)
			{
				GetRelicsFromPool(RareScenarioRelicPool, effects, ref results);
			}
			return results;
		}

		private static void GetRelicsFromPool(List<Relic> pool, List<RelicEffect> effects, ref List<Relic> results)
		{
			foreach (Relic item in pool)
			{
				if (effects.Contains(item.effect))
				{
					results.Add(item);
					effects.Remove(item.effect);
					if (effects.Count == 0)
					{
						break;
					}
				}
			}
		}

		public void SaveRelicData()
		{
			if (_ownedRelics == null)
			{
				Debug.LogWarning("SaveRelicData(): Owned Relics is null. This shouldn't happen when requesting a save. Skipping save...");
				return;
			}
			List<RelicSaveObject> list = new List<RelicSaveObject>();
			foreach (RelicEffect key in _ownedRelics.Keys)
			{
				RelicSaveObject item = new RelicSaveObject((int)key, _orderOfRelicsObtained[key]);
				list.Add(item);
			}
			List<RelicCountdownStatus> list2 = CreateRelicCountdownsForSave();
			RelicPoolSaveObject relicPoolSaveObject = GetRelicPoolSaveObject(_availableCommonRelics);
			RelicPoolSaveObject relicPoolSaveObject2 = GetRelicPoolSaveObject(_availableRareRelics);
			RelicPoolSaveObject relicPoolSaveObject3 = GetRelicPoolSaveObject(_availableBossRelics);
			RelicQueueSaveObject relicQueueSaveObject = GetRelicQueueSaveObject(AllCommonRelicsRandomQueue);
			RelicQueueSaveObject relicQueueSaveObject2 = GetRelicQueueSaveObject(AllRareRelicsRandomQueue);
			RelicQueueSaveObject relicQueueSaveObject3 = GetRelicQueueSaveObject(AllBossRelicsRandomQueue);
			RelicPoolSaveObject relicPoolSaveObject4 = GetRelicPoolSaveObject(AllShopRelicsForCurrentClassRandomQueue.ToList());
			List<int> list3 = new List<int>();
			if (StaticGameData.StartingRelics != null)
			{
				for (int i = 0; i < StaticGameData.StartingRelics.Length; i++)
				{
					int effect = (int)StaticGameData.StartingRelics[i].effect;
					list3.Add(effect);
				}
			}
			SeededTreasureNodeSaveData[] array = new SeededTreasureNodeSaveData[20];
			if (_orbertsStorySeededTreasureDataQueue != null)
			{
				for (int j = 0; j < _orbertsStorySeededTreasureDataQueue.Count; j++)
				{
					SeededTreasureNodeData seededTreasureNodeData = _orbertsStorySeededTreasureDataQueue.Dequeue();
					_orbertsStorySeededTreasureDataQueue.Enqueue(seededTreasureNodeData);
					array[j] = seededTreasureNodeData.GetSaveData() as SeededTreasureNodeSaveData;
				}
			}
			OrbertsStorySaveObject orbertsStorySeededTresureData = new OrbertsStorySaveObject(array);
			new RelicManagerSaveData(list.ToArray(), list2.ToArray(), relicPoolSaveObject, relicPoolSaveObject2, relicPoolSaveObject3, relicQueueSaveObject, relicQueueSaveObject2, relicQueueSaveObject3, relicPoolSaveObject4, list3.ToArray(), _selectedClass, orbertsStorySeededTresureData).Save();
		}

		private RelicPoolSaveObject GetRelicPoolSaveObject(List<Relic> pool)
		{
			List<int> list = new List<int>();
			foreach (Relic item in pool)
			{
				list.Add((int)item.effect);
			}
			return new RelicPoolSaveObject(list.ToArray());
		}

		public RelicQueueSaveObject GetRelicQueueSaveObject(Queue<RelicClassRarityOverrides> relicQueue)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < relicQueue.Count; i++)
			{
				RelicClassRarityOverrides item = relicQueue.Dequeue();
				relicQueue.Enqueue(item);
				list.Add((int)item.relic.effect);
			}
			return new RelicQueueSaveObject(list.ToArray());
		}

		public void LoadRelicData()
		{
			_availableCommonRelics.Clear();
			_availableRareRelics.Clear();
			_availableBossRelics.Clear();
			RelicManagerSaveData saveData = (RelicManagerSaveData)DataSerializer.Load<SaveObjectData>(RelicManagerSaveData.KEY, DataSerializer.SaveType.RUN);
			if (saveData == null)
			{
				return;
			}
			PopulateRelicPools(saveData.SelectedClass);
			List<RelicSaveObject> list = saveData.Relics.OrderBy((RelicSaveObject x) => x.OrderOfAcquisition).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				foreach (Relic relic in globalRelics.relics)
				{
					if (relic.effect == (RelicEffect)list[num].RelicEffect)
					{
						LoadRelicFromSaveFile(relic);
					}
				}
			}
			List<Relic> list2 = new List<Relic>();
			foreach (Relic relic2 in globalRelics.relics)
			{
				if (saveData.CommonRelicPool.RelicEffects.Contains((int)relic2.effect))
				{
					_availableCommonRelics.Add(relic2);
				}
				else if (saveData.RareRelicPool.RelicEffects.Contains((int)relic2.effect))
				{
					_availableRareRelics.Add(relic2);
				}
				else if (saveData.BossRelicPool.RelicEffects.Contains((int)relic2.effect))
				{
					_availableBossRelics.Add(relic2);
				}
				if (saveData.StartingRelics.Contains((int)relic2.effect))
				{
					list2.Add(relic2);
				}
			}
			AllCommonRelicsRandomQueue.Clear();
			AllRareRelicsRandomQueue.Clear();
			AllBossRelicsRandomQueue.Clear();
			AllShopRelicsForCurrentClassRandomQueue.Clear();
			int i;
			for (i = 0; i < saveData.CommonRelicQueue.RelicEffects.Length; i++)
			{
				AllCommonRelicsRandomQueue.Enqueue(AllCommonRelics.Find((RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)saveData.CommonRelicQueue.RelicEffects[i]));
			}
			int i2;
			for (i2 = 0; i2 < saveData.RareRelicQueue.RelicEffects.Length; i2++)
			{
				AllRareRelicsRandomQueue.Enqueue(AllRareRelics.Find((RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)saveData.RareRelicQueue.RelicEffects[i2]));
			}
			int i3;
			for (i3 = 0; i3 < saveData.BossRelicQueue.RelicEffects.Length; i3++)
			{
				AllBossRelicsRandomQueue.Enqueue(AllBossRelics.Find((RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)saveData.BossRelicQueue.RelicEffects[i3]));
			}
			if (saveData.ShopRelicQueue != null)
			{
				int i4;
				for (i4 = 0; i4 < saveData.ShopRelicQueue.RelicEffects.Length; i4++)
				{
					AllShopRelicsForCurrentClassRandomQueue.Enqueue(AllShopRelicsForCurrentClass.Find((Relic x) => x.effect == (RelicEffect)saveData.ShopRelicQueue.RelicEffects[i4]));
				}
			}
			else
			{
				AllShopRelicsForCurrentClassRandomQueue = new Queue<Relic>(AllShopRelicsForCurrentClass);
			}
			StaticGameData.StartingRelics = list2.ToArray();
			LoadRelicCountdownsFromSaveFile(saveData.RelicCountdowns);
			if (saveData.OrbertsStorySeededTresureData != null && saveData.OrbertsStorySeededTresureData.orbertsTreasureData != null && saveData.OrbertsStorySeededTresureData.orbertsTreasureData.Length != 0 && saveData.OrbertsStorySeededTresureData.orbertsTreasureData[0] != null)
			{
				SeededTreasureNodeData[] array = new SeededTreasureNodeData[20];
				for (int num2 = 0; num2 < saveData.OrbertsStorySeededTresureData.orbertsTreasureData.Length; num2++)
				{
					SeededTreasureNodeData seededTreasureNodeData = new SeededTreasureNodeData();
					seededTreasureNodeData.LoadSaveData(saveData.OrbertsStorySeededTresureData.orbertsTreasureData[num2], _deckManager, this, null);
					array[num2] = seededTreasureNodeData;
				}
				_orbertsStorySeededTreasureDataQueue = new Queue<SeededTreasureNodeData>(array);
			}
			else
			{
				SeedOrbertsStoryChests();
			}
		}

		public static int GetDeckSizeHealValue()
		{
			return Mathf.FloorToInt((float)DeckManager.DeckSize * 0.01f * 100f);
		}

		public Relic[] GetOrderedListOfOwnedRelics()
		{
			Relic[] array = new Relic[_orderOfRelicsObtained.Count];
			foreach (RelicEffect key in _orderOfRelicsObtained.Keys)
			{
				int num = _orderOfRelicsObtained[key];
				array[num] = _ownedRelics[key];
			}
			return array;
		}

		private List<RelicCountdownStatus> CreateRelicCountdownsForSave()
		{
			List<RelicCountdownStatus> list = new List<RelicCountdownStatus>();
			foreach (KeyValuePair<RelicEffect, int> relicRemainingCountdown in _relicRemainingCountdowns)
			{
				RelicCountdownStatus item = new RelicCountdownStatus((int)relicRemainingCountdown.Key, relicRemainingCountdown.Value);
				list.Add(item);
			}
			foreach (KeyValuePair<RelicEffect, int> item3 in _relicRemainingUsesPerRun)
			{
				RelicCountdownStatus item2 = new RelicCountdownStatus((int)item3.Key, item3.Value);
				list.Add(item2);
			}
			return list;
		}

		public void PopulateRelicPools(Class cl)
		{
			CommonRelicPool.Clear();
			RareRelicPool.Clear();
			BossRelicPool.Clear();
			RareScenarioRelicPool.Clear();
			UnavailableRelicPool.Clear();
			AllCommonRelics.Clear();
			AllRareRelics.Clear();
			AllBossRelics.Clear();
			AllShopRelicsForCurrentClass.Clear();
			_selectedClass = cl;
			ClassLoadoutData classLoadoutData = null;
			ClassLoadoutPairs[] classLoadouts = _classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class == cl)
				{
					classLoadoutData = classLoadoutPairs.Loadout;
				}
			}
			if (classLoadoutData == null)
			{
				Debug.LogError("Missing class loadout in loadout data!");
				return;
			}
			foreach (Relic relic in globalRelics.relics)
			{
				switch (GetFinalRarityForRelic(relic, classLoadoutData))
				{
				case RelicRarity.COMMON:
					CommonRelicPool.Add(relic);
					break;
				case RelicRarity.RARE:
					RareRelicPool.Add(relic);
					break;
				case RelicRarity.BOSS:
					BossRelicPool.Add(relic);
					break;
				case RelicRarity.NONE:
					RareScenarioRelicPool.Add(relic);
					break;
				case RelicRarity.UNAVAILABLE:
					UnavailableRelicPool.Add(relic);
					break;
				}
			}
			PopulateAllRelicsForQueues();
			AddClassSpecificRelicsToPools(classLoadoutData);
		}

		private void PopulateAllRelicsForQueues()
		{
			ClassLoadoutData classLoadoutData = null;
			ClassLoadoutPairs[] classLoadouts = _classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class == _selectedClass)
				{
					classLoadoutData = classLoadoutPairs.Loadout;
					break;
				}
			}
			foreach (Relic relic in _globalRelics.relics)
			{
				Dictionary<Class, RelicRarity> dictionary = new Dictionary<Class, RelicRarity>();
				List<RelicRarity> list = new List<RelicRarity>();
				classLoadouts = _classLoadouts;
				for (int i = 0; i < classLoadouts.Length; i++)
				{
					ClassLoadoutPairs classLoadoutPairs2 = classLoadouts[i];
					if (classLoadoutPairs2.Loadout.CommonRelicOverrides != null && classLoadoutPairs2.Loadout.CommonRelicOverrides.relics.Contains(relic))
					{
						dictionary[classLoadoutPairs2.Class] = RelicRarity.COMMON;
						if (!list.Contains(RelicRarity.COMMON))
						{
							list.Add(RelicRarity.COMMON);
						}
					}
					else if (classLoadoutPairs2.Loadout.RareRelicOverrides != null && classLoadoutPairs2.Loadout.RareRelicOverrides.relics.Contains(relic))
					{
						dictionary[classLoadoutPairs2.Class] = RelicRarity.RARE;
						if (!list.Contains(RelicRarity.RARE))
						{
							list.Add(RelicRarity.RARE);
						}
					}
					else if (classLoadoutPairs2.Loadout.BossRelicOverrides != null && classLoadoutPairs2.Loadout.BossRelicOverrides.relics.Contains(relic))
					{
						dictionary[classLoadoutPairs2.Class] = RelicRarity.BOSS;
						if (!list.Contains(RelicRarity.BOSS))
						{
							list.Add(RelicRarity.BOSS);
						}
					}
					else if (classLoadoutPairs2.Loadout.UnavailableRelicOverrides != null && classLoadoutPairs2.Loadout.UnavailableRelicOverrides.relics.Contains(relic))
					{
						dictionary[classLoadoutPairs2.Class] = RelicRarity.UNAVAILABLE;
						if (!list.Contains(RelicRarity.UNAVAILABLE))
						{
							list.Add(RelicRarity.UNAVAILABLE);
						}
					}
					else if (classLoadoutPairs2.Loadout.RareScenarioRelicOverrides != null && classLoadoutPairs2.Loadout.RareScenarioRelicOverrides.relics.Contains(relic))
					{
						dictionary[classLoadoutPairs2.Class] = RelicRarity.NONE;
						if (!list.Contains(RelicRarity.NONE))
						{
							list.Add(RelicRarity.NONE);
						}
					}
					else
					{
						dictionary[classLoadoutPairs2.Class] = relic.globalRarity;
						if (!list.Contains(relic.globalRarity))
						{
							list.Add(relic.globalRarity);
						}
					}
				}
				foreach (RelicRarity item in list)
				{
					switch (item)
					{
					case RelicRarity.COMMON:
						AllCommonRelics.Add(new RelicClassRarityOverrides(relic, dictionary));
						break;
					case RelicRarity.RARE:
						AllRareRelics.Add(new RelicClassRarityOverrides(relic, dictionary));
						break;
					case RelicRarity.BOSS:
						AllBossRelics.Add(new RelicClassRarityOverrides(relic, dictionary));
						break;
					}
				}
				if (classLoadoutData.ShopRelicOverrides.relics.Contains(relic))
				{
					AllShopRelicsForCurrentClass.Add(relic);
				}
			}
		}

		private RelicRarity GetFinalRarityForRelic(Relic r, ClassLoadoutData classLoadout)
		{
			if (classLoadout.CommonRelicOverrides != null && classLoadout.CommonRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.COMMON;
			}
			if (classLoadout.RareRelicOverrides != null && classLoadout.RareRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.RARE;
			}
			if (classLoadout.BossRelicOverrides != null && classLoadout.BossRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.BOSS;
			}
			if (classLoadout.RareScenarioRelicOverrides != null && classLoadout.RareScenarioRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.NONE;
			}
			if (classLoadout.UnavailableRelicOverrides != null && classLoadout.UnavailableRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.UNAVAILABLE;
			}
			return r.globalRarity;
		}

		private void AddClassSpecificRelicsToPools(ClassLoadoutData classLoadoutData)
		{
			if (classLoadoutData.CommonRelicOverrides != null)
			{
				AddRelicOverridesToPool(classLoadoutData.CommonRelicOverrides.relics, CommonRelicPool);
			}
			if (classLoadoutData.RareRelicOverrides != null)
			{
				AddRelicOverridesToPool(classLoadoutData.RareRelicOverrides.relics, RareRelicPool);
			}
			if (classLoadoutData.BossRelicOverrides != null)
			{
				AddRelicOverridesToPool(classLoadoutData.BossRelicOverrides.relics, BossRelicPool);
			}
			if (classLoadoutData.RareScenarioRelicOverrides != null)
			{
				AddRelicOverridesToPool(classLoadoutData.RareScenarioRelicOverrides.relics, RareScenarioRelicPool);
			}
		}

		private static void AddRelicOverridesToPool(List<Relic> overrideList, List<Relic> pool)
		{
			foreach (Relic @override in overrideList)
			{
				if (!pool.Contains(@override))
				{
					pool.Add(@override);
				}
			}
		}

		private void SeedRelicDeckOrder()
		{
			AllCommonRelics.Shuffle();
			AllRareRelics.Shuffle();
			AllBossRelics.Shuffle();
			AllShopRelicsForCurrentClass.Shuffle();
			AllCommonRelicsRandomQueue = new Queue<RelicClassRarityOverrides>(AllCommonRelics);
			AllRareRelicsRandomQueue = new Queue<RelicClassRarityOverrides>(AllRareRelics);
			AllBossRelicsRandomQueue = new Queue<RelicClassRarityOverrides>(AllBossRelics);
			AllShopRelicsForCurrentClassRandomQueue = new Queue<Relic>(AllShopRelicsForCurrentClass);
		}

		public void SeedOrbertsStoryChests(MapController _mapController = null)
		{
			SeededTreasureNodeData[] array = new SeededTreasureNodeData[20];
			for (int i = 0; i < 20; i++)
			{
				SeededTreasureNodeData seededTreasureNodeData = new SeededTreasureNodeData();
				seededTreasureNodeData.GenerateData(_deckManager, this, _mapController);
				array[i] = seededTreasureNodeData;
			}
			_orbertsStorySeededTreasureDataQueue = new Queue<SeededTreasureNodeData>(array);
		}

		public SeededTreasureNodeData GetNextOrbertsStorySeededTreasureData()
		{
			SeededTreasureNodeData seededTreasureNodeData = _orbertsStorySeededTreasureDataQueue.Dequeue();
			_orbertsStorySeededTreasureDataQueue.Enqueue(seededTreasureNodeData);
			return seededTreasureNodeData;
		}

		public float GetShopRelicChanceForCurrentClass()
		{
			ClassLoadoutPairs[] classLoadouts = _classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class == _selectedClass)
				{
					return classLoadoutPairs.Loadout.chanceForGuaranteedShopRelicInShops;
				}
			}
			return 1f;
		}

		public static bool IsNumberPrime(int num)
		{
			if (num == 2 || num == 3)
			{
				return true;
			}
			if (num <= 1 || num % 2 == 0 || num % 3 == 0)
			{
				return false;
			}
			for (int i = 2; i < Mathf.CeilToInt(Mathf.Sqrt(num)) + 1; i++)
			{
				if (num % i == 0)
				{
					return false;
				}
			}
			return true;
		}

		public bool WandOfGreedEffectActive()
		{
			if (RelicEffectActive(RelicEffect.DOUBLE_COINS_AND_PRICES))
			{
				return !ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE);
			}
			return false;
		}

		public void PrintOrbertsStorySeededData()
		{
			string text = "Orberts Story Data (rare chance rolls): ";
			foreach (SeededTreasureNodeData item in _orbertsStorySeededTreasureDataQueue)
			{
				text = text + item.rareRelicChanceRoll + ", ";
			}
			Debug.Log(text);
		}

		public int GetRelicCount()
		{
			return _ownedRelics.Count;
		}

		public void DisableRandomRelicsForBattle(int amount)
		{
			ClearDisabledRelics();
			List<RelicEffect> list = new List<RelicEffect>(_ownedRelics.Keys.ToArray());
			for (int i = 0; i < amount; i++)
			{
				if (list.Count <= 0)
				{
					break;
				}
				RelicEffect relicEffect = list[UnityEngine.Random.Range(0, list.Count)];
				_disabledRelics.Add(relicEffect);
				list.Remove(relicEffect);
				OnRelicPaused?.Invoke(_ownedRelics[relicEffect]);
			}
		}

		public void ClearDisabledRelics()
		{
			foreach (RelicEffect disabledRelic in _disabledRelics)
			{
				OnRelicUnpaused?.Invoke(_ownedRelics[disabledRelic]);
			}
			_disabledRelics.Clear();
		}
	}
}
