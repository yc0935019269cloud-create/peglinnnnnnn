using System;
using Coffee.UIEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Relics;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public delegate void RelicTooltip(Relic relic, RectTransform icon, Vector2 anchoredPos);

	public delegate void HideRelicTooltip(bool onDisable = false);

	public static RelicTooltip ShowTooltip = delegate
	{
	};

	public static HideRelicTooltip HideTooltip = delegate
	{
	};

	[Tooltip("Where should the tooltip be anchored? (0,0) is top left, (1,1) is bottom right.")]
	public Vector2 tooltipAnchor = new Vector2(0.5f, 0.5f);

	public float flashTime = 0.2f;

	public float fadeTime = 0.6f;

	public float hoverWaitTime = 0.2f;

	public bool alwaysShowTooltip;

	public GameObject newRewardIndicator;

	public Color fadeColor = Color.white;

	private Color origColor = Color.white;

	private Color _textFadeColor = Color.white;

	private Color _textOrigColor = Color.white;

	private Color _dropShadowOrigColor = Color.black;

	private Color _dropShadowFadeColor = Color.black;

	protected Image _image;

	public TMP_Text countdownText;

	public TMP_Text countdownTextDropShadow;

	private RectTransform _rectTransform;

	public Relic relic;

	private UIEffect _uiEffect;

	public bool grayScaleActive;

	public Image xImage;

	[HideInInspector]
	public bool paused;

	[HideInInspector]
	public bool shouldShowTooltip = true;

	private float _hoverTime;

	private bool _isMouseOver;

	private bool _isShowingTooltip;

	private bool _isShowingTooltipWithMouseInput;

	private bool _queueCheckForHideTooltip;

	private void Awake()
	{
		_image = GetComponent<Image>();
		_uiEffect = GetComponent<UIEffect>();
		origColor = _image.color;
		if (countdownText != null)
		{
			_textFadeColor = (_textOrigColor = countdownText.color);
			_textFadeColor.a = fadeColor.a;
		}
		if (countdownTextDropShadow != null)
		{
			_dropShadowFadeColor = (_dropShadowOrigColor = countdownTextDropShadow.color);
			_dropShadowFadeColor.a = fadeColor.a;
		}
		_rectTransform = GetComponent<RectTransform>();
	}

	protected virtual void OnEnable()
	{
		ShowTooltip = (RelicTooltip)Delegate.Combine(ShowTooltip, new RelicTooltip(CheckDuplicateTooltip));
		HideTooltip = (HideRelicTooltip)Delegate.Combine(HideTooltip, new HideRelicTooltip(RestoreColor));
	}

	private void OnDisable()
	{
		HideTooltip = (HideRelicTooltip)Delegate.Remove(HideTooltip, new HideRelicTooltip(RestoreColor));
		HideTooltip(onDisable: true);
		ShowTooltip = (RelicTooltip)Delegate.Remove(ShowTooltip, new RelicTooltip(CheckDuplicateTooltip));
	}

	public void SetRelic(Relic r)
	{
		relic = r;
		_image.sprite = r.sprite;
		if (newRewardIndicator != null)
		{
			if (!PersistentPlayerData.Instance.UnlockedRelics.Contains(relic.effect) && !PersistentPlayerData.Instance.QueuedRelicUnlocks.Contains(relic.effect) && PersistentPlayerData.Instance.AnyCruciballUnlockedForClass(StaticGameData.chosenClass) && relic.effect != RelicEffect.NONE)
			{
				newRewardIndicator.SetActive(value: true);
			}
			else
			{
				newRewardIndicator.SetActive(value: false);
			}
		}
		if (!(countdownText == null))
		{
			if (RelicManager.relicCountdownValues.ContainsKey(r.effect))
			{
				SetCountdownText(RelicManager.relicCountdownValues[r.effect].ToString());
				return;
			}
			if (RelicManager.relicUsesPerRunCounts.ContainsKey(r.effect))
			{
				SetCountdownText(RelicManager.relicUsesPerRunCounts[r.effect].ToString());
				return;
			}
			countdownText.enabled = false;
			countdownTextDropShadow.enabled = false;
		}
	}

	public void Remove()
	{
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOScale(Vector3.zero, fadeTime);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(Delete));
	}

	private void Delete()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void Flash()
	{
		DOTween.To(() => _uiEffect.colorFactor, (float x) =>
		{
			_uiEffect.colorFactor = x;
		}, 0f, flashTime).From(1f).SetUpdate(isIndependentUpdate: true);
	}

	public void ApplyGrayscale()
	{
		_uiEffect.effectFactor = 1f;
		grayScaleActive = true;
	}

	public void RemoveGrayscale()
	{
		_uiEffect.effectFactor = 0f;
		grayScaleActive = false;
	}

	public void ApplyXOut()
	{
		xImage.enabled = true;
		paused = true;
	}

	public void RemoveXOut()
	{
		xImage.enabled = false;
		paused = false;
	}

	public void HideCountdownText()
	{
		TMP_Text tMP_Text = countdownText;
		bool flag = (countdownTextDropShadow.enabled = false);
		tMP_Text.enabled = flag;
	}

	public void SetCountdownText(string countdown)
	{
		if ((RelicManager.relicUsesPerRunCounts.ContainsKey(relic.effect) && RelicManager.relicUsesPerRunCounts[relic.effect] <= 1) || (RelicManager.relicUsesPerShotCounts.ContainsKey(relic.effect) && RelicManager.relicUsesPerShotCounts[relic.effect] <= 1))
		{
			countdownText.enabled = false;
			countdownTextDropShadow.enabled = false;
			return;
		}
		TMP_Text tMP_Text = countdownText;
		bool flag = (countdownTextDropShadow.enabled = true);
		tMP_Text.enabled = flag;
		TMP_Text tMP_Text2 = countdownText;
		string text = (countdownTextDropShadow.text = countdown);
		tMP_Text2.text = text;
	}

	public void ResetCountdownText()
	{
		SetCountdownText(RelicManager.relicCountdownValues[relic.effect].ToString());
	}

	public void Update()
	{
		if (!shouldShowTooltip)
		{
			return;
		}
		if (relic != null && !_isShowingTooltip && (alwaysShowTooltip || _isMouseOver))
		{
			bool mouseButtonDown = Input.GetMouseButtonDown(0);
			_hoverTime += Time.deltaTime;
			if (mouseButtonDown)
			{
				_isShowingTooltip = true;
				_isShowingTooltipWithMouseInput = true;
				HideTooltip();
				ShowTooltip?.Invoke(relic, _rectTransform, tooltipAnchor);
			}
			else if ((_hoverTime >= hoverWaitTime || alwaysShowTooltip) && !_isShowingTooltip)
			{
				_isShowingTooltip = true;
				HideTooltip();
				ShowTooltip?.Invoke(relic, _rectTransform, tooltipAnchor);
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
		ShowTooltip?.Invoke(relic, _rectTransform, tooltipAnchor);
	}

	public void OnDeselect(BaseEventData data)
	{
		_isShowingTooltip = false;
		HideTooltip();
	}

	private void CheckDuplicateTooltip(Relic displayRelic, RectTransform rect, Vector2 anchoredPos)
	{
		if (relic != displayRelic)
		{
			_hoverTime = 0f;
			_isMouseOver = false;
			_isShowingTooltip = false;
			alwaysShowTooltip = false;
			_image.DOColor(fadeColor, 0.35f);
			if (countdownText != null)
			{
				countdownText.DOColor(_textFadeColor, 0.35f);
			}
			if (countdownTextDropShadow != null)
			{
				countdownTextDropShadow.DOColor(_dropShadowFadeColor, 0.35f);
			}
		}
	}

	private void RestoreColor(bool onDisable = false)
	{
		_image.DOKill();
		if (!(_image.color != origColor))
		{
			return;
		}
		if (onDisable)
		{
			_image.color = origColor;
			return;
		}
		_image.DOColor(origColor, 0.35f);
		if (countdownText != null)
		{
			countdownText.DOColor(_textOrigColor, 0.35f);
		}
		if (countdownTextDropShadow != null)
		{
			countdownTextDropShadow.DOColor(_dropShadowOrigColor, 0.35f);
		}
	}
}
