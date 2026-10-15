using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class UITextColor : MonoBehaviour
	{
		public Color color;

		public Text text;

		private Color m_originalColor;

		private void Awake()
		{
			if (text == null)
			{
				text = GetComponentInChildren<Text>();
			}
			if (text != null)
			{
				m_originalColor = text.color;
			}
		}

		public void ApplyColor()
		{
			if (!(text == null))
			{
				m_originalColor = text.color;
				text.color = color;
			}
		}

		public void UndoColor()
		{
			if (!(text == null))
			{
				text.color = m_originalColor;
			}
		}
	}
}
