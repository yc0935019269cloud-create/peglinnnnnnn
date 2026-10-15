using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[Serializable]
	public class UnityUIQuestTemplateAlternateDescriptions
	{
		[Tooltip("(Optional) If set, use if state is success")]
		public Text successDescription;

		[Tooltip("(Optional) If set, use if state is failure")]
		public Text failureDescription;

		public void SetActive(bool value)
		{
			if (successDescription != null)
			{
				successDescription.gameObject.SetActive(value);
			}
			if (failureDescription != null)
			{
				failureDescription.gameObject.SetActive(value);
			}
		}
	}
}
