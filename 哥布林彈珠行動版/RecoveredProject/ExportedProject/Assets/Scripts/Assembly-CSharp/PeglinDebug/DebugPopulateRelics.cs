using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace PeglinDebug
{
	public class DebugPopulateRelics : MonoBehaviour
	{
		public RelicManager rm;

		public RelicSet globalRelics;

		private static bool hasPopulated;

		private void Awake()
		{
			if (!hasPopulated)
			{
				rm.Reset();
				PopulateRelicPoolsWithDefaultRarity(globalRelics.relics, rm);
				rm.SetupInternalRelicPools();
				hasPopulated = true;
			}
		}

		public static void PopulateRelicPoolsWithDefaultRarity(List<Relic> relics, RelicManager rm)
		{
			foreach (Relic relic in relics)
			{
				switch (relic.globalRarity)
				{
				case RelicRarity.COMMON:
					rm.CommonRelicPool.Add(relic);
					break;
				case RelicRarity.RARE:
					rm.RareRelicPool.Add(relic);
					break;
				case RelicRarity.BOSS:
					rm.BossRelicPool.Add(relic);
					break;
				case RelicRarity.NONE:
					rm.RareScenarioRelicPool.Add(relic);
					break;
				case RelicRarity.UNAVAILABLE:
					rm.UnavailableRelicPool.Add(relic);
					break;
				}
			}
		}
	}
}
