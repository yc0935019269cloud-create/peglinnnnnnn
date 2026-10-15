using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIButtonTemplate : StandardUIContentTemplate
	{
		[Tooltip("Button UI element.")]
		public Button button;

		public UITextField label;

		private void Awake()
		{
			if (button == null && DialogueDebug.logWarnings)
			{
				Debug.LogError("Dialogue System: UI Button is unassigned.", this);
			}
		}

		public void Assign(string labelText)
		{
			label.text = labelText;
		}
	}
}
