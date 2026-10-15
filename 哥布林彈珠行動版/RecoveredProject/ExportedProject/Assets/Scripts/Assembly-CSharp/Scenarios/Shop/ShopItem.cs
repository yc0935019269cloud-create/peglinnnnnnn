using System;
using System.Globalization;
using Battle;
using Challenges;
using Coffee.UIEffects;
using Currency;
using DG.Tweening;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using PeglinUI.UIUtils;
using Relics;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scenarios.Shop
{
	public class ShopItem : MonoBehaviour
	{
		public delegate void ShopItemHoverEvent(IPurchasableItem item);

		public static ShopItemHoverEvent OnShopItemHover;

		public static InventoryViewController.InventoryViewToggle OnShopItemViewToggle;

		[HideInInspector]
		public Selectable selectable;

		[Header("Scene Refs")]
		public Image itemImage;

		public GameObject itemObject;

		public GameObject itemBackground;

		public float itemBackgroundAngleRange;

		public TextMeshProUGUI goldText;

		public GameObject outOfStockOverlay;

		public GameObject priceDisplay;

		public Button button;

		public GameObject newRewardIcon;

		[Header("Parameters")]
		public Color affordableColor = Color.white;

		[FormerlySerializedAs("unaffodableColor")]
		public Color unaffordableColor = Color.red;

		public AudioClip purchaseSfx;

		private IPurchasableItem _item;

		private Button _button;

		private ShopManager _sm;

		private bool _purchased;

		private bool _affordable;

		private UIEffect _grayscaleEffect;

		private UIShadow _shadowEffect;

		private UIFlip _flipEffect;

		private PlayerHealthController _playerHealthController;

		private RelicManager _relicManager;

		private void OnEnable()
		{
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(CheckItemOnCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(CheckItemOnCurrencyChange));
		}

		private void OnDisable()
		{
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(CheckItemOnCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(CheckItemOnCurrencyChange));
			PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(CheckItemOnHealthChange));
			PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(CheckItemOnHealthChange));
		}

		private void CheckItemOnCurrencyChange(int originalAmount, int currencyChange, bool silent)
		{
			if (_item != null)
			{
				ValidateItemIsAffordable();
			}
		}

		private void CheckItemOnHealthChange(float healAmount)
		{
			if (_item != null)
			{
				ValidateItemIsAffordable();
			}
		}

		private void ValidateItemIsAffordable()
		{
			if (!_purchased)
			{
				int num = _item.GetCost();
				bool num2 = ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE);
				if (num2)
				{
					num = ChallengeManager.GetHpCostForGold(num);
				}
				bool flag = (num2 ? (_playerHealthController.CurrentHealth > (float)num) : (CurrencyManager.Instance.GoldAmount >= num));
				goldText.text = num.ToString(CultureInfo.InvariantCulture);
				goldText.color = (flag ? affordableColor : unaffordableColor);
				_grayscaleEffect.enabled = !flag;
				_affordable = flag;
			}
		}

		public void Initialize(IPurchasableItem item, ShopManager sm, PlayerHealthController playerHealthController, RelicManager relicManager)
		{
			if (_grayscaleEffect == null)
			{
				_grayscaleEffect = GetComponentInChildren<UIEffect>(includeInactive: true);
			}
			if (_flipEffect == null)
			{
				_flipEffect = GetComponentInChildren<UIFlip>(includeInactive: true);
			}
			_item = item;
			_playerHealthController = playerHealthController;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(CheckItemOnHealthChange));
				PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(CheckItemOnHealthChange));
			}
			itemImage.sprite = item.GetPurchasableImage();
			itemImage.rectTransform.localScale = item.GetSpriteLocalScale();
			itemImage.rectTransform.pivot = OrbUIUtils.GetSpritePivotPointRelative(itemImage.sprite);
			newRewardIcon.SetActive(!_item.GetUnlocked());
			float zAngle = UnityEngine.Random.value * itemBackgroundAngleRange - itemBackgroundAngleRange / 2f;
			itemBackground.GetComponent<RectTransform>().Rotate(0f, 0f, zAngle, Space.Self);
			int gold = item.GetCost();
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				gold = ChallengeManager.GetHpCostForGold(gold);
			}
			goldText.text = gold.ToString(CultureInfo.InvariantCulture);
			_button = GetComponentInChildren<Button>();
			_sm = sm;
			selectable = _button;
			ValidateItemIsAffordable();
			MouseOverDetectorPointerHandler component = GetComponent<MouseOverDetectorPointerHandler>();
			SelectionEventListener componentInChildren = GetComponentInChildren<SelectionEventListener>(includeInactive: true);
			component.OnPointerEntered = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerEntered, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				if (!_purchased)
				{
					OnShopItemHover?.Invoke(_item);
					item.ShowTooltip(base.transform.position, TooltipManager.TOOLTIP_DEFAULT_OFFSET + new Vector2(0.25f, 0f), followMousePosition: true);
				}
			}));
			component.OnPointerExited = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerExited, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				TooltipManager.Instance.HideTooltip();
			}));
			componentInChildren.OnSelectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(componentInChildren.OnSelectionEvent, (SelectionEventListener.SelectionEvent)(() =>
			{
				if (!ControllerInputManager.IsPointerBased && !_purchased)
				{
					OnShopItemHover?.Invoke(_item);
					item.ShowTooltip(base.transform.position, TooltipManager.TOOLTIP_DEFAULT_OFFSET);
				}
			}));
			componentInChildren.OnDeselectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(componentInChildren.OnDeselectionEvent, (SelectionEventListener.SelectionEvent)(() =>
			{
				TooltipManager.Instance.HideTooltip();
			}));
		}

		public void UpdateCostText()
		{
			ValidateItemIsAffordable();
		}

		public void ShowUpgradeInformation()
		{
			if (!_purchased)
			{
				OnShopItemViewToggle(open: true);
				if (_item is PurchasableOrb)
				{
					_sm.upgradeConfirmationOrbInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = selectable.gameObject;
					_sm.upgradeConfirmationOrbInspect.gameObject.SetActive(value: true);
					((PurchasableOrb)_item).ShowUpgradeDetailsPanel(_sm.upgradeConfirmationOrbInspect);
				}
				else if (_item is PurchasableRelic)
				{
					_sm.upgradeConfirmationRelic.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = selectable.gameObject;
					_sm.upgradeConfirmationRelic.gameObject.SetActive(value: true);
					((PurchasableRelic)_item).ShowUpgradeDetailsPanel(_sm.upgradeConfirmationRelic);
				}
				UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(OnUpgradeOptionClicked));
				UIControlsSelf.OnClosePressed = (UIControlsSelf.ClosePressed)Delegate.Combine(UIControlsSelf.OnClosePressed, new UIControlsSelf.ClosePressed(RemoveUpgradeCallback));
			}
		}

		private void RemoveUpgradeCallback()
		{
			OnShopItemViewToggle(open: false);
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(OnUpgradeOptionClicked));
			UIControlsSelf.OnClosePressed = (UIControlsSelf.ClosePressed)Delegate.Remove(UIControlsSelf.OnClosePressed, new UIControlsSelf.ClosePressed(RemoveUpgradeCallback));
			_sm.upgradeConfirmationOrbInspect.gameObject.SetActive(value: false);
			_sm.upgradeConfirmationRelic.gameObject.SetActive(value: false);
		}

		private void OnUpgradeOptionClicked(UpgradeOption.UpgradeType type, GameObject clickedbutton, GameObject chosen)
		{
			PurchaseItem();
		}

		public void PurchaseItem()
		{
			if (_purchased || !_affordable)
			{
				UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
				return;
			}
			UIControlsSelf.OnClosePressed = (UIControlsSelf.ClosePressed)Delegate.Remove(UIControlsSelf.OnClosePressed, new UIControlsSelf.ClosePressed(RemoveUpgradeCallback));
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(OnUpgradeOptionClicked));
			_sm.upgradeConfirmationOrbInspect.gameObject.SetActive(value: false);
			_sm.upgradeConfirmationRelic.gameObject.SetActive(value: false);
			_sm.PurchaseItem(_item);
			_purchased = true;
			UIButtonClickEventDispatcher.OnUIButtonCustomSfx(purchaseSfx);
			TooltipManager.Instance.HideTooltip();
			GetComponentInChildren<ButtonHandleHover>().enabled = false;
			DoPurchaseAnimation();
			priceDisplay.SetActive(value: false);
			newRewardIcon.SetActive(value: false);
		}

		private void DoPurchaseAnimation()
		{
			itemImage.transform.DOMoveY(base.transform.position.y + 1f, 0.5f).SetEase(Ease.OutSine);
			itemImage.DOFade(0f, 0.5f).SetEase(Ease.OutCubic).From(1f);
		}

		public void SetInteractable(bool value)
		{
			if (_button != null)
			{
				_button.enabled = false;
			}
		}
	}
}
