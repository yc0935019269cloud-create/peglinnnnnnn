using System;
using System.Collections.Generic;
using Battle.StatusEffects;
using Peglin.ClassSystem;
using Relics;
using ToolBox.Serialization;
using UnityEngine;
using Worldmap;

namespace Stats
{
	public class PermanentStats
	{
		public class HistoricEnemyPlayData
		{
			public int amountFought;

			public int meleeDamageReceived;

			public int rangedDamageReceived;

			public int defeatedByAmount;

			public HistoricEnemyPlayData(RunStats.EnemyPlayData data)
			{
				amountFought = data.amountFought;
				meleeDamageReceived = data.meleeDamageReceived;
				rangedDamageReceived = data.rangedDamageReceived;
				defeatedByAmount = (data.defeatedBy ? 1 : 0);
			}
		}

		public class PermanentStatsSaveData : SaveObjectData
		{
			public static string KEY = "PermanentStats";

			public int totalRuns;

			public int totalWins;

			public int totalCustomRuns;

			public int totalCustomRunWins;

			public int mostDamageDealt;

			public int mostCoinsEarned;

			public int mostCoinsSpent;

			public int mostPegsHitInOneTurn;

			public int totalDamageDealt;

			public int totalDamageNegated;

			public int totalPegsHit;

			public int totalPegsHitRefresh;

			public int totalPegsHitCrit;

			public int totalPegsRefreshed;

			public int totalCoinsEarned;

			public int totalCoinsSpent;

			public int totalShotsTaken;

			public int totalCritShotsTaken;

			public int totalBombsThrown;

			public int totalBombsThrownRigged;

			public int totalBombsCreated;

			public int totalBombsCreatedRigged;

			public int[] runsPerClass;

			public int[] winsPerClass;

			public RunStats.OrbPlayData[] historicOrbPlayData;

			public HistoricEnemyPlayData[] historicEnemyPlayData;

			public int[] historicStatusEffectData;

			public int[] historicRelicData;

			public int[] historicRelicCruciballData;

			public int[] visitsPerRoomType;

			public override string Name => KEY;

			public PermanentStatsSaveData()
				: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
			{
			}

