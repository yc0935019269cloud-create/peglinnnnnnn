using System.Collections.Generic;
using Data;
using Relics;
using UnityEngine;

namespace Map
{
	public class SeededBattleNodeData : SeededMapNodeData
	{
		public MapDataBattle easyBattle;

		public MapDataBattle regularBattle;

		public MapDataBattle minibossBattle;

		public float rainbowSlimeRoll;

		public SeededOrbAndRarity[] postBattleOrbs;

		public float[] orbUpgradeRolls;

		public float[] c19ExtraEnemyRolls;

		public float randomNavRoll;

		public SerializableRandomState deckOrderRandomState;

		public SerializableRandomState specialPegShufflingRandomState;

		public List<SerializableRandomState> bossMechanicsRandomStatesPerAct;

		public override void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			easyBattle = _mapController.GetSeededRandomBattleOfType(MapDataType.EASY_BATTLE);
			regularBattle = _mapController.GetSeededRandomBattleOfType(MapDataType.RANDOM_BATTLE);
			minibossBattle = _mapController.GetSeededRandomBattleOfType(MapDataType.ELITE_BATTLE);
			rainbowSlimeRoll = Random.value;
			postBattleOrbs = _deckManager.GetMultipleSeededRandomOrbs(5);
			orbUpgradeRolls = MapController.GetMultipleSeededRolls(10);
			c19ExtraEnemyRolls = MapController.GetMultipleSeededRolls(50);
			randomNavRoll = Random.value;
			deckOrderRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			specialPegShufflingRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
		}

