using PeglinUI.ControllerSupport;
using Rewired;
using UnityEngine;

public static class ControllerVibration
{
	public static void DoVibration(float intensityPercentage, float duration, float intensityRandomOffest = 0f, float durationRandomOffset = 0f, bool leftSide = true, bool rightSide = true)
	{
		if (ControllerInputManager.primaryInputType != InputType.Controller)
		{
			return;
		}
		float num = Mathf.Clamp(intensityPercentage + Random.Range(0f - intensityRandomOffest, intensityRandomOffest), 0f, 1f);
		if (SettingsManager.Instance != null)
		{
			if (SettingsManager.Instance.RumbleInstensity <= 0f)
			{
				return;
			}
			num *= SettingsManager.Instance.RumbleInstensity;
		}
		float duration2 = Mathf.Max(duration + Random.Range(0f - durationRandomOffset, durationRandomOffset), 0.01f);
		foreach (Joystick joystick in ReInput.players.GetPlayer(0).controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				if ((joystick.vibrationMotorCount > 0) & leftSide)
				{
					joystick.SetVibration(0, num, duration2);
				}
				if ((joystick.vibrationMotorCount > 1) & rightSide)
				{
					joystick.SetVibration(1, num, duration2);
				}
			}
		}
	}

	public static void DoEnemyHitVibration(float damage, float enemyMaxHealth, float screenShakeIntensityReference)
	{
		if (!(damage < screenShakeIntensityReference))
		{
			if (damage > enemyMaxHealth)
			{
				DoVibration(1f, 0.35f, 0f, 0.05f);
			}
			else
			{
				DoVibration(Mathf.Lerp(0.3f, 0.9f, (damage - screenShakeIntensityReference) / (enemyMaxHealth - screenShakeIntensityReference)), 0.2f, 0.05f, 0.05f);
			}
		}
	}

	public static void DoPlayerHitVibration(float damage, float maxHealth)
	{
		if (!(damage < 5f))
		{
			DoVibration(Mathf.Lerp(0.3f, 1f, damage / (maxHealth / 2f)), 0.2f, 0.05f, 0.05f);
		}
	}
}
