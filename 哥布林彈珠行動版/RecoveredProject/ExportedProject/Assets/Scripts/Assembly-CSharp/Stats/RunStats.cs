using System;
using System.Collections.Generic;
using System.Diagnostics;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Challenges;
using Peglin.ClassSystem;
using Peglin.OdinSerializer.Utilities;
using Relics;
using Saving;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Worldmap;

namespace Stats
{
	public class RunStats
	{
		[Serializable]
		public class OrbPlayData
		{
			public string id;

			public string name;

			public int damageDealt;

			public int timesFired;

			public int timesDiscarded;

			public int timesRemoved;

			public bool starting;

			public int amountInDeck;

			public int[] levelInstances = new int[3];

			public int highestCruciballBeat;

			public virtual void AddLevelInstance(Attack attack)
			{
				int num = Mathf.Clamp(attack.Level, 1, 3);
				levelInstances[num - 1]++;
			}

			public virtual void UpgradeLevel(Attack attack)
			{
				if (attack.Level <= 1 || attack.Level > 3)
				{
					UnityEngine.Debug.LogWarning("Received upgrade of orb level 1, skipping");
					return;
				}
				levelInstances[attack.Level - 2]--;
				levelInstances[attack.Level - 1]++;
			}

			public virtual void AddTimesRemoved(Attack attack)
			{
				int num = Mathf.Clamp(attack.Level, 1, 3);
				levelInstances[num - 1]--;
				timesRemoved++;
			}
		}

		[Serializable]
		public class AssemballPlayData : OrbPlayData
		{
			public int[] types = new int[5];

			public void ApplyAssemballType(AssemballType type)
			{
				int indexForType = AssemballComponent.GetIndexForType(type);
				if (indexForType < 0 || indexForType > types.Length - 1)
				{
					UnityEngine.Debug.LogError($"Provided the wrong assemball type! {type}");
					return;
				}
				int num = 0;
				int[] array = types;
				foreach (int num2 in array)
				{
					if (num2 > num)
					{
						num = num2;
					}
				}
				if (++types[indexForType] > num)
				{
					amountInDeck++;
				}
			}

			public override void AddTimesRemoved(Attack attack)
			{
				AssemballType assemballComponents = attack.GetComponent<AssemballParent>().assemballComponents;
				for (int i = 0; i < types.Length; i++)
				{
					AssemballType typeForIndex = AssemballComponent.GetTypeForIndex(i);
					if (assemballComponents.HasFlag(typeForIndex))
					{
						types[i]--;
					}
				}
				timesRemoved++;
			}
		}

		[Serializable]
		public class UnlimitedUpgradesPlayData : OrbPlayData
		{
			public int[] levelsPerOrb;

			public void AddOrb(int timesUpgraded)
			{
				if (levelsPerOrb == null)
				{
					levelsPerOrb = new int[1];
					levelsPerOrb[0] = timesUpgraded;
					return;
				}
				int[] array = new int[levelsPerOrb.Length + 1];
				for (int i = 0; i < levelsPerOrb.Length; i++)
				{
					array[i] = levelsPerOrb[i];
				}
				array[^1] = timesUpgraded;
				levelsPerOrb = array;
			}

			public override void UpgradeLevel(Attack attack)
			{
				UnlimitedUpgrades component = attack.GetComponent<UnlimitedUpgrades>();
				int num = 0;
				num = ((!(component != null)) ? attack.Level : (attack.Level + component.timesUpgraded));
				bool flag = false;
				for (int i = 0; i < levelsPerOrb.Length; i++)
				{
					if (flag)
					{
						break;
					}
					if (levelsPerOrb[i] == num - 1)
					{
						levelsPerOrb[i] = num;
						flag = true;
					}
				}
				if (!flag)
				{
					UnityEngine.Debug.LogError("We can't find the old level in our upgrade list for this UnlimitedUpgredes orb. Adding a new level");
					AddOrb(num);
				}
			}

			public override void AddTimesRemoved(Attack attack)
			{
				UnlimitedUpgrades component = attack.GetComponent<UnlimitedUpgrades>();
				int num = 0;
				num = ((!(component != null)) ? attack.Level : (attack.Level + component.timesUpgraded));
				int num2 = -1;
				for (int i = 0; i < levelsPerOrb.Length; i++)
				{
					if (num == levelsPerOrb[i])
					{
						num2 = i;
					}
				}
				int[] array = new int[levelsPerOrb.Length - 1];
				int num3 = 0;
				for (int j = 0; j < array.Length; j++)
				{
					if (num3 == num2)
					{
						num3++;
					}
					array[j] = levelsPerOrb[num3];
					num3++;
				}
				levelsPerOrb = array;
				timesRemoved++;
			}
		}

