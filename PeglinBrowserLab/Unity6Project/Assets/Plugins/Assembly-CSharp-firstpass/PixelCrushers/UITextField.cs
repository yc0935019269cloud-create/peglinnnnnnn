using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[Serializable]
	public class UITextField
	{
		[SerializeField]
		private Text m_uiText;

		[SerializeField]
		private TextMeshProUGUI m_textMeshProUGUI;

		public Text uiText
		{
			get
			{
				return m_uiText;
			}
			set
			{
				m_uiText = value;
			}
		}

		public TextMeshProUGUI textMeshProUGUI
		{
			get
			{
				return m_textMeshProUGUI;
			}
			set
			{
				m_textMeshProUGUI = value;
			}
		}

		public string text
		{
			get
			{
				if (textMeshProUGUI != null)
				{
					return textMeshProUGUI.text;
				}
				if (uiText != null)
				{
					return uiText.text;
				}
				return string.Empty;
			}
			set
			{
				if (textMeshProUGUI != null)
				{
					textMeshProUGUI.text = value;
				}
				if (uiText != null)
				{
					uiText.text = value;
				}
			}
		}

		public bool enabled
		{
			get
			{
				if (textMeshProUGUI != null)
				{
					return textMeshProUGUI.enabled;
				}
				if (uiText != null)
				{
					return uiText.enabled;
				}
				return false;
			}
			set
			{
				if (textMeshProUGUI != null)
				{
					textMeshProUGUI.enabled = value;
				}
				if (uiText != null)
				{
					uiText.enabled = value;
				}
			}
		}

		public Color color
		{
			get
			{
				if (textMeshProUGUI != null)
				{
					return textMeshProUGUI.color;
				}
				if (uiText != null)
				{
					return uiText.color;
				}
				return Color.black;
			}
			set
			{
				if (textMeshProUGUI != null)
				{
					textMeshProUGUI.color = value;
				}
				if (uiText != null)
				{
					uiText.color = value;
				}
			}
		}

		public GameObject gameObject
		{
			get
			{
				if (textMeshProUGUI != null)
				{
					return textMeshProUGUI.gameObject;
				}
				if (!(uiText != null))
				{
					return null;
				}
				return uiText.gameObject;
			}
		}

		public bool isActiveSelf
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
		}

		public bool activeInHierarchy
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeInHierarchy;
			}
		}

		public UITextField()
		{
			uiText = null;
			textMeshProUGUI = null;
		}

		public UITextField(Text uiText)
		{
			this.uiText = uiText;
			textMeshProUGUI = null;
		}

		public UITextField(TextMeshProUGUI textMeshProUGUI)
		{
			uiText = null;
			this.textMeshProUGUI = textMeshProUGUI;
		}

		public void SetActive(bool value)
		{
			if (uiText != null)
			{
				uiText.gameObject.SetActive(value);
			}
			if (textMeshProUGUI != null)
			{
				textMeshProUGUI.gameObject.SetActive(value);
			}
		}

		public static bool IsNull(UITextField uiTextField)
		{
			if (uiTextField == null)
			{
				return true;
			}
			if (uiTextField.uiText != null)
			{
				return false;
			}
			if (uiTextField.textMeshProUGUI != null)
			{
				return false;
			}
			return true;
		}
	}
}
