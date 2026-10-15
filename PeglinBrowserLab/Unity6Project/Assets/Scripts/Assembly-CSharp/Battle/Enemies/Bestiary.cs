using System.Collections.Generic;
using UnityEngine;

namespace Battle.Enemies
{
	[CreateAssetMenu(menuName = "Encirclopedia/Bestiary")]
	public class Bestiary : ScriptableObject
	{
		public EnemyData[] enemyDataList;

		private Dictionary<string, EnemyData> bestiaryDict;

		public void UpdateFromSaveData(EnemyData.EnemySaveData[] saveData)
		{
			bestiaryDict = new Dictionary<string, EnemyData>();
			EnemyData[] array = enemyDataList;
			foreach (EnemyData enemyData in array)
			{
				bestiaryDict.Add(enemyData.LocKey, enemyData);
			}
			foreach (EnemyData.EnemySaveData enemySaveData in saveData)
			{
				if (bestiaryDict.TryGetValue(enemySaveData.LocKey, out var value))
				{
					value.Seen = enemySaveData.Seen;
					value.Killed = enemySaveData.Killed;
					value.KilledBy = enemySaveData.KilledBy;
					value.CardDrop = enemySaveData.CardDrop;
					value.LootDrop = enemySaveData.LootDrop;
				}
			}
		}
	}
}