		[Serializable]
		public class EnemyPlayData
		{
			public string name;

			public int amountFought;

			public int meleeDamageReceived;

			public int rangedDamageReceived;

			public bool defeatedBy;
		}

		public class RunStatsSaveData : SaveObjectData
		{
			public static readonly string KEY = "RunStats";

			public string runId;

			public bool hasWon;

			public bool hasWonCore;

			public bool isCustomRun;

			public int selectedClass;

			public int cruciballLevel;

			public string defeatedBy;

			public int finalHp;

			public int maxHp;

			public int defeatedOnLevel;

			public int defeatedOnRoom;

			public int mostDamageDealtWithSingleAttack;

			public int totalDamageNegated;

			public int totalDamageDealt;

			public int pegsHit;

			public int pegsHitRefresh;

			public int pegsHitCrit;

			public int pegsRefreshed;

			public int bombsThrown;

			public int bombsThrownRigged;

			public int bombsCreated;

			public int bombsCreatedRigged;

			public int mostPegsHitInOneTurn;

			public int coinsEarned;

			public int coinsSpent;

			public int shotsTaken;

			public int critShotsTaken;

			public long runTimerElapsedMilliseconds;

			public DateTime startDate;

			public DateTime endDate;

			public string seed;

			public int balladinProgress;

			public int roundrelProgress;

			public int spinventorProgress;

			public OrbPlayData[] orbPlayData;

			public EnemyPlayData[] enemyPlayData;

			public AssemballPlayData[] assemballPlayData;

			public UnlimitedUpgradesPlayData[] unlimitedUpgradesPlayData;

			public RelicEffect[] relics;

			public ChallengeEffect[] challenges;

			public RoomType[] visitedRooms;

			public BossType[] visitedBosses;

			public int[] statusEffects;

			public int[] slimePegs;

			public bool vampireDealTaken;

			public override string Name => KEY;

			public RunStatsSaveData()
				: base(deleteOnLose: false, DataSerializer.SaveType.STATS)
			{
			}

