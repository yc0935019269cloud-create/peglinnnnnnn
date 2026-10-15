using UnityEngine;

namespace Challenges
{
	public class ChallengeUIUpdater : MonoBehaviour
	{
		public int act = -1;

		private void OnEnable()
		{
			ChallengeUI.OnExternalUpdateUIRequest?.Invoke(act);
		}
	}
}
