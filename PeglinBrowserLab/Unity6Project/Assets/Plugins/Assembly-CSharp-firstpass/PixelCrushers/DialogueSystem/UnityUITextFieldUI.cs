using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class UnityUITextFieldUI : MonoBehaviour, ITextFieldUI
	{
		[Tooltip("Optional panel containing the UI elements")]
		public Graphic panel;

		[Tooltip("Optional text element for prompt")]
		public Text label;

		public InputField textField;

		[Tooltip("Optional key code that accepts the input")]
		public KeyCode acceptKey = KeyCode.Return;

		[Tooltip("Optional key code that cancels the input")]
		public KeyCode cancelKey = KeyCode.Escape;

		[Tooltip("Automatically open touchscreen keyboard")]
		public bool showTouchScreenKeyboard;

		public UnityEvent onAccept = new UnityEvent();

		public UnityEvent onCancel = new UnityEvent();

		private AcceptedTextDelegate acceptedText;

		private bool isAwaitingInput;

		private TouchScreenKeyboard touchScreenKeyboard;

		private void Awake()
		{
			Tools.DeprecationWarning(this);
		}

		private void Start()
		{
			if (DialogueDebug.logWarnings && textField == null)
			{
				Debug.LogWarning(string.Format("{0}: No InputField is assigned to the text field UI {1}. TextInput() sequencer commands or [var?=] won't work.", new object[2] { "Dialogue System", base.name }));
			}
			Hide();
		}

		public void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
		{
			if (label != null)
			{
				label.text = labelText;
			}
			if (textField != null)
			{
				textField.text = text;
				textField.characterLimit = maxLength;
			}
			this.acceptedText = acceptedText;
			Show();
			isAwaitingInput = true;
		}

		public void Update()
		{
			if (isAwaitingInput && !DialogueManager.IsDialogueSystemInputDisabled())
			{
				if (Input.GetKeyDown(acceptKey))
				{
					AcceptTextInput();
				}
				else if (Input.GetKeyDown(cancelKey))
				{
					CancelTextInput();
				}
			}
		}

		public void CancelTextInput()
		{
			isAwaitingInput = false;
			Hide();
			onCancel.Invoke();
		}

		public void AcceptTextInput()
		{
			isAwaitingInput = false;
			if (acceptedText != null)
			{
				if (textField != null)
				{
					acceptedText(textField.text);
				}
				acceptedText = null;
			}
			Hide();
			onAccept.Invoke();
		}

		private void Show()
		{
			SetActive(value: true);
			if (textField != null)
			{
				if (showTouchScreenKeyboard)
				{
					touchScreenKeyboard = TouchScreenKeyboard.Open(textField.text);
				}
				textField.ActivateInputField();
				if (EventSystem.current != null)
				{
					EventSystem.current.SetSelectedGameObject(textField.gameObject, null);
				}
			}
		}

		private void Hide()
		{
			SetActive(value: false);
			if (touchScreenKeyboard != null)
			{
				touchScreenKeyboard.active = false;
				touchScreenKeyboard = null;
			}
		}

		private void SetActive(bool value)
		{
			if (textField != null)
			{
				textField.enabled = value;
			}
			if (panel != null)
			{
				Tools.SetGameObjectActive(panel, value);
				return;
			}
			Tools.SetGameObjectActive(label, value);
			Tools.SetGameObjectActive(textField, value);
		}
	}
}
