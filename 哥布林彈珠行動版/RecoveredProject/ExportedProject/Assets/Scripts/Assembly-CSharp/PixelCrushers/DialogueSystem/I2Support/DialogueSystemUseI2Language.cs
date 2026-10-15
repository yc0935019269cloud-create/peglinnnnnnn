using I2.Loc;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.I2Support
{
	[AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/I2 Localization Support/Dialogue System Use I2 Language")]
	public class DialogueSystemUseI2Language : MonoBehaviour
	{
		public enum AssetIdentifierType
		{
			ID = 0,
			Name = 1
		}

		public enum DialogueEntryInfo
		{
			None = 0,
			Actor = 1,
			Text = 2
		}

		public AssetIdentifierType assetsUse;

		[Tooltip("Specifies whether fields in the Dialogue System are named by language code or language name.")]
		public I2LanguageIdentifierType specifyLanguageBy = I2LanguageIdentifierType.LanguageCode;

		[Tooltip("Extra info included at end of I2 term.")]
		public DialogueEntryInfo extraEntryInfo;

		public int dialogueEntryMinDigits = 1;

		[Tooltip("On start, set the Dialogue System's current language to match I2 Localization's current language.")]
		public bool useI2LanguageOnStart = true;

		[Tooltip("")]
		public bool useI2LanguageAtRuntime;

		private void Start()
		{
			UseCurrentI2Language();
		}

		public void UseCurrentI2Language()
		{
			DialogueManager.SetLanguage((specifyLanguageBy == I2LanguageIdentifierType.LanguageCode) ? LocalizationManager.CurrentLanguageCode : LocalizationManager.CurrentLanguage);
		}

		protected void OnConversationLine(Subtitle subtitle)
		{
			if (!useI2LanguageAtRuntime || subtitle == null)
			{
				return;
			}
			DialogueEntry dialogueEntry = subtitle.dialogueEntry;
			string dialogueEntryHeader = GetDialogueEntryHeader(dialogueEntry);
			if (!string.IsNullOrEmpty(dialogueEntry.DialogueText))
			{
				dialogueEntryHeader += "/Dialogue Text";
			}
			else
			{
				if (string.IsNullOrEmpty(dialogueEntry.MenuText))
				{
					return;
				}
				dialogueEntryHeader += "/Menu Text";
			}
			dialogueEntryHeader = I2Utils.RemoveNonASCII(dialogueEntryHeader, allowCategory: true);
			string termTranslation = LocalizationManager.GetTermTranslation(dialogueEntryHeader, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, LocalizationManager.CurrentLanguage);
			if (!string.IsNullOrEmpty(termTranslation) && !string.Equals(termTranslation, "Null"))
			{
				subtitle.formattedText = FormattedText.Parse(termTranslation);
			}
		}

		protected void OnConversationResponseMenu(Response[] responses)
		{
			if (!useI2LanguageAtRuntime || responses == null)
			{
				return;
			}
			foreach (Response response in responses)
			{
				DialogueEntry destinationEntry = response.destinationEntry;
				string dialogueEntryHeader = GetDialogueEntryHeader(destinationEntry);
				if (!string.IsNullOrEmpty(destinationEntry.MenuText))
				{
					dialogueEntryHeader += "/Menu Text";
				}
				else
				{
					if (string.IsNullOrEmpty(destinationEntry.DialogueText))
					{
						continue;
					}
					dialogueEntryHeader += "/Dialogue Text";
				}
				dialogueEntryHeader = I2Utils.RemoveNonASCII(dialogueEntryHeader, allowCategory: true);
				string termTranslation = LocalizationManager.GetTermTranslation(dialogueEntryHeader, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, LocalizationManager.CurrentLanguage);
				if (!string.IsNullOrEmpty(termTranslation) && !string.Equals(termTranslation, "Null"))
				{
					response.formattedText = FormattedText.Parse(termTranslation);
				}
			}
		}

		private string GetDialogueEntryHeader(DialogueEntry entry)
		{
			if (entry == null)
			{
				return string.Empty;
			}
			string text = "Dialogue System/Conversation/";
			text += ((assetsUse == AssetIdentifierType.ID) ? entry.conversationID.ToString("D" + dialogueEntryMinDigits) : SanitizedConversationTitle(entry.conversationID));
			text = text + "/Entry/" + entry.id.ToString("D" + dialogueEntryMinDigits);
			switch (extraEntryInfo)
			{
			case DialogueEntryInfo.Actor:
			{
				Actor actor = DialogueManager.masterDatabase.GetActor(entry.ActorID);
				text = text + "/" + ((actor != null) ? actor.Name : string.Empty);
				break;
			}
			case DialogueEntryInfo.Text:
				text = text + "/" + entry.DialogueText;
				break;
			}
			return text;
		}

		private string SanitizedConversationTitle(int conversationID)
		{
			Conversation conversation = DialogueManager.masterDatabase.GetConversation(conversationID);
			if (conversation == null)
			{
				return conversationID.ToString("D" + dialogueEntryMinDigits);
			}
			return SanitizeTerm(conversation.Title);
		}

		private string SanitizeTerm(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				return I2Utils.RemoveNonASCII(s.Replace("/", ".").Replace("\\", ".").Replace("[", ".")
					.Replace("]", "."));
			}
			return s;
		}
	}
}
