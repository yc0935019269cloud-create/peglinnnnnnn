using Febucci.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	public class TextAnimatorContinueButtonFastForward : MonoBehaviour
	{
		[Tooltip("Dialogue UI that the continue button affects.")]
		public StandardDialogueUI dialogueUI;

		[Tooltip("Text Animator Player to fast forward if it's not done playing.")]
		public TextAnimatorPlayer textAnimatorPlayer;

		[Tooltip("Hide the continue button when continuing.")]
		public bool hideContinueButtonOnContinue;

		private Button continueButton;

		private AbstractDialogueUI m_runtimeDialogueUI;

		private AbstractDialogueUI runtimeDialogueUI
		{
			get
			{
				if (m_runtimeDialogueUI == null)
				{
					m_runtimeDialogueUI = dialogueUI;
					if (m_runtimeDialogueUI == null)
					{
						m_runtimeDialogueUI = GetComponentInParent<AbstractDialogueUI>();
						if (m_runtimeDialogueUI == null)
						{
							m_runtimeDialogueUI = DialogueManager.dialogueUI as AbstractDialogueUI;
						}
					}
				}
				return m_runtimeDialogueUI;
			}
		}

		public virtual void Awake()
		{
			continueButton = GetComponent<Button>();
		}

		public virtual void OnFastForward()
		{
			if (textAnimatorPlayer != null && !textAnimatorPlayer.textAnimator.allLettersShown)
			{
				textAnimatorPlayer.SkipTypewriter();
				return;
			}
			if (hideContinueButtonOnContinue && continueButton != null)
			{
				continueButton.gameObject.SetActive(value: false);
			}
			if (runtimeDialogueUI != null)
			{
				runtimeDialogueUI.OnContinue();
			}
		}

		public void ContinueIfDialogueHasResponses()
		{
			if (!(dialogueUI == null) && (textAnimatorPlayer == null || textAnimatorPlayer.textAnimator.allLettersShown) && DialogueManager.currentConversationState.hasPCResponses)
			{
				dialogueUI.OnContinue();
			}
		}
	}
}
