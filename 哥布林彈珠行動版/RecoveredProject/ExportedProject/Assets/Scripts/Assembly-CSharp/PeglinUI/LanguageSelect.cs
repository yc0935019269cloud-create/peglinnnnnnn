using System.Collections.Generic;
using I2.Loc;
using Rewired;
using Rewired.Integration.UnityUI;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI
{
	public class LanguageSelect : MonoBehaviour
	{
		public delegate void LanguageChangeAction();

		public static LanguageChangeAction OnLanguageChangeAction;

		public GameObject warningText;

		public TMP_Text languageText;

		public List<string> completedLanguages = new List<string> { "English" };

		public Button leftArrow;

		public Button rightArrow;

		[Tooltip("Languages that exist in our sheet, but might have blocking issues in one form or another.")]
		public List<string> blockedLanguages;

		private List<string> _languages;

		private int _currentLanguage;

		private bool _inputLock;

		private EventSystem _eventSystem;

		private Player _player;

		private void OnEnable()
		{
			if (_eventSystem == null)
			{
				_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			}
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(0);
			}
			if (_languages == null)
			{
				_languages = LocalizationManager.GetAllLanguages();
				_languages.Sort();
				foreach (string blockedLanguage in blockedLanguages)
				{
					_languages.Remove(blockedLanguage);
				}
			}
			_currentLanguage = _languages.IndexOf(LocalizationManager.CurrentLanguage);
			if (_currentLanguage < 0)
			{
				_currentLanguage = 0;
			}
			UpdateLanguage();
			leftArrow.onClick.AddListener(PrevLanguage);
			rightArrow.onClick.AddListener(NextLanguage);
		}

		private void OnDisable()
		{
			leftArrow.onClick.RemoveListener(PrevLanguage);
			rightArrow.onClick.RemoveListener(NextLanguage);
		}

		private void Update()
		{
			float axis = _player.GetAxis("UIHorizontal");
			if (Mathf.Abs(axis) > 0.05f && _eventSystem.currentSelectedGameObject == base.gameObject)
			{
				if (!_inputLock)
				{
					if (axis > 0.05f)
					{
						NextLanguage();
					}
					else
					{
						PrevLanguage();
					}
					_inputLock = true;
				}
			}
			else
			{
				_inputLock = false;
			}
		}

		public void NextLanguage()
		{
			_currentLanguage++;
			if (_currentLanguage >= _languages.Count)
			{
				_currentLanguage = 0;
			}
			UpdateLanguage();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		public void PrevLanguage()
		{
			_currentLanguage--;
			if (_currentLanguage < 0)
			{
				_currentLanguage = _languages.Count - 1;
			}
			UpdateLanguage();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		public void UpdateLanguage()
		{
			languageText.text = _languages[_currentLanguage];
			LocalizationManager.SetLanguageAndCode(_languages[_currentLanguage], LocalizationManager.GetLanguageCode(_languages[_currentLanguage]));
			SettingsManager.Instance.Language = _languages[_currentLanguage];
			OnLanguageChangeAction?.Invoke();
		}
	}
}