			public RunStatsSaveData(RunStats runStats)
				: base(deleteOnLose: false, DataSerializer.SaveType.STATS)
			{
				runId = runStats.runId.ToString();
				finalHp = runStats.finalHp;
				maxHp = runStats.maxHp;
				runTimerElapsedMilliseconds = runStats.runTimerSw.ElapsedMilliseconds + runStats.runTimerElapsedMilliseconds;
				hasWon = runStats.hasWon;
				hasWonCore = runStats.hasWonCore;
				isCustomRun = runStats.isCustomRun;
				cruciballLevel = runStats.cruciballLevel;
				selectedClass = (int)runStats.selectedClass;
				defeatedOnLevel = runStats.defeatedOnLevel;
				defeatedOnRoom = runStats.defeatedOnRoom;
				mostDamageDealtWithSingleAttack = runStats.mostDamageDealtWithSingleAttack;
				totalDamageNegated = runStats.totalDamageNegated;
				pegsHit = runStats.pegsHit;
				pegsHitRefresh = runStats.pegsHitRefresh;
				pegsHitCrit = runStats.pegsHitCrit;
				bombsThrown = runStats.bombsThrown;
				bombsThrownRigged = runStats.bombsThrownRigged;
				bombsCreated = runStats.bombsCreated;
				bombsCreatedRigged = runStats.bombsCreatedRigged;
				mostPegsHitInOneTurn = runStats.mostPegsHitInOneTurn;
				coinsEarned = runStats.coinsEarned;
				coinsSpent = runStats.coinsSpent;
				shotsTaken = runStats.shotsTaken;
				critShotsTaken = runStats.critShotsTaken;
				startDate = runStats.startDate;
				endDate = runStats.endDate;
				seed = runStats.seed;
				totalDamageDealt = runStats.totalDamageDealt;
				vampireDealTaken = runStats.vampireDealTaken;
				balladinProgress = runStats.balladinProgress;
				roundrelProgress = runStats.roundrelProgress;
				spinventorProgress = runStats.spinventorProgress;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				foreach (string key in runStats.orbStats.Keys)
				{
					OrbPlayData orbPlayData = runStats.orbStats[key];
					if (orbPlayData is AssemballPlayData)
					{
						num3++;
					}
					else if (orbPlayData is UnlimitedUpgradesPlayData)
					{
						num4++;
					}
					else
					{
						num2++;
					}
				}
				this.orbPlayData = new OrbPlayData[num2];
				foreach (string key2 in runStats.orbStats.Keys)
				{
					OrbPlayData orbPlayData2 = runStats.orbStats[key2];
					if (!(orbPlayData2 is AssemballPlayData) && !(orbPlayData2 is UnlimitedUpgradesPlayData))
					{
						this.orbPlayData[num] = orbPlayData2;
						num++;
					}
				}
				assemballPlayData = new AssemballPlayData[num3];
				num = 0;
				foreach (string key3 in runStats.orbStats.Keys)
				{
					OrbPlayData orbPlayData3 = runStats.orbStats[key3];
					if (orbPlayData3 is AssemballPlayData)
					{
						assemballPlayData[num] = (AssemballPlayData)orbPlayData3;
						num++;
					}
				}
				unlimitedUpgradesPlayData = new UnlimitedUpgradesPlayData[num4];
				num = 0;
				foreach (string key4 in runStats.orbStats.Keys)
				{
					OrbPlayData orbPlayData4 = runStats.orbStats[key4];
					if (orbPlayData4 is UnlimitedUpgradesPlayData)
					{
						unlimitedUpgradesPlayData[num] = (UnlimitedUpgradesPlayData)orbPlayData4;
						num++;
					}
				}
				enemyPlayData = new EnemyPlayData[runStats.enemyData.Count];
				num = 0;
				foreach (string key5 in runStats.enemyData.Keys)
				{
					enemyPlayData[num] = runStats.enemyData[key5];
					num++;
				}
				relics = new RelicEffect[runStats.relics.Count];
				for (int i = 0; i < relics.Length; i++)
				{
					relics[i] = runStats.relics[i];
				}
				challenges = new ChallengeEffect[runStats.challenges.Count];
				for (int j = 0; j < challenges.Length; j++)
				{
					challenges[j] = runStats.challenges[j];
				}
				Array values = Enum.GetValues(typeof(StatusEffectType));
				int num5 = 0;
				foreach (int item in values)
				{
					if (num5 < item)
					{
						num5 = item;
					}
				}
				statusEffects = new int[num5 + 1];
				foreach (StatusEffectType key6 in runStats.stacksPerStatusEffect.Keys)
				{
					int num7 = (int)key6;
					if (statusEffects.Length > num7)
					{
						statusEffects[num7] = runStats.stacksPerStatusEffect[key6];
					}
				}
				Array values2 = Enum.GetValues(typeof(Peg.SlimeType));
				int num8 = 0;
				foreach (int item2 in values2)
				{
					if (Peg.IsSlimeTypePlayerApplied((Peg.SlimeType)item2) && num8 < item2)
					{
						num8 = item2;
					}
				}
				slimePegs = new int[num8 + 1];
				foreach (Peg.SlimeType key7 in runStats.slimePegsPerSlimeType.Keys)
				{
					int num10 = (int)key7;
					slimePegs[num10] = runStats.slimePegsPerSlimeType[key7];
				}
				visitedRooms = new RoomType[runStats.visitedRooms.Count];
				Queue<RoomType> queue = new Queue<RoomType>(runStats.visitedRooms);
				for (int k = 0; k < visitedRooms.Length; k++)
				{
					visitedRooms[k] = queue.Dequeue();
				}
				visitedBosses = new BossType[runStats.visitedBosses.Count];
				Queue<BossType> queue2 = new Queue<BossType>(runStats.visitedBosses);
				for (int l = 0; l < visitedBosses.Length; l++)
				{
					visitedBosses[l] = queue2.Dequeue();
				}
			}
		}

		public class RunStatsHistory : SaveObjectData
		{
			[SerializeField]
			public RunStatsSaveData[] runsHistory;

			public static readonly string KEY = "RunStatsHistory";

			public override string Name => KEY;

			public RunStatsHistory(RunStatsSaveData[] history)
				: base(deleteOnLose: false, DataSerializer.SaveType.STATS)
			{
				runsHistory = history;
			}
		}

		public enum BossType
		{
			NONE = 0,
			SLIME = 1,
			MOLE = 2,
			LESHY = 3,
			BALLISTA = 4,
			DEMON_WALL = 5,
			DRAGON = 6,
			SUPER_SAPPER = 7,
			RUINS = 8,
			PAINTER = 9,
			SPIRIT_OF_RADIA = 10
		}

		public Guid runId;

		public const string ASSEMBALL_LOCSTRING = "assemball";

		public const string ASSEMBALL_PREFAB = "Assemball-Lvl1";

		public bool isRunActive;

		public bool hasWon;

		public bool hasWonCore;

		public bool isCustomRun;

		public Class selectedClass;

		public int cruciballLevel;

		public DateTime startDate;

		public DateTime endDate;

		public readonly Stopwatch runTimerSw = new Stopwatch();

		public List<RelicEffect> relics = new List<RelicEffect>();

		public List<ChallengeEffect> challenges = new List<ChallengeEffect>();

		public string defeatedBy;

		public Queue<RoomType> visitedRooms = new Queue<RoomType>();

		public Queue<BossType> visitedBosses = new Queue<BossType>();