			public PermanentStatsSaveData(PermanentStats stats)
				: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
			{
				totalRuns = stats.totalRuns;
				totalWins = stats.totalWins;
				totalCustomRuns = stats.totalCustomRuns;
				totalCustomRunWins = stats.totalCustomRunWins;
				mostDamageDealt = stats.mostDamageDealt;
				mostCoinsEarned = stats.mostCoinsEarned;
				mostCoinsSpent = stats.mostCoinsSpent;
				mostPegsHitInOneTurn = stats.mostPegsHitInOneTurn;
				totalDamageDealt = stats.totalDamageDealt;
				totalDamageNegated = stats.totalDamageNegated;
				totalPegsHit = stats.totalPegsHit;
				totalPegsHitRefresh = stats.totalPegsHitRefresh;
				totalPegsHitCrit = stats.totalPegsHitCrit;
				totalPegsRefreshed = stats.totalPegsRefreshed;
				totalCoinsEarned = stats.totalCoinsEarned;
				totalCoinsSpent = stats.totalCoinsSpent;
				totalShotsTaken = stats.totalShotsTaken;
				totalCritShotsTaken = stats.totalCritShotsTaken;
				totalBombsThrown = stats.totalBombsThrown;
				totalBombsThrownRigged = stats.totalBombsThrownRigged;
				totalBombsCreated = stats.totalBombsCreated;
				totalBombsCreatedRigged = stats.totalBombsCreatedrigged;
				int num = 0;
				historicOrbPlayData = new RunStats.OrbPlayData[stats.historicOrbPlayData.Count];
				foreach (string key in stats.historicOrbPlayData.Keys)
				{
					historicOrbPlayData[num] = stats.historicOrbPlayData[key];
					num++;
				}
				historicEnemyPlayData = new HistoricEnemyPlayData[stats.historicEnemyPlayData.Count];
				num = 0;
				foreach (string key2 in stats.historicEnemyPlayData.Keys)
				{
					historicEnemyPlayData[num] = stats.historicEnemyPlayData[key2];
					num++;
				}
				historicStatusEffectData = new int[Enum.GetValues(typeof(StatusEffectType)).Length];
				for (int i = 0; i < historicStatusEffectData.Length; i++)
				{
					if (stats.historicStatusEffectData.TryGetValue((StatusEffectType)i, out var value))
					{
						historicStatusEffectData[i] = value;
					}
					else
					{
						historicStatusEffectData[i] = 0;
					}
				}
				int num2 = 0;
				foreach (int value4 in Enum.GetValues(typeof(RelicEffect)))
				{
					num2 = value4;
				}
				num2++;
				historicRelicData = new int[num2];
				for (int j = 0; j < historicRelicData.Length; j++)
				{
					if (stats.historicRelicData.TryGetValue((RelicEffect)j, out var value2))
					{
						historicRelicData[j] = value2.amount;
					}
					else
					{
						historicRelicData[j] = 0;
					}
				}
				historicRelicCruciballData = new int[num2];
				for (int k = 0; k < historicRelicCruciballData.Length; k++)
				{
					if (stats.historicRelicData.TryGetValue((RelicEffect)k, out var value3))
					{
						historicRelicCruciballData[k] = value3.highestCruciballBeat;
					}
					else
					{
						historicRelicCruciballData[k] = 0;
					}
				}
				Array values = Enum.GetValues(typeof(StatusEffectType));
				historicStatusEffectData = new int[values.Length];
				foreach (StatusEffectType key3 in stats.historicStatusEffectData.Keys)
				{
					int num3 = (int)key3;
					if (historicStatusEffectData.Length > num3)
					{
						historicStatusEffectData[num3] = stats.historicStatusEffectData[key3];
					}
				}
				visitsPerRoomType = new int[Enum.GetValues(typeof(RoomType)).Length];
				for (int l = 0; l < visitsPerRoomType.Length; l++)
				{
					if (stats.visitsPerRoomType.Length > l)
					{
						visitsPerRoomType[l] = stats.visitsPerRoomType[l];
					}
				}
				winsPerClass = new int[Enum.GetValues(typeof(Class)).Length];
				for (int m = 0; m < winsPerClass.Length; m++)
				{
					if (stats.winsPerClass.Length > m)
					{
						winsPerClass[m] = stats.winsPerClass[m];
					}
				}
				runsPerClass = new int[Enum.GetValues(typeof(Class)).Length];
				for (int n = 0; n < runsPerClass.Length; n++)
				{
					if (stats.runsPerClass.Length > n)
					{
						runsPerClass[n] = stats.runsPerClass[n];
					}
				}
			}
		}

		[Serializable]
		public class RelicPlayData
		{
			public int amount;

			public int highestCruciballBeat;
		}

		public static int MAX_VALUE = 99999999;

		public int totalRuns;

		public int totalWins;

		public int totalCoreWins;

		public int totalCustomRuns;

		public int totalCustomRunWins;

		public int totalCustomRunCoreWins;

		public int mostDamageDealt;

		public int mostCoinsEarned;

		public int mostCoinsSpent;

		public int mostPegsHitInOneTurn;

		public int totalDamageDealt;

		public int totalDamageNegated;

		public int totalPegsHit;

		public int totalPegsHitRefresh;

		public int totalPegsHitCrit;

		public int totalPegsRefreshed;

		public int totalCoinsEarned;

		public int totalCoinsSpent;

		public int totalShotsTaken;

		public int totalCritShotsTaken;

		public int totalBombsThrown;

		public int totalBombsThrownRigged;

		public int totalBombsCreated;

		public int totalBombsCreatedrigged;

		public int[] runsPerClass = new int[Enum.GetValues(typeof(Class)).Length];

		public int[] winsPerClass = new int[Enum.GetValues(typeof(Class)).Length];

		public int[] coreWinsPerClass = new int[Enum.GetValues(typeof(Class)).Length];

		public Dictionary<string, RunStats.OrbPlayData> historicOrbPlayData = new Dictionary<string, RunStats.OrbPlayData>();

		public Dictionary<string, HistoricEnemyPlayData> historicEnemyPlayData = new Dictionary<string, HistoricEnemyPlayData>();

