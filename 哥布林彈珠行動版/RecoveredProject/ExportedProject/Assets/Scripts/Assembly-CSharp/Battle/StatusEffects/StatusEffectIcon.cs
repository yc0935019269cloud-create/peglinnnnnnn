using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Battle.StatusEffects
{
	public class StatusEffectIcon : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public delegate void StatusEffectTooltipEvent(StatusEffectIcon icon);

		public static StatusEffectTooltipEvent OnStatusEffectTooltipShown;

		public static StatusEffectTooltipEvent OnStatusEffectTooltipHidden;

		private const float FADE_TIME = 0.3f;

		private const float FADE_ALPHA = 0.5f;

		[SerializeField]
		private TextMeshProUGUI _intensityText;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private Image _plusIcon;

		[SerializeField]
		private Sprite[] _ballwarkAnimation;

		[FormerlySerializedAs("_effectType")]
		public StatusEffectType effectType;

		private FlashStatusEffectIcon _hitFlash;

		private bool _tooltipVisible;

		private Vector3 _plusIconPos;

		private RectTransform _plusIconRectTransform;

		private Tween _activeScaleTween;

		private bool isOnPlayer;

		private bool _destructionAnimationPlaying;

		[HideInInspector]
		public bool iconForcefullyRemovedDuringDestructionAnimation;

		[HideInInspector]
		public Selectable selectable;

		public int intensity { get; protected set; }

		public bool IsDestructionAnimationPlaying => _destructionAnimationPlaying;

		public FlashStatusEffectIcon hitFlash
		{
			set
			{
				_hitFlash = value;
				_hitFlash.SetSprite(_image.sprite);
			}
		}

		private void Awake()
		{
			_plusIconRectTransform = _plusIcon.GetComponent<RectTransform>();
			_plusIconPos = _plusIconRectTransform.anchoredPosition;
			selectable = GetComponent<Selectable>();
		}

		private void OnEnable()
		{
			OnStatusEffectTooltipShown = (StatusEffectTooltipEvent)Delegate.Combine(OnStatusEffectTooltipShown, new StatusEffectTooltipEvent(OnAnIconShowingTooltip));
			OnStatusEffectTooltipHidden = (StatusEffectTooltipEvent)Delegate.Combine(OnStatusEffectTooltipHidden, new StatusEffectTooltipEvent(OnAnIconHidingTooltip));
			hitFlash = GetComponent<FlashStatusEffectIcon>();
		}

		private void OnDisable()
		{
			OnStatusEffectTooltipShown = (StatusEffectTooltipEvent)Delegate.Remove(OnStatusEffectTooltipShown, new StatusEffectTooltipEvent(OnAnIconShowingTooltip));
			OnStatusEffectTooltipHidden = (StatusEffectTooltipEvent)Delegate.Remove(OnStatusEffectTooltipHidden, new StatusEffectTooltipEvent(OnAnIconHidingTooltip));
			if (_tooltipVisible)
			{
				TooltipManager.Instance.HideTooltip();
				_tooltipVisible = false;
			}
		}

		public void SetData(Sprite icon, int intensity, StatusEffectType type, bool isOnPlayer)
		{
			this.isOnPlayer = isOnPlayer;
			this.intensity = intensity;
			_image.sprite = icon;
			if (_hitFlash != null)
			{
				_hitFlash.SetSprite(icon);
			}
			effectType = type;
			SetData(intensity);
		}

		public void SetData(int intensity)
		{
			this.intensity = intensity;
			if (intensity <= 0)
			{
				Debug.LogWarning("StatusEffectIcon::SetData: Intensity was <= 0, deleting icon...");
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (intensity > 999)
			{
				_intensityText.text = Mathf.FloorToInt((float)intensity / 1000f).ToString(CultureInfo.InvariantCulture) + "K";
			}
			else
			{
				_intensityText.text = intensity.ToString(CultureInfo.InvariantCulture);
			}
		}

		public void Pulse()
		{
			if (_plusIconRectTransform == null)
			{
				return;
			}
			if (_activeScaleTween != null)
			{
				_activeScaleTween.Kill(complete: true);
			}
			Transform transform = _image.transform;
			Vector3 localScale = transform.localScale;
			transform.localScale *= 1.75f;
			_activeScaleTween = transform.DOScale(localScale, 1f);
			_plusIcon.gameObject.SetActive(value: true);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _plusIconRectTransform.DOLocalMoveY(_plusIconRectTransform.anchoredPosition.y + 0.5f, 0.5f).SetEase(Ease.OutCubic);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				if (base.gameObject.activeInHierarchy)
				{
					StartCoroutine(BlinkPlus());
				}
			}));
			_intensityText.color = Color.green;
			_intensityText.DOColor(Color.white, 0.5f);
		}

		private IEnumerator BlinkPlus()
		{
			WaitForSeconds wait = new WaitForSeconds(0.1f);
			yield return wait;
			_plusIcon.color = new Color(1f, 1f, 1f, 0f);
			yield return wait;
			_plusIcon.color = Color.white;
			yield return wait;
			RestorePlusPosition();
		}

		private void RestorePlusPosition()
		{
			_plusIconRectTransform.anchoredPosition = _plusIconPos;
			_plusIcon.gameObject.SetActive(value: false);
		}

		private void OnAnIconShowingTooltip(StatusEffectIcon icon)
		{
			if (icon == this)
			{
				_image.DOKill();
				_image.DOColor(Color.white, 0.3f);
				_intensityText.DOKill();
				_intensityText.DOColor(Color.white, 0.3f);
			}
			else
			{
				_image.DOKill();
				_image.DOColor(new Color(1f, 1f, 1f, 0.5f), 0.3f);
				_intensityText.DOKill();
				_intensityText.DOColor(new Color(1f, 1f, 1f, 0.5f), 0.3f);
			}
		}

		private void OnAnIconHidingTooltip(StatusEffectIcon icon)
		{
			_image.DOKill();
			_image.DOColor(Color.white, 0.3f);
			_intensityText.DOKill();
			_intensityText.DOColor(Color.white, 0.3f);
		}

		private void ShowTooltip()
		{
			_tooltipVisible = true;
			TooltipManager.Instance.ShowTooltipStatusEffect(effectType, base.transform.position, new Vector3(1f, -1f), isOnPlayer);
			OnStatusEffectTooltipShown(this);
		}

		private void HideTooltip()
		{
			TooltipManager.Instance.HideTooltip();
			_tooltipVisible = false;
			OnStatusEffectTooltipHidden(this);
		}

		public void OnMouseOver()
		{
			if (!_tooltipVisible)
			{
				ShowTooltip();
			}
		}

		public void OnMouseExit()
		{
			if (_tooltipVisible)
			{
				HideTooltip();
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!_tooltipVisible)
			{
				ShowTooltip();
			}
		}

		public void OnDeselect(BaseEventData data)
		{
			if (_tooltipVisible)
			{
				HideTooltip();
			}
		}

		public void RemoveIcon(Dictionary<StatusEffectType, StatusEffectIcon> trackingDictionary)
		{
			if (effectType == StatusEffectType.Ballwark)
			{
				StartCoroutine(PlayDestroyAnimation(trackingDictionary, _ballwarkAnimation));
				return;
			}
			trackingDictionary.Remove(effectType);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private IEnumerator PlayDestroyAnimation(Dictionary<StatusEffectType, StatusEffectIcon> trackingDictionary, Sprite[] spritesheet)
		{
			_destructionAnimationPlaying = true;
			float timeBetweenFrames = 0.1f;
			int totalFrames = spritesheet.Length - 1;
			int currentFrame = 0;
			_intensityText.text = "";
			while (currentFrame < totalFrames)
			{
				_image.sprite = spritesheet[currentFrame];
				currentFrame++;
				yield return new WaitForSecondsRealtime(timeBetweenFrames);
			}
			if (!iconForcefullyRemovedDuringDestructionAnimation)
			{
				trackingDictionary.Remove(effectType);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
