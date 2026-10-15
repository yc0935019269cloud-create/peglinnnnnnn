using System.Collections;
using UnityEngine;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class UILocalizationManager : MonoBehaviour
	{
		[Tooltip("The PlayerPrefs key to store the player's selected language code.")]
		[SerializeField]
		private string m_currentLanguagePlayerPrefsKey = "Language";

		[Tooltip("Overrides the global text table.")]
		[SerializeField]
		private TextTable m_textTable;

		[Tooltip("When starting, set current language to value saved in PlayerPrefs.")]
		[SerializeField]
		private bool m_saveLanguageInPlayerPrefs = true;

		[Tooltip("When updating UIs, perform longer search that also finds LocalizeUI components on inactive GameObjects.")]
		[SerializeField]
		private bool m_alsoUpdateInactiveLocalizeUI = true;

		[Tooltip("If a language's field value is blank, use default language's field value.")]
		[SerializeField]
		private bool m_useDefaultLanguageForBlankTranslations = true;

		private string m_currentLanguage = string.Empty;

		private static UILocalizationManager s_instance;

		public static UILocalizationManager instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = Object.FindObjectOfType<UILocalizationManager>();
					if (s_instance == null && Application.isPlaying)
					{
						GlobalTextTable globalTextTable = Object.FindObjectOfType<GlobalTextTable>();
						s_instance = ((globalTextTable != null) ? globalTextTable.gameObject.AddComponent<UILocalizationManager>() : new GameObject("UILocalizationManager").AddComponent<UILocalizationManager>());
					}
				}
				return s_instance;
			}
			set
			{
				s_instance = value;
			}
		}

		public TextTable textTable
		{
			get
			{
				if (!(instance.m_textTable != null))
				{
					return GlobalTextTable.textTable;
				}
				return instance.m_textTable;
			}
			set
			{
				instance.m_textTable = value;
			}
		}

		public string currentLanguage
		{
			get
			{
				return instance.m_currentLanguage;
			}
			set
			{
				instance.m_currentLanguage = value;
				instance.UpdateUIs(value);
			}
		}

		public string currentLanguagePlayerPrefsKey
		{
			get
			{
				return m_currentLanguagePlayerPrefsKey;
			}
			set
			{
				m_currentLanguagePlayerPrefsKey = value;
			}
		}

		public bool saveLanguageInPlayerPrefs
		{
			get
			{
				return m_saveLanguageInPlayerPrefs;
			}
			set
			{
				m_saveLanguageInPlayerPrefs = value;
			}
		}

		public bool useDefaultLanguageForBlankTranslations
		{
			get
			{
				return m_useDefaultLanguageForBlankTranslations;
			}
			set
			{
				m_useDefaultLanguageForBlankTranslations = value;
				TextTable.useDefaultLanguageForBlankTranslations = value;
			}
		}

		private void Awake()
		{
			if (s_instance == null)
			{
				s_instance = this;
			}
			Initialize();
		}

		public void Initialize()
		{
			if (saveLanguageInPlayerPrefs && !string.IsNullOrEmpty(currentLanguagePlayerPrefsKey) && PlayerPrefs.HasKey(currentLanguagePlayerPrefsKey))
			{
				m_currentLanguage = PlayerPrefs.GetString(currentLanguagePlayerPrefsKey);
			}
			TextTable.useDefaultLanguageForBlankTranslations = m_useDefaultLanguageForBlankTranslations;
		}

		private IEnumerator Start()
		{
			yield return new WaitForEndOfFrame();
			UpdateUIs(currentLanguage);
		}

		public void UpdateUIs(string language)
		{
			m_currentLanguage = language;
			if (saveLanguageInPlayerPrefs && !string.IsNullOrEmpty(currentLanguagePlayerPrefsKey))
			{
				PlayerPrefs.SetString(currentLanguagePlayerPrefsKey, language);
			}
			LocalizeUI[] array = (m_alsoUpdateInactiveLocalizeUI ? GameObjectUtility.FindObjectsOfTypeAlsoInactive<LocalizeUI>() : Object.FindObjectsOfType<LocalizeUI>());
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateText();
			}
		}
	}
}
