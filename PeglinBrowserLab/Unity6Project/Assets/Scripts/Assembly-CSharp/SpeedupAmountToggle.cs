using UnityEngine;
using UnityEngine.UI;

public class SpeedupAmountToggle : MonoBehaviour
{
	public float Value = 1f;

	private void OnEnable()
	{
		if (SettingsManager.Instance.SpeedupLevel == Value)
		{
			GetComponent<Toggle>().isOn = true;
		}
	}

	public void SetSpeedupAmount()
	{
	}
}
