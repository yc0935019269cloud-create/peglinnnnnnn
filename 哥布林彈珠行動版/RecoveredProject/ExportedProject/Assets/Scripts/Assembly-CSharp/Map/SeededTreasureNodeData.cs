using Relics;
using UnityEngine;

namespace Map
{
	public class SeededTreasureNodeData : SeededMapNodeData
	{
		public float rareRelicChanceRoll;

		public float mimicChallengeChanceRoll;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			rareRelicChanceRoll = Random.value;
			mimicChallengeChanceRoll = Random.value;
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			return new SeededTreasureNodeSaveData(rareRelicChanceRoll, mimicChallengeChanceRoll);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededTreasureNodeSaveData seededTreasureNodeSaveData = data as SeededTreasureNodeSaveData;
			rareRelicChanceRoll = seededTreasureNodeSaveData.rareRelicChanceRoll;
			mimicChallengeChanceRoll = seededTreasureNodeSaveData.mimicChallengeChanceRoll;
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededTreasureNodeData -");
			Debug.Log("\t rareRelicChanceRoll: " + rareRelicChanceRoll);
			Debug.Log("\t mimicChanceRoll: " + mimicChallengeChanceRoll);
			Debug.Log("\t See the seeded relic queue for what relics will be inside.");
		}
	}
}
