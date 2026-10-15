using UnityEngine;

public class ChangeMusicOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		BGMController.instance.SetDefeatAudio();
	}
}