		public void GenerateMinibossData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController, bool allowMimic, bool forceMimic = false)
		{
			minibossBattle = _mapController.GetSeededRandomBattleOfType(MapDataType.ELITE_BATTLE, allowMimic);
			if (forceMimic)
			{
				minibossBattle = _mapController.MimicBattleData;
			}
			rainbowSlimeRoll = Random.value;
			postBattleOrbs = _deckManager.GetMultipleSeededRandomOrbs(5);
			orbUpgradeRolls = MapController.GetMultipleSeededRolls(10);
			randomNavRoll = Random.value;
			deckOrderRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			specialPegShufflingRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
		}

		public void GenerateBossData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			postBattleOrbs = _deckManager.GetMultipleSeededRandomOrbs(5);
			orbUpgradeRolls = MapController.GetMultipleSeededRolls(10);
			randomNavRoll = Random.value;
			deckOrderRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			specialPegShufflingRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			bossMechanicsRandomStatesPerAct = new List<SerializableRandomState>();
			for (int i = 0; i < 4; i++)
			{
				bossMechanicsRandomStatesPerAct.Add(new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0));
			}
		}

		public override SeededMapNodeSaveData GetSaveData()
		{
			string mapDataNameSave = GetMapDataNameSave(easyBattle);
			string mapDataNameSave2 = GetMapDataNameSave(regularBattle);
			string mapDataNameSave3 = GetMapDataNameSave(minibossBattle);
			(string[], int[]) orbNameAndRaritySave = GetOrbNameAndRaritySave(postBattleOrbs);
			string[] item = orbNameAndRaritySave.Item1;
			int[] item2 = orbNameAndRaritySave.Item2;
			string[] postBattleOrbNamesSaveData = item;
			int[] postBattleOrbRaritiesSaveData = item2;
			SerializableRandomState[] array = null;
			if (bossMechanicsRandomStatesPerAct != null)
			{
				array = bossMechanicsRandomStatesPerAct.ToArray();
			}
			return new SeededBattleNodeSaveData(rainbowSlimeRoll, orbUpgradeRolls, mapDataNameSave, mapDataNameSave2, mapDataNameSave3, postBattleOrbNamesSaveData, postBattleOrbRaritiesSaveData, c19ExtraEnemyRolls, randomNavRoll, deckOrderRandomState, specialPegShufflingRandomState, array);
		}

		public override void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController)
		{
			SeededBattleNodeSaveData seededBattleNodeSaveData = data as SeededBattleNodeSaveData;
			rainbowSlimeRoll = seededBattleNodeSaveData.rainbowSlimeRoll;
			orbUpgradeRolls = seededBattleNodeSaveData.orbUpgradeRolls;
			c19ExtraEnemyRolls = seededBattleNodeSaveData.c19ExtraEnemyRolls;
			easyBattle = LoadMapDataName(seededBattleNodeSaveData.easyBattleSaveData, MapDataType.EASY_BATTLE, _mapController) as MapDataBattle;
			regularBattle = LoadMapDataName(seededBattleNodeSaveData.regularBattleSaveData, MapDataType.RANDOM_BATTLE, _mapController) as MapDataBattle;
			minibossBattle = LoadMapDataName(seededBattleNodeSaveData.minibossBattleSaveData, MapDataType.ELITE_BATTLE, _mapController) as MapDataBattle;
			postBattleOrbs = LoadOrbsSaveData(seededBattleNodeSaveData.postBattleOrbNamesSaveData, seededBattleNodeSaveData.postBattleOrbRaritiesSaveData, _deckManager);
			randomNavRoll = seededBattleNodeSaveData.randomNavRoll;
			if (seededBattleNodeSaveData.deckOrderRandomState != null && seededBattleNodeSaveData.deckOrderRandomState != null)
			{
				deckOrderRandomState = new SerializableRandomState(seededBattleNodeSaveData.deckOrderRandomState.initializationSeed, seededBattleNodeSaveData.deckOrderRandomState.timesUsed);
			}
			else
			{
				deckOrderRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			}
			if (seededBattleNodeSaveData.specialPegShufflingRandomState != null && seededBattleNodeSaveData.specialPegShufflingRandomState != null)
			{
				specialPegShufflingRandomState = new SerializableRandomState(seededBattleNodeSaveData.specialPegShufflingRandomState.initializationSeed, seededBattleNodeSaveData.specialPegShufflingRandomState.timesUsed);
			}
			else
			{
				specialPegShufflingRandomState = new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0);
			}
			if (seededBattleNodeSaveData.bossMechanicsRandomStatesPerAct != null && seededBattleNodeSaveData.bossMechanicsRandomStatesPerAct.Length != 0 && seededBattleNodeSaveData.bossMechanicsRandomStatesPerAct[0] != null)
			{
				bossMechanicsRandomStatesPerAct = new List<SerializableRandomState>();
				for (int i = 0; i < 4; i++)
				{
					bossMechanicsRandomStatesPerAct.Add(new SerializableRandomState(seededBattleNodeSaveData.bossMechanicsRandomStatesPerAct[i].initializationSeed, seededBattleNodeSaveData.bossMechanicsRandomStatesPerAct[i].timesUsed));
				}
			}
			else
			{
				bossMechanicsRandomStatesPerAct = new List<SerializableRandomState>();
				for (int j = 0; j < 4; j++)
				{
					bossMechanicsRandomStatesPerAct.Add(new SerializableRandomState(Random.Range(int.MinValue, int.MaxValue), 0));
				}
			}
		}

		public override void PrintSeededData()
		{
			Debug.Log("\n- SeededBattleNodeData -");
			if (easyBattle != null)
			{
				Debug.Log("\t easyBattle: " + easyBattle.ToString());
			}
			else
			{
				Debug.Log("\t easyBattle: None");
			}
			if (regularBattle != null)
			{
				Debug.Log("\t regularBattle: " + regularBattle.ToString());
			}
			else
			{
				Debug.Log("\t regularBattle: None");
			}
			if (minibossBattle != null)
			{
				Debug.Log("\t minibossBattle: " + minibossBattle.ToString());
			}
			else
			{
				Debug.Log("\t minibossBattle: None");
			}
			Debug.Log("\t rainbowSlimeRoll: " + rainbowSlimeRoll);
			PrintSeededOrbAndRarityArray("\t postBattleOrbs", postBattleOrbs);
			PrintSeededDataArray("\t orbUpgradeRolls", orbUpgradeRolls);
			PrintSeededDataArray("\t c19ExtraEnemyRolls", c19ExtraEnemyRolls);
			Debug.Log("\t randomNavRoll: " + randomNavRoll);
			Debug.Log("\t See the seeded relic queue for what the relic rewards will be.");
		}
	}
}
