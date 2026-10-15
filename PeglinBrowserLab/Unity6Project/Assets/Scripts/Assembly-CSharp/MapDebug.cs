using Loading;
using Map;
using UnityEngine;

public class MapDebug : MonoBehaviour
{
	private void Update()
	{
		if (Application.isEditor && Input.GetKeyDown(KeyCode.F5))
		{
			PeglinSceneLoader.Instance.LoadScene(GetComponent<MapController>().thisScene);
		}
	}
}
