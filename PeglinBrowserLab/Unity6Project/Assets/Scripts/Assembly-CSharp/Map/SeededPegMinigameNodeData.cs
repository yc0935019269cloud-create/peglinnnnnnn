using Relics;
using UnityEngine;

namespace Map
{
	public class SeededPegMinigameNodeData : SeededMapNodeData
	{
		public float pegMinigameChoiceRoll;

		public SeededOrbAndRarity[] orbRewards;

		public float mimicChallengeChanceRoll;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			pegMinigameChoiceRoll = Random.value;
			orbRewards = _deckManager.GetMultipleSeededRandomOrbs(3);
			mimicChallengeChanceRoll = Random.value;
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			(string[], int[]) orbNameAndRaritySave = GetOrbNameAndRaritySave(orbRewards);
			string[] item = orbNameAndRaritySave.Item1;
			int[] item2 = orbNameAndRaritySave.Item2;
			string[] orbRewardsNamesSaveData = item;
			int[] orbRewardsRaritiesSaveData = item2;
			return new SeededPegMinigameNodeSaveData(pegMinigameChoiceRoll, orbRewardsNamesSaveData, orbRewardsRaritiesSaveData, mimicChallengeChanceRoll);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededPegMinigameNodeSaveData seededPegMinigameNodeSaveData = data as SeededPegMinigameNodeSaveData;
			pegMinigameChoiceRoll = seededPegMinigameNodeSaveData.pegMinigameChoiceRoll;
			orbRewards = LoadOrbsSaveData(seededPegMinigameNodeSaveData.orbRewardsNamesSaveData, seededPegMinigameNodeSaveData.orbRewardsRaritiesSaveData, _deckManager);
			mimicChallengeChanceRoll = seededPegMinigameNodeSaveData.mimicChallengeChanceRoll;
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededPegMinigameNodeData -");
			Debug.Log("\t pegMinigameChoiceRoll: " + pegMinigameChoiceRoll);
			Debug.Log("\t mimicChanceRoll: " + mimicChallengeChanceRoll);
			PrintSeededOrbAndRarityArray("\t orbRewards", orbRewards);
			Debug.Log("\t See the seeded relic queue for what the relic rewards will be.");
		}
	}
}
