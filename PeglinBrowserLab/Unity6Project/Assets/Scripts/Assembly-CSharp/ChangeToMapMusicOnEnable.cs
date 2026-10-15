using Map;
using UnityEngine;

public class ChangeToMapMusicOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		if (MapController.instance != null)
		{
			StartCoroutine(BGMController.instance.WaitAndPlayRunSummaryMapMusic(BGMController.instance.mapTracks[MapController.instance.Act - 1]));
		}
	}
}
