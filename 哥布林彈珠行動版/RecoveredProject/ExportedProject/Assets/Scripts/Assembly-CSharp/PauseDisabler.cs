using UnityEngine;

public class PauseDisabler : MonoBehaviour
{
	private void OnEnable()
	{
		PauseMenu.PauseBlock = true;
	}
}
