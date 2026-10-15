using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIResponseButton : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		[HelpBox("If Button's OnClick() event is empty, this Standard UI Response Button component will automatically assign its OnClick method at runtime. If Button's OnClick() event has other elements, you *must* manually assign the StandardUIResponseButton.OnClick method to it.", HelpBoxMessageType.Info)]
		public Button button;

		[Tooltip("Text element to display response text.")]
		public UITextField label;

		[Tooltip("Apply emphasis tag colors to button text.")]
		public bool setLabelColor = true;

		[Tooltip("Set button's text to this color by default.")]
		public Color defaultColor = Color.white;

		public string text
		{
			get
			{
				return label.text;
			}
			set
			{
				label.text = UITools.StripRPGMakerCodes(value);
				UITools.SendTextChangeMessage(label);
			}
		}

		public bool isClickable
		{
			get
			{
				if (button != null)
				{
					return button.interactable;
				}
				return false;
			}
			set
			{
				if (button != null)
				{
					button.interactable = value;
				}
			}
		}

		public bool isVisible { get; set; }

		public Response response { get; set; }

		public Transform target { get; set; }

		public void Reset()
		{
			isClickable = false;
			isVisible = false;
			response = null;
			if (label != null)
			{
				label.text = string.Empty;
				SetColor(defaultColor);
			}
		}

		public virtual void Awake()
		{
			if (button == null)
			{
				button = GetComponent<Button>();
			}
			if (button == null)
			{
				Debug.LogWarning("Dialogue System: Response button '" + base.name + "' is missing a Unity UI Button component!", this);
			}
		}

		public virtual void Start()
		{
			if (button != null && button.onClick.GetPersistentEventCount() == 0)
			{
				button.onClick.AddListener(OnClick);
			}
		}

		public virtual void SetFormattedText(FormattedText formattedText)
		{
			if (formattedText != null)
			{
				text = UITools.GetUIFormattedText(formattedText);
				SetColor((formattedText.emphases.Length != 0) ? formattedText.emphases[0].color : defaultColor);
			}
		}

		public virtual void SetUnformattedText(string unformattedText)
		{
			text = unformattedText;
			SetColor(defaultColor);
		}

		protected virtual void SetColor(Color currentColor)
		{
			if (setLabelColor)
			{
				label.color = currentColor;
			}
		}

		public virtual void OnClick()
		{
			if (target != null)
			{
				SetCurrentResponse();
				target.SendMessage("OnClick", response, SendMessageOptions.RequireReceiver);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			SetCurrentResponse();
		}

		protected virtual void SetCurrentResponse()
		{
			if (DialogueManager.instance.conversationController != null)
			{
				DialogueManager.instance.conversationController.SetCurrentResponse(response);
			}
		}
	}
}
