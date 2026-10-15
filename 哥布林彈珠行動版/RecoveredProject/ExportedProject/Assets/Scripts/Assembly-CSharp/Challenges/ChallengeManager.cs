using System.Collections.Generic;
using System.Linq;
using Cruciball;
using Data;
using Loading;
using Map;
using Peglin.ClassSystem;
using PeglinUtils;
using Relics;
using Saving;
using ToolBox.Serialization;
using UnityEngine;

namespace Challenges
{
	public class ChallengeManager
	{
		public delegate void ChallengeEvent(int act, ChallengeData effect);

		public delegate void ChallengesResetEvent();

		public static ChallengeEvent OnChallengeAdded = delegate
		{
		};

		public static ChallengeEvent OnChallengeCompleted = delegate
		{
		};

		public static ChallengesResetEvent OnChallengesReset = () =>
		{
		};

		public List<ChallengeEffect> ChallengesAvailableInRun = new List<ChallengeEffect>();

		public Queue<ChallengeEffect> ChallengeRandomQueue = new Queue<ChallengeEffect>();

		[HideInInspector]
		public bool chosenToGoToCore;

		[HideInInspector]
		public bool chosenToSkipCore;

		[HideInInspector]
		public bool beatenMinesBossAndCanGoToCore;

		public const float MIMIC_TREASURE_CHANCE = 0.4f;

		public const float MIMIC_QUESTION_MARK_NODE_CHANCE = 0.3f;

		public const float HP_COST_MULTIPLIER = 0.5f;

		public const float BALLWARK_PER_TURN_PERCENT = 0.05f;

		public const float ENEMY_DAMAGE_MULTIPLIER = 1.5f;

		public const int NERFED_DAMAGE_NORMAL = 3;

		public const int NERFED_DAMAGE_CRIT = 3;

		private const int HORRIBALL_AMOUNT = 2;

		public static readonly int DISABLE_RELICS_AMOUNT = 2;

		public static readonly int SPINFECTION_PEG_AMOUNT = 3;

		private DeckManager _deckManager;

		private RelicManager _relicManager;

		private CruciballManager _cruciballManager;

		private Dictionary<ChallengeEffect, ChallengeData> _availableChallenges = new Dictionary<ChallengeEffect, ChallengeData>();

		private Dictionary<int, bool> _challengeCompletedForAct = new Dictionary<int, bool>
		{
			{ 1, false },
			{ 2, false },
			{ 3, false },
			{ 4, false }
		};

		private Dictionary<int, ChallengeEffect> _challengesOfEachAct = new Dictionary<int, ChallengeEffect>
		{
			{
				1,
				ChallengeEffect.NONE
			},
			{
				2,
				ChallengeEffect.NONE
			},
			{
				3,
				ChallengeEffect.NONE
			},
			{
				4,
				ChallengeEffect.NONE
			}
		};

		private const int MAX_MIMIC_BATTLES = 16;

		private static ChallengeManager _instance;

		private Queue<SeededBattleNodeData> _seededMimicBattleQueue = new Queue<SeededBattleNodeData>();

		private List<SeededBattleNodeData> _seededMinibossBattlesForEachAct = new List<SeededBattleNodeData>();

		private float[] _seededMinibossForEachActRoll;

		private List<SeededBattleNodeData> _seededBossBattlesForEachAct = new List<SeededBattleNodeData>();

		private float[] _seededBossForEachActRoll;

		private List<SeededBattleNodeData> _seededQueue = new List<SeededBattleNodeData>();

		[HideInInspector]
		public int currentPainbowSlimeHealth = -1;

		[HideInInspector]
		public int painbowCoinsStolen = -1;

		[HideInInspector]
		public List<RelicEffect> disabledRelics = new List<RelicEffect>();

		public Dictionary<ChallengeEffect, ChallengeData> availableChallenges => _availableChallenges;

		public Dictionary<int, ChallengeEffect> challengesOfEachAct => _challengesOfEachAct;

		public float[] seededBossForEachActRoll => _seededBossForEachActRoll;

