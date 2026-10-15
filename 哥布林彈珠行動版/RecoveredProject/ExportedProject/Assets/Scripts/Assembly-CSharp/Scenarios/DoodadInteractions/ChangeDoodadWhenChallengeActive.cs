using Challenges;
using UnityEngine;

namespace Scenarios.DoodadInteractions
{
	public class ChangeDoodadWhenChallengeActive : MonoBehaviour
	{
		public Sprite toReplace;

		private void Start()
		{
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				GetComponent<Animator>().enabled = false;
				GetComponent<SpriteRenderer>().sprite = toReplace;
			}
		}
	}
}
