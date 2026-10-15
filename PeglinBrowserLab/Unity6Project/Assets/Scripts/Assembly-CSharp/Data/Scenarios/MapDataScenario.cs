using Battle.Attacks;
using Currency;
using Map;
using Relics;
using UnityEngine;
using Worldmap;

namespace Data.Scenarios
{
	[CreateAssetMenu(fileName = "New Text Scenario", menuName = "Scenarios/Scenario")]
	public class MapDataScenario : MapData
	{
		public ScenarioPreReq[] scenarioPreReqs;

		public string scenarioName = "";

		public GameObject[] scenarioDoodads;

		public PegLayout navigationPegLayout;

		[Tooltip("Currently mostly used when layout contains coins and there's a benefit to navigating through it")]
		public bool forceNavigation;

		public override SceneType GetSceneType()
		{
			return SceneType.SCENARIO;
		}

		public virtual void Init(DeckManager deckManager)
		{
		}

		public bool ValidatePreReqs(MapController mapController, DeckManager deckManager = null, RelicManager relicManager = null, MapNode node = null, float currentHealth = 100f, float maxHealth = 100f)
		{
			if (scenarioPreReqs == null)
			{
				return true;
			}
			ScenarioPreReq[] array = scenarioPreReqs;
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case ScenarioPreReq.OneUpgradableOrb:
					if (deckManager == null || deckManager.GetUpgradeableOrbs().Count < 1)
					{
						return false;
					}
					break;
				case ScenarioPreReq.TwoUpgradableOrbs:
					if (deckManager == null || deckManager.GetUpgradeableOrbs().Count < 2)
					{
						return false;
					}
					break;
				case ScenarioPreReq.TwoUpgradableAndRemovableOrbs:
					if (deckManager == null || deckManager.GetUpgradeableAndRemovableOrbs().Count < 2)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreRemovableOrbs:
					if (deckManager.GetRemovableOrbs().Count < 1)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreOrbs:
					if (DeckManager.completeDeck.Count < 1)
					{
						return false;
					}
					break;
				case ScenarioPreReq.TwoOrMoreOrbs:
					if (DeckManager.completeDeck.Count < 2)
					{
						return false;
					}
					break;
				case ScenarioPreReq.ThreeOrMoreOrbs:
					if (DeckManager.completeDeck.Count < 3)
					{
						return false;
					}
					break;
				case ScenarioPreReq.ThreeOrMoreRemovableOrbs:
					if (deckManager.GetRemovableOrbs().Count < 3)
					{
						return false;
					}
					break;
				case ScenarioPreReq.FiveOrMoreOrbs:
					if (DeckManager.completeDeck.Count < 5)
					{
						return false;
					}
					break;
				case ScenarioPreReq.TwoOrMoreStones:
					if (deckManager == null || deckManager.GetNumberOfStonesInDeck() < 2)
					{
						return false;
					}
					break;
				case ScenarioPreReq.FourOrMoreStones:
					if (deckManager == null || deckManager.GetNumberOfStonesInDeck() < 4)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreUpgradeableStones:
					if (deckManager == null || deckManager.GetNumberOfUpgradeableStonesInDeck() <= 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreOrbelisk:
				{
					int num = 0;
					foreach (GameObject item in DeckManager.completeDeck)
					{
						Attack component2 = item.GetComponent<Attack>();
						if (component2 != null && component2.IsOrbelisk)
						{
							num++;
						}
					}
					if (num <= 0)
					{
						return false;
					}
					break;
				}
				case ScenarioPreReq.TenPlusDamage:
					if (maxHealth - currentHealth < 10f)
					{
						return false;
					}
					break;
				case ScenarioPreReq.LessThan35PercentHP:
					if (currentHealth / maxHealth > 0.35f)
					{
						return false;
					}
					break;
				case ScenarioPreReq.PreviousBossFightsAvailable:
				{
					ScenarioBossFightData component = mapController.GetComponent<ScenarioBossFightData>();
					if (component == null || component.availableBossFights.Count > 0)
					{
						return false;
					}
					break;
				}
				case ScenarioPreReq.SecretTunnelAvailable:
					if (node == null || node.secretTunnelConnection == null)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreLevelOneOrbs:
					if (deckManager == null || deckManager.GetOrbsForParticularLevel(1).Count == 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreRemovableLevelOneOrbs:
					if (deckManager == null || deckManager.GetRemovableOrbsForParticularLevel(1).Count == 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreLevelTwoOrbs:
					if (deckManager == null || deckManager.GetOrbsForParticularLevel(2).Count == 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreLevelThreeOrbs:
					if (deckManager == null || deckManager.GetOrbsForParticularLevel(3).Count == 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.NoRelicPokerChip:
					if (relicManager == null || relicManager.RelicEffectActive(RelicEffect.SLOT_MULTIPLIERS))
					{
						return false;
					}
					break;
				case ScenarioPreReq.NoRelicInfernalIngot:
					if (relicManager == null || relicManager.RelicEffectActive(RelicEffect.LIFESTEAL_PEG_HITS))
					{
						return false;
					}
					break;
				case ScenarioPreReq.NoRelicCake:
					if (relicManager == null || relicManager.RelicEffectActive(RelicEffect.MAX_HEALTH_LARGE))
					{
						return false;
					}
					break;
				case ScenarioPreReq.NoRelicSmelter:
					if (relicManager == null || relicManager.RelicEffectActive(RelicEffect.UPGRADE_ADDED_ORBS))
					{
						return false;
					}
					break;
				case ScenarioPreReq.NoRelicIOU:
					if (relicManager == null || relicManager.RelicEffectActive(RelicEffect.GAMBLIN_IOU))
					{
						return false;
					}
					break;
				case ScenarioPreReq.RelicIOUHasValue:
					if (relicManager == null || relicManager.GetRemainingCountdownForRelic(RelicEffect.GAMBLIN_IOU) <= 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.OneOrMoreGold:
					if (CurrencyManager.Instance.GoldAmount <= 0)
					{
						return false;
					}
					break;
				case ScenarioPreReq.MoreThan1Gold:
					if (CurrencyManager.Instance.GoldAmount < 1)
					{
						return false;
					}
					break;
				case ScenarioPreReq.MoreThan15Gold:
					if (CurrencyManager.Instance.GoldAmount < 15)
					{
						return false;
					}
					break;
				case ScenarioPreReq.MoreThan30Gold:
					if (CurrencyManager.Instance.GoldAmount < 30)
					{
						return false;
					}
					break;
				case ScenarioPreReq.LessThan300Gold:
					if (CurrencyManager.Instance.GoldAmount >= 300)
					{
						return false;
					}
					break;
				}
			}
			return true;
		}
	}
}
