using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.I2Support
{
	[AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/I2 Localization Support/Dialogue System Use I2 RTL Fix")]
	public class DialogueSystemUseI2RTLFix : MonoBehaviour
	{
		[Tooltip("Extra language codes that I2 doesn't recognize as RTL.")]
		public List<string> additionalRTLLanguageCodes = new List<string>(new string[1] { "fa" });

		[Tooltip("Use right side alignment for RTL text.")]
		public bool alignRTLToRight = true;

		[Tooltip("Insert new line after this many characters.")]
		public int textLimit = 999;

		private bool IsRTL()
		{
			if (!LocalizationManager.IsRight2Left)
			{
				return additionalRTLLanguageCodes.Contains(LocalizationManager.CurrentLanguageCode);
			}
			return true;
		}

		private void OnConversationStart(Transform actor)
		{
			StandardDialogueUI standardDialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
			if (!(standardDialogueUI != null))
			{
				return;
			}
			StandardUISubtitlePanel[] subtitlePanels = standardDialogueUI.conversationUIElements.subtitlePanels;
			foreach (StandardUISubtitlePanel standardUISubtitlePanel in subtitlePanels)
			{
				if (standardUISubtitlePanel != null)
				{
					if (alignRTLToRight)
					{
						SetTextFieldAlignment(standardUISubtitlePanel.subtitleText);
					}
					AbstractTypewriterEffect typewriter = standardUISubtitlePanel.GetTypewriter();
					if (typewriter != null)
					{
						typewriter.rightToLeft = IsRTL();
					}
				}
			}
		}

		private void SetTextFieldAlignment(UITextField textField)
		{
			if (textField.uiText != null)
			{
				switch (textField.uiText.alignment)
				{
				case TextAnchor.LowerLeft:
				case TextAnchor.LowerRight:
					textField.uiText.alignment = (IsRTL() ? TextAnchor.UpperRight : TextAnchor.UpperLeft);
					break;
				case TextAnchor.MiddleLeft:
				case TextAnchor.MiddleRight:
					textField.uiText.alignment = (IsRTL() ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
					break;
				case TextAnchor.UpperLeft:
				case TextAnchor.UpperRight:
					textField.uiText.alignment = (IsRTL() ? TextAnchor.UpperRight : TextAnchor.UpperLeft);
					break;
				}
			}
			if (textField.textMeshProUGUI != null)
			{
				switch (textField.textMeshProUGUI.alignment)
				{
				case TextAlignmentOptions.BaselineLeft:
				case TextAlignmentOptions.BaselineRight:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.BaselineRight : TextAlignmentOptions.BaselineLeft);
					break;
				case TextAlignmentOptions.BottomLeft:
				case TextAlignmentOptions.BottomRight:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.BottomRight : TextAlignmentOptions.BottomLeft);
					break;
				case TextAlignmentOptions.CaplineLeft:
				case TextAlignmentOptions.CaplineRight:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.CaplineRight : TextAlignmentOptions.CaplineLeft);
					break;
				case TextAlignmentOptions.MidlineLeft:
				case TextAlignmentOptions.Midline:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft);
					break;
				case TextAlignmentOptions.Left:
				case TextAlignmentOptions.Right:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
					break;
				case TextAlignmentOptions.TopLeft:
				case TextAlignmentOptions.TopRight:
					textField.textMeshProUGUI.alignment = (IsRTL() ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft);
					break;
				}
			}
		}

		private void OnConversationLine(Subtitle subtitle)
		{
			if (IsRTL())
			{
				subtitle.formattedText.text = LocalizationManager.ApplyRTLfix(subtitle.formattedText.text, textLimit, ignoreNumbers: false);
			}
		}

		private void OnConversationResponseMenu(Response[] responses)
		{
			if (IsRTL())
			{
				for (int i = 0; i < responses.Length; i++)
				{
					responses[i].formattedText.text = LocalizationManager.ApplyRTLfix(responses[i].formattedText.text, textLimit, ignoreNumbers: false);
				}
			}
		}
	}
}
