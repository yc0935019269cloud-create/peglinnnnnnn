using System.Collections.Generic;
using Battle.Attacks;
using Map;
using UnityEngine;

namespace Peglin.PegMinigame
{
	[CreateAssetMenu(fileName = "New PegMinigameOrbPrize", menuName = "Scenarios/PegMinigameOrbPrize")]
	public class MapDataPegMinigameOrbs : MapDataPegMinigame
	{
		public DeckManager deckManager;

		public int numberOfRewards = 3;

		public override void PopulateRewards(SeededPegMinigameNodeData seededPegMinigameNodeData)
		{
			Rewards = new List<Reward>();
			if (seededPegMinigameNodeData != null)
			{
				GameObject[] array = deckManager.CheckAndUpdateSeededOrbOffers(seededPegMinigameNodeData.orbRewards);
				foreach (GameObject gameObject in array)
				{
					GameObject gameObject2 = gameObject;
					if (MapController.instance != null && MapController.instance.Act > 1 && gameObject.TryGetComponent<Attack>(out var component) && component.NextLevelPrefab != null)
					{
						gameObject2 = component.NextLevelPrefab;
						if (MapController.instance.Act > 2 && gameObject2.TryGetComponent<Attack>(out var component2) && component2.NextLevelPrefab != null)
						{
							gameObject2 = component2.NextLevelPrefab;
						}
					}
					Rewards.Add(new OrbReward(gameObject2, deckManager));
				}
				return;
			}
			List<int> list = new List<int>();
			for (int j = 0; j < numberOfRewards; j++)
			{
				List<GameObject> randomOrbPool = deckManager.GetRandomOrbPool();
				int num = Random.Range(0, randomOrbPool.Count);
				while (list.Contains(num) && numberOfRewards < randomOrbPool.Count)
				{
					num = Random.Range(0, randomOrbPool.Count);
				}
				list.Add(num);
				Rewards.Add(new OrbReward(randomOrbPool[num], deckManager));
			}
		}
	}
}
