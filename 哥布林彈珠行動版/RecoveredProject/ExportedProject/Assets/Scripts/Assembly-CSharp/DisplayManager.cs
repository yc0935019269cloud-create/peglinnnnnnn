using UnityEngine;

public class DisplayManager : MonoBehaviour
{
	public delegate void DisplayChanged();

	public static DisplayChanged OnDisplayChanged = () =>
	{
	};

	private Resolution _currentResolution;

	private void OnEnable()
	{
		_currentResolution = Screen.currentResolution;
		OnDisplayChanged();
	}

	private void Update()
	{
		if (AreResolutionsDifferent(_currentResolution, Screen.currentResolution))
		{
			_currentResolution = Screen.currentResolution;
			OnDisplayChanged();
		}
	}

	public static bool AreResolutionsDifferent(Resolution r1, Resolution r2)
	{
		if (r1.width == r2.width && r1.height == r2.height)
		{
			return r1.refreshRate != r2.refreshRate;
		}
		return true;
	}
}
