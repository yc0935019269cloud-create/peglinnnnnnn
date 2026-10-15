using Cruciball;
using Currency;
using I2.Loc;
using PeglinUI.OrbDisplay;
using Relics;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	public class UpgradeConfirmationBase : MonoBehaviour
	{
		public static readonly string LOC_TERM_ADD = "Menu/Add";

		public static readonly string LOC_TERM_UPGRADE = "Menu/Upgrade";

		public static readonly string LOC_TERM_REMOVE = "Menu/Remove";

		public static readonly string LOC_TERM_DUPLICATE = "Menu/Duplicate";

		public static readonly string LOC_TERM_ACCEPT = "Menu/Accept";

		public static bool isOpen = false;

		public Image itemImage;

		public Button confirmationButton;

		public Localize confirmationText;

		public TooltipKeywordDescriptions keywordDescriptions;

		public SimpleCostText goldText;

		public RelicManager relicManager;

		public CruciballManager cruciballManager;

		public DeckManager deckManager;

		public bool shouldBlockToolbarButtonInput = true;

		[Header("Sound Effects")]
		public UIButtonClickEventDispatcher acceptAudioDispatcher;

		public AudioClip acceptSfx;

		private SimpleCostText _goldText;

		private LocalizationParamsManager[] locParamsManager;

		private void OnEnable()
		{
			if (TooltipManager.Instance != null)
			{
				TooltipManager.Instance.HideTooltip();
			}
			if (shouldBlockToolbarButtonInput)
			{
				isOpen = true;
			}
			if (locParamsManager == null)
			{
				locParamsManager = GetComponentsInChildren<LocalizationParamsManager>();
			}
		}

		private void OnDisable()
		{
			isOpen = false;
		}

		public void SetGold(int gold)
		{
			goldText.SetGold(gold);
		}

		public void SetUpgradeType(UpgradeOption.UpgradeType type)
		{
			confirmationButton.GetComponent<UpgradeOption>().upgradeType = type;
		}

		public void UpdateLocalizationParameters(string key, string value)
		{
			if (locParamsManager == null)
			{
				locParamsManager = GetComponentsInChildren<LocalizationParamsManager>();
			}
			LocalizationParamsManager[] array = locParamsManager;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetParameterValue(key, value);
			}
		}

		public void ShowKeywordDescriptions(string desc)
		{
			int num = keywordDescriptions.UpdateString(desc);
			keywordDescriptions.gameObject.SetActive(num > 0);
		}
	}
}
