using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tutorial
{
	public class TutorialElement : MonoBehaviour
	{
		[FormerlySerializedAs("Name")]
		public string prefSaveKey;

		[FormerlySerializedAs("TutorialText")]
		[SerializeField]
		private string[] _tutorialTextLocKey;

		[Tooltip("Readonly, this is populated from the loc text onValidate")]
		[TextArea]
		public string textFromLoc;

		public string[] TutorialText
		{
			get
			{
				List<string> list = new List<string>();
				string[] tutorialTextLocKey = _tutorialTextLocKey;
				foreach (string text in tutorialTextLocKey)
				{
					list.Add(LocalizationManager.GetTranslation("Tutorial/" + text));
				}
				return list.ToArray();
			}
		}
	}
}
