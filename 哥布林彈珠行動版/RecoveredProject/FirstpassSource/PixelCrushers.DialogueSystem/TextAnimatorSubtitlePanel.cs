using System.Collections;
using Febucci.UI;

namespace PixelCrushers.DialogueSystem
{
	public class TextAnimatorSubtitlePanel : StandardUISubtitlePanel
	{
		public bool clearTextOnOpen;

		public override void Open()
		{
			if (!base.isOpen)
			{
				ClearText();
			}
			base.Open();
		}

		public override void SetContent(Subtitle subtitle)
		{
			if (accumulateText)
			{
				int characterCount = subtitleText.textMeshProUGUI.textInfo.characterCount;
				StartCoroutine(SkipTypewriterAhead(characterCount));
			}
			base.SetContent(subtitle);
		}

		protected IEnumerator SkipTypewriterAhead(int numChars)
		{
			TextAnimator textAnimator = subtitleText.gameObject.GetComponent<TextAnimator>();
			if (textAnimator != null)
			{
				yield return null;
				for (int i = 0; i < numChars; i++)
				{
					textAnimator.IncreaseVisibleChars();
				}
			}
		}
	}
}
