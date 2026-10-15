using UnityEngine;

public class LoadMainMenuBGM : MonoBehaviour
{
	public AudioClip MainMenuBGM;

	private void Start()
	{
		if (BGMController.instance != null)
		{
			BGMController.instance.SetMapAudio(MainMenuBGM);
		}
	}
}
