using System;
using Battle.Enemies;
using Map;
using Peglin.Achievements;
using Peglin.ClassSystem;
using Relics;
using Saving;
using ToolBox.Serialization;
using UnityEngine;

namespace Cruciball
{
	[CreateAssetMenu(fileName = "CruciballManager", menuName = "Managers/Cruciball")]
	public class CruciballManager : ScriptableObject
	{
		private enum CruciballLevels
		{
			NONE = 0,
			ADDITIONAL_STONES = 1,
			ELITES_IN_SCENARIO = 2,
			FEWER_CRIT_PEGS = 3,
			INCREASED_MISNAVIGATION_DAMAGE = 4,
			FEWER_REFRESH_PEGS = 5,
			REGULAR_ENEMIES_INCREASED_HEALTH = 6,
			REDUCED_POST_BATTLE_HEAL = 7,
			LESS_GOLD_AVAILABLE = 8,
			INCREASED_RIGGED_DMG = 9,
			BOSSES_INCREASED_HEALTH = 10,
			DISCARDABLE_DUD_ORB = 11,
			REDUCED_BOMB_DAMAGE = 12,
			LESS_HEALING_FROM_BOSSES = 13,
			LOWER_MAX_HEALTH = 14,
			DOUBLE_ENEMY_RELOAD_TURNS = 15,
			UNREMOVABLE_DUD_ORB = 16,
			MINIBOSS_INC_DAMAGE = 17,
			SHOP_PRICE_INC = 18,
			REGULAR_BATTLES_ADD_ENEMIES = 19,
			BOSSES_SPECIAL_MECHANICS = 20
		}

		public static Action CruciballLevelChanged = () =>
		{
		};

		public const int NUM_CRUCIBALL_LEVELS = 20;

		public const int AVAILABLE_CRUCIBALL_LEVELS = 20;

		public const int NUM_STONES_TO_ADD = 1;

		public const int WEAK_STONE_DMG_REDUCTION = 0;

		public const int WEAK_STONE_CRIT_REDUCTION = -1;

		public const float ELITE_BATTLE_CHANCE = 0.09f;

		public const float INCREASED_NAV_DAMAGE = 4f;

		public const float CRUCIBALL_POST_BATTLE_HEAL = 0.15f;

		public const int DEFAULT_BATTLE_GOLD = 50;

		public const int CRUCIBALL_BATTLE_GOLD = 40;

		public const int DEFAULT_MINIBOSS_GOLD = 65;

		public const int CRUCIBALL_MINIBOSS_GOLD = 50;

		public const int DEFAULT_BOSS_GOLD = 100;

		public const int CRUCIBALL_BOSS_GOLD = 80;

		private const float FOREST_MINIBOSS_DAMAGE_MOD = 1.1f;

		private const float CASTLE_MINIBOSS_DAMAGE_MOD = 1.2f;

		private const float MINES_MINIBOSS_DAMAGE_MOD = 1.3f;

		private const float CORE_MINIBOSS_DAMAGE_MOD = 1.4f;

		[Tooltip("Also used as the debug cruciball level when running from debug maps.")]
		private int _currentCruciballLevel = -1;

		public Class currentClass;

		public PachinkoBall stonePrefab;

		public PachinkoBall dudOrb1Prefab;

		public PachinkoBall unremovalbeDudOrb1Prefab;

		private bool _cruciballVictoryThisRun;

		private bool _victoryThisRun;

		public int currentCruciballLevel
		{
			get
			{
				return _currentCruciballLevel;
			}
			set
			{
				if (_currentCruciballLevel != value)
				{
					_currentCruciballLevel = value;
					CruciballLevelChanged?.Invoke();
				}
			}
		}

		public bool CruciballVictoryThisRun => _cruciballVictoryThisRun;

		public bool VictoryThisRun => _victoryThisRun;

		public void SetRunCruciballLevelAndClass(int level, Class c)
		{
			currentCruciballLevel = level;
			currentClass = c;
			_cruciballVictoryThisRun = false;
			_victoryThisRun = false;
			StaticGameData.chosenClass = c;
		}

		public void VictoryAchieved()
		{
			_victoryThisRun = true;
			if (AchievementManager.AchievementsOn && PersistentPlayerData.Instance.CruciballLevels[currentClass] < currentCruciballLevel)
			{
				PersistentPlayerData.Instance.CruciballLevels[currentClass] = currentCruciballLevel;
				PersistentPlayerData.Instance.SetLastSelectedCruciball(currentClass, Mathf.Min(currentCruciballLevel + 1, 20));
				_cruciballVictoryThisRun = true;
				PersistentPlayerData.Instance.CheckCruciballAchievements();
			}
		}

