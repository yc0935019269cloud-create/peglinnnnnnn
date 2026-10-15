using System;
using UnityEngine;
using UnityEngine.Events;

namespace PeglinUI.SettingsMenu.Options
{
	[CreateAssetMenu(fileName = "BooleanOption", menuName = "Option/Boolean Option")]
	public class BooleanOption : Option
	{
		[Serializable]
		public class BooleanAction : UnityEvent<bool>
		{
		}

		public BooleanAction changeSettingAction = new BooleanAction();

		public override void ApplyOption(int index)
		{
			bool arg = index != 0;
			changeSettingAction.Invoke(arg);
		}
	}
}