		public Dictionary<StatusEffectType, int> stacksPerStatusEffect = new Dictionary<StatusEffectType, int>();

		public Dictionary<Peg.SlimeType, int> slimePegsPerSlimeType = new Dictionary<Peg.SlimeType, int>();

		public Dictionary<string, EnemyPlayData> enemyData = new Dictionary<string, EnemyPlayData>();

		public int finalHp;

		public int maxHp;

		public int defeatedOnLevel;

		public int defeatedOnRoom;

		public int mostDamageDealtWithSingleAttack;

		public int totalDamageDealt;

		public int totalDamageNegated;

		public int pegsHit;

		public int pegsHitRefresh;

		public int pegsHitCrit;

		public int pegsRefreshed;

		public int bombsThrown;

		public int bombsThrownRigged;

		public int bombsCreated;

		public int bombsCreatedRigged;

		public int mostPegsHitInOneTurn;

		public int coinsEarned;

		public int coinsSpent;

		public int shotsTaken;

		public int critShotsTaken;

		public long runTimerElapsedMilliseconds;

		public int balladinProgress;

		public int roundrelProgress;

		public int spinventorProgress;

		public string seed;

		public bool vampireDealTaken;

		public List<int> pegUpgradeEvents = new List<int>();

		public Dictionary<string, OrbPlayData> orbStats = new Dictionary<string, OrbPlayData>();

		private RunStatsSaveData saveData;

		public static readonly Dictionary<BossType, int> BossTypeToActMap = new Dictionary<BossType, int>
		{
			{
				BossType.SLIME,
				1
			},
			{
				BossType.MOLE,
				1
			},
			{
				BossType.LESHY,
				1
			},
			{
				BossType.BALLISTA,
				2
			},
			{
				BossType.DEMON_WALL,
				2
			},
			{
				BossType.PAINTER,
				2
			},
			{
				BossType.DRAGON,
				3
			},
			{
				BossType.SUPER_SAPPER,
				3
			},
			{
				BossType.RUINS,
				3
			},
			{
				BossType.SPIRIT_OF_RADIA,
				4
			}
		};

		public void Reset()
		{
			runId = Guid.NewGuid();
			hasWon = false;
			hasWonCore = false;
			isCustomRun = false;
			defeatedBy = "";
			relics.Clear();
			challenges.Clear();
			visitedRooms.Clear();
			stacksPerStatusEffect.Clear();
			slimePegsPerSlimeType.Clear();
			enemyData.Clear();
			pegUpgradeEvents.Clear();
			orbStats.Clear();
			visitedBosses.Clear();
			cruciballLevel = 0;
			defeatedOnLevel = 0;
			defeatedOnRoom = 0;
			mostDamageDealtWithSingleAttack = 0;
			totalDamageNegated = 0;
			totalDamageDealt = 0;
			pegsHit = 0;
			pegsHitRefresh = 0;
			pegsHitCrit = 0;
			pegsRefreshed = 0;
			bombsThrown = 0;
			bombsThrownRigged = 0;
			bombsCreated = 0;
			bombsCreatedRigged = 0;
			mostPegsHitInOneTurn = 0;
			coinsEarned = 0;
			coinsSpent = 0;
			shotsTaken = 0;
			critShotsTaken = 0;
			runTimerElapsedMilliseconds = 0L;
			maxHp = 0;
			finalHp = 0;
			runTimerSw.Reset();
			vampireDealTaken = false;
		}

		public RunStats()
		{
			runId = Guid.NewGuid();
			StartRunTimer();
		}

