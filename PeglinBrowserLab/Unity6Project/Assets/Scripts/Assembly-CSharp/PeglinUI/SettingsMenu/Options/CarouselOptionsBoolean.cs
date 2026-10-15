namespace PeglinUI.SettingsMenu.Options
{
	public class CarouselOptionsBoolean : CarouselOptions
	{
		public delegate void BooleanAction(bool value);

		public BooleanAction changeSettingAction;

		public override void ApplyOption()
		{
			bool value = _displayedOptionIndex == 1;
			changeSettingAction(value);
		}

		public void ShowOption(bool value)
		{
			_displayedOptionIndex = (value ? 1 : 0);
			ShowOption();
		}
	}
}
