using System.Collections.Generic;
using System.Linq;
using Battle.Attacks;
using Challenges;
using Cruciball;
using Currency;
using Map;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using Relics;
using Rewired.Integration.UnityUI;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class PopulateSuggestionOrbs : MonoBehaviour
{
	public UpgradeOption[] addOrbButtons;

	public Button[] potentialDownButtons;

	public Button continueButton;

	public CruciballManager cruciballManager;

	public DeckManager deckManager;

	public RelicManager relicManager;

	private RewiredEventSystem _rewiredEventSystem;

	public void Initialize(int initialAddCost, SeededBattleNodeData seededBattleData)
	{
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		if (addOrbButtons.Length < 3)
		{
			Debug.LogError("PopulateSuggestionOrbs::CreateNewOrbOptions(): addOrbButtons wasn't populated with enough objects");
			return;
		}
		GenerateAddableOrbs(seededBattleData);
		UpdateOrbAdditionCosts(initialAddCost);
	}

	private void GenerateAddableOrbs(SeededBattleNodeData seededBattleData)
	{
		int num = 3;
		if (relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS))
		{
			num++;
		}
		if (relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_PEGLIN_CHOICES))
		{
			num++;
		}
		Button selectOnDown = null;
		Button[] array = potentialDownButtons;
		foreach (Button button in array)
		{
			if (button.gameObject.activeInHierarchy)
			{
				selectOnDown = button;
				break;
			}
		}
		for (int j = 0; j < addOrbButtons.Length; j++)
		{
			bool flag = j < num;
			bool flag2 = j + 1 < num;
			addOrbButtons[j].gameObject.SetActive(flag);
			if (flag)
			{
				Button component = addOrbButtons[j].GetComponent<Button>();
				Navigation navigation = component.navigation;
				if (j > 0)
				{
					navigation.selectOnLeft = addOrbButtons[j - 1].GetComponent<Button>();
				}
				if (flag2 && j < addOrbButtons.Length - 1)
				{
					navigation.selectOnRight = addOrbButtons[j + 1].GetComponent<Button>();
				}
				navigation.selectOnDown = selectOnDown;
				navigation.selectOnUp = continueButton;
				component.navigation = navigation;
			}
		}
		List<GameObject> list = new List<GameObject>();
		if (seededBattleData != null)
		{
			list = new List<GameObject>(new List<GameObject>(deckManager.CheckAndUpdateSeededOrbOffers(seededBattleData.postBattleOrbs)).Take(num));
			for (int k = 0; k < list.Count; k++)
			{
				if (!(seededBattleData.orbUpgradeRolls[k * 2] < (float)(StaticGameData.totalFloorCount - 5) / 100f))
				{
					continue;
				}
				Attack component2 = list[k].GetComponent<Attack>();
				if (!component2 || !component2.NextLevelPrefab)
				{
					continue;
				}
				list[k] = component2.NextLevelPrefab;
				if (seededBattleData.orbUpgradeRolls[k * 2 + 1] < (float)(StaticGameData.totalFloorCount - 5) / 200f)
				{
					component2 = list[k].GetComponent<Attack>();
					if ((bool)component2 && (bool)component2.NextLevelPrefab)
					{
						list[k] = component2.NextLevelPrefab;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < num; l++)
			{
				List<GameObject> randomOrbPool = GetRandomOrbPool();
				int index = Random.Range(0, randomOrbPool.Count);
				GameObject gameObject = randomOrbPool[index];
				int num2 = 0;
				while (list.Contains(gameObject))
				{
					index = Random.Range(0, randomOrbPool.Count);
					gameObject = randomOrbPool[index];
					num2++;
					if (num2 > 20)
					{
						Debug.LogWarning("While Loop hit max count while trying to avoid duplicate orbs.Possible Orb pool misconfiguration for " + randomOrbPool);
						break;
					}
				}
				if (Random.value < (float)(StaticGameData.totalFloorCount - 5) / 100f)
				{
					Attack component3 = gameObject.GetComponent<Attack>();
					if ((bool)component3 && (bool)component3.NextLevelPrefab)
					{
						gameObject = component3.NextLevelPrefab;
						if (Random.value < (float)(StaticGameData.totalFloorCount - 5) / 200f)
						{
							component3 = gameObject.GetComponent<Attack>();
							if ((bool)component3 && (bool)component3.NextLevelPrefab)
							{
								gameObject = component3.NextLevelPrefab;
							}
						}
					}
				}
				list.Add(gameObject);
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			GameObject gameObject2 = list[m];
			gameObject2.GetComponent<Attack>().SoftInit(deckManager, relicManager, cruciballManager);
			UpgradeOption obj = addOrbButtons[m];
			obj.SpecifiedOrb = gameObject2;
			obj.upgradeType = UpgradeOption.UpgradeType.INSPECT_NEW_ORB;
		}
		if (ControllerInputManager.IsPointerBased)
		{
			_rewiredEventSystem.GetComponent<ControllerMenuHookup>().LastSelected = addOrbButtons[0].gameObject;
		}
		else
		{
			_rewiredEventSystem.SetSelectedGameObject(addOrbButtons[0].gameObject);
		}
	}

	private List<GameObject> GetRandomOrbPool()
	{
		float value = Random.value;
		if (value <= 0.6f)
		{
			return deckManager.CommonOrbPool;
		}
		if (value <= 0.9f)
		{
			return deckManager.UncommonOrbPool;
		}
		return deckManager.RareOrbPool;
	}

	public void UpdateOrbAdditionCosts(int baseCost)
	{
		UpgradeOption[] array = addOrbButtons;
		foreach (UpgradeOption upgradeOption in array)
		{
			if (!upgradeOption)
			{
				continue;
			}
			SimpleCostText componentInChildren = upgradeOption.GetComponentInChildren<SimpleCostText>();
			if (!componentInChildren)
			{
				continue;
			}
			int num = baseCost;
			if ((bool)upgradeOption.SpecifiedOrb)
			{
				Attack component = upgradeOption.SpecifiedOrb.GetComponent<Attack>();
				num += component.PostBattleAddLevelCostMod;
				if (relicManager.WandOfGreedEffectActive())
				{
					num += component.PostBattleAddLevelCostMod;
				}
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				num = ChallengeManager.GetHpCostForGold(num);
			}
			componentInChildren.SetGold(num);
		}
	}

	public void DisableAddButton(GameObject forOrb)
	{
		UpgradeOption[] array = addOrbButtons;
		foreach (UpgradeOption upgradeOption in array)
		{
			if (!upgradeOption.gameObject || !(upgradeOption.SpecifiedOrb == forOrb))
			{
				continue;
			}
			Navigation navigation = upgradeOption.GetComponent<Button>().navigation;
			upgradeOption.SetOptionInactive();
			Selectable selectOnLeft = navigation.selectOnLeft;
			Selectable selectOnRight = navigation.selectOnRight;
			if (selectOnLeft != null && selectOnLeft.gameObject.activeInHierarchy)
			{
				AssignButtonActive(navigation.selectOnLeft.gameObject);
			}
			else if (selectOnRight != null && selectOnRight.gameObject.activeInHierarchy)
			{
				AssignButtonActive(navigation.selectOnRight.gameObject);
			}
			else
			{
				AssignButtonActive(navigation.selectOnDown.gameObject);
			}
			if (selectOnLeft != null && selectOnRight != null)
			{
				Navigation navigation2 = selectOnLeft.navigation;
				navigation2.selectOnRight = selectOnRight;
				selectOnLeft.navigation = navigation2;
				navigation2 = selectOnRight.navigation;
				navigation2.selectOnLeft = selectOnLeft;
				selectOnRight.navigation = navigation2;
			}
			foreach (Transform item in upgradeOption.GetComponentInChildren<SimpleCostText>().transform.parent)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	private void AssignButtonActive(GameObject button)
	{
		_rewiredEventSystem.GetComponent<ControllerMenuHookup>().LastSelected = button;
		if (!ControllerInputManager.IsPointerBased)
		{
			_rewiredEventSystem.SetSelectedGameObject(button);
			ButtonHandleHover component = button.GetComponent<ButtonHandleHover>();
			if (component != null)
			{
				component.Highlight();
			}
		}
	}
}
