using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace Map
{
	public class SeededShopNodeData : SeededMapNodeData
	{
		public const int NUMBER_OF_COMMON_RELICS = 5;

		public const int NUMBER_OF_RARE_RELICS = 1;

		public const int NUMBER_OF_ORBS = 6;

		public float rareRelicChanceRoll;

		public float shopRelicChanceRoll;

		public SeededOrbAndRarity[] shopOrbs;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			rareRelicChanceRoll = Random.value;
			shopRelicChanceRoll = Random.value;
			shopOrbs = _deckManager.GetMultipleSeededRandomOrbs(6);
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			(string[], int[]) orbNameAndRaritySave = GetOrbNameAndRaritySave(shopOrbs);
			string[] item = orbNameAndRaritySave.Item1;
			int[] item2 = orbNameAndRaritySave.Item2;
			string[] shopOrbsSaveData = item;
			int[] shopOrbsRaritySaveData = item2;
			return new SeededShopNodeSaveData(rareRelicChanceRoll, shopRelicChanceRoll, shopOrbsSaveData, shopOrbsRaritySaveData);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededShopNodeSaveData seededShopNodeSaveData = data as SeededShopNodeSaveData;
			rareRelicChanceRoll = seededShopNodeSaveData.rareRelicChanceRoll;
			shopRelicChanceRoll = seededShopNodeSaveData.shopRelicChanceRoll;
			shopOrbs = LoadOrbsSaveData(seededShopNodeSaveData.shopOrbsSaveData, seededShopNodeSaveData.shopOrbsRaritySaveData, _deckManager);
			if (shopOrbs != null && shopOrbs.Length != 0 && shopOrbs.Length < 6)
			{
				List<SeededOrbAndRarity> list = new List<SeededOrbAndRarity>(shopOrbs);
				for (int i = 0; i < 6 - shopOrbs.Length; i++)
				{
					list.Add(shopOrbs[0]);
				}
				shopOrbs = list.ToArray();
			}
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededShopNodeData -");
			Debug.Log("\t rareRelicChanceRoll: " + rareRelicChanceRoll);
			Debug.Log("\t shopRelicChanceRoll: " + shopRelicChanceRoll);
			PrintSeededOrbAndRarityArray("\t shopOrbs", shopOrbs);
			Debug.Log("\t See the seeded relic queue for what relics will be inside.");
		}
	}
}
