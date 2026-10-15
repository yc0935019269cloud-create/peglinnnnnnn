using UnityEngine;

namespace PeglinUI.SettingsMenu.Options
{
	[CreateAssetMenu(fileName = "ControlModeOption", menuName = "Options/Control Mode Option")]
	public class ControlModeOption : Option
	{
		public override void ApplyOption(int index)
		{
			bool clickToAim = index != 0;
			SettingsManager.Instance.ClickToAim = clickToAim;
		}
	}
}
