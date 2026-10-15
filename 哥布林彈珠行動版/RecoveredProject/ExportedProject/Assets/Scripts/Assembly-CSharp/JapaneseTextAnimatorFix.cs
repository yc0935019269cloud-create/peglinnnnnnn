using System.Text.RegularExpressions;
using Febucci.UI;
using I2.Loc;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;

public class JapaneseTextAnimatorFix : MonoBehaviour
{
	private const string tagPattern = "<(.|\\n)*?>";

	public DialogueSystemEvents dialogueSystemEvents;

	private TextAnimator _textAnimator;

	private TMP_Text _textField;

	private void Start()
	{
		if (LocalizationManager.CurrentLanguageCode != "ja")
		{
			base.enabled = false;
			return;
		}
		_textAnimator = GetComponent<TextAnimator>();
		_textField = GetComponent<TMP_Text>();
		GetComponent<TextAnimatorPlayer>().enabled = false;
		_textAnimator.enabled = false;
		_textField.renderMode = TextRenderFlags.Render;
		RemoveTags(_textField.text);
	}

	private void OnEnable()
	{
		dialogueSystemEvents.conversationEvents.onConversationLine.AddListener(CheckTagRemoval);
	}

	private void OnDisable()
	{
		dialogueSystemEvents.conversationEvents.onConversationLine.RemoveListener(CheckTagRemoval);
	}

	private void CheckTagRemoval(Subtitle arg0)
	{
		RemoveTags(arg0.formattedText.text);
	}

	private void RemoveTags(string newString)
	{
		_textField.text = Regex.Replace(newString, "<(.|\\n)*?>", string.Empty);
	}
}
