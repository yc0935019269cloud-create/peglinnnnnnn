using Data.Scenarios;
using Relics;
using UnityEngine;

namespace Map
{
	public class SeededTextScenarioNodeData : SeededMapNodeData
	{
		public MapDataScenario textScenario;

		public SerializableRandomState randomState;

		public SeededUnknownNodeData unknownNodeData;

		public float mimicChallengeChanceRoll;

		public bool generateUnknownData = true;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			textScenario = _mapController.GetSeededRandomScenario();
			randomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			if (generateUnknownData)
			{
				unknownNodeData = new SeededUnknownNodeData();
				unknownNodeData.generateTextData = false;
				unknownNodeData.GenerateData(_deckManager, _relicManager, _mapController);
			}
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			string mapDataNameSave = GetMapDataNameSave(textScenario);
			SeededUnknownNodeSaveData seededUnknownNodeSaveData = null;
			if (generateUnknownData)
			{
				seededUnknownNodeSaveData = unknownNodeData.GetSaveData() as SeededUnknownNodeSaveData;
			}
			return new SeededTextScenarioNodeSaveData(randomState, seededUnknownNodeSaveData, mapDataNameSave, mimicChallengeChanceRoll, generateUnknownData);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededTextScenarioNodeSaveData seededTextScenarioNodeSaveData = data as SeededTextScenarioNodeSaveData;
			if (seededTextScenarioNodeSaveData.randomState != null && seededTextScenarioNodeSaveData.randomState != null)
			{
				randomState = new SerializableRandomState(seededTextScenarioNodeSaveData.randomState.initializationSeed, seededTextScenarioNodeSaveData.randomState.timesUsed);
			}
			else
			{
				randomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			}
			textScenario = LoadMapDataName(seededTextScenarioNodeSaveData.textScenarioNameSaveData, MapDataType.RANDOM_SCENARIO, _mapController) as MapDataScenario;
			mimicChallengeChanceRoll = seededTextScenarioNodeSaveData.mimicChallengeChanceRoll;
			generateUnknownData = seededTextScenarioNodeSaveData.generateUnknownData;
			unknownNodeData = null;
			if (generateUnknownData)
			{
				unknownNodeData = new SeededUnknownNodeData();
				unknownNodeData.LoadSaveData(seededTextScenarioNodeSaveData.unknownNodeSaveData, _deckManager, _relicManager, _mapController);
			}
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededTextScenarioNodeData -");
			Debug.Log("\t textScenario: " + textScenario.ToString());
			Debug.Log("\t mimicChanceRoll: " + mimicChallengeChanceRoll);
			Debug.Log("\t randomState: " + randomState.ToString());
		}
	}
}
