using Challenges;
using UnityEngine;
using UnityEngine.UI;

namespace Currency
{
	public class GoldImageChanger : MonoBehaviour
	{
		public bool changeToHeartWithHealthToPurchaseChallange = true;

		public Sprite healthSprite;

		private void OnEnable()
		{
			if (changeToHeartWithHealthToPurchaseChallange && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				GetComponent<Image>().sprite = healthSprite;
			}
		}
	}
}
