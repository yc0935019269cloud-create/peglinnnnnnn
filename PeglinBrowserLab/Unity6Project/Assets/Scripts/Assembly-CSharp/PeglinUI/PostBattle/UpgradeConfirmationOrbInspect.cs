using Battle.Attacks;
using Challenges;
using Coffee.UIEffects;
using Cruciball;
using Relics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	[RequireComponent(typeof(UpgradeConfirmationBase))]
	public class UpgradeConfirmationOrbInspect : MonoBehaviour
	{
		public Button leftArrow;

		public Button rightArrow;

		public TextMeshProUGUI orbDescription;

		public TextMeshProUGUI orbTitle;

		public TextMeshProUGUI damageText;

		public TextMeshProUGUI critText;

		public GameObject lv2background;

		public GameObject lv3background;

		private UpgradeConfirmationBase _base;

		private UpgradeOrbInfo orbInfo;

		private Image _leftArrowImage;

		private Image _rightArrowImage;

		private DeckManager _deckManager;

		private RelicManager _relicManager;

		private CruciballManager _cruciballManager;

		private int startingLevel;

		private UIHsvModifier _hsvModifier;

		public void Init(DeckManager deckManager, RelicManager relicManager, CruciballManager cruciballManager)
		{
			_deckManager = deckManager;
			_relicManager = relicManager;
			_cruciballManager = cruciballManager;
		}

		private void OnEnable()
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
			if (_hsvModifier == null)
			{
				_hsvModifier = GetComponentInChildren<UIHsvModifier>();
			}
			_leftArrowImage = leftArrow.GetComponent<Image>();
			_rightArrowImage = rightArrow.GetComponent<Image>();
			_leftArrowImage.enabled = false;
			_rightArrowImage.enabled = false;
			leftArrow.enabled = false;
			rightArrow.enabled = false;
			leftArrow.onClick.AddListener(ShowPreviousLevel);
			rightArrow.onClick.AddListener(ShowNextLevel);
		}

		private void OnDisable()
		{
			leftArrow.onClick.RemoveListener(ShowPreviousLevel);
			rightArrow.onClick.RemoveListener(ShowNextLevel);
		}

		public void PopulateForNewOrb(GameObject orb, int gold)
		{
			Populate(orb, gold, UpgradeOption.UpgradeType.NEW_ORB_CONFIRMED, UpgradeConfirmationBase.LOC_TERM_ADD, activeConfirmation: true);
			UpgradeOption component = _base.confirmationButton.GetComponent<UpgradeOption>();
			component.upgradeType = UpgradeOption.UpgradeType.NEW_ORB_CONFIRMED;
			component.SpecifiedOrb = orb;
		}

		public void PopulateForRemoveOrb(GameObject orb, int gold)
		{
			Populate(orb, gold, UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED, UpgradeConfirmationBase.LOC_TERM_REMOVE, activeConfirmation: true);
			UpgradeOption component = _base.confirmationButton.GetComponent<UpgradeOption>();
			component.upgradeType = UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED;
			component.SpecifiedOrb = orb;
		}

		public void PopulateForDuplicateOrb(GameObject orb, int gold)
		{
			Populate(orb, gold, UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED, UpgradeConfirmationBase.LOC_TERM_DUPLICATE, activeConfirmation: true);
			UpgradeOption component = _base.confirmationButton.GetComponent<UpgradeOption>();
			component.upgradeType = UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED;
			component.SpecifiedOrb = orb;
		}

		public void PopulateForInspectOrb(GameObject orb)
		{
			Populate(orb, 0, UpgradeOption.UpgradeType.INSPECT_NEW_ORB, UpgradeConfirmationBase.LOC_TERM_ADD, activeConfirmation: false);
		}

		private void SetupArrows(GameObject orb)
		{
			orbInfo = new UpgradeOrbInfo(orb);
			startingLevel = orbInfo.currentLevel;
			CheckShowElements();
		}

		private void ShowNextLevel()
		{
			orbInfo.currentLevel = Mathf.Min(orbInfo.orderedOrbUpgradesList.Count - 1, orbInfo.currentLevel + 1);
			ShowLevelInfo(orbInfo.orderedOrbUpgradesList[orbInfo.currentLevel].attack);
			CheckShowElements();
		}

		private void ShowPreviousLevel()
		{
			orbInfo.currentLevel = Mathf.Max(0, orbInfo.currentLevel - 1);
			ShowLevelInfo(orbInfo.orderedOrbUpgradesList[orbInfo.currentLevel].attack);
			CheckShowElements();
		}

		private void CheckShowElements()
		{
			Button button = leftArrow;
			bool flag = (_leftArrowImage.enabled = orbInfo.currentLevel != 0);
			button.enabled = flag;
			Button button2 = rightArrow;
			flag = (_rightArrowImage.enabled = orbInfo.currentLevel < orbInfo.maxLevel);
			button2.enabled = flag;
			lv3background.SetActive(value: true);
			lv2background.SetActive(value: true);
			if (orbInfo.currentLevel < 2)
			{
				lv3background.SetActive(value: false);
			}
			if (orbInfo.currentLevel < 1)
			{
				lv2background.SetActive(value: false);
			}
			_hsvModifier.enabled = orbInfo.currentLevel != startingLevel;
		}

		private void ShowLevelInfo(Attack attack)
		{
			attack.SoftInit(_deckManager, _relicManager, _cruciballManager);
			attack.InitializeInventory();
			if (orbInfo != null && orbInfo.currentLevel != startingLevel)
			{
				attack.CheckUniqueBuff(orbInfo.orderedOrbUpgradesList[startingLevel].attack.instanceID);
			}
			orbTitle.text = attack.GetNameWithLevel();
			orbDescription.text = attack.Description;
			Tooltip.UpdateDamage(attack, damageText, critText);
			_base.ShowKeywordDescriptions(orbDescription.text);
		}

		private void Populate(GameObject orb, int gold, UpgradeOption.UpgradeType confirmUpgradeType, string term, bool activeConfirmation)
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				gold = ChallengeManager.GetHpCostForGold(gold);
			}
			_base.SetGold(gold);
			_base.confirmationButton.gameObject.SetActive(activeConfirmation);
			_base.confirmationButton.transform.parent.gameObject.SetActive(activeConfirmation);
			_base.confirmationText.Term = term;
			_base.SetUpgradeType(confirmUpgradeType);
			PachinkoBall component = orb.GetComponent<PachinkoBall>();
			_base.itemImage.sprite = component.sprite;
			_base.itemImage.color = component.color;
			ShowLevelInfo(orb.GetComponent<Attack>());
			SetupArrows(orb);
		}
	}
}