		public static bool IsClassUnlocked(Class classToCheck)
		{
			switch (classToCheck)
			{
			case Class.Peglin:
				return true;
			case Class.Balladin:
			{
				if (AchievementData.Achievements[AchievementData.AchievementId.SHIELD_AMOUNT_AT_END_OF_BATTLE].Unlocked)
				{
					return true;
				}
				int num3 = PersistentPlayerData.Instance.BalladinProgress;
				if (StaticGameData.CurrentRunStats != null)
				{
					num3 += StaticGameData.CurrentRunStats.balladinProgress;
				}
				return num3 >= PersistentPlayerData.BALLADIN_PROGRESS_MAX;
			}
			case Class.Roundrel:
			{
				if (AchievementData.Achievements[AchievementData.AchievementId.POISON_AMOUNT_ON_ENEMY].Unlocked)
				{
					return true;
				}
				int num2 = PersistentPlayerData.Instance.RoundrelProgress;
				if (StaticGameData.CurrentRunStats != null)
				{
					num2 += StaticGameData.CurrentRunStats.roundrelProgress;
				}
				return num2 >= PersistentPlayerData.ROUNDREL_PROGRESS_MAX;
			}
			case Class.Spinventor:
			{
				if (AchievementData.Achievements[AchievementData.AchievementId.MULTIPLE_SLIME_BOUNCES].Unlocked)
				{
					return true;
				}
				int num = PersistentPlayerData.Instance.SpinventorProgress;
				if (StaticGameData.CurrentRunStats != null)
				{
					num += StaticGameData.CurrentRunStats.spinventorProgress;
				}
				return num >= PersistentPlayerData.SPINVENTOR_PROGRESS_MAX;
			}
			default:
				return false;
			}
		}

		public bool AtMaxCruciballLevel()
		{
			return currentCruciballLevel == 20;
		}

		public int AdditionalStarterStones()
		{
			if (currentCruciballLevel >= 1)
			{
				return 1;
			}
			return 0;
		}

		public bool WeakStones()
		{
			return currentCruciballLevel >= 1;
		}

		public bool ShouldAddCursedOrb()
		{
			return currentCruciballLevel >= 11;
		}

		public bool ShouldAddUnremovableCursedOrb()
		{
			return currentCruciballLevel >= 16;
		}

		public float GetMinibossDamageModifier()
		{
			if (currentCruciballLevel < 17)
			{
				return 1f;
			}
			return MapController.instance.Act switch
			{
				1 => 1.1f, 
				2 => 1.2f, 
				3 => 1.3f, 
				4 => 1.4f, 
				_ => 1.1f, 
			};
		}

		public static float GetMinibossDamageModifierForLocation(EnemyData.LocationTypes location)
		{
			return location switch
			{
				EnemyData.LocationTypes.FOREST => 1.1f, 
				EnemyData.LocationTypes.CASTLE => 1.2f, 
				EnemyData.LocationTypes.MINES => 1.3f, 
				EnemyData.LocationTypes.CORE => 1.4f, 
				_ => 1f, 
			};
		}

		public bool ShouldIncreaseShopPrices()
		{
			return currentCruciballLevel >= 18;
		}

		public bool CanSpawnBonusEnemies()
		{
			return currentCruciballLevel >= 19;
		}

		public float GetUnknownMapEliteChance()
		{
			if (currentCruciballLevel >= 2)
			{
				return 0.09f;
			}
			return 0f;
		}

		public bool EnemyShouldUseImprovedHealth(bool boss = false, bool miniboss = false)
		{
			if (boss)
			{
				return currentCruciballLevel >= 10;
			}
			return currentCruciballLevel >= 6;
		}

		public float GetModifiedMisnavigationDamage()
		{
			if (currentCruciballLevel >= 4)
			{
				return 4f;
			}
			return 2f;
		}

		public float GetModifiedPostBattleHeal(float defaultHeal)
		{
			if (currentCruciballLevel >= 7)
			{
				return 0.15f;
			}
			return defaultHeal;
		}

		public bool FewerRefreshPegs()
		{
			return currentCruciballLevel >= 5;
		}

		public bool FewerCritPegs()
		{
			return currentCruciballLevel >= 3;
		}

		public int GetStartingGold()
		{
			if (currentCruciballLevel < 8)
			{
				return 15;
			}
			return 0;
		}

		public int GetAvailableBattleGold(RelicRarity battleRarity)
		{
			bool flag = currentCruciballLevel >= 8;
			switch (battleRarity)
			{
			case RelicRarity.RARE:
				if (!flag)
				{
					return 65;
				}
				return 50;
			case RelicRarity.BOSS:
				if (!flag)
				{
					return 100;
				}
				return 80;
			default:
				if (!flag)
				{
					return 50;
				}
				return 40;
			}
		}

		public bool IsLessGoldAvailable()
		{
			return currentCruciballLevel >= 8;
		}

		public int GetModifiedExternalGoldAdd(int gold)
		{
			if (currentCruciballLevel >= 8)
			{
				return Mathf.RoundToInt((float)gold * 0.8f);
			}
			return gold;
		}

		public bool IncreasedRiggedDamage()
		{
			return currentCruciballLevel >= 9;
		}

		public bool DecreasedBombDamage()
		{
			return currentCruciballLevel >= 12;
		}

		public bool ShouldReduceMaxHP()
		{
			return currentCruciballLevel >= 14;
		}

		public bool EnemiesGetExtraReloadTurn()
		{
			return currentCruciballLevel >= 15;
		}

		public bool LessHealingFromBosses()
		{
			return currentCruciballLevel >= 13;
		}

		public bool ShouldUpgradeBosses()
		{
			return currentCruciballLevel >= 20;
		}

		public void SaveCruciballData()
		{
			new CruciballManagerSaveData(currentCruciballLevel, currentClass).Save();
		}

		public void LoadCruciballData()
		{
			CruciballManagerSaveData cruciballManagerSaveData = (CruciballManagerSaveData)DataSerializer.Load<SaveObjectData>(CruciballManagerSaveData.KEY, DataSerializer.SaveType.RUN);
			if (cruciballManagerSaveData != null)
			{
				currentCruciballLevel = cruciballManagerSaveData.CruciballLevel;
				currentClass = cruciballManagerSaveData.Class;
				StaticGameData.chosenClass = cruciballManagerSaveData.Class;
			}
		}
	}
}
