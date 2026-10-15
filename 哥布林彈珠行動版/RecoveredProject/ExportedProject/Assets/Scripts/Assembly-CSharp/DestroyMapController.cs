using System;
using Map;
using Relics;
using UnityEngine;

public class DestroyMapController : MonoBehaviour
{
	public static Action OnLoadMainMenu;

	[SerializeField]
	private RelicManager _relicManager;

	private void OnEnable()
	{
		StaticGameData.dataToLoad = null;
		StaticGameData.currentNode = null;
		if (MapController.instance != null)
		{
			UnityEngine.Object.Destroy(MapController.instance.gameObject);
		}
		if (_relicManager != null)
		{
			_relicManager.Reset();
		}
		OnLoadMainMenu?.Invoke();
	}
}
