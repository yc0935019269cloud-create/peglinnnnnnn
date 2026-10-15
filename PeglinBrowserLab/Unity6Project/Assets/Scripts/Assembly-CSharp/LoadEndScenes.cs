using DG.Tweening;
using Map;
using UnityEngine;

public class LoadEndScenes : MonoBehaviour
{
	public void LoadWinScene()
	{
		DOTween.Clear();
		Object.FindObjectOfType<MapController>().ProceedAfterBattle();
	}
}
