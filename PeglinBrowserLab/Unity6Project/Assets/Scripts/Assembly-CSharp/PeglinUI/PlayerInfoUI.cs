using System;
using Battle;
using Currency;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using Map;
using Map.MinimapView;
using TMPro;
using UnityEngine;

namespace PeglinUI
{
	public class PlayerInfoUI : MonoBehaviour
	{
		[Header("Scene Refs")]
		public TextMeshProUGUI goldText;

		public TextMeshProUGUI floorText;

		public TextMeshProUGUI cruciballText;

		public TextMeshProUGUI mapTitleText;

		public TextMeshProUGUI goldChangeText;

		public GameObject mapTitle;

		[Header("Params")]
		public Color addGoldColor = Color.green;

		public Color removeGoldColor = Color.red;

		private Tween goldChangeTween;

		private Tween goldColorChangeTween;

		private Tween goldCounterTween;

		private bool goldChangeTriggered;

		private int currentGoldChange;

		private float goldChangeInitialPosY;

		private RectTransform goldChangeRectTransform;

		private void OnEnable()
		{
			goldText.text = CurrencyManager.Instance.GoldAmount.ToString();
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(ChangeGoldTextAdd));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(ChangeGoldTextRemove));
			CurrencyManager.OnUpdateGoldTextRequest = (Action)Delegate.Combine(CurrencyManager.OnUpdateGoldTextRequest, new Action(UpdateGoldAddedTextRequest));
			GetComponent<Canvas>().worldCamera = Camera.main;
			MapController.OnNodeSelectionEvent = (MapController.NodeSelectionEvent)Delegate.Combine(MapController.OnNodeSelectionEvent, new MapController.NodeSelectionEvent(ChangeFloorText));
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Combine(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ShowMapTitle));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(OnBattleEnded));
			BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Combine(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
			goldChangeRectTransform = goldChangeText.GetComponent<RectTransform>();
			goldChangeInitialPosY = goldChangeRectTransform.anchoredPosition.y;
		}

		private void OnDisable()
		{
			CleanupGoldText();
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(ChangeGoldTextAdd));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(ChangeGoldTextRemove));
			CurrencyManager.OnUpdateGoldTextRequest = (Action)Delegate.Remove(CurrencyManager.OnUpdateGoldTextRequest, new Action(UpdateGoldAddedTextRequest));
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Remove(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ShowMapTitle));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(OnBattleEnded));
			BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Remove(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
			goldColorChangeTween.Kill();
			goldColorChangeTween = null;
		}

		private void ChangeGoldTextAdd(int original, int amountChange, bool silent)
		{
			if (silent)
			{
				goldText.text = CurrencyManager.Instance.GoldAmount.ToString();
				return;
			}
			if (!goldChangeTriggered)
			{
				goldChangeTriggered = true;
				goldChangeText.gameObject.SetActive(value: true);
				currentGoldChange = 0;
			}
			DoGoldTextChange(amountChange, addGoldColor);
		}

		private void ChangeGoldTextRemove(int original, int amountChange, bool silent)
		{
			if (silent)
			{
				goldText.text = CurrencyManager.Instance.GoldAmount.ToString();
				return;
			}
			if (!goldChangeText.gameObject.activeInHierarchy)
			{
				currentGoldChange = 0;
				goldChangeText.gameObject.SetActive(value: true);
			}
			DoGoldTextChange(amountChange, removeGoldColor, proceedToEnd: true);
		}

		private void DoGoldTextChange(int amountChange, Color color, bool proceedToEnd = false)
		{
			currentGoldChange += amountChange;
			if (currentGoldChange >= 0)
			{
				goldChangeText.text = $"+{currentGoldChange}";
			}
			else
			{
				goldChangeText.text = $"{currentGoldChange}";
			}
			goldColorChangeTween.Kill();
			goldColorChangeTween = goldChangeText.DOColor(Color.white, 1f).From(color).SetAutoKill(autoKillOnCompletion: false);
			if (proceedToEnd)
			{
				Tween tween = goldColorChangeTween.Play();
				tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, new TweenCallback(EndOfTurnGoldAdd));
			}
			else
			{
				goldColorChangeTween.Play();
			}
		}

		private void CleanupGoldText()
		{
			if (goldChangeTween != null && goldChangeTween.active)
			{
				goldChangeTween.Kill();
				goldChangeTween = null;
				goldText.text = CurrencyManager.Instance.GoldAmount.ToString();
			}
		}

		private void ChangeFloorText(string mapName, int floor, int cruciballLevel)
		{
			if (cruciballText != null)
			{
				cruciballText.transform.parent.gameObject.SetActive(cruciballLevel > 0);
				cruciballText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("Menu/play_cruciball"), cruciballLevel);
			}
			floorText.text = string.Format("{0} - {1} {2}", mapName, LocalizationManager.GetTranslation("Menu/floor_label"), floor);
			mapTitleText.text = floorText.text;
		}

		private void ShowMapTitle(bool open)
		{
			floorText.enabled = !open;
			cruciballText.enabled = !open;
			mapTitle.SetActive(open);
		}

		private void UpdateGoldAddedTextRequest()
		{
			EndOfTurnGoldAdd();
		}

		private void OnBattleEnded()
		{
			EndOfTurnGoldAdd();
		}

		private void OnShotComplete()
		{
			EndOfTurnGoldAdd();
		}

		private void EndOfTurnGoldAdd()
		{
			goldChangeRectTransform.DOMoveY(goldText.transform.position.y, 0.5f);
			TweenerCore<Color, Color, ColorOptions> tweenerCore = goldChangeText.DOFade(0f, 0.5f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				goldChangeText.gameObject.SetActive(value: false);
				goldChangeRectTransform.anchoredPosition = new Vector3(0f, goldChangeInitialPosY);
				goldChangeText.DOFade(1f, 0f);
			}));
			int fromValue = CurrencyManager.Instance.GoldAmount - currentGoldChange;
			if (goldCounterTween != null && goldCounterTween.IsActive() && !goldCounterTween.IsPlaying())
			{
				goldCounterTween = goldText.DOCounter(fromValue, CurrencyManager.Instance.GoldAmount, 0.5f, addThousandsSeparator: false);
			}
			else
			{
				goldCounterTween.Kill();
				goldCounterTween = null;
				goldCounterTween = goldText.DOCounter(int.Parse(goldText.text), CurrencyManager.Instance.GoldAmount, 0.5f, addThousandsSeparator: false);
			}
			currentGoldChange = 0;
			goldChangeTriggered = false;
		}
	}
}
