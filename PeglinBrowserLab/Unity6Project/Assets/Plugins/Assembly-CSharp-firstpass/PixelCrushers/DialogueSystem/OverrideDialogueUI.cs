using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class OverrideDialogueUI : OverrideUIBase
	{
		[Tooltip("Use this dialogue UI when this GameObject is involved in conversation.")]
		public GameObject ui;
	}
}
