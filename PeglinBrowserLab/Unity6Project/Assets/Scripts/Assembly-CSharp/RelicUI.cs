using System;
using System.Collections.Generic;
using Battle;
using Battle.Attacks;
using DG.Tweening;
using Peglin.PegMinigame;
using PeglinUI.LoadoutManager;
using Relics;
using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
	[SerializeField]
	private RelicManager _relicManager;

	public GameObject relicIconPrefab;

	public RelicTooltip relicTooltip;

	public AnimatedRelicLinkage[] animatedRelicLinkages;

	private Dictionary<RelicEffect, RelicIcon> icons;

	private Dictionary<RelicEffect, GameObject> _animatedRelicPrefabs;

	private AudioSource _audioSource;

	private ImageCarousel _imageCarousel;

	private List<RelicEffect> _relicSfxPlayedThisFrame = new List<RelicEffect>();

	public ImageCarousel imageCarousel => _imageCarousel;

	private void Awake()
	{
		icons = new Dictionary<RelicEffect, RelicIcon>();
		_animatedRelicPrefabs = new Dictionary<RelicEffect, GameObject>();
		AnimatedRelicLinkage[] array = animatedRelicLinkages;
		for (int i = 0; i < array.Length; i++)
		{
			AnimatedRelicLinkage animatedRelicLinkage = array[i];
			_animatedRelicPrefabs.Add(animatedRelicLinkage.relicEffect, animatedRelicLinkage.iconPrefab);
		}
		_audioSource = GetComponent<AudioSource>();
		_imageCarousel = GetComponent<ImageCarousel>();
		_imageCarousel.Initialize(new List<GameObject>());
	}

	private void OnEnable()
	{
		RelicManager.OnRelicsReset = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicsReset, new RelicManager.RelicManagement(RemoveAll));
		RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(AddRelic));
		RelicManager.OnRelicRemoved = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicRemoved, new RelicManager.RelicManagement(RemoveRelic));
		RelicManager.OnRelicsReset = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicsReset, new RelicManager.RelicManagement(RemoveAll));
		RelicManager.OnRelicUsed = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicUsed, new RelicManager.RelicManagement(UseRelic));
		RelicManager.OnFlashRelic = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnFlashRelic, new RelicManager.RelicManagement(FlashRelic));
		RelicManager.OnRelicEnabled = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicEnabled, new RelicManager.RelicManagement(EnableRelic));
		RelicManager.OnRelicDisabled = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicDisabled, new RelicManager.RelicManagement(DisableRelic));
		RelicManager.OnRelicPaused = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicPaused, new RelicManager.RelicManagement(PauseRelic));
		RelicManager.OnRelicUnpaused = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicUnpaused, new RelicManager.RelicManagement(UnpauseRelic));
		RelicManager.OnCountdownDecremented = (RelicManager.CountdownRelicEvent)Delegate.Combine(RelicManager.OnCountdownDecremented, new RelicManager.CountdownRelicEvent(UpdateRelicCountdown));
		AttackManager.OnFireComplete = (Action)Delegate.Combine(AttackManager.OnFireComplete, new Action(FireComplete));
		BattleController.OnBattleStarted = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleStarted, new BattleController.BattleEvent(ResetRelicsPage));
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(BattleEnded));
		BattleController.UpdateRelicCountdownText = (BattleController.SetRelicCountdownText)Delegate.Combine(BattleController.UpdateRelicCountdownText, new BattleController.SetRelicCountdownText(SetRelicCountdownText));
		RelicIcon.ShowTooltip = (RelicIcon.RelicTooltip)Delegate.Combine(RelicIcon.ShowTooltip, new RelicIcon.RelicTooltip(ShowTooltip));
		RelicIcon.HideTooltip = (RelicIcon.HideRelicTooltip)Delegate.Combine(RelicIcon.HideTooltip, new RelicIcon.HideRelicTooltip(HideTooltip));
		RelicReward.ShowTooltipWorldPos = (RelicReward.RelicTooltipWorldPos)Delegate.Combine(RelicReward.ShowTooltipWorldPos, new RelicReward.RelicTooltipWorldPos(ShowTooltipWorldPos));
		RelicReward.HideTooltip = (RelicReward.HideRelicTooltip)Delegate.Combine(RelicReward.HideTooltip, new RelicReward.HideRelicTooltip(HideTooltip));
	}

	private void OnDisable()
	{
		RelicManager.OnRelicsReset = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicsReset, new RelicManager.RelicManagement(RemoveAll));
		RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(AddRelic));
		RelicManager.OnRelicRemoved = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicRemoved, new RelicManager.RelicManagement(RemoveRelic));
		RelicManager.OnRelicsReset = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicsReset, new RelicManager.RelicManagement(RemoveAll));
		RelicManager.OnRelicUsed = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicUsed, new RelicManager.RelicManagement(UseRelic));
		RelicManager.OnFlashRelic = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnFlashRelic, new RelicManager.RelicManagement(FlashRelic));
		RelicManager.OnRelicEnabled = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicEnabled, new RelicManager.RelicManagement(EnableRelic));
		RelicManager.OnRelicDisabled = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicDisabled, new RelicManager.RelicManagement(DisableRelic));
		RelicManager.OnRelicPaused = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicPaused, new RelicManager.RelicManagement(PauseRelic));
		RelicManager.OnRelicUnpaused = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicUnpaused, new RelicManager.RelicManagement(UnpauseRelic));
		RelicManager.OnCountdownDecremented = (RelicManager.CountdownRelicEvent)Delegate.Remove(RelicManager.OnCountdownDecremented, new RelicManager.CountdownRelicEvent(UpdateRelicCountdown));
		AttackManager.OnFireComplete = (Action)Delegate.Remove(AttackManager.OnFireComplete, new Action(FireComplete));
		BattleController.OnBattleStarted = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleStarted, new BattleController.BattleEvent(ResetRelicsPage));
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(BattleEnded));
		BattleController.UpdateRelicCountdownText = (BattleController.SetRelicCountdownText)Delegate.Remove(BattleController.UpdateRelicCountdownText, new BattleController.SetRelicCountdownText(SetRelicCountdownText));
		RelicIcon.ShowTooltip = (RelicIcon.RelicTooltip)Delegate.Remove(RelicIcon.ShowTooltip, new RelicIcon.RelicTooltip(ShowTooltip));
		RelicIcon.HideTooltip = (RelicIcon.HideRelicTooltip)Delegate.Remove(RelicIcon.HideTooltip, new RelicIcon.HideRelicTooltip(HideTooltip));
		RelicReward.ShowTooltipWorldPos = (RelicReward.RelicTooltipWorldPos)Delegate.Remove(RelicReward.ShowTooltipWorldPos, new RelicReward.RelicTooltipWorldPos(ShowTooltipWorldPos));
	}

	private void Update()
	{
		_relicSfxPlayedThisFrame.Clear();
	}

	private void RemoveAll(Relic none = null)
	{
		foreach (RelicIcon value in icons.Values)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		icons.Clear();
		_imageCarousel.RemoveAllElements();
	}

	private void AddRelic(Relic toAdd)
	{
		GameObject original = ((!_animatedRelicPrefabs.ContainsKey(toAdd.effect)) ? relicIconPrefab : _animatedRelicPrefabs[toAdd.effect]);
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		RelicIcon component = gameObject.GetComponent<RelicIcon>();
		component.SetRelic(toAdd);
		if (!icons.ContainsKey(toAdd.effect))
		{
			icons.Add(toAdd.effect, component);
		}
		gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		gameObject.GetComponent<Image>().DOFade(1f, 0.5f).From(0f);
		_imageCarousel.AddElement(gameObject);
	}

	private void RemoveRelic(Relic toRemove)
	{
		RelicIcon relicIcon = icons[toRemove.effect];
		if (relicIcon != null)
		{
			relicIcon.Remove();
			_imageCarousel.RemoveElement(relicIcon.gameObject);
		}
		icons.Remove(toRemove.effect);
	}

	private void UseRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect))
		{
			icons[relic.effect].Flash();
			if (RelicManager.relicCountdownValues.ContainsKey(relic.effect))
			{
				icons[relic.effect].ResetCountdownText();
			}
		}
		if (relic.useSfx != null && !_relicSfxPlayedThisFrame.Contains(relic.effect))
		{
			_audioSource.PlayOneShot(relic.useSfx);
			_relicSfxPlayedThisFrame.Add(relic.effect);
		}
	}

	private void FlashRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect))
		{
			icons[relic.effect].Flash();
		}
	}

	private void EnableRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect) && icons[relic.effect].grayScaleActive && !icons[relic.effect].paused)
		{
			icons[relic.effect].RemoveGrayscale();
		}
	}

	private void DisableRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect) && !icons[relic.effect].grayScaleActive && !icons[relic.effect].paused)
		{
			icons[relic.effect].ApplyGrayscale();
		}
	}

	private void PauseRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect) && !icons[relic.effect].paused)
		{
			icons[relic.effect].ApplyXOut();
			icons[relic.effect].ApplyGrayscale();
		}
	}

	private void UnpauseRelic(Relic relic)
	{
		if (icons.ContainsKey(relic.effect) && icons[relic.effect].paused)
		{
			icons[relic.effect].RemoveXOut();
			icons[relic.effect].RemoveGrayscale();
		}
	}

	private void FireComplete()
	{
		if (icons.ContainsKey(RelicEffect.ALL_ATTACKS_ECHO))
		{
			string countdownText = DamageCountDisplay.FormatDamageNumberAsString((long)((float)AttackManager.PreviousAttackDamage * RelicManager.ECHO_CHAMBER_PERCENT));
			icons[RelicEffect.ALL_ATTACKS_ECHO].SetCountdownText(countdownText);
		}
	}

	private void SetRelicCountdownText(RelicEffect relicEffect, string text)
	{
		if (icons.ContainsKey(relicEffect))
		{
			icons[relicEffect].SetCountdownText(text);
		}
	}

	private void BattleEnded()
	{
		ClearEchoCounter();
	}

	private void ClearEchoCounter()
	{
		if (icons.ContainsKey(RelicEffect.ALL_ATTACKS_ECHO))
		{
			icons[RelicEffect.ALL_ATTACKS_ECHO].HideCountdownText();
		}
	}

	private void UpdateRelicCountdown(Relic relic, int countdown)
	{
		if (icons.ContainsKey(relic.effect))
		{
			icons[relic.effect].SetCountdownText(countdown.ToString());
		}
	}

	private void ShowTooltip(Relic relic, RectTransform iconRect, Vector2 anchorPoint)
	{
		if (TooltipManager.Instance != null)
		{
			TooltipManager.Instance.ShowTooltipRelic(relic, iconRect.transform.position, anchorPoint, followMousePos: true);
		}
	}

	private void ShowTooltipWorldPos(Relic relic, Vector2 worldAnchorPosition, Vector2 anchorPoint)
	{
		relicTooltip.gameObject.SetActive(value: true);
		Rect rect = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect;
		RectTransform component = relicTooltip.GetComponent<RectTransform>();
		Rect rect2 = component.rect;
		Rect rect3 = new Rect(worldAnchorPosition, new Vector2(12f, 12f));
		component.position = worldAnchorPosition;
		Vector2 anchoredPosition = component.anchoredPosition;
		Vector2 zero = Vector2.zero;
		if (anchorPoint.x < 0.5f)
		{
			zero.x = 0f - (rect3.width + rect2.width * component.pivot.x);
		}
		else if (anchorPoint.x > 0.5f)
		{
			zero.x = rect3.width + rect2.width * component.pivot.x;
		}
		if (anchorPoint.y < 0.5f)
		{
			zero.y = 0f - (rect3.height + rect2.height * component.pivot.y);
		}
		else if (anchorPoint.y > 0.5f)
		{
			zero.y = rect3.height + rect2.height * component.pivot.y;
		}
		component.anchoredPosition += zero;
		float num = rect2.width * component.pivot.x;
		float num2 = rect.width - component.sizeDelta.x * (1f - component.pivot.x);
		float num3 = -1f * (rect.height - component.sizeDelta.y * component.pivot.y);
		float num4 = -1f * component.sizeDelta.y * (1f - component.pivot.y);
		Vector2 anchoredPosition2 = component.anchoredPosition;
		Vector2 vector = zero;
		if (anchoredPosition2.x < num)
		{
			vector.x = 0f - (rect3.width + rect2.width * component.pivot.x);
		}
		else if (anchoredPosition2.x > num2)
		{
			vector.x = rect3.width + rect2.width * component.pivot.x;
		}
		if (anchoredPosition2.y < num3)
		{
			vector.y = 0f - (rect3.height + rect2.height * component.pivot.y);
		}
		else if (anchoredPosition2.y > num4)
		{
			vector.y = rect3.height + rect2.height * component.pivot.y;
		}
		if (vector != zero)
		{
			component.anchoredPosition = anchoredPosition + zero;
		}
		anchoredPosition = component.anchoredPosition;
		component.anchoredPosition = new Vector2(Mathf.Clamp(anchoredPosition.x, num, num2), Mathf.Clamp(anchoredPosition.y, num3, num4));
		relicTooltip.SetupTooltip(relic);
	}

	private void HideTooltip(bool onDisable = false)
	{
		if (TooltipManager.Instance != null)
		{
			TooltipManager.Instance.HideTooltip();
		}
	}

	private void ResetRelicsPage()
	{
		if (_imageCarousel != null)
		{
			_imageCarousel.ShowPage(0);
		}
	}
}
