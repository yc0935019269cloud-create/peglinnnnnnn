using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIInputField : UIPanel, ITextFieldUI
	{
		[Tooltip("(Optional) Text field panel.")]
		public Graphic panel;

		[Tooltip("(Optional) Text element for prompt.")]
		public UITextField label;

		[Tooltip("Input field.")]
		public UIInputField inputField;

		[Tooltip("(Optional) Key code that accepts input.")]
		public KeyCode acceptKey = KeyCode.Return;

		[Tooltip("(Optional) key code that cancels input.")]
		public KeyCode cancelKey = KeyCode.Escape;

		[Tooltip("Automatically open touchscreen keyboard.")]
		public bool showTouchScreenKeyboard;

		public UnityEvent onAccept = new UnityEvent();

		public UnityEvent onCancel = new UnityEvent();

		protected AcceptedTextDelegate m_acceptedText;

		protected bool m_isAwaitingInput;

		protected TouchScreenKeyboard m_touchScreenKeyboard;

		protected override void Start()
		{
			if (DialogueDebug.logWarnings && inputField == null)
			{
				Debug.LogWarning("Dialogue System: No InputField is assigned to the text field UI " + base.name + ". TextInput() sequencer commands or [var?=] won't work.");
			}
			SetActive(value: false);
		}

		public virtual void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
		{
			label.text = labelText;
			inputField.text = text;
			inputField.characterLimit = maxLength;
			m_acceptedText = acceptedText;
			m_isAwaitingInput = true;
			Show();
		}

		protected override void Update()
		{
			if (m_isAwaitingInput && !DialogueManager.IsDialogueSystemInputDisabled())
			{
				if (InputDeviceManager.IsKeyDown(acceptKey))
				{
					AcceptTextInput();
				}
				else if (InputDeviceManager.IsKeyDown(cancelKey))
				{
					CancelTextInput();
				}
			}
		}

		public virtual void CancelTextInput()
		{
			m_isAwaitingInput = false;
			Hide();
			onCancel.Invoke();
		}

		public virtual void AcceptTextInput()
		{
			m_isAwaitingInput = false;
			if (m_acceptedText != null)
			{
				if (inputField != null)
				{
					m_acceptedText(inputField.text);
				}
				m_acceptedText = null;
			}
			Hide();
			onAccept.Invoke();
		}

		protected virtual void Show()
		{
			SetActive(value: true);
			Open();
			if (showTouchScreenKeyboard)
			{
				ShowTouchScreenKeyboard();
			}
			inputField.ActivateInputField();
			EventSystem.current.SetSelectedGameObject(inputField.gameObject);
		}

		protected virtual void ShowTouchScreenKeyboard()
		{
			m_touchScreenKeyboard = TouchScreenKeyboard.Open(inputField.text);
		}

		protected virtual void Hide()
		{
			Close();
			SetActive(value: false);
			if (m_touchScreenKeyboard != null)
			{
				m_touchScreenKeyboard.active = false;
				m_touchScreenKeyboard = null;
			}
		}

		protected virtual void SetActive(bool value)
		{
			if (panel != null)
			{
				panel.gameObject.SetActive(value);
			}
			if (panel == null || value)
			{
				label.SetActive(value);
				inputField.SetActive(value);
			}
		}
	}
}
