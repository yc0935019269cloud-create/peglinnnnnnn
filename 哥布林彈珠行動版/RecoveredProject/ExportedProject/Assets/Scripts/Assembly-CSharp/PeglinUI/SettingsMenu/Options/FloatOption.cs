using System;
using UnityEngine;
using UnityEngine.Events;

namespace PeglinUI.SettingsMenu.Options
{
	[CreateAssetMenu(fileName = "FloatOption", menuName = "Options/Float Option")]
	public class FloatOption : Option
	{
		[Serializable]
		public class FloatAction : UnityEvent<float>
		{
		}

		public float[] floatValues;

		public FloatAction changeSettingAction = new FloatAction();

		private void OnValidate()
		{
			if (floatValues.Length != locStrings.Length)
			{
				Debug.LogError("Uneven length in locstrings and options in FloatOption " + base.name);
			}
		}

		public override void ApplyOption(int index)
		{
			float arg = floatValues[index];
			changeSettingAction.Invoke(arg);
		}
	}
}
