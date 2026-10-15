using Peglin.OdinSerializer;
using UnityEngine;

namespace PeglinUI.SettingsMenu
{
	[CreateAssetMenu(fileName = "GenericOption", menuName = "Options/Generic Option")]
	public class Option : SerializedScriptableObject
	{
		public string[] locStrings;

		public bool isTranslatable;

		public virtual void ApplyOption(int index)
		{
		}
	}
}