		public RunStats(RunStatsSaveData saveData)
		{
			runId = (saveData.runId.IsNullOrWhitespace() ? Guid.NewGuid() : new Guid(saveData.runId));
			selectedClass = (Class)saveData.selectedClass;
			hasWon = saveData.hasWon;
			hasWonCore = saveData.hasWonCore;
			isCustomRun = saveData.isCustomRun;
			defeatedBy = saveData.defeatedBy;
			relics.Clear();
			RelicEffect[] array = saveData.relics;
			foreach (RelicEffect item in array)
			{
				relics.Add(item);
			}
			challenges.Clear();
			if (saveData.challenges != null)
			{
				ChallengeEffect[] array2 = saveData.challenges;
				foreach (ChallengeEffect item2 in array2)
				{
					challenges.Add(item2);
				}
			}
			visitedRooms.Clear();
			for (int j = 0; j < saveData.visitedRooms.Length; j++)
			{
				visitedRooms.Enqueue(saveData.visitedRooms[j]);
			}
			stacksPerStatusEffect.Clear();
			for (int k = 0; k < saveData.statusEffects.Length; k++)
			{
				stacksPerStatusEffect.Add((StatusEffectType)k, saveData.statusEffects[k]);
			}
			slimePegsPerSlimeType.Clear();
			for (int l = 0; l < saveData.slimePegs.Length; l++)
			{
				slimePegsPerSlimeType.Add((Peg.SlimeType)l, saveData.slimePegs[l]);
			}
			enemyData.Clear();
			EnemyPlayData[] enemyPlayData = saveData.enemyPlayData;
			foreach (EnemyPlayData enemyPlayData2 in enemyPlayData)
			{
				enemyData.Add(enemyPlayData2.name, enemyPlayData2);
			}
			pegUpgradeEvents.Clear();
			orbStats.Clear();
			OrbPlayData[] orbPlayData = saveData.orbPlayData;
			foreach (OrbPlayData orbPlayData2 in orbPlayData)
			{
				orbStats.Add(orbPlayData2.id, orbPlayData2);
			}
			AssemballPlayData[] assemballPlayData = saveData.assemballPlayData;
			foreach (AssemballPlayData assemballPlayData2 in assemballPlayData)
			{
				if (assemballPlayData2 != null)
				{
					orbStats.Add("assemball", assemballPlayData2);
				}
			}
			if (saveData.unlimitedUpgradesPlayData != null)
			{
				UnlimitedUpgradesPlayData[] unlimitedUpgradesPlayData = saveData.unlimitedUpgradesPlayData;
				foreach (UnlimitedUpgradesPlayData unlimitedUpgradesPlayData2 in unlimitedUpgradesPlayData)
				{
					orbStats.Add(unlimitedUpgradesPlayData2.id, unlimitedUpgradesPlayData2);
				}
			}
			BossType[] array3 = saveData.visitedBosses;
			foreach (BossType item3 in array3)
			{
				visitedBosses.Enqueue(item3);
			}
			cruciballLevel = saveData.cruciballLevel;
			defeatedOnLevel = saveData.defeatedOnLevel;
			defeatedOnRoom = saveData.defeatedOnRoom;
			mostDamageDealtWithSingleAttack = saveData.mostDamageDealtWithSingleAttack;
			totalDamageNegated = saveData.totalDamageNegated;
			pegsHit = saveData.pegsHit;
			pegsHitRefresh = saveData.pegsHitRefresh;
			pegsHitCrit = saveData.pegsHitCrit;
			pegsRefreshed = saveData.pegsRefreshed;
			bombsThrown = saveData.bombsThrown;
			bombsThrownRigged = saveData.bombsThrownRigged;
			bombsCreated = saveData.bombsCreated;
			bombsCreatedRigged = saveData.bombsCreatedRigged;
			mostPegsHitInOneTurn = saveData.mostPegsHitInOneTurn;
			coinsEarned = saveData.coinsEarned;
			coinsSpent = saveData.coinsSpent;
			shotsTaken = saveData.shotsTaken;
			critShotsTaken = saveData.critShotsTaken;
			runTimerElapsedMilliseconds = saveData.runTimerElapsedMilliseconds;
			finalHp = saveData.finalHp;
			maxHp = saveData.maxHp;
			startDate = saveData.startDate;
			endDate = saveData.endDate;
			runTimerSw.Reset();
			seed = saveData.seed;
			totalDamageDealt = saveData.totalDamageDealt;
			vampireDealTaken = saveData.vampireDealTaken;
			balladinProgress = saveData.balladinProgress;
			roundrelProgress = saveData.roundrelProgress;
			spinventorProgress = saveData.spinventorProgress;
		}

		public void StartRunTimer()
		{
			runTimerSw.Start();
		}