		public static ChallengeManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ChallengeManager();
				}
				return _instance;
			}
		}

		public void Init(ChallengeSet globalChallenges, MapController mapController, DeckManager deckManager, RelicManager relicManager, CruciballManager cruciballManager)
		{
			_deckManager = deckManager;
			_relicManager = relicManager;
			_cruciballManager = cruciballManager;
			_availableChallenges = new Dictionary<ChallengeEffect, ChallengeData>();
			PopulateAndSeedChallenges(globalChallenges, mapController, deckManager, relicManager);
			Reset();
		}

		private void Reset()
		{
			chosenToGoToCore = false;
			chosenToSkipCore = false;
			beatenMinesBossAndCanGoToCore = false;
			_challengeCompletedForAct = new Dictionary<int, bool>
			{
				{ 1, false },
				{ 2, false },
				{ 3, false },
				{ 4, false }
			};
			_challengesOfEachAct = new Dictionary<int, ChallengeEffect>
			{
				{
					1,
					ChallengeEffect.NONE
				},
				{
					2,
					ChallengeEffect.NONE
				},
				{
					3,
					ChallengeEffect.NONE
				},
				{
					4,
					ChallengeEffect.NONE
				}
			};
			disabledRelics.Clear();
			OnChallengesReset?.Invoke();
		}

		public void SaveChallengeData()
		{
			ChallengeSaveData challengeSaveData = new ChallengeSaveData();
			challengeSaveData.ForestChallenge = (int)_challengesOfEachAct[1];
			challengeSaveData.ForestChallengeComplete = _challengeCompletedForAct[1];
			challengeSaveData.CastleChallenge = (int)_challengesOfEachAct[2];
			challengeSaveData.CastleChallengeComplete = _challengeCompletedForAct[2];
			challengeSaveData.MinesChallenge = (int)_challengesOfEachAct[3];
			challengeSaveData.MinesChallengeComplete = _challengeCompletedForAct[3];
			challengeSaveData.CoreChallenge = (int)_challengesOfEachAct[4];
			challengeSaveData.CoreChallengeComplete = _challengeCompletedForAct[4];
			challengeSaveData.SetAvailableChallenges(ChallengesAvailableInRun);
			challengeSaveData.SetRandomChallengesQueue(ChallengeRandomQueue);
			challengeSaveData.BeatenMinesBossAndCanGoToCore = beatenMinesBossAndCanGoToCore;
			challengeSaveData.SetMimicChallengeQueue(_seededMimicBattleQueue);
			challengeSaveData.SetNextActMinibossChallengeList(_seededMinibossBattlesForEachAct);
			challengeSaveData.MinibossForEachActRoll = _seededMinibossForEachActRoll;
			challengeSaveData.SetBossChallengeList(_seededBossBattlesForEachAct);
			challengeSaveData.BossForEachActRoll = _seededBossForEachActRoll;
			challengeSaveData.PainbowSlimeCurrentHealth = currentPainbowSlimeHealth;
			challengeSaveData.PainbowSlimeCoinsStolen = painbowCoinsStolen;
			challengeSaveData.Save();
		}

		public void LoadChallengeData(ChallengeSet globalChallenges, MapController mapController, DeckManager deckManager, RelicManager relicManager, CruciballManager cruciballManager)
		{
			_deckManager = deckManager;
			_relicManager = relicManager;
			_cruciballManager = cruciballManager;
			InitializeAvailableChallenges(globalChallenges);
			_challengesOfEachAct = new Dictionary<int, ChallengeEffect>
			{
				{
					1,
					ChallengeEffect.NONE
				},
				{
					2,
					ChallengeEffect.NONE
				},
				{
					3,
					ChallengeEffect.NONE
				},
				{
					4,
					ChallengeEffect.NONE
				}
			};
			_challengeCompletedForAct = new Dictionary<int, bool>
			{
				{ 1, false },
				{ 2, false },
				{ 3, false },
				{ 4, false }
			};
			ChallengesAvailableInRun.Clear();
			ChallengeRandomQueue.Clear();
			ChallengeSaveData challengeSaveData = (ChallengeSaveData)DataSerializer.Load<SaveObjectData>(ChallengeSaveData.KEY, DataSerializer.SaveType.RUN);
			_challengesOfEachAct[1] = (ChallengeEffect)challengeSaveData.ForestChallenge;
			_challengesOfEachAct[2] = (ChallengeEffect)challengeSaveData.CastleChallenge;
			_challengesOfEachAct[3] = (ChallengeEffect)challengeSaveData.MinesChallenge;
			_challengesOfEachAct[4] = (ChallengeEffect)challengeSaveData.CoreChallenge;
			_challengeCompletedForAct[1] = challengeSaveData.ForestChallengeComplete;
			_challengeCompletedForAct[2] = challengeSaveData.CastleChallengeComplete;
			_challengeCompletedForAct[3] = challengeSaveData.MinesChallengeComplete;
			_challengeCompletedForAct[4] = challengeSaveData.CoreChallengeComplete;
			int[] array = challengeSaveData.AvailableChallenges;
			foreach (int item in array)
			{
				ChallengesAvailableInRun.Add((ChallengeEffect)item);
			}
			array = challengeSaveData.RandomChallengesQueue;
			foreach (int item2 in array)
			{
				ChallengeRandomQueue.Enqueue((ChallengeEffect)item2);
			}
			beatenMinesBossAndCanGoToCore = challengeSaveData.BeatenMinesBossAndCanGoToCore;
			_seededMimicBattleQueue.Clear();
			SeededBattleNodeSaveData[] mimicChallengeQueue = challengeSaveData.MimicChallengeQueue;
			foreach (SeededBattleNodeSaveData data in mimicChallengeQueue)
			{
				SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
				seededBattleNodeData.LoadSaveData(data, deckManager, relicManager, mapController);
				_seededMimicBattleQueue.Enqueue(seededBattleNodeData);
			}
			_seededMinibossBattlesForEachAct.Clear();
			mimicChallengeQueue = challengeSaveData.MinibossesForEachAct;
			foreach (SeededBattleNodeSaveData data2 in mimicChallengeQueue)
			{
				SeededBattleNodeData seededBattleNodeData2 = new SeededBattleNodeData();
				seededBattleNodeData2.LoadSaveData(data2, deckManager, relicManager, mapController);
				_seededMinibossBattlesForEachAct.Add(seededBattleNodeData2);
			}
			_seededMinibossForEachActRoll = challengeSaveData.MinibossForEachActRoll;
			_seededBossBattlesForEachAct.Clear();
			mimicChallengeQueue = challengeSaveData.BossesForEachAct;
			foreach (SeededBattleNodeSaveData data3 in mimicChallengeQueue)
			{
				SeededBattleNodeData seededBattleNodeData3 = new SeededBattleNodeData();
				seededBattleNodeData3.LoadSaveData(data3, deckManager, relicManager, mapController);
				_seededBossBattlesForEachAct.Add(seededBattleNodeData3);
			}
			_seededBossForEachActRoll = challengeSaveData.BossForEachActRoll;
			currentPainbowSlimeHealth = challengeSaveData.PainbowSlimeCurrentHealth;
			painbowCoinsStolen = challengeSaveData.PainbowSlimeCoinsStolen;
		}

		private void PopulateAndSeedChallenges(ChallengeSet globalChallengesData, MapController mapController, DeckManager deckManager, RelicManager relicManager)
		{
			InitializeAvailableChallenges(globalChallengesData);
			ChallengesAvailableInRun.Clear();
			foreach (ChallengeData challenge in globalChallengesData.challenges)
			{
				ChallengesAvailableInRun.Add(challenge.effect);
			}
			ChallengesAvailableInRun.Shuffle();
			ChallengeRandomQueue.Clear();
			foreach (ChallengeEffect item in ChallengesAvailableInRun)
			{
				ChallengeRandomQueue.Enqueue(item);
			}
			SeedMimicChallenge(mapController, deckManager, relicManager);
			SeedNextActMinibossChallenge(mapController, deckManager, relicManager);
			SeedBossChallenge(mapController, deckManager, relicManager);
		}

		private void InitializeAvailableChallenges(ChallengeSet globalChallengesData)
		{
			_availableChallenges = new Dictionary<ChallengeEffect, ChallengeData>();
			foreach (ChallengeData challenge in globalChallengesData.challenges)
			{
				_availableChallenges.Add(challenge.effect, challenge);
			}
		}

		public ChallengeData GetDataForChallenge(ChallengeEffect effect)
		{
			return _availableChallenges[effect];
		}

		public static ChallengeData GetDataForChallengeFromGlobal(ChallengeEffect effect, ChallengeSet globalChallenges)
		{
			ChallengeData result = null;
			foreach (ChallengeData challenge in globalChallenges.challenges)
			{
				if (challenge.effect == effect)
				{
					result = challenge;
					break;
				}
			}
			return result;
		}

		public bool AttemptCheckChallenge(ChallengeEffect effect, int actOverride = -1)
		{
			int num;
			if (actOverride != -1)
			{
				num = actOverride;
			}
			else if (MapController.instance != null)
			{
				num = MapController.instance.Act;
			}
			else
			{
				num = -1;
				for (int num2 = _challengesOfEachAct.Count; num2 > 0; num2--)
				{
					if (_challengesOfEachAct[num2] != ChallengeEffect.NONE)
					{
						num = num2;
						break;
					}
				}
				if (num == -1)
				{
					return false;
				}
			}
			if (_challengesOfEachAct[num] == effect && !_challengeCompletedForAct[num])
			{
				return true;
			}
			return false;
		}

		public void AddChallenge(ChallengeEffect effect, int forAct)
		{
			if (!(MapController.instance == null) && _challengesOfEachAct[forAct] == ChallengeEffect.NONE && effect != ChallengeEffect.NONE)
			{
				_challengesOfEachAct[forAct] = effect;
				_challengeCompletedForAct[forAct] = false;
				OnChallengeAdded?.Invoke(forAct, GetDataForChallenge(effect));
				if (DoesChallengeHaveAnImmediateEffect(effect))
				{
					ProcessImmediateChallenge(effect);
				}
				if (IsChallengeCompletionImmediate(effect))
				{
					ClearChallengeForAct(forAct);
				}
				PersistentPlayerData.Instance.SetChallengeUnlocked(effect);
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.challenges = new List<ChallengeEffect>(_challengesOfEachAct.Values.ToArray());
				}
			}
		}

		private void ClearChallengeForAct(int act)
		{
			ChallengeEffect effect = _challengesOfEachAct[act];
			if (DoesChallengeHaveAnEffectOnClear(effect))
			{
				ProcessChallengeOnClearEffect(effect);
			}
			_challengeCompletedForAct[act] = true;
			OnChallengeCompleted?.Invoke(act, GetDataForChallenge(effect));
		}

		public void AttemptClearChallengeForAct(int act)
		{
			if (act > 0 && act < 5 && _challengesOfEachAct[act] != ChallengeEffect.NONE && !_challengeCompletedForAct[act])
			{
				ClearChallengeForAct(act);
			}
		}

		public bool IsAct4Unlocked()
		{
			if (_challengesOfEachAct[2] == ChallengeEffect.NONE || _challengesOfEachAct[3] == ChallengeEffect.NONE)
			{
				return false;
			}
			bool flag = _challengeCompletedForAct[2];
			if (_challengesOfEachAct[1] != ChallengeEffect.NONE)
			{
				flag = flag && _challengeCompletedForAct[1];
			}
			return flag;
		}

		public ChallengeEffect[] GetMultipleChallengesOffOfQueue(int numberOfOptions)
		{
			ChallengeEffect[] array = new ChallengeEffect[numberOfOptions];
			if (ChallengeRandomQueue.Count > 0)
			{
				for (int i = 0; i < numberOfOptions; i++)
				{
					array[i] = ChallengeRandomQueue.Dequeue();
				}
				return array;
			}
			return null;
		}

		public ChallengeEffect[] GetRandomUnseededChallenges(int numberOfOptions)
		{
			ChallengeEffect[] array = new ChallengeEffect[numberOfOptions];
			for (int i = 0; i < numberOfOptions; i++)
			{
				int index = Random.Range(0, ChallengesAvailableInRun.Count);
				array[i] = ChallengesAvailableInRun[index];
				ChallengesAvailableInRun.Remove(array[i]);
			}
			return array;
		}

		public void SeedMimicChallenge(MapController mapController, DeckManager deckManager, RelicManager relicManager)
		{
			SeededBattleNodeData[] array = new SeededBattleNodeData[16];
			for (int i = 0; i < 16; i++)
			{
				SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
				seededBattleNodeData.GenerateMinibossData(deckManager, relicManager, mapController, allowMimic: true, forceMimic: true);
				array[i] = seededBattleNodeData;
			}
			_seededMimicBattleQueue = new Queue<SeededBattleNodeData>(array);
		}

		public void SeedNextActMinibossChallenge(MapController mapController, DeckManager deckManager, RelicManager relicManager)
		{
			SeededBattleNodeData[] array = new SeededBattleNodeData[4];
			for (int i = 1; i <= 4; i++)
			{
				SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
				seededBattleNodeData.GenerateMinibossData(deckManager, relicManager, mapController, allowMimic: false);
				seededBattleNodeData.minibossBattle = null;
				array[i - 1] = seededBattleNodeData;
			}
			_seededMinibossBattlesForEachAct = new List<SeededBattleNodeData>(array);
			_seededMinibossForEachActRoll = MapController.GetMultipleSeededRolls(4);
		}

		public void SeedBossChallenge(MapController mapController, DeckManager deckManager, RelicManager relicManager)
		{
			SeededBattleNodeData[] array = new SeededBattleNodeData[4];
			for (int i = 1; i <= 4; i++)
			{
				SeededBattleNodeData seededBattleNodeData = new SeededBattleNodeData();
				seededBattleNodeData.GenerateBossData(deckManager, relicManager, mapController);
				array[i - 1] = seededBattleNodeData;
			}
			_seededBossBattlesForEachAct = new List<SeededBattleNodeData>(array);
			_seededBossForEachActRoll = MapController.GetMultipleSeededRolls(4);
		}

		public SeededBattleNodeData GetNextSeededMimicBattle()
		{
			SeededBattleNodeData seededBattleNodeData = _seededMimicBattleQueue.Dequeue();
			_seededMimicBattleQueue.Enqueue(seededBattleNodeData);
			return seededBattleNodeData;
		}

		public static int GetHpCostForGold(int gold)
		{
			return Mathf.FloorToInt(0.5f * (float)gold);
		}

		private bool IsChallengeCompletionImmediate(ChallengeEffect effect)
		{
			if (effect == ChallengeEffect.ADD_PEBBALLS)
			{
				return true;
			}
			return false;
		}

		private bool DoesChallengeHaveAnImmediateEffect(ChallengeEffect effect)
		{
			if ((uint)(effect - 7) <= 2u)
			{
				return true;
			}
			return false;
		}

		private void ProcessImmediateChallenge(ChallengeEffect effect)
		{
			switch (effect)
			{
			case ChallengeEffect.ADD_PEBBALLS:
				AddPebballs();
				break;
			case ChallengeEffect.ADD_HORRIBALLS:
				AddHorriballs();
				break;
			case ChallengeEffect.TASTE_THE_PAINBOW:
				SaveInitialPainbowSlimeHealth();
				break;
			}
		}

		private bool DoesChallengeHaveAnEffectOnClear(ChallengeEffect effect)
		{
			if (effect == ChallengeEffect.ADD_HORRIBALLS)
			{
				return true;
			}
			return false;
		}

		private void ProcessChallengeOnClearEffect(ChallengeEffect effect)
		{
			if (effect == ChallengeEffect.ADD_HORRIBALLS)
			{
				_deckManager.RemoveAllOwnedHorriballs();
			}
		}

		public int GetNumberOfOwnedChallenges()
		{
			int num = 0;
			foreach (ChallengeEffect value in _challengesOfEachAct.Values)
			{
				if (value != ChallengeEffect.NONE)
				{
					num++;
				}
			}
			return num;
		}

		public (MapData, SeededMapNodeData) GetExtraChallengeMiniboss(MapController mapController, List<MapDataBattle> potentialEliteBattles)
		{
			MapDataBattle mapDataBattle = potentialEliteBattles[SeededMapNodeData.ConvertPercentageToListIndex(_seededMinibossForEachActRoll[mapController.Act - 1], potentialEliteBattles.Count)];
			SeededBattleNodeData seededBattleNodeData = _seededMinibossBattlesForEachAct[mapController.Act - 1];
			seededBattleNodeData.minibossBattle = mapDataBattle;
			return (mapDataBattle, seededBattleNodeData);
		}

		public (MapData, SeededMapNodeData) GetExtraChallengeBoss(MapController mapController, MapData[] potentialBossBattles)
		{
			MapDataBattle item = potentialBossBattles[SeededMapNodeData.ConvertPercentageToListIndex(_seededBossForEachActRoll[mapController.Act - 1], potentialBossBattles.Length)] as MapDataBattle;
			SeededBattleNodeData item2 = _seededBossBattlesForEachAct[mapController.Act - 1];
			return (item, item2);
		}

		public float GetExtraChallengeBossRoll(int act)
		{
			return _seededBossForEachActRoll[Mathf.Clamp(act - 1, 0, 3)];
		}

		private void AddPebballs()
		{
			GameObject orbPrefab = AssetLoading.Instance.GetOrbPrefab("StoneOrb-Lvl1");
			GameObject orbPrefab2 = AssetLoading.Instance.GetOrbPrefab("StoneOrb-Lvl2");
			GameObject orbPrefab3 = AssetLoading.Instance.GetOrbPrefab("StoneOrb-Lvl3");
			GameObject orbPrefab4 = AssetLoading.Instance.GetOrbPrefab("Oreb-Lvl1");
			GameObject orbPrefab5 = AssetLoading.Instance.GetOrbPrefab("Oreb-Lvl2");
			GameObject orbPrefab6 = AssetLoading.Instance.GetOrbPrefab("Oreb-Lvl3");
			int numberOfOrebsInDeckAtLevel = _deckManager.GetNumberOfOrebsInDeckAtLevel(1);
			int numberOfOrebsInDeckAtLevel2 = _deckManager.GetNumberOfOrebsInDeckAtLevel(2);
			int numberOfOrebsInDeckAtLevel3 = _deckManager.GetNumberOfOrebsInDeckAtLevel(3);
			int num = _deckManager.GetNumberOfStonesInDeckAtLevel(1) - numberOfOrebsInDeckAtLevel;
			int num2 = _deckManager.GetNumberOfStonesInDeckAtLevel(2) - numberOfOrebsInDeckAtLevel2;
			int num3 = _deckManager.GetNumberOfStonesInDeckAtLevel(3) - numberOfOrebsInDeckAtLevel3;
			for (int i = 0; i < num; i++)
			{
				_deckManager.AddOrbToDeck(orbPrefab);
			}
			for (int j = 0; j < num2; j++)
			{
				_deckManager.AddOrbToDeck(orbPrefab2);
			}
			for (int k = 0; k < num3; k++)
			{
				_deckManager.AddOrbToDeck(orbPrefab3);
			}
			for (int l = 0; l < numberOfOrebsInDeckAtLevel; l++)
			{
				_deckManager.AddOrbToDeck(orbPrefab4);
			}
			for (int m = 0; m < numberOfOrebsInDeckAtLevel2; m++)
			{
				_deckManager.AddOrbToDeck(orbPrefab5);
			}
			for (int n = 0; n < numberOfOrebsInDeckAtLevel3; n++)
			{
				_deckManager.AddOrbToDeck(orbPrefab6);
			}
		}

		private void AddHorriballs()
		{
			GameObject orbPrefab = AssetLoading.Instance.GetOrbPrefab("Horriball-Lvl1");
			for (int i = 0; i < 2; i++)
			{
				_deckManager.AddOrbToDeck(orbPrefab);
			}
		}

		private void SaveInitialPainbowSlimeHealth()
		{
			currentPainbowSlimeHealth = -1;
			painbowCoinsStolen = 0;
		}

		public bool AreChallengesAvailable()
		{
			Dictionary<Class, int> cruciballLevels = PersistentPlayerData.Instance.CruciballLevels;
			if (cruciballLevels.Count < 4)
			{
				return false;
			}
			if (cruciballLevels[Class.Peglin] < 0)
			{
				return false;
			}
			if (cruciballLevels[Class.Balladin] < 0)
			{
				return false;
			}
			if (cruciballLevels[Class.Roundrel] < 0)
			{
				return false;
			}
			if (cruciballLevels[Class.Spinventor] < 0)
			{
				return false;
			}
			return true;
		}
	}
}
