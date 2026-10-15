using Steamworks;
using UnityEngine;

public class SteamRichPresence : MonoBehaviour
{
	public string Message = "default";

	public string Key = "steam_display";

	public void OnEnable()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (Message.Equals("default"))
		{
			Debug.LogWarning("Steam Rich Presence has not been set, clearing and returning...");
			SteamFriends.ClearRichPresence();
			return;
		}
		if (Key.Length > 64 || Message.Length > 256)
		{
			Debug.LogWarning("Steam Rich Presence key or message longer than the limit, clearing and returning...");
			SteamFriends.ClearRichPresence();
		}
		SteamFriends.SetRichPresence(Key, Message);
	}
}