		public RunStats CreateTestObject()
		{
			foreach (object value2 in Enum.GetValues(typeof(RelicEffect)))
			{
				if ((RelicEffect)value2 != RelicEffect.NONE && relics.Count < 20)
				{
					relics.Add((RelicEffect)value2);
				}
			}
			foreach (object value3 in Enum.GetValues(typeof(ChallengeEffect)))
			{
				if ((ChallengeEffect)value3 != ChallengeEffect.NONE && challenges.Count < 4)
				{
					challenges.Add((ChallengeEffect)value3);
				}
			}
			bombsCreated = UnityEngine.Random.Range(0, 500);
			bombsThrown = UnityEngine.Random.Range(0, 500);
			coinsEarned = UnityEngine.Random.Range(0, 500);
			coinsSpent = UnityEngine.Random.Range(0, 500);
			pegsHit = UnityEngine.Random.Range(0, 500);
			pegsHitCrit = UnityEngine.Random.Range(0, 500);
			pegsHitRefresh = UnityEngine.Random.Range(0, 500);
			pegsRefreshed = UnityEngine.Random.Range(0, 500);
			shotsTaken = UnityEngine.Random.Range(0, 500);
			critShotsTaken = UnityEngine.Random.Range(0, 500);
			mostPegsHitInOneTurn = UnityEngine.Random.Range(0, 500);
			mostDamageDealtWithSingleAttack = UnityEngine.Random.Range(0, 500);
			totalDamageNegated = UnityEngine.Random.Range(0, 500);
			cruciballLevel = 15;
			Addressables.LoadAssetsAsync("Orbs", (GameObject o) =>
			{
				Attack component = o.GetComponent<Attack>();
				if (!(component == null))
				{
					if (orbStats.TryGetValue(component.locNameString, out var value))
					{
						if (component.Level > 0)
						{
							value.levelInstances[component.Level - 1] = UnityEngine.Random.Range(0, 10);
						}
					}
					else
					{
						OrbPlayData orbPlayData = new OrbPlayData
						{
							name = o.name,
							damageDealt = UnityEngine.Random.Range(0, 500),
							timesFired = UnityEngine.Random.Range(0, 500),
							timesDiscarded = UnityEngine.Random.Range(0, 500),
							timesRemoved = UnityEngine.Random.Range(0, 500),
							starting = true,
							levelInstances = new int[3]
						};
						if (component.Level > 0)
						{
							orbPlayData.levelInstances[component.Level - 1] = UnityEngine.Random.Range(0, 10);
						}
						else
						{
							orbPlayData.levelInstances[0] = UnityEngine.Random.Range(0, 10);
						}
						if (orbStats.Count < 15 && PersistentPlayerData.Instance.UnlockedOrbs.Contains(component.locNameString))
						{
							orbStats.Add(component.locNameString, orbPlayData);
						}
					}
				}
			}).WaitForCompletion();
			startDate = DateTime.Today;
			endDate = DateTime.Now;
			AsyncOperationHandle<IList<GameObject>> asyncOperationHandle = Addressables.LoadAssetsAsync("Enemies", (GameObject e) =>
			{
				EnemyPlayData value = new EnemyPlayData
				{
					name = "Random.Range(0, 500)",
					amountFought = UnityEngine.Random.Range(0, 500),
					meleeDamageReceived = UnityEngine.Random.Range(0, 500),
					rangedDamageReceived = UnityEngine.Random.Range(0, 500)
				};
				if (enemyData.Count < 20)
				{
					enemyData.Add(e.name, value);
				}
				defeatedBy = e.name;
			});
			for (int num = 0; num < 30; num++)
			{
				if ((num > 0 && num % 10 == 0) || num == 29)
				{
					visitedRooms.Enqueue(RoomType.BOSS);
				}
				else
				{
					visitedRooms.Enqueue((RoomType)UnityEngine.Random.Range(1, 5));
				}
			}
			asyncOperationHandle.WaitForCompletion();
			return this;
		}

		public (int, string) GetRandomInterestingStat()
		{
			if (pegsHitRefresh > pegsHitCrit && pegsHitRefresh > bombsThrown + bombsThrownRigged)
			{
				return (pegsHitRefresh, "Menu/RunSummary/pegs_hit_refresh");
			}
			if (pegsHitCrit > pegsHitRefresh && pegsHitCrit > bombsThrown + bombsThrownRigged)
			{
				return (pegsHitCrit, "Menu/RunSummary/pegs_hit_crit");
			}
			return (bombsThrown + bombsThrownRigged, "Menu/RunSummary/bombs_thrown");
		}

		public int GetNumberOfBuffsApplied()
		{
			int num = 0;
			foreach (StatusEffectType key in stacksPerStatusEffect.Keys)
			{
				if (StatusEffect.IsStatusEffectBuff(key))
				{
					num += stacksPerStatusEffect[key];
				}
			}
			return num;
		}

		public int GetNumberOfDefbuffsApplied()
		{
			int num = 0;
			foreach (StatusEffectType key in stacksPerStatusEffect.Keys)
			{
				if (StatusEffect.IsStatusEffectDebuff(key))
				{
					num += stacksPerStatusEffect[key];
				}
			}
			return num;
		}

		public (StatusEffectType, int) GetBuffWithMostStacks()
		{
			int item = 0;
			float num = 0f;
			StatusEffectType item2 = StatusEffectType.None;
			foreach (StatusEffectType key in stacksPerStatusEffect.Keys)
			{
				float num2 = ((key != StatusEffectType.Ballusion) ? ((float)stacksPerStatusEffect[key]) : ((float)stacksPerStatusEffect[key] * 0.1f));
				if (StatusEffect.IsStatusEffectBuff(key) && num2 > num)
				{
					item2 = key;
					num = num2;
					item = stacksPerStatusEffect[key];
				}
			}
			return (item2, item);
		}