		public Dictionary<StatusEffectType, int> historicStatusEffectData = new Dictionary<StatusEffectType, int>();

		public Dictionary<RelicEffect, RelicPlayData> historicRelicData = new Dictionary<RelicEffect, RelicPlayData>();

		public int[] visitsPerRoomType = new int[Enum.GetValues(typeof(RoomType)).Length];

		public PermanentStats()
		{
		}

		public PermanentStats(PermanentStatsSaveData data)
		{
			totalRuns = data.totalRuns;
			totalWins = data.totalWins;
			totalCustomRuns = data.totalCustomRuns;
			totalCustomRunWins = data.totalCustomRunWins;
			mostDamageDealt = data.mostDamageDealt;
			mostCoinsEarned = data.mostCoinsEarned;
			mostCoinsSpent = data.mostCoinsSpent;
			mostPegsHitInOneTurn = data.mostPegsHitInOneTurn;
			totalDamageDealt = data.totalDamageDealt;
			totalDamageNegated = data.totalDamageNegated;
			totalPegsHit = data.totalPegsHit;
			totalPegsHitRefresh = data.totalPegsHitRefresh;
			totalPegsHitCrit = data.totalPegsHitCrit;
			totalPegsRefreshed = data.totalPegsRefreshed;
			totalCoinsEarned = data.totalCoinsEarned;
			totalCoinsSpent = data.totalCoinsSpent;
			totalShotsTaken = data.totalShotsTaken;
			totalCritShotsTaken = data.totalCritShotsTaken;
			totalBombsThrown = data.totalBombsThrown;
			totalBombsThrownRigged = data.totalBombsThrownRigged;
			totalBombsCreated = data.totalBombsCreated;
			totalBombsCreatedrigged = data.totalBombsCreatedRigged;
			if (data.winsPerClass != null)
			{
				for (int i = 0; i < data.winsPerClass.Length; i++)
				{
					int num = data.winsPerClass[i];
					winsPerClass[i] = num;
				}
			}
			else
			{
				for (int j = 0; j < winsPerClass.Length; j++)
				{
					winsPerClass[j] = 0;
				}
			}
			if (data.runsPerClass != null)
			{
				for (int k = 0; k < data.runsPerClass.Length; k++)
				{
					int num2 = data.runsPerClass[k];
					if (num2 == 0 && winsPerClass[k] > 0)
					{
						winsPerClass[k] = 0;
					}
					runsPerClass[k] = num2;
				}
			}
			else
			{
				for (int l = 0; l < runsPerClass.Length; l++)
				{
					runsPerClass[l] = 0;
				}
			}
			if (data.historicOrbPlayData != null)
			{
				RunStats.OrbPlayData[] array = data.historicOrbPlayData;
				foreach (RunStats.OrbPlayData orbPlayData in array)
				{
					if (orbPlayData.id != null)
					{
						historicOrbPlayData.Add(orbPlayData.id, orbPlayData);
					}
				}
			}
			if (data.historicRelicData != null)
			{
				for (int n = 0; n < data.historicRelicData.Length; n++)
				{
					RelicPlayData relicPlayData = new RelicPlayData
					{
						amount = data.historicRelicData[n]
					};
					if (data.historicRelicCruciballData != null)
					{
						if (data.historicRelicCruciballData.Length > n)
						{
							relicPlayData.highestCruciballBeat = ((data.historicRelicCruciballData != null) ? data.historicRelicCruciballData[n] : 0);
							historicRelicData.Add((RelicEffect)n, relicPlayData);
						}
						else
						{
							Debug.LogError("Mismatch between array lenghts in historic relic cruciball data and historic relic data. This should not happen in a build out in the open. We are skipping cruciball history for " + $"relic {(RelicEffect)n} on loading this data");
						}
					}
				}
			}
			if (data.historicStatusEffectData != null)
			{
				for (int num3 = 0; num3 < data.historicStatusEffectData.Length; num3++)
				{
					historicStatusEffectData.Add((StatusEffectType)num3, data.historicStatusEffectData[num3]);
				}
			}
			if (data.visitsPerRoomType != null)
			{
				for (int num4 = 0; num4 < data.visitsPerRoomType.Length; num4++)
				{
					if (visitsPerRoomType.Length > num4)
					{
						visitsPerRoomType[num4] = data.visitsPerRoomType[num4];
					}
				}
			}
			else
			{
				for (int num5 = 0; num5 < visitsPerRoomType.Length; num5++)
				{
					visitsPerRoomType[num5] = 0;
				}
			}
		}

