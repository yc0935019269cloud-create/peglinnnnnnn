using UnityEngine;

namespace PeglinUI.SettingsMenu
{
	[CreateAssetMenu(fileName = "ScreenModeOptions", menuName = "Options/Screen Mode")]
	public class ScreenModeOption : Option
	{
		public FullScreenMode[] screenModes;

		private void OnValidate()
		{
			if (screenModes.Length != locStrings.Length)
			{
				Debug.LogError("Uneven length in locstrings and options in ScreenModeOptions");
			}
		}

		public override void ApplyOption(int i)
		{
			SettingsManager.Instance.ScreenMode = screenModes[i];
		}
	}
}
