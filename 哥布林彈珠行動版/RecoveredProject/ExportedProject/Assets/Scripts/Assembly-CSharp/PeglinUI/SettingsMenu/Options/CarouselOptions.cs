using System;
using I2.Loc;
using PeglinUI.ControllerSupport;
using Rewired;
using Rewired.Integration.UnityUI;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.SettingsMenu.Options
{
	public class CarouselOptions : MonoBehaviour
	{
		[Header("Parameters")]
		public CarouselTextItem[] textItems;

		[Header("Scene References")]
		[SerializeField]
		private Button _leftArrow;

		[SerializeField]
		private Button _rightArrow;

		[SerializeField]
		private TextMeshProUGUI _optionText;

		protected int _displayedOptionIndex;

		private RewiredEventSystem _eventSystem;

		private bool _horizontalAxisDetected;

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
			_leftArrow.onClick.AddListener(ScrollOptionLeft);
			_rightArrow.onClick.AddListener(ScrollOptionRight);
			LanguageSelect.OnLanguageChangeAction = (LanguageSelect.LanguageChangeAction)Delegate.Combine(LanguageSelect.OnLanguageChangeAction, new LanguageSelect.LanguageChangeAction(TranslateOptionText));
		}

		private void OnDisable()
		{
			_leftArrow.onClick.RemoveListener(ScrollOptionLeft);
			_rightArrow.onClick.RemoveListener(ScrollOptionRight);
			LanguageSelect.OnLanguageChangeAction = (LanguageSelect.LanguageChangeAction)Delegate.Remove(LanguageSelect.OnLanguageChangeAction, new LanguageSelect.LanguageChangeAction(TranslateOptionText));
		}

		private void Update()
		{
			if (_eventSystem.currentSelectedGameObject == base.gameObject && !ControllerInputManager.IsPointerBased)
			{
				float axis = _player.GetAxis("Horizontal");
				if (axis == 0f)
				{
					axis = _player.GetAxis("UIHorizontal");
				}
				if (axis < 0f && !_horizontalAxisDetected && _displayedOptionIndex > 0)
				{
					_horizontalAxisDetected = true;
					ScrollOptionLeft();
				}
				else if (axis > 0f && !_horizontalAxisDetected && _displayedOptionIndex < textItems.Length - 1)
				{
					_horizontalAxisDetected = true;
					ScrollOptionRight();
				}
				if (axis == 0f)
				{
					_horizontalAxisDetected = false;
				}
			}
		}

		public virtual void ApplyOption()
		{
		}

		private void ScrollOptionLeft()
		{
			_displayedOptionIndex = Mathf.Clamp(_displayedOptionIndex - 1, 0, textItems.Length - 1);
			ShowOption();
			ApplyOption();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		private void ScrollOptionRight()
		{
			_displayedOptionIndex = Mathf.Clamp(_displayedOptionIndex + 1, 0, textItems.Length - 1);
			ShowOption();
			ApplyOption();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		protected void ShowOption()
		{
			TranslateOptionText();
			CheckShowArrows();
			CheckHideArrows();
		}

		public void TranslateOptionText()
		{
			CarouselTextItem carouselTextItem = textItems[_displayedOptionIndex];
			_optionText.text = (carouselTextItem.isTranslatable ? LocalizationManager.GetTranslation(carouselTextItem.item) : carouselTextItem.item);
		}

		private void CheckShowArrows()
		{
			if (_displayedOptionIndex < textItems.Length - 1)
			{
				_rightArrow.gameObject.SetActive(value: true);
			}
			if (_displayedOptionIndex > 0)
			{
				_leftArrow.gameObject.SetActive(value: true);
			}
		}

		private void CheckHideArrows()
		{
			if (_displayedOptionIndex == textItems.Length - 1)
			{
				_rightArrow.gameObject.SetActive(value: false);
			}
			if (_displayedOptionIndex == 0)
			{
				_leftArrow.gameObject.SetActive(value: false);
			}
		}
	}
}
