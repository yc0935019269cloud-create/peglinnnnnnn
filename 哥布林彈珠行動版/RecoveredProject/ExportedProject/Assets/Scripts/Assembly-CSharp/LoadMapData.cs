using Loading;
using UnityEngine;

[CreateAssetMenu(menuName = "Save System/Load Map Data")]
public class LoadMapData : ScriptableObject
{
	public PeglinSceneLoader.Scene NewGameScene = PeglinSceneLoader.Scene.FOREST_MAP;

	public PeglinSceneLoader.Scene SceneToLoad = PeglinSceneLoader.Scene.FOREST_MAP;

	public bool NewGame;

	public bool ContinueGame;
}
