using UnityEngine;

public class PlatformSpecificToggle : MonoBehaviour
{
	public enum Platform
	{
		PC = 0,
		PC_STANDALONE = 1,
		CONSOLE = 2,
		MOBILE = 3,
		WEB = 4,
		IOS = 5
	}

	[Tooltip("Show or hide?")]
	public bool show = true;

	[Tooltip("Active when we are the target platform, or not?")]
	public bool when = true;

	public Platform platform;

	[Tooltip("Sometimes we want to display on Steam (and itch) but not other platforms like GOG or Epic")]
	public bool steamSpecific;

	[Tooltip("null = this. Used because we can't enable ourselves")]
	public GameObject[] targetObjs;

	private void Awake()
	{
		if (when && currentPlatformMatchesTarget())
		{
			if (targetObjs == null || targetObjs.Length == 0)
			{
				base.gameObject.SetActive(show);
				return;
			}
			GameObject[] array = targetObjs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(show);
			}
		}
		else
		{
			if (when || currentPlatformMatchesTarget())
			{
				return;
			}
			if (targetObjs == null || targetObjs.Length == 0)
			{
				base.gameObject.SetActive(show);
				return;
			}
			GameObject[] array = targetObjs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(show);
			}
		}
	}

	private bool currentPlatformMatchesTarget()
	{
		switch (platform)
		{
		case Platform.PC:
		{
			bool flag = !Application.isConsolePlatform && !Application.isMobilePlatform;
			_ = steamSpecific & flag;
			return flag;
		}
		case Platform.PC_STANDALONE:
			if (!Application.isConsolePlatform && !Application.isMobilePlatform)
			{
				return Application.platform != RuntimePlatform.WebGLPlayer;
			}
			return false;
		case Platform.CONSOLE:
			return Application.isConsolePlatform;
		case Platform.MOBILE:
			return Application.isMobilePlatform;
		case Platform.WEB:
			return Application.platform == RuntimePlatform.WebGLPlayer;
		case Platform.IOS:
			return false;
		default:
			return false;
		}
	}
}
