using System;
using Battle;
using Challenges;
using Currency;
using DG.Tweening;
using Scenarios.Shop;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	[RequireComponent(typeof(Button))]
	public class RemoveOrbButton : MonoBehaviour
	{
		[HideInInspector]
		public Selectable selectable;

		[Header("Scene Refs")]
		public TextMeshProUGUI removeOrbText;

		public Color removeOrbTextUnaffordableColor;

		public Color removeOrbTextAffordableColor;

		public TextMeshProUGUI removeOrbCostText;

		public Image goldImage;

		public GameObject removeOrbContainer;

		private int _removeOrbCost;

		private Button _button;

		private InventoryViewController _inventoryViewController;

		private ShopManager _shopManager;

		private bool _used;

		private bool _enabled;

		private PlayerHealthController _playerHealthController;

		private void OnEnable()
		{
			if (_button == null)
			{
				_button = GetComponent<Button>();
			}
			_button.onClick.AddListener(ShowOrbRemoval);
		}

		private void OnDisable()
		{
			_button.onClick.RemoveListener(ShowOrbRemoval);
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(ValidateButtonActiveOnCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(ValidateButtonActiveOnCurrencyChange));
		}

		public void Initialize(ShopManager shopManager, int removeOrbCost)
		{
			if (_button == null)
			{
				_button = GetComponent<Button>();
			}
			selectable = _button;
			_shopManager = shopManager;
			_removeOrbCost = removeOrbCost;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_playerHealthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
				_removeOrbCost = ChallengeManager.GetHpCostForGold(removeOrbCost);
			}
			removeOrbCostText.text = _removeOrbCost.ToString();
			ValidateButtonActive();
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(ValidateButtonActiveOnCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(ValidateButtonActiveOnCurrencyChange));
			if (_inventoryViewController == null)
			{
				_inventoryViewController = UnityEngine.Object.FindObjectOfType<InventoryViewController>();
			}
		}

		public void DisablePurchase()
		{
			_enabled = false;
			GetComponentInChildren<UIButtonClickEventDispatcher>().enabled = false;
		}

		public void UpdateRemoveCost(int removeOrbCost)
		{
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_removeOrbCost = ChallengeManager.GetHpCostForGold(removeOrbCost);
			}
			else
			{
				_removeOrbCost = removeOrbCost;
			}
			removeOrbCostText.text = _removeOrbCost.ToString();
			ValidateButtonActive();
		}

		private void ValidateButtonActiveOnCurrencyChange(int originalamount, int currencychange, bool silent)
		{
			ValidateButtonActive();
		}

		private void ValidateButtonActive()
		{
			if (!_used)
			{
				bool flag = CurrencyManager.Instance.GoldAmount >= _removeOrbCost;
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
				{
					flag = _playerHealthController.CurrentHealth > (float)_removeOrbCost;
				}
				removeOrbText.color = (flag ? removeOrbTextAffordableColor : removeOrbTextUnaffordableColor);
				removeOrbCostText.color = (flag ? Color.white : Color.red);
				_enabled = flag;
				GetComponentInChildren<UIButtonClickEventDispatcher>().enabled = _enabled;
			}
		}

		private void ShowOrbRemoval()
		{
			if (!_enabled)
			{
				UIAudioManager.InvalidSelectionSfxRequested();
			}
			else
			{
				_inventoryViewController.ShowInventoryForOrbRemovalShop();
			}
		}

		public void OrbRemoved()
		{
			_shopManager.OrbRemoved();
			_used = true;
			_button.image.DOFade(0f, 0.5f).From(1f);
			removeOrbText.DOFade(0f, 0.5f).From(1f);
			removeOrbCostText.DOFade(0f, 0.5f).From(1f);
			goldImage.DOFade(0f, 0.5f).From(1f);
			_button.interactable = false;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_playerHealthController.DealUnblockableDamage(_removeOrbCost);
			}
			else
			{
				CurrencyManager.Instance.RemoveGold(_removeOrbCost);
			}
			if (TryGetComponent<UpdateButtonNavsOnToggle>(out var component))
			{
				component.DoOnDisabledNavUpdate();
			}
		}
	}
}
