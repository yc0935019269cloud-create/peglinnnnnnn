using System.Collections.Generic;
using Map;
using Relics;
using UnityEngine;

namespace Peglin.PegMinigame
{
	[CreateAssetMenu(fileName = "New PegMinigameRelics", menuName = "Scenarios/PegMinigameRelics")]
	public class MapDataPegMinigameRelics : MapDataPegMinigame
	{
		public RelicManager relicManager;

		public RelicRarity rarity;

		public GameObject relicInfoWidget;

		public int numberOfRewards = 3;

		public override void PopulateRewards(SeededPegMinigameNodeData seededPegMinigameNodeData)
		{
			Rewards = new List<Reward>();
			Relic[] array;
			if (seededPegMinigameNodeData != null)
			{
				Relic[] multipleRelicsOffOfQueue = relicManager.GetMultipleRelicsOffOfQueue(numberOfRewards, rarity);
				List<RelicRarity> list = new List<RelicRarity>();
				for (int i = 0; i < numberOfRewards; i++)
				{
					list.Add(rarity);
				}
				array = relicManager.CheckAndUpdateSeededRelicOffers(multipleRelicsOffOfQueue, list.ToArray());
			}
			else
			{
				array = relicManager.GetMultipleRelicsOfRarity(numberOfRewards, rarity);
			}
			Relic[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RelicReward item = new RelicReward(array2[j], relicManager, relicInfoWidget);
				Rewards.Add(item);
			}
		}
	}
}
