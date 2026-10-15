using System.Collections.Generic;
using Map;
using ToolBox.Serialization;
using UnityEngine;

namespace Challenges
{
	internal class ChallengeSaveData : SaveObjectData
	{
		public static readonly string KEY = "ChallengeManager";

		[SerializeField]
		public int ForestChallenge;

		[SerializeField]
		public int CastleChallenge;

		[SerializeField]
		public int MinesChallenge;

		[SerializeField]
		public int CoreChallenge;

		[SerializeField]
		public bool ForestChallengeComplete;

		[SerializeField]
		public bool CastleChallengeComplete;

		[SerializeField]
		public bool MinesChallengeComplete;

		[SerializeField]
		public bool CoreChallengeComplete;

		[SerializeField]
		public int[] RandomChallengesQueue;

		[SerializeField]
		public int[] AvailableChallenges;

		[SerializeField]
		public bool BeatenMinesBossAndCanGoToCore;

		[SerializeField]
		public SeededBattleNodeSaveData[] MimicChallengeQueue;

		[SerializeField]
		public SeededBattleNodeSaveData[] MinibossesForEachAct;

		[SerializeField]
		public float[] MinibossForEachActRoll;

		[SerializeField]
		public SeededBattleNodeSaveData[] BossesForEachAct;

		[SerializeField]
		public float[] BossForEachActRoll;

		[SerializeField]
		public int PainbowSlimeCurrentHealth;

		[SerializeField]
		public int PainbowSlimeCoinsStolen;

		public override string Name => KEY;

		public ChallengeSaveData(int forestChallenge = 0, bool forestChallengeComplete = false, int castleChallenge = 0, bool castleChallengeComplete = false, int minesChallenge = 0, bool minesChallengeComplete = false, int coreChallenge = 0, bool coreChallengeComplete = false)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			ForestChallenge = forestChallenge;
			CastleChallenge = castleChallenge;
			MinesChallenge = minesChallenge;
			CoreChallenge = coreChallenge;
			ForestChallengeComplete = forestChallengeComplete;
			CastleChallengeComplete = castleChallengeComplete;
			MinesChallengeComplete = minesChallengeComplete;
			CoreChallengeComplete = coreChallengeComplete;
		}

		public void SetRandomChallengesQueue(Queue<ChallengeEffect> queue)
		{
			int num = 0;
			RandomChallengesQueue = new int[queue.Count];
			foreach (ChallengeEffect item in queue)
			{
				RandomChallengesQueue[num++] = (int)item;
			}
		}

		public void SetAvailableChallenges(List<ChallengeEffect> list)
		{
			AvailableChallenges = new int[list.Count];
			int num = 0;
			foreach (ChallengeEffect item in list)
			{
				AvailableChallenges[num++] = (int)item;
			}
		}

		public void SetMimicChallengeQueue(Queue<SeededBattleNodeData> queue)
		{
			int num = 0;
			MimicChallengeQueue = new SeededBattleNodeSaveData[queue.Count];
			foreach (SeededBattleNodeData item in queue)
			{
				MimicChallengeQueue[num++] = item.GetSaveData() as SeededBattleNodeSaveData;
			}
		}

		public void SetNextActMinibossChallengeList(List<SeededBattleNodeData> minibossesList)
		{
			int num = 0;
			MinibossesForEachAct = new SeededBattleNodeSaveData[minibossesList.Count];
			foreach (SeededBattleNodeData minibosses in minibossesList)
			{
				MinibossesForEachAct[num++] = minibosses.GetSaveData() as SeededBattleNodeSaveData;
			}
		}

		public void SetBossChallengeList(List<SeededBattleNodeData> BossesList)
		{
			int num = 0;
			BossesForEachAct = new SeededBattleNodeSaveData[BossesList.Count];
			foreach (SeededBattleNodeData Bosses in BossesList)
			{
				BossesForEachAct[num++] = Bosses.GetSaveData() as SeededBattleNodeSaveData;
			}
		}
	}
}
