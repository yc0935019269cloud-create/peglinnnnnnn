using Loading;
using Peglin.Achievements;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.Analytics;

public class MainMenuInit : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log("v" + Application.version);
		AnalyticsCommon.ugsAnalyticsEnabled = false;
		Time.timeScale = 1f;
		Screen.sleepTimeout = 120;
	}

	private void Start()
	{
		LoadAllAssets();
	}

	private static void LoadAllAssets()
	{
		DataSerializer.Setup();
		new GameObject("AchievementsManager").AddComponent<AchievementManager>();
		AssetLoading.Instance.LoadAllAssets(() =>
		{
			if (!Application.isConsolePlatform && !Application.isMobilePlatform)
			{
				Application.runInBackground = true;
			}
			PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MAIN_MENU);
		});
	}

	public void LoadCloudSaves()
	{
	}
}
