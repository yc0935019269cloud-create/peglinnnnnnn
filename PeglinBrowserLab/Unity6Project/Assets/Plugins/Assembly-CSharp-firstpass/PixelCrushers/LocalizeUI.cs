using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class LocalizeUI : MonoBehaviour
	{
		[Tooltip("Overrides the global text table.")]
		[SerializeField]
		private TextTable m_textTable;

		[Tooltip("(Optional) If assigned, use this instead of the UI element's text's value as the field lookup value.")]
		[SerializeField]
		private string m_fieldName = string.Empty;

		private bool m_started;

		private List<string> m_fieldNames = new List<string>();

		private Text m_text;

		private Dropdown m_dropdown;

		private TextMeshPro m_textMeshPro;

		private TextMeshProUGUI m_textMeshProUGUI;

		private bool m_lookedForTMP;

		public TextTable textTable
		{
			get
			{
				if (!(m_textTable != null))
				{
					if (!(UILocalizationManager.instance.textTable != null))
					{
						return GlobalTextTable.textTable;
					}
					return UILocalizationManager.instance.textTable;
				}
				return m_textTable;
			}
			set
			{
				m_textTable = value;
			}
		}

		public string fieldName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_fieldName))
				{
					return m_fieldName;
				}
				return null;
			}
			set
			{
				m_fieldName = value;
			}
		}

		protected bool started
		{
			get
			{
				return m_started;
			}
			private set
			{
				m_started = value;
			}
		}

		protected List<string> fieldNames
		{
			get
			{
				return m_fieldNames;
			}
			set
			{
				m_fieldNames = value;
			}
		}

		public Text text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		public Dropdown dropdown
		{
			get
			{
				return m_dropdown;
			}
			set
			{
				m_dropdown = value;
			}
		}

		public TextMeshPro textMeshPro
		{
			get
			{
				return m_textMeshPro;
			}
			set
			{
				m_textMeshPro = value;
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

		protected virtual void Start()
		{
			started = true;
			UpdateText();
		}

		protected virtual void OnEnable()
		{
			UpdateText();
		}

		public virtual void UpdateText()
		{
			if (!started)
			{
				return;
			}
			TextTable textTable = this.textTable;
			string text = ((UILocalizationManager.instance != null) ? UILocalizationManager.instance.currentLanguage : string.Empty);
			if (textTable == null)
			{
				Debug.LogWarning("No localized text table is assigned to " + base.name + " or a UI Localized Manager component.", this);
				return;
			}
			if (!textTable.HasLanguage(text))
			{
				Debug.LogWarning("Text table " + textTable.name + " does not have a language '" + text + "'.", textTable);
			}
			if (this.text == null && dropdown == null)
			{
				this.text = GetComponent<Text>();
				dropdown = GetComponent<Dropdown>();
			}
			bool num = this.text != null || dropdown != null;
			if (!m_lookedForTMP)
			{
				m_lookedForTMP = true;
				textMeshPro = GetComponent<TextMeshPro>();
				textMeshProUGUI = GetComponent<TextMeshProUGUI>();
			}
			if (!num && !(textMeshProUGUI != null))
			{
				Debug.LogWarning("Localize UI didn't find a localizable UI component on " + base.name + ".", this);
				return;
			}
			if (string.IsNullOrEmpty(fieldName))
			{
				fieldName = ((this.text != null) ? this.text.text : string.Empty);
			}
			if (fieldNames.Count == 0 && dropdown != null)
			{
				dropdown.options.ForEach((Dropdown.OptionData opt) =>
				{
					fieldNames.Add(opt.text);
				});
			}
			if (this.text != null)
			{
				if (!textTable.HasField(fieldName))
				{
					Debug.LogWarning("Text table " + textTable.name + " does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					this.text.text = GetLocalizedText(fieldName);
				}
			}
			if (dropdown != null)
			{
				for (int num2 = 0; num2 < dropdown.options.Count; num2++)
				{
					if (num2 < fieldNames.Count)
					{
						dropdown.options[num2].text = GetLocalizedText(fieldNames[num2]);
					}
				}
				dropdown.captionText.text = GetLocalizedText(fieldNames[dropdown.value]);
			}
			if (!m_lookedForTMP)
			{
				m_lookedForTMP = true;
				textMeshPro = GetComponent<TextMeshPro>();
				textMeshProUGUI = GetComponent<TextMeshProUGUI>();
			}
			if (textMeshPro != null)
			{
				if (string.IsNullOrEmpty(fieldName))
				{
					fieldName = ((textMeshPro != null) ? textMeshPro.text : string.Empty);
				}
				if (!textTable.HasField(fieldName))
				{
					Debug.LogWarning("Text table " + textTable.name + " does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					textMeshPro.text = GetLocalizedText(fieldName);
				}
			}
			if (textMeshProUGUI != null)
			{
				if (string.IsNullOrEmpty(fieldName))
				{
					fieldName = ((textMeshProUGUI != null) ? textMeshProUGUI.text : string.Empty);
				}
				if (!textTable.HasField(fieldName))
				{
					Debug.LogWarning("Text table " + textTable.name + " does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					textMeshProUGUI.text = GetLocalizedText(fieldName);
				}
			}
		}

		protected virtual string GetLocalizedText(string fieldName)
		{
			if (!(textTable != null) || !textTable.HasField(fieldName))
			{
				return GlobalTextTable.Lookup(fieldName);
			}
			return textTable.GetFieldTextForLanguage(fieldName, GlobalTextTable.currentLanguage);
		}

		public virtual void SetFieldName(string newFieldName = "")
		{
			if (text == null)
			{
				text = GetComponent<Text>();
			}
			fieldName = ((string.IsNullOrEmpty(newFieldName) && text != null) ? text.text : newFieldName);
		}

		public virtual void UpdateDropdownOptions()
		{
			fieldNames.Clear();
			UpdateText();
		}
	}
}
