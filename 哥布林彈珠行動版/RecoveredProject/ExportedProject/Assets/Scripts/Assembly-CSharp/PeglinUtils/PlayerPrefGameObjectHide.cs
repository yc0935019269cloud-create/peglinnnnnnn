using NaughtyAttributes;
using UnityEngine;

namespace PeglinUtils
{
	public class PlayerPrefGameObjectHide : MonoBehaviour
	{
		public string playerPrefKey = "SteamVisited";

		public int threshold;

		[Tooltip("Used for modals that only need to be viewed X times instead of interacted with")]
		public bool incrementOnStart;

		private void Start()
		{
			if (incrementOnStart)
			{
				IncrementCount();
			}
			else
			{
				CheckForDisable();
			}
		}

		public void IncrementCount()
		{
			int num = PlayerPrefs.GetInt(playerPrefKey, 0);
			if (num < 1000)
			{
				PlayerPrefs.SetInt(playerPrefKey, num + 1);
			}
			CheckForDisable();
		}

		public void CheckForDisable()
		{
			if (PlayerPrefs.GetInt(playerPrefKey, 0) > threshold)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		[Button(null, EButtonEnableMode.Always)]
		public void ResetPlayerPref()
		{
			PlayerPrefs.SetInt(playerPrefKey, 0);
		}
	}
}
