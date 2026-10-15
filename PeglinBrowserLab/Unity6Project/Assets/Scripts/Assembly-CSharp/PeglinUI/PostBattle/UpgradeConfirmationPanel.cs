using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using I2.Loc;
using Peglin.PegMinigame;
using Relics;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	public class UpgradeConfirmationPanel : MonoBehaviour
	{
		[HideInInspector]
		public static bool isOpen;

		[SerializeField]
		private Image _orbImage;

		[SerializeField]
		private Image _levelTextHolder;

		[SerializeField]
		private GameObject _confirmationButton;

		[SerializeField]
		private TextMeshProUGUI _confirmationText;

		[SerializeField]
		private UpgradeDetailsManager _upgradeDetailsManager;

		[SerializeField]
		private Sprite _orbLevel1Sprite;

		[SerializeField]
		private Sprite _orbLevel2Sprite;

		[SerializeField]
		private Sprite _orbLevel3Sprite;

		[SerializeField]
		private bool _shouldBlockToolbarButtonInput = true;

		[Header("Sound Effects")]
		[SerializeField]
		private UIButtonClickEventDispatcher _acceptAudioDispacher;

		[SerializeField]
		private AudioClip orbAcceptSFX;

		[SerializeField]
		private AudioClip relicAcceptSFX;

		public GameObject confirmationButton => _confirmationButton;

		private void OnEnable()
		{
			if (_acceptAudioDispacher != null)
			{
				_acceptAudioDispacher.sfxOverride = orbAcceptSFX;
			}
			TooltipManager.Instance.HideTooltip();
			if (_shouldBlockToolbarButtonInput)
			{
				isOpen = true;
			}
		}

		private void OnDisable()
		{
			isOpen = false;
		}

		public void PopulateDataForOrbUpgrade(GameObject orb)
		{
			Attack component = orb.GetComponent<Attack>();
			if (!component.HasUpgrade)
			{
				_confirmationButton.SetActive(value: false);
			}
			else
			{
				_confirmationButton.SetActive(value: true);
				_confirmationText.text = LocalizationManager.GetTranslation("Menu/Upgrade");
				UpgradeOption component2 = _confirmationButton.GetComponent<UpgradeOption>();
				component2.upgradeType = UpgradeOption.UpgradeType.UPGRADE_CONFIRMED;
				component2.SpecifiedOrb = orb;
			}
			PachinkoBall component3 = orb.GetComponent<PachinkoBall>();
			_orbImage.sprite = component3.sprite;
			_orbImage.color = component3.color;
			PopulateOrbLevelDisplay(component);
			Attack component4 = orb.GetComponent<Attack>();
			Attack component5 = orb.GetComponent<Attack>();
			Attack after = null;
			if (component5.NextLevelPrefab != null)
			{
				after = component5.NextLevelPrefab.GetComponent<Attack>();
			}
			else if (component5.GetComponent<UnlimitedUpgrades>() != null)
			{
				after = component4;
			}
			_upgradeDetailsManager.PopulateUpgradeDetails(component4, after);
		}

		public void PopulateDataForNewOrb(GameObject orb)
		{
			if (_confirmationButton != null)
			{
				_confirmationButton.SetActive(value: true);
				_confirmationText.text = LocalizationManager.GetTranslation("Menu/Add");
				_confirmationButton.GetComponent<UpgradeOption>().upgradeType = UpgradeOption.UpgradeType.NEW_ORB_CONFIRMED;
			}
			PopulateSingleOrbDetails(orb);
		}

		public void PopulateDataForRemoveOrb(GameObject orb)
		{
			if (_confirmationButton != null)
			{
				_confirmationButton.SetActive(value: true);
				_confirmationText.text = LocalizationManager.GetTranslation("Menu/Remove");
				UpgradeOption component = _confirmationButton.GetComponent<UpgradeOption>();
				component.upgradeType = UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED;
				component.SpecifiedOrb = orb;
			}
			PopulateSingleOrbDetails(orb);
		}

		public void PopulateDataForDuplication(GameObject orb)
		{
			if (_confirmationButton != null)
			{
				_confirmationButton.SetActive(value: true);
				_confirmationText.text = LocalizationManager.GetTranslation("Menu/Duplicate");
				UpgradeOption component = _confirmationButton.GetComponent<UpgradeOption>();
				component.upgradeType = UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED;
				component.SpecifiedOrb = orb;
			}
			PopulateSingleOrbDetails(orb);
		}

		public void PopulateDataForInspectOrb(GameObject orb)
		{
			if (_confirmationButton != null)
			{
				_confirmationButton.SetActive(value: false);
			}
			PopulateSingleOrbDetails(orb);
		}

		public void PopulateDataForRelic(Relic relic)
		{
			if (_confirmationButton != null)
			{
				_confirmationButton.SetActive(value: true);
				_confirmationText.text = LocalizationManager.GetTranslation("Menu/Add");
				_confirmationButton.GetComponent<UpgradeOption>().upgradeType = UpgradeOption.UpgradeType.ACCEPT_RELIC;
				if (_acceptAudioDispacher != null)
				{
					_acceptAudioDispacher.sfxOverride = relicAcceptSFX;
				}
			}
			_orbImage.sprite = relic.sprite;
			if (_levelTextHolder != null)
			{
				_levelTextHolder.gameObject.SetActive(value: false);
			}
			_upgradeDetailsManager.PopulateRelicDetails(relic);
		}

		public void PopulateDataForReward(Reward reward)
		{
			if (reward is OrbReward)
			{
				PopulateDataForNewOrb(((OrbReward)reward).Orb);
			}
			else if (reward is RelicReward)
			{
				PopulateDataForRelic(((RelicReward)reward).Relic);
			}
		}

		private void PopulateSingleOrbDetails(GameObject orb)
		{
			if (orb != null)
			{
				PachinkoBall component = orb.GetComponent<PachinkoBall>();
				_orbImage.sprite = component.sprite;
				_orbImage.color = component.color;
				Attack component2 = orb.GetComponent<Attack>();
				if (component2 != null)
				{
					PopulateOrbLevelDisplay(component2);
				}
				_upgradeDetailsManager.PopulateSingleOrbDetails(component2);
			}
		}

		private void PopulateOrbLevelDisplay(Attack attack)
		{
			if (attack != null && (attack.Level > 0 || attack.NextLevelPrefab != null))
			{
				_levelTextHolder.gameObject.SetActive(value: true);
				if (attack.Level == 1)
				{
					_levelTextHolder.GetComponent<Image>().sprite = _orbLevel1Sprite;
				}
				else if (attack.Level == 2)
				{
					_levelTextHolder.GetComponent<Image>().sprite = _orbLevel2Sprite;
				}
				else
				{
					_levelTextHolder.GetComponent<Image>().sprite = _orbLevel3Sprite;
				}
			}
			else
			{
				_levelTextHolder?.gameObject.SetActive(value: false);
			}
		}
	}
}
