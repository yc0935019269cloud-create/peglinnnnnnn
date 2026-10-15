using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	public TextMeshProUGUI Text;

	public float HudRefreshRate = 1f;

	private int avgFrameRate;

	private float timer;

	private void Update()
	{
		if (Time.unscaledTime > timer)
		{
			int num = (int)(1f / Time.unscaledDeltaTime);
			Text.text = "FPS: " + num;
			timer = Time.unscaledTime + HudRefreshRate;
		}
	}
}
