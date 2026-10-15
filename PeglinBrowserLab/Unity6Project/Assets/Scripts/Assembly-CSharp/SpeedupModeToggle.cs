using UnityEngine;
using UnityEngine.UI;

public class SpeedupModeToggle : MonoBehaviour
{
	public bool Toggle = true;

	private void OnEnable()
	{
		if (SettingsManager.Instance.SpeedupToggle == Toggle)
		{
			GetComponent<Toggle>().isOn = true;
		}
	}

	public void SetSpeedupMode()
	{
		SettingsManager.Instance.SpeedupToggle = Toggle;
	}
}
