using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[Serializable]
	public class UIInputField
	{
		[SerializeField]
		private InputField m_uiInputField;

		[SerializeField]
		private TMP_InputField m_textMeshProInputField;

		public InputField uiInputField
		{
			get
			{
				return m_uiInputField;
			}
			set
			{
				m_uiInputField = value;
			}
		}

		public TMP_InputField textMeshProInputField
		{
			get
			{
				return m_textMeshProInputField;
			}
			set
			{
				m_textMeshProInputField = value;
			}
		}

		public string text
		{
			get
			{
				if (textMeshProInputField != null)
				{
					return textMeshProInputField.text;
				}
				if (uiInputField != null)
				{
					return uiInputField.text;
				}
				return string.Empty;
			}
			set
			{
				if (textMeshProInputField != null)
				{
					textMeshProInputField.text = value;
				}
				if (uiInputField != null)
				{
					uiInputField.text = value;
				}
			}
		}

		public int characterLimit
		{
			get
			{
				if (textMeshProInputField != null)
				{
					return textMeshProInputField.characterLimit;
				}
				if (uiInputField != null)
				{
					return uiInputField.characterLimit;
				}
				return 0;
			}
			set
			{
				if (textMeshProInputField != null)
				{
					textMeshProInputField.characterLimit = value;
				}
				if (uiInputField != null)
				{
					uiInputField.characterLimit = value;
				}
			}
		}

		public bool enabled
		{
			get
			{
				if (textMeshProInputField != null)
				{
					return textMeshProInputField.enabled;
				}
				if (uiInputField != null)
				{
					return uiInputField.enabled;
				}
				return false;
			}
			set
			{
				if (textMeshProInputField != null)
				{
					textMeshProInputField.enabled = value;
				}
				if (uiInputField != null)
				{
					uiInputField.enabled = value;
				}
			}
		}

		public GameObject gameObject
		{
			get
			{
				if (textMeshProInputField != null)
				{
					return textMeshProInputField.gameObject;
				}
				if (!(uiInputField != null))
				{
					return null;
				}
				return uiInputField.gameObject;
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

		public UIInputField()
		{
			uiInputField = null;
			textMeshProInputField = null;
		}

		public UIInputField(InputField uiInputField)
		{
			this.uiInputField = uiInputField;
			textMeshProInputField = null;
		}

		public UIInputField(TMP_InputField textMeshProInputField)
		{
			uiInputField = null;
			this.textMeshProInputField = textMeshProInputField;
		}

		public void SetActive(bool value)
		{
			if (uiInputField != null)
			{
				uiInputField.gameObject.SetActive(value);
			}
			if (textMeshProInputField != null)
			{
				textMeshProInputField.gameObject.SetActive(value);
			}
		}

		public void ActivateInputField()
		{
			if (uiInputField != null)
			{
				uiInputField.ActivateInputField();
			}
			if (textMeshProInputField != null)
			{
				textMeshProInputField.ActivateInputField();
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
