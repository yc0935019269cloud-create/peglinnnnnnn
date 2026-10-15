using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class ConversationControl : MonoBehaviour
	{
		[Tooltip("Skip all subtitles until response menu or end of conversation is reached. Set by SkipAll().")]
		public bool skipAll;

		[Tooltip("Stop SkipAll() when response menu is reached.")]
		public bool stopSkipAllOnResponseMenu = true;

		[Tooltip("Stop SkipAll() when end of conversation is reached.")]
		public bool stopSkipAllOnConversationEnd;

		private AbstractDialogueUI dialogueUI;

		private void Awake()
		{
			dialogueUI = GetComponent<AbstractDialogueUI>() ?? DialogueManager.standardDialogueUI ?? Object.FindObjectOfType<AbstractDialogueUI>();
		}

		public void ToggleAutoPlay()
		{
			DisplaySettings.SubtitleSettings.ContinueButtonMode continueButtonMode = ((DialogueManager.displaySettings.subtitleSettings.continueButton == DisplaySettings.SubtitleSettings.ContinueButtonMode.Never) ? DisplaySettings.SubtitleSettings.ContinueButtonMode.Always : DisplaySettings.SubtitleSettings.ContinueButtonMode.Never);
			DialogueManager.displaySettings.subtitleSettings.continueButton = continueButtonMode;
			if (continueButtonMode == DisplaySettings.SubtitleSettings.ContinueButtonMode.Never)
			{
				dialogueUI.OnContinueConversation();
			}
		}

		public void SkipAll()
		{
			skipAll = true;
			if (dialogueUI != null)
			{
				dialogueUI.OnContinueConversation();
			}
		}

		public void StopSkipAll()
		{
			skipAll = false;
		}

		public void OnConversationLine(Subtitle subtitle)
		{
			if (skipAll)
			{
				subtitle.sequence = "Continue(); " + subtitle.sequence;
			}
		}

		public void OnConversationResponseMenu(Response[] responses)
		{
			if (skipAll)
			{
				if (stopSkipAllOnResponseMenu)
				{
					skipAll = false;
				}
				if (dialogueUI != null)
				{
					dialogueUI.ShowSubtitle(DialogueManager.currentConversationState.subtitle);
				}
			}
		}

		public void OnConversationEnd(Transform actor)
		{
			if (stopSkipAllOnConversationEnd)
			{
				skipAll = false;
			}
		}
	}
}
