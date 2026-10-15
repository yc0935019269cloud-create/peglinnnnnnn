using System.Collections.Generic;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using UnityEngine;

namespace PeglinUI.PostBattle
{
	public class UpgradeOrbInfo
	{
		public struct OrbData
		{
			private sealed class LevelRelationalComparer : IComparer<OrbData>
			{
				public int Compare(OrbData x, OrbData y)
				{
					return x.level.CompareTo(y.level);
				}
			}

			public int level;

			public Attack attack;

			public static IComparer<OrbData> LevelComparer { get; } = new LevelRelationalComparer();

			public OrbData(int level, Attack attack)
			{
				this.level = level;
				this.attack = attack;
			}
		}

		public List<OrbData> orderedOrbUpgradesList = new List<OrbData>();

		public int currentLevel;

		public int maxLevel;

		public UpgradeOrbInfo(GameObject orb)
		{
			SetupOrbList(orb.GetComponent<Attack>());
		}

		private void SetupOrbList(Attack attack)
		{
			UnlimitedUpgrades component = attack.GetComponent<UnlimitedUpgrades>();
			if (attack.PreviousLevelPrefab == null)
			{
				currentLevel = 0;
				orderedOrbUpgradesList.Add(new OrbData(currentLevel, attack));
				CompileLevelUps(attack, currentLevel, attack);
			}
			else
			{
				GameObject previousLevelPrefab = attack.PreviousLevelPrefab;
				Attack attack2 = null;
				currentLevel = 0;
				while (previousLevelPrefab != null)
				{
					currentLevel++;
					Attack component2 = previousLevelPrefab.GetComponent<Attack>();
					if (component2.PreviousLevelPrefab == null)
					{
						attack2 = component2;
						break;
					}
					previousLevelPrefab = component2.PreviousLevelPrefab;
				}
				orderedOrbUpgradesList.Add(new OrbData(0, attack2));
				CompileLevelUps(attack2, 0, attack);
			}
			orderedOrbUpgradesList.Sort(OrbData.LevelComparer);
			if (component != null)
			{
				orderedOrbUpgradesList[orderedOrbUpgradesList.Count - 1].attack.gameObject.GetComponent<UnlimitedUpgrades>().timesUpgraded = component.timesUpgraded;
			}
		}

		private void CompileLevelUps(Attack attack, int level, Attack originalAttack)
		{
			maxLevel = level;
			GameObject nextLevelPrefab = attack.NextLevelPrefab;
			while (nextLevelPrefab != null)
			{
				maxLevel++;
				Attack component = nextLevelPrefab.GetComponent<Attack>();
				if (component.Level == originalAttack.Level)
				{
					component.CloneInstanceId(originalAttack);
				}
				OrbData item = new OrbData(++level, component);
				orderedOrbUpgradesList.Add(item);
				nextLevelPrefab = item.attack.NextLevelPrefab;
			}
		}
	}
}
