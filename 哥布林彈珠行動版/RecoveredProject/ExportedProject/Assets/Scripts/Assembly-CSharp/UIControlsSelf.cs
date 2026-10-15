using UnityEngine;

public class UIControlsSelf : MonoBehaviour
{
	public delegate void ClosePressed();

	public static ClosePressed OnClosePressed;

	public void Close()
	{
		if (!PauseMenu.Paused)
		{
			OnClosePressed?.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