		public void Clear()
		{
			totalRuns = 0;
			totalWins = 0;
			totalCustomRuns = 0;
			totalCustomRunWins = 0;
			mostDamageDealt = 0;
			mostCoinsEarned = 0;
			mostCoinsSpent = 0;
			mostPegsHitInOneTurn = 0;
			totalDamageDealt = 0;
			totalDamageNegated = 0;
			totalPegsHit = 0;
			totalPegsHitRefresh = 0;
			totalPegsHitCrit = 0;
			totalPegsRefreshed = 0;
			totalCoinsEarned = 0;
			totalCoinsSpent = 0;
			totalShotsTaken = 0;
			totalCritShotsTaken = 0;
			totalBombsThrown = 0;
			totalBombsThrownRigged = 0;
			totalBombsCreated = 0;
			totalBombsCreatedrigged = 0;
			for (int i = 0; i < winsPerClass.Length; i++)
			{
				winsPerClass[i] = 0;
			}
			for (int j = 0; j < runsPerClass.Length; j++)
			{
				_ = runsPerClass[j];
				runsPerClass[j] = 0;
			}
			for (int k = 0; k < visitsPerRoomType.Length; k++)
			{
				visitsPerRoomType[k] = 0;
			}
			historicOrbPlayData.Clear();
			historicRelicData.Clear();
			historicStatusEffectData.Clear();
		}

		public void AddStatsToPermanent(RunStats stats)
		{
			if (stats.isCustomRun)
			{
				totalCustomRuns++;
				if (stats.hasWon)
				{
					totalCustomRunWins++;
				}
				if (stats.hasWonCore)
				{
					totalCustomRunCoreWins++;
				}
			}
			else
			{
				totalRuns++;
				if (stats.hasWon)
				{
					totalWins++;
				}
				if (stats.hasWonCore)
				{
					totalCoreWins++;
				}
			}
			int selectedClass = (int)stats.selectedClass;
			if (runsPerClass == null || runsPerClass.Length <= selectedClass)
			{
				runsPerClass = new int[Enum.GetValues(typeof(Class)).Length];
			}
			if (winsPerClass == null || winsPerClass.Length <= selectedClass)
			{
				winsPerClass = new int[Enum.GetValues(typeof(Class)).Length];
			}
			if (coreWinsPerClass == null || coreWinsPerClass.Length <= selectedClass)
			{
				coreWinsPerClass = new int[Enum.GetValues(typeof(Class)).Length];
			}
			runsPerClass[selectedClass]++;
			if (stats.hasWon)
			{
				winsPerClass[selectedClass]++;
			}
			if (stats.hasWonCore)
			{
				coreWinsPerClass[selectedClass]++;
			}
			if (runsPerClass[selectedClass] < winsPerClass[selectedClass])
			{
				runsPerClass[selectedClass] = winsPerClass[selectedClass];
			}
			if (stats.mostDamageDealtWithSingleAttack > mostDamageDealt)
			{
				mostDamageDealt = stats.mostDamageDealtWithSingleAttack;
			}
			if (mostCoinsEarned < stats.coinsEarned)
			{
				mostCoinsEarned = stats.coinsEarned;
			}
			if (mostCoinsSpent < stats.coinsSpent)
			{
				mostCoinsSpent = stats.coinsSpent;
			}
			if (mostPegsHitInOneTurn < stats.mostPegsHitInOneTurn)
			{
				mostPegsHitInOneTurn = stats.mostPegsHitInOneTurn;
			}
			totalDamageDealt += ClampToMax(stats.totalDamageDealt);
			totalDamageNegated += ClampToMax(stats.totalDamageNegated);
			totalPegsHit += ClampToMax(stats.pegsHit);
			totalPegsHitRefresh += ClampToMax(stats.pegsHitRefresh);
			totalPegsHitCrit += ClampToMax(stats.pegsHitCrit);
			totalPegsRefreshed += ClampToMax(stats.pegsRefreshed);
			totalCoinsEarned += ClampToMax(stats.coinsEarned);
			totalCoinsSpent += ClampToMax(stats.coinsSpent);
			totalShotsTaken += ClampToMax(stats.shotsTaken);
			totalCritShotsTaken += ClampToMax(stats.critShotsTaken);
			totalBombsThrown += ClampToMax(stats.bombsThrown);
			totalBombsThrownRigged += ClampToMax(stats.bombsThrownRigged);
			totalBombsCreated += ClampToMax(stats.bombsCreated);
			totalBombsCreatedrigged += ClampToMax(stats.bombsCreatedRigged);
			UpdateOrbs(stats);
			UpdateRelics(stats);
			UpdateRooms(stats.visitedRooms);
		}

