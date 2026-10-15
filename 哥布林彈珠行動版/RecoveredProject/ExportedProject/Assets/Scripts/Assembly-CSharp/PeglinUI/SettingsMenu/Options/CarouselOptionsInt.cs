namespace PeglinUI.SettingsMenu.Options
{
	public class CarouselOptionsInt : CarouselOptions
	{
		public delegate void IntAction(int value);

		public IntAction changeSettingAction;

		public override void ApplyOption()
		{
			changeSettingAction(_displayedOptionIndex);
		}

		public void ShowOption(int value)
		{
			_displayedOptionIndex = value;
			ShowOption();
		}
	}
}
