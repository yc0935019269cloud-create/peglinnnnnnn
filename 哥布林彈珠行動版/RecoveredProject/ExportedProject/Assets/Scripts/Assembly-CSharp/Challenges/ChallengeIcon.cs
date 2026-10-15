using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Challenges
{
	public class ChallengeIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		public delegate void ChallengeTooltip(ChallengeEffect challenge, RectTransform icon, Vector2 anchoredPos);

		public delegate void HideChallengeTooltip(bool onDisable = false);

		public static ChallengeTooltip ShowTooltip = delegate
		{
		};

		public static HideChallengeTooltip HideTooltip = delegate
		{
		};

		private const float FADE_TIME = 0.3f;

		private const float FADE_ALPHA = 0.4f;

		public Vector2 tooltipAnchor = new Vector2(0.5f, 0.5f);

		public ChallengeEffect challenge;

		public bool alwaysShowTooltip;

		public float hoverWaitTime = 0.2f;

		public Selectable selectable;

		[HideInInspector]
		public bool shouldShowTooltip = true;

		protected Image _image;

		private float _hoverTime;

		private bool _isMouseOver;

		private bool _isShowingTooltip;

		private bool _isShowingTooltipWithMouseInput;

		private RectTransform _rectTransform;

		private bool _queueCheckForHideTooltip;

		private void Awake()
		{
			_image = GetComponent<Image>();
			_rectTransform = GetComponent<RectTransform>();
		}

		private void OnEnable()
		{
			ShowTooltip = (ChallengeTooltip)Delegate.Combine(ShowTooltip, new ChallengeTooltip(CheckDuplicateTooltip));
			ShowTooltip = (ChallengeTooltip)Delegate.Combine(ShowTooltip, new ChallengeTooltip(OnASlotIconShowingTooltip));
			HideTooltip = (HideChallengeTooltip)Delegate.Combine(HideTooltip, new HideChallengeTooltip(OnASlotIconHidingTooltip));
		}

		private void OnDisable()
		{
			ShowTooltip = (ChallengeTooltip)Delegate.Remove(ShowTooltip, new ChallengeTooltip(CheckDuplicateTooltip));
			ShowTooltip = (ChallengeTooltip)Delegate.Combine(ShowTooltip, new ChallengeTooltip(OnASlotIconShowingTooltip));
			HideTooltip = (HideChallengeTooltip)Delegate.Remove(HideTooltip, new HideChallengeTooltip(OnASlotIconHidingTooltip));
		}

		private void Update()
		{
			if (!shouldShowTooltip)
			{
				return;
			}
			if (challenge != ChallengeEffect.NONE && !_isShowingTooltip && (alwaysShowTooltip || _isMouseOver))
			{
				bool mouseButtonDown = Input.GetMouseButtonDown(0);
				_hoverTime += Time.deltaTime;
				if (mouseButtonDown)
				{
					_isShowingTooltip = true;
					_isShowingTooltipWithMouseInput = true;
					HideTooltip();
					ShowTooltip?.Invoke(challenge, _rectTransform, tooltipAnchor);
				}
				else if ((_hoverTime >= hoverWaitTime || alwaysShowTooltip) && !_isShowingTooltip)
				{
					_isShowingTooltip = true;
					HideTooltip();
					ShowTooltip?.Invoke(challenge, _rectTransform, tooltipAnchor);
				}
			}
			if (_queueCheckForHideTooltip && _isShowingTooltip && !_isMouseOver)
			{
				_isShowingTooltipWithMouseInput = false;
				_isShowingTooltip = false;
				_queueCheckForHideTooltip = false;
				HideTooltip();
			}
			else
			{
				_queueCheckForHideTooltip = false;
			}
			if (Input.GetMouseButtonDown(0) && !_queueCheckForHideTooltip)
			{
				_queueCheckForHideTooltip = true;
			}
		}

		private void CheckDuplicateTooltip(ChallengeEffect displayChallenge, RectTransform icon, Vector2 anchoredpos)
		{
			if (displayChallenge != challenge)
			{
				_hoverTime = 0f;
				_isMouseOver = false;
				_isShowingTooltip = false;
			}
		}

		public void SetChallenge(ChallengeEffect c)
		{
			challenge = c;
			_image.sprite = ChallengeManager.Instance.GetDataForChallenge(c).sprite;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isMouseOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_hoverTime = 0f;
			_isMouseOver = false;
			if (_isShowingTooltip && !alwaysShowTooltip && !_isShowingTooltipWithMouseInput)
			{
				_isShowingTooltip = false;
				HideTooltip();
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			_isShowingTooltip = true;
			HideTooltip();
			ShowTooltip?.Invoke(challenge, _rectTransform, tooltipAnchor);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			_isShowingTooltip = false;
			HideTooltip();
		}

		public void OnASlotIconShowingTooltip(ChallengeEffect challenge, RectTransform icon, Vector2 anchoredPos)
		{
			if (icon.GetComponent<ChallengeIcon>() == this)
			{
				_image.DOKill();
				_image.DOColor(Color.white, 0.3f);
			}
			else
			{
				_image.DOKill();
				_image.DOColor(new Color(1f, 1f, 1f, 0.4f), 0.3f);
			}
		}

		public void OnASlotIconHidingTooltip(bool onDisable = false)
		{
			_image.DOKill();
			_image.DOColor(Color.white, 0.3f);
		}
	}
}
