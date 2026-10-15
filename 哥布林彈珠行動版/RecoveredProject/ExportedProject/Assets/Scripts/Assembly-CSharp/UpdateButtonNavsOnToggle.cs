using UnityEngine;
using UnityEngine.UI;

public class UpdateButtonNavsOnToggle : MonoBehaviour
{
	public ButtonNavigationUpdateData[] allButtonsToUpdateOnEnable;

	public ButtonNavigationUpdateData[] allButtonsToUpdateOnDiable;

	private void OnEnable()
	{
		if (allButtonsToUpdateOnEnable == null)
		{
			return;
		}
		ButtonNavigationUpdateData[] array = allButtonsToUpdateOnEnable;
		foreach (ButtonNavigationUpdateData buttonNavigationUpdateData in array)
		{
			Navigation navigation = buttonNavigationUpdateData.button.navigation;
			if (buttonNavigationUpdateData.navOnUp != null)
			{
				navigation.selectOnUp = buttonNavigationUpdateData.navOnUp;
			}
			if (buttonNavigationUpdateData.navOnDown != null)
			{
				navigation.selectOnDown = buttonNavigationUpdateData.navOnDown;
			}
			if (buttonNavigationUpdateData.navOnLeft != null)
			{
				navigation.selectOnLeft = buttonNavigationUpdateData.navOnLeft;
			}
			if (buttonNavigationUpdateData.navOnRight != null)
			{
				navigation.selectOnRight = buttonNavigationUpdateData.navOnRight;
			}
			buttonNavigationUpdateData.button.navigation = navigation;
		}
	}

	private void OnDisable()
	{
		if (allButtonsToUpdateOnDiable == null)
		{
			return;
		}
		ButtonNavigationUpdateData[] array = allButtonsToUpdateOnDiable;
		foreach (ButtonNavigationUpdateData buttonNavigationUpdateData in array)
		{
			Navigation navigation = buttonNavigationUpdateData.button.navigation;
			if (buttonNavigationUpdateData.navOnUp != null)
			{
				navigation.selectOnUp = buttonNavigationUpdateData.navOnUp;
			}
			if (buttonNavigationUpdateData.navOnDown != null)
			{
				navigation.selectOnDown = buttonNavigationUpdateData.navOnDown;
			}
			if (buttonNavigationUpdateData.navOnLeft != null)
			{
				navigation.selectOnLeft = buttonNavigationUpdateData.navOnLeft;
			}
			if (buttonNavigationUpdateData.navOnRight != null)
			{
				navigation.selectOnRight = buttonNavigationUpdateData.navOnRight;
			}
			buttonNavigationUpdateData.button.navigation = navigation;
		}
	}

	public void DoOnEnabledNavUpdate()
	{
		OnEnable();
	}

	public void DoOnDisabledNavUpdate()
	{
		OnDisable();
	}
}
