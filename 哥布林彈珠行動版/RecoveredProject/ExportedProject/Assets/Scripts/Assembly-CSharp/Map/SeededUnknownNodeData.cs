using Relics;
using UnityEngine;

namespace Map
{
	public class SeededUnknownNodeData : SeededMapNodeData
	{
		public float typeRoll;

		public float mimicChallengeChanceRoll;

		public SeededTreasureNodeData treasureData;

		public SeededTextScenarioNodeData textScenerioData;

		public SeededShopNodeData shopData;

		public SeededBattleNodeData battleData;

		public SeededBattleNodeData minibossData;

		public SeededPegMinigameNodeData pegMinigameData;

		public bool generateTextData = true;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			typeRoll = Random.value;
			mimicChallengeChanceRoll = Random.value;
			treasureData = new SeededTreasureNodeData();
			treasureData.GenerateData(_deckManager, _relicManager, _mapController);
			shopData = new SeededShopNodeData();
			shopData.GenerateData(_deckManager, _relicManager, _mapController);
			battleData = new SeededBattleNodeData();
			battleData.GenerateData(_deckManager, _relicManager, _mapController);
			minibossData = new SeededBattleNodeData();
			minibossData.GenerateMinibossData(_deckManager, _relicManager, _mapController, allowMimic: true);
			pegMinigameData = new SeededPegMinigameNodeData();
			pegMinigameData.GenerateData(_deckManager, _relicManager, _mapController);
			if (generateTextData)
			{
				textScenerioData = new SeededTextScenarioNodeData();
				textScenerioData.generateUnknownData = false;
				textScenerioData.GenerateData(_deckManager, _relicManager, _mapController);
			}
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			SeededTreasureNodeSaveData seededTreasureNodeSaveData = treasureData.GetSaveData() as SeededTreasureNodeSaveData;
			SeededShopNodeSaveData seededShopNodeSaveData = shopData.GetSaveData() as SeededShopNodeSaveData;
			SeededBattleNodeSaveData seededBattleNodeSaveData = battleData.GetSaveData() as SeededBattleNodeSaveData;
			SeededBattleNodeSaveData seededBattleNodeSaveData2 = minibossData.GetSaveData() as SeededBattleNodeSaveData;
			SeededPegMinigameNodeSaveData seededPegMinigameNodeSaveData = pegMinigameData.GetSaveData() as SeededPegMinigameNodeSaveData;
			SeededTextScenarioNodeSaveData seededTextScenarioNodeSaveData = null;
			if (generateTextData)
			{
				seededTextScenarioNodeSaveData = textScenerioData.GetSaveData() as SeededTextScenarioNodeSaveData;
			}
			return new SeededUnknownNodeSaveData(typeRoll, mimicChallengeChanceRoll, seededTreasureNodeSaveData, seededTextScenarioNodeSaveData, seededShopNodeSaveData, seededBattleNodeSaveData, seededBattleNodeSaveData2, seededPegMinigameNodeSaveData, generateTextData);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededUnknownNodeSaveData seededUnknownNodeSaveData = data as SeededUnknownNodeSaveData;
			typeRoll = seededUnknownNodeSaveData.typeRoll;
			mimicChallengeChanceRoll = seededUnknownNodeSaveData.mimicChallengeChanceRoll;
			treasureData = new SeededTreasureNodeData();
			treasureData.LoadSaveData(seededUnknownNodeSaveData.treasureData, _deckManager, _relicManager, _mapController);
			shopData = new SeededShopNodeData();
			shopData.LoadSaveData(seededUnknownNodeSaveData.shopData, _deckManager, _relicManager, _mapController);
			battleData = new SeededBattleNodeData();
			battleData.LoadSaveData(seededUnknownNodeSaveData.battleData, _deckManager, _relicManager, _mapController);
			minibossData = new SeededBattleNodeData();
			minibossData.LoadSaveData(seededUnknownNodeSaveData.minibossData, _deckManager, _relicManager, _mapController);
			pegMinigameData = new SeededPegMinigameNodeData();
			pegMinigameData.LoadSaveData(seededUnknownNodeSaveData.pegMinigameData, _deckManager, _relicManager, _mapController);
			generateTextData = seededUnknownNodeSaveData.generateTextData;
			if (generateTextData)
			{
				textScenerioData = new SeededTextScenarioNodeData();
				textScenerioData.LoadSaveData(seededUnknownNodeSaveData.textScenerioData, _deckManager, _relicManager, _mapController);
			}
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededUnknownNodeData -");
			Debug.Log("\t typeRoll: " + typeRoll);
			Debug.Log("\t mimicChanceRoll: " + mimicChallengeChanceRoll);
			GetDataType();
		}

		public SeededMapNodeData GetDataType(bool canGetElitesInUnknowns = true, bool printResult = true)
		{
			if (printResult)
			{
				Debug.Log("Printing what this node will be...");
			}
			float num = 0.05f;
			float num2 = 0.56f;
			float num3 = (canGetElitesInUnknowns ? 0.09f : 0f);
			num2 -= num3 / 2f;
			float num4 = typeRoll;
			if (num4 < num)
			{
				if (printResult)
				{
					treasureData.PrintSeededData();
				}
				return treasureData;
			}
			if (num4 < num2 + num)
			{
				if (generateTextData)
				{
					if (printResult)
					{
						textScenerioData.PrintSeededData();
					}
				}
				else
				{
					Debug.LogWarning("Should never get here?");
					textScenerioData.PrintSeededData();
				}
				return textScenerioData;
			}
			if (num4 < num3 + num2 + num)
			{
				if (printResult)
				{
					Debug.Log("Miniboss battle");
					minibossData.PrintSeededData();
				}
				return minibossData;
			}
			if (printResult)
			{
				Debug.Log("Could be either an easy or regular battle.");
				battleData.PrintSeededData();
			}
			return battleData;
		}
	}
}