		public (StatusEffectType, int) GetDebuffsWithMostStacks()
		{
			int item = 0;
			float num = 0f;
			StatusEffectType item2 = StatusEffectType.None;
			foreach (StatusEffectType key in stacksPerStatusEffect.Keys)
			{
				float num2 = ((key != StatusEffectType.Transpherency) ? ((float)stacksPerStatusEffect[key]) : ((float)stacksPerStatusEffect[key] * 0.5f));
				if (StatusEffect.IsStatusEffectDebuff(key) && num2 > num)
				{
					item2 = key;
					num = num2;
					item = stacksPerStatusEffect[key];
				}
			}
			return (item2, item);
		}

		public int GetNumberOfSlimedPegs()
		{
			int num = 0;
			foreach (KeyValuePair<Peg.SlimeType, int> item in slimePegsPerSlimeType)
			{
				num += item.Value;
			}
			return num;
		}

		public (Peg.SlimeType, int) GetSlimeTypeWithHighestValue()
		{
			int num = 0;
			Peg.SlimeType item = Peg.SlimeType.None;
			foreach (KeyValuePair<Peg.SlimeType, int> item2 in slimePegsPerSlimeType)
			{
				if (item2.Value > num)
				{
					item = item2.Key;
					num = item2.Value;
				}
			}
			return (item, num);
		}

		public string GetRunTime()
		{
			return TimeSpan.FromMilliseconds(runTimerElapsedMilliseconds + runTimerSw.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}, {14}: {15}, {16}: {17}, {18}: {19}, {20}: {21}, {22}: {23}, {24}: {25}, {26}: {27}, {28}: {29}, {30}: {31}, {32}: {33}, {34}: {35}, {36}: {37}, {38}: {39}", "isRunActive", isRunActive, "hasWon", hasWon, "isCustomRun", isCustomRun, "selectedClass", selectedClass, "startDate", startDate, "mostDamageDealtWithSingleAttack", mostDamageDealtWithSingleAttack, "totalDamageNegated", totalDamageNegated, "pegsHit", pegsHit, "pegsHitRefresh", pegsHitRefresh, "pegsHitCrit", pegsHitCrit, "pegsRefreshed", pegsRefreshed, "bombsThrown", bombsThrown, "bombsThrownRigged", bombsThrownRigged, "bombsCreated", bombsCreated, "bombsCreatedRigged", bombsCreatedRigged, "mostPegsHitInOneTurn", mostPegsHitInOneTurn, "coinsEarned", coinsEarned, "coinsSpent", coinsSpent, "shotsTaken", shotsTaken, "critShotsTaken", critShotsTaken);
		}

		public void AddEnemyCount(string enemyName)
		{
			if (enemyData.TryGetValue(enemyName, out var value))
			{
				value.amountFought++;
				return;
			}
			EnemyPlayData enemyPlayData = new EnemyPlayData();
			enemyPlayData.name = enemyName;
			enemyPlayData.amountFought = 1;
			enemyData.Add(enemyName, enemyPlayData);
		}

		public void AddDamageForEnemy(string enemyName, float damage, bool melee)
		{
			int num = Mathf.RoundToInt(damage);
			if (enemyData.TryGetValue(enemyName, out var value))
			{
				if (melee)
				{
					value.meleeDamageReceived += num;
				}
				else
				{
					value.rangedDamageReceived += num;
				}
				return;
			}
			EnemyPlayData enemyPlayData = new EnemyPlayData();
			enemyPlayData.name = enemyName;
			enemyPlayData.amountFought = 1;
			if (melee)
			{
				enemyPlayData.meleeDamageReceived = num;
			}
			else
			{
				enemyPlayData.rangedDamageReceived = num;
			}
			enemyData.Add(enemyName, enemyPlayData);
		}

		public OrbPlayData AddOrbToDeck(Attack attack, bool starting = false)
		{
			AssemballComponent component = attack.GetComponent<AssemballComponent>();
			UnlimitedUpgrades component2 = attack.GetComponent<UnlimitedUpgrades>();
			if (component != null)
			{
				return AddAssemballToDeck(attack, component.type, starting);
			}
			if (component2 != null || attack.locNameString.Equals("orbbery"))
			{
				return AddUnlimitedUpgradesOrbToDeck(attack, component2, starting);
			}
			if (orbStats.TryGetValue(attack.locNameString, out var value))
			{
				value.AddLevelInstance(attack);
				value.amountInDeck++;
				value.starting = starting;
				return value;
			}
			OrbPlayData orbPlayData = new OrbPlayData();
			orbPlayData.id = attack.locNameString;
			orbPlayData.name = attack.name;
			orbPlayData.AddLevelInstance(attack);
			orbPlayData.amountInDeck = 1;
			orbStats.Add(attack.locNameString, orbPlayData);
			return orbPlayData;
		}

