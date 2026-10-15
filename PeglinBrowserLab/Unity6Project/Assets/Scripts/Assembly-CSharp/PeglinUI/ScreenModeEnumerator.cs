using UnityEngine;

namespace PeglinUI
{
	public class ScreenModeEnumerator : MonoBehaviour
	{
		public ResolutionModeEnumerator ResolutionModeEnumerator;

		private void OnEnable()
		{
		}

		public bool GetScreenModeStatus(int fsm)
		{
			return SettingsManager.Instance.ScreenMode == (FullScreenMode)fsm;
		}

		public void SetScreenModeStatus(int fsm)
		{
			SettingsManager.Instance.ScreenMode = (FullScreenMode)fsm;
			ResolutionModeEnumerator.SetAvailableResolutionOptions(SettingsManager.Instance.ScreenMode);
		}
	}
}
