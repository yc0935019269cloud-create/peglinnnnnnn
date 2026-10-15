using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Challenges;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	[RequireComponent(typeof(UpgradeConfirmationBase))]
	public class UpgradeConfirmationOrbUpgrade : MonoBehaviour
	{
		public UpgradeConfirmationTooltip[] tooltips;

		public GameObject arrow12;

		public GameObject arrow23;

		public Toggle showAllLevelsToggle;

		private UpgradeConfirmationBase _base;

		private UpgradeOrbInfo _orbInfo;

		private int _startingLevel;

		private void OnEnable()
		{
			_base = GetComponent<UpgradeConfirmationBase>();
			showAllLevelsToggle.isOn = false;
			showAllLevelsToggle.onValueChanged.AddListener(ToggleClicked);
		}

		private void OnDisable()
		{
			showAllLevelsToggle.onValueChanged.RemoveListener(ToggleClicked);
		}

		public void Populate(GameObject orb, int gold)
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
			_orbInfo = new UpgradeOrbInfo(orb);
			PachinkoBall component = orb.GetComponent<PachinkoBall>();
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				gold = ChallengeManager.GetHpCostForGold(gold);
			}
			_base.SetGold(gold);
			_base.itemImage.sprite = component.sprite;
			_base.itemImage.color = component.color;
			_startingLevel = _orbInfo.currentLevel;
			arrow12.SetActive(_startingLevel == 0);
			arrow23.SetActive(_startingLevel > 0);
			ShowAvailableUpgrade();
			UpgradeOption component2 = _base.confirmationButton.GetComponent<UpgradeOption>();
			component2.upgradeType = UpgradeOption.UpgradeType.UPGRADE_CONFIRMED;
			component2.SpecifiedOrb = orb;
		}

		private void ToggleClicked(bool val)
		{
			if (val)
			{
				ShowAllUpgrades();
			}
			else
			{
				ShowAvailableUpgrade();
			}
		}

		private void ShowAvailableUpgrade()
		{
			Attack comparison = ((_startingLevel != 0) ? _orbInfo.orderedOrbUpgradesList[_startingLevel].attack : null);
			UnlimitedUpgrades component = _orbInfo.orderedOrbUpgradesList[_startingLevel].attack.GetComponent<UnlimitedUpgrades>();
			if ((bool)component)
			{
				ShowAvailableUnlimitedUpgrades(_orbInfo.orderedOrbUpgradesList[_startingLevel].attack, component);
			}
			else
			{
				showAllLevelsToggle.gameObject.SetActive(value: true);
				for (int i = 0; i < _orbInfo.orderedOrbUpgradesList.Count; i++)
				{
					Attack attack = _orbInfo.orderedOrbUpgradesList[i].attack;
					attack.SoftInit(_base.deckManager, _base.relicManager, _base.cruciballManager);
					attack.ApplyUniqueBuffToUpgrades(_orbInfo.orderedOrbUpgradesList[_startingLevel].attack);
					tooltips[i].Init(attack, comparison);
					tooltips[i].SetGrayedOut(i > _startingLevel);
					if (Mathf.Abs(i - _startingLevel) >= 2)
					{
						tooltips[i].gameObject.SetActive(value: false);
					}
					else
					{
						tooltips[i].gameObject.SetActive(value: true);
					}
					if (i < _startingLevel)
					{
						tooltips[i].gameObject.SetActive(value: false);
					}
					if (i >= _startingLevel)
					{
						comparison = attack;
					}
				}
			}
			string description = _orbInfo.orderedOrbUpgradesList[_orbInfo.orderedOrbUpgradesList.Count - 1].attack.Description;
			_base.ShowKeywordDescriptions(description);
		}

		private void ShowAvailableUnlimitedUpgrades(Attack attack, UnlimitedUpgrades unlimitedUpgrades)
		{
			showAllLevelsToggle.gameObject.SetActive(value: false);
			arrow12.gameObject.SetActive(value: true);
			arrow23.gameObject.SetActive(value: false);
			tooltips[0].InitUnlimitedUpgrades(attack);
			tooltips[0].gameObject.SetActive(value: true);
			tooltips[1].InitUnlimitedUpgrades(attack, unlimitedUpgrades);
			tooltips[1].gameObject.SetActive(value: true);
			tooltips[2].gameObject.SetActive(value: false);
		}

		private void ShowAllUpgrades()
		{
			UpgradeConfirmationTooltip[] array = tooltips;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
	}
}