		private void UpdateRooms(Queue<RoomType> statsVisitedRooms)
		{
			foreach (RoomType statsVisitedRoom in statsVisitedRooms)
			{
				if (visitsPerRoomType == null || visitsPerRoomType.Length <= (int)statsVisitedRoom)
				{
					visitsPerRoomType = new int[Enum.GetValues(typeof(RoomType)).Length];
				}
				visitsPerRoomType[(int)statsVisitedRoom]++;
			}
		}

		private void UpdateOrbs(RunStats stats)
		{
			Dictionary<string, RunStats.OrbPlayData> orbStats = stats.orbStats;
			foreach (string key in orbStats.Keys)
			{
				if (historicOrbPlayData.TryGetValue(key, out var value))
				{
					RunStats.OrbPlayData orbPlayData = orbStats[key];
					value.timesDiscarded += orbPlayData.timesDiscarded;
					value.timesFired += orbPlayData.timesFired;
					value.damageDealt += orbPlayData.damageDealt;
					value.timesRemoved += orbPlayData.timesRemoved;
					if (!stats.isCustomRun && stats.hasWon && value.highestCruciballBeat < stats.cruciballLevel)
					{
						value.highestCruciballBeat = stats.cruciballLevel;
					}
				}
				else
				{
					historicOrbPlayData.Add(key, orbStats[key]);
					if (!stats.isCustomRun && stats.hasWon && orbStats[key].highestCruciballBeat < stats.cruciballLevel)
					{
						orbStats[key].highestCruciballBeat = stats.cruciballLevel;
					}
				}
			}
		}

		private void UpdateRelics(RunStats stats)
		{
			foreach (RelicEffect relic in stats.relics)
			{
				if (historicRelicData.TryGetValue(relic, out var value))
				{
					historicRelicData[relic].amount = ++value.amount;
					if (!stats.isCustomRun && stats.hasWon && historicRelicData[relic].highestCruciballBeat < stats.cruciballLevel)
					{
						historicRelicData[relic].highestCruciballBeat = stats.cruciballLevel;
					}
					continue;
				}
				RelicPlayData relicPlayData = new RelicPlayData
				{
					amount = 1
				};
				if (!stats.isCustomRun && stats.hasWon)
				{
					relicPlayData.highestCruciballBeat = stats.cruciballLevel;
				}
				historicRelicData.Add(relic, relicPlayData);
			}
		}

		private void UpdateEnemies(Dictionary<string, RunStats.EnemyPlayData> runEnemyData)
		{
			foreach (string key in runEnemyData.Keys)
			{
				RunStats.EnemyPlayData enemyPlayData = runEnemyData[key];
				if (historicEnemyPlayData.TryGetValue(key, out var value))
				{
					value.amountFought += enemyPlayData.amountFought;
					value.meleeDamageReceived += enemyPlayData.meleeDamageReceived;
					value.rangedDamageReceived += enemyPlayData.rangedDamageReceived;
					if (enemyPlayData.defeatedBy)
					{
						value.defeatedByAmount++;
					}
				}
				else
				{
					HistoricEnemyPlayData value2 = new HistoricEnemyPlayData(enemyPlayData);
					historicEnemyPlayData.Add(key, value2);
				}
			}
		}

		private static int ClampToMax(int value)
		{
			return Mathf.Clamp(value, 0, MAX_VALUE);
		}
	}
}
