using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace Peglin.ClassSystem
{
	[CreateAssetMenu(menuName = "Classes/Class Loadout")]
	public class ClassLoadoutData : ScriptableObject
	{
		public Class Class;

		public List<GameObject> StartingOrbs;

		public List<Relic> StartingRelics;

		public RelicSet CommonRelicOverrides;

		public RelicSet RareRelicOverrides;

		public RelicSet BossRelicOverrides;

		public RelicSet RareScenarioRelicOverrides;

		public RelicSet UnavailableRelicOverrides;

		public RelicSet ShopRelicOverrides;

		[Range(0f, 1f)]
		public float chanceForGuaranteedShopRelicInShops = 1f;
	}
}
