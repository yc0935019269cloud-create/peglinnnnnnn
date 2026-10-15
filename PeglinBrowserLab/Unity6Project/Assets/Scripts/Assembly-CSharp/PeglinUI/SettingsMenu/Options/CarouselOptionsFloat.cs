using UnityEngine;

namespace PeglinUI.SettingsMenu.Options
{
	public class CarouselOptionsFloat : CarouselOptions
	{
		public delegate void FloatAction(float value);

		public float[] FloatValues;

		public FloatAction changeSettingAction;

		private void OnValidate()
		{
			if (FloatValues.Length != textItems.Length)
			{
				Debug.LogError("Uneven length in locstrings and options in CarouselFloatOption " + base.gameObject.name);
			}
		}

		public void ShowOption(float value)
		{
			for (int i = 0; i < FloatValues.Length; i++)
			{
				if (FloatValues[i] == value)
				{
					_displayedOptionIndex = i;
					ShowOption();
					return;
				}
			}
			_displayedOptionIndex = 0;
			ShowOption();
		}

		public override void ApplyOption()
		{
			float value = FloatValues[_displayedOptionIndex];
			changeSettingAction(value);
		}
	}
}
