using System;
using Steamworks;
using UnityEngine;

public class OpenUrl : MonoBehaviour
{
	public string url;

	public void Open()
	{
		try
		{
			SteamFriends.ActivateGameOverlayToWebPage(url);
		}
		catch (Exception)
		{
			Application.OpenURL(url);
		}
	}
}
