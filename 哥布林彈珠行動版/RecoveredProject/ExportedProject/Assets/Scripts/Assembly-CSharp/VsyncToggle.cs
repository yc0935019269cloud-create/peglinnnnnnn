using UnityEngine;
using UnityEngine.UI;

public class VsyncToggle : MonoBehaviour
{
	private Toggle vsyncToggle;

	private void OnEnable()
	{
		if (vsyncToggle == null)
		{
			vsyncToggle = GetComponent<Toggle>();
		}
		vsyncToggle.isOn = SettingsManager.Instance.Vsync;
	}

	public void ToggleVsync()
	{
		SettingsManager.Instance.Vsync = vsyncToggle.isOn;
	}
}