		public OrbPlayData AddAssemballToDeck(Attack attack, AssemballType type, bool starting = false)
		{
			if (orbStats.TryGetValue("assemball", out var value))
			{
				AssemballPlayData assemballPlayData = (AssemballPlayData)value;
				if (assemballPlayData == null)
				{
					UnityEngine.Debug.LogError("We tried to cast an assemball to the wrong type of orb");
					return value;
				}
				assemballPlayData.ApplyAssemballType(type);
				assemballPlayData.AddLevelInstance(attack);
				return assemballPlayData;
			}
			AssemballPlayData assemballPlayData2 = new AssemballPlayData();
			assemballPlayData2.id = "assemball";
			assemballPlayData2.name = "Assemball-Lvl1";
			assemballPlayData2.AddLevelInstance(attack);
			assemballPlayData2.ApplyAssemballType(type);
			orbStats.Add("assemball", assemballPlayData2);
			return assemballPlayData2;
		}

		public OrbPlayData AddUnlimitedUpgradesOrbToDeck(Attack attack, UnlimitedUpgrades upgrades, bool starting = false)
		{
			if (orbStats.TryGetValue(attack.locNameString, out var value))
			{
				if (!(value is UnlimitedUpgradesPlayData unlimitedUpgradesPlayData))
				{
					UnityEngine.Debug.LogError("We tried to cast an unlimited upgrades to the wrong type of orb");
					return value;
				}
				if (upgrades == null)
				{
					unlimitedUpgradesPlayData.AddOrb(attack.Level);
				}
				else
				{
					unlimitedUpgradesPlayData.AddOrb(attack.Level + upgrades.timesUpgraded);
				}
				unlimitedUpgradesPlayData.amountInDeck++;
				unlimitedUpgradesPlayData.starting = starting;
				return unlimitedUpgradesPlayData;
			}
			UnlimitedUpgradesPlayData unlimitedUpgradesPlayData2 = new UnlimitedUpgradesPlayData();
			unlimitedUpgradesPlayData2.id = attack.locNameString;
			unlimitedUpgradesPlayData2.name = attack.name;
			unlimitedUpgradesPlayData2.amountInDeck = 1;
			if (upgrades == null)
			{
				unlimitedUpgradesPlayData2.AddOrb(attack.Level);
			}
			else
			{
				unlimitedUpgradesPlayData2.AddOrb(attack.Level + upgrades.timesUpgraded);
			}
			orbStats.Add(attack.locNameString, unlimitedUpgradesPlayData2);
			return unlimitedUpgradesPlayData2;
		}

		public void UpgradeOrbInDeck(Attack attack)
		{
			if (!attack.locNameString.IsNullOrWhitespace())
			{
				if (orbStats.TryGetValue(attack.locNameString, out var value))
				{
					value.UpgradeLevel(attack);
				}
				else
				{
					AddOrbToDeck(attack);
				}
			}
		}

		public void AddShotForAttack(Attack attack)
		{
			if (!attack.locNameString.IsNullOrWhitespace())
			{
				if (orbStats.TryGetValue(attack.locNameString, out var value))
				{
					value.timesFired++;
					return;
				}
				UnityEngine.Debug.LogWarning("Received orb to upgrade we haven't received before, adding...");
				AddOrbToDeck(attack).timesFired++;
			}
		}

		public void AddOrbDiscard(Attack attack)
		{
			if (!attack.locNameString.IsNullOrWhitespace())
			{
				if (orbStats.TryGetValue(attack.locNameString, out var value))
				{
					value.timesDiscarded++;
					return;
				}
				UnityEngine.Debug.LogWarning("Received orb to upgrade we haven't received before, adding...");
				AddOrbToDeck(attack).timesDiscarded++;
			}
		}

		public void AddOrbRemoval(Attack attack)
		{
			if (!attack.locNameString.IsNullOrWhitespace())
			{
				if (orbStats.TryGetValue(attack.locNameString, out var value))
				{
					value.AddTimesRemoved(attack);
					return;
				}
				UnityEngine.Debug.LogWarning("Received orb to upgrade we haven't received before, adding...");
				AddOrbToDeck(attack).AddTimesRemoved(attack);
			}
		}

		public void AddDamageForOrb(float attackDamage, Attack attack)
		{
			if (!attack.locNameString.IsNullOrWhitespace())
			{
				if (orbStats.TryGetValue(attack.locNameString, out var value))
				{
					value.damageDealt += Mathf.RoundToInt(attackDamage);
					return;
				}
				UnityEngine.Debug.LogWarning("Received orb to add damage to we haven't received before, adding...");
				AddOrbToDeck(attack).damageDealt += Mathf.RoundToInt(attackDamage);
			}
		}
	}
}
