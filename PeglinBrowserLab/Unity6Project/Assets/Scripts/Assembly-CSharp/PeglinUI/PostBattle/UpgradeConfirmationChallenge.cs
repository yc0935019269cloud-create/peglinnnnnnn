using Challenges;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace PeglinUI.PostBattle
{
	[RequireComponent(typeof(UpgradeConfirmationBase))]
	public class UpgradeConfirmationChallenge : MonoBehaviour
	{
		public AudioClip challengeAcceptSfx;

		public TextMeshProUGUI challengeName;

		public TextMeshProUGUI challengeDescription;

		public LocalizationParamsManager challengeDescParams;

		private UpgradeConfirmationBase _base;

		private void OnEnable()
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
		}

		public void Populate(ChallengeEffect challenge)
		{
			if (_base == null)
			{
				_base = GetComponent<UpgradeConfirmationBase>();
			}
			_base.SetGold(0);
			_base.confirmationButton.gameObject.SetActive(value: true);
			_base.confirmationButton.gameObject.SetActive(value: true);
			_base.confirmationText.Term = UpgradeConfirmationBase.LOC_TERM_ACCEPT;
			_base.SetUpgradeType(UpgradeOption.UpgradeType.ADD_CHALLENGE);
			if (_base.acceptAudioDispatcher != null && challengeAcceptSfx != null)
			{
				_base.acceptAudioDispatcher.sfxOverride = challengeAcceptSfx;
			}
			ChallengeData dataForChallenge = ChallengeManager.Instance.GetDataForChallenge(challenge);
			_base.itemImage.sprite = dataForChallenge.sprite;
			challengeName.GetComponent<Localize>().Term = dataForChallenge.nameKey;
			challengeDescription.GetComponent<Localize>().Term = dataForChallenge.descKey;
			_base.ShowKeywordDescriptions(challengeDescription.text);
		}
	}
}
