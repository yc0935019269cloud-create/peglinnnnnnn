using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace Map
{
	public abstract class SeededMapNodeData
	{
		public abstract void GenerateData(DeckManager _deckManager, RelicManager _relicManager, MapController _mapController);

		public abstract SeededMapNodeSaveData GetSaveData();

		public abstract void LoadSaveData(SeededMapNodeSaveData data, DeckManager _deckManager, RelicManager _relicManager, MapController _mapController);

		protected (string[], int[]) GetOrbNameAndRaritySave(SeededOrbAndRarity[] orbsArray)
		{
			List<string> list = new List<string>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < orbsArray.Length; i++)
			{
				SeededOrbAndRarity seededOrbAndRarity = orbsArray[i];
				list.Add(seededOrbAndRarity.orb.name);
				list2.Add((int)seededOrbAndRarity.orbRarity);
			}
			return (list.ToArray(), list2.ToArray());
		}

		protected SeededOrbAndRarity[] LoadOrbsSaveData(string[] orbNames, int[] orbRarities, DeckManager deckManager)
		{
			List<SeededOrbAndRarity> list = new List<SeededOrbAndRarity>();
			for (int i = 0; i < orbNames.Length; i++)
			{
				list.Add(new SeededOrbAndRarity(deckManager.GetOrbPrefabFromName(orbNames[i]), (PachinkoBall.OrbRarity)orbRarities[i]));
			}
			return list.ToArray();
		}

		protected string GetMapDataNameSave(MapData mapData)
		{
			string result = "";
			if (mapData != null)
			{
				result = mapData.name;
			}
			return result;
		}

		protected MapData LoadMapDataName(string mapDataName, MapDataType mapDataType, MapController mapController)
		{
			if (mapDataName == "")
			{
				return null;
			}
			return mapController.LoadMapDataByNameFromAllData(mapDataName, mapDataType);
		}

		public abstract void PrintSeededData();

		public static int ConvertPercentageToListIndex(float randomNumber, int numItemsInList)
		{
			int num = Mathf.FloorToInt(randomNumber * (float)numItemsInList);
			if (num == numItemsInList && num > 0)
			{
				num--;
			}
			return num;
		}

		protected void PrintSeededOrbAndRarityArray(string orbArrayName, SeededOrbAndRarity[] seededOrbAndRaritiesArray)
		{
			if (seededOrbAndRaritiesArray == null)
			{
				Debug.Log(orbArrayName + ": []");
				return;
			}
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < seededOrbAndRaritiesArray.Length; i++)
			{
				SeededOrbAndRarity seededOrbAndRarity = seededOrbAndRaritiesArray[i];
				list.Add(seededOrbAndRarity.orb);
			}
			PrintSeededDataArray(orbArrayName, list.ToArray());
		}

		protected void PrintSeededDataArray<T>(string arrayName, T[] array)
		{
			string text = arrayName + ": ";
			if (array == null)
			{
				Debug.Log(text + "[]");
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				T val = array[i];
				text = text + val.ToString() + ", ";
			}
			Debug.Log(text);
		}
	}
}
