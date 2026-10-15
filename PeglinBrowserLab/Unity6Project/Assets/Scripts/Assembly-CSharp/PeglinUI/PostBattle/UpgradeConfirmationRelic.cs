using Challenges;
using I2.Loc;
using Relics;
using TMPro;
using UnityEngine;

namespace PeglinUI.PostBattle
{
	[RequireComponent(typeof(UpgradeConfirmationBase))]
	public class UpgradeConfirmationRelic : MonoBehaviour
	{
		public AudioClip relicAcceptSfx;

		public TextMeshProUGUI relicName;

		public TextMeshProUGUI relicDescription;

		public LocalizationParamsManager relicDescParams;

		private UpgradeConfirmationBase _base;

		private void OnEnable()
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
		}

		public void Populate(Relic relic, int gold)
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
			_base.confirmationButton.gameObject.SetActive(value: true);
			_base.confirmationText.Term = UpgradeConfirmationBase.LOC_TERM_ADD;
			_base.SetUpgradeType(UpgradeOption.UpgradeType.ACCEPT_RELIC);
			if (_base.acceptAudioDispatcher != null && relicAcceptSfx != null)
			{
				_base.acceptAudioDispatcher.sfxOverride = relicAcceptSfx;
			}
			_base.itemImage.sprite = relic.sprite;
			relicName.GetComponent<Localize>().Term = relic.nameKey;
			relicDescription.GetComponent<Localize>().Term = relic.descKey;
			_base.UpdateLocalizationParameters("DECK_SIZE_HEAL_TOTAL", RelicManager.GetDeckSizeHealValue().ToString());
			_base.ShowKeywordDescriptions(relicDescription.text);
			UpdateRelicParams();
		}

		private void UpdateRelicParams()
		{
			if (relicDescParams != null)
			{
				relicDescParams.SetParameterValue("ECHO_CURRENT_DMG", "");
				relicDescParams.SetParameterValue("DECK_SIZE_HEAL_TOTAL", RelicManager.GetDeckSizeHealValue().ToString());
			}
		}
	}
}
