using Steamworks;
using UnityEngine;

public class SteamworksDebug : MonoBehaviour
{
	private void Start()
	{
		if (SteamManager.Initialized)
		{
			Debug.Log(SteamFriends.GetPersonaName());
		}
	}

	private void Update()
	{
	}
}
