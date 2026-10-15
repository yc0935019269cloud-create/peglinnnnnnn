using System;
using System.Collections;
using System.Globalization;
using Battle;
using Challenges;
using DG.Tweening;
using PeglinUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Currency
{
	public class SimpleCostText : MonoBehaviour
	{
		private int goldAmount;

		private TMP_Text goldText;

		private Color _defaultColor;

		public Color _unaffordableColor = Color.red;

		private Vector3 _initialParentPos;

		private PlayerHealthController _playerHealthController;

		public bool alwaysForceGold;

		private void Awake()
		{
			goldText = GetComponent<TMP_Text>();
			_defaultColor = goldText.color;
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_playerHealthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
			}
		}

		private void OnEnable()
		{
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(HandleCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(HandleCurrencyChange));
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(PlayerHealthChanged));
				PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(PlayerHealthChanged));
			}
			CheckForAffordability();
		}

		private void OnDisable()
		{
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(HandleCurrencyChange));
			CurrencyManager.OnGoldRemoved = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldRemoved, new CurrencyManager.CurrencyEvent(HandleCurrencyChange));
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(PlayerHealthChanged));
				PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(PlayerHealthChanged));
			}
		}

		private IEnumerator Start()
		{
			yield return null;
		}

		private void HandleCurrencyChange(int originalAmount, int currencyChange, bool silent)
		{
			CheckForAffordability();
		}

		private void PlayerHealthChanged(float healAmount)
		{
			CheckForAffordability();
		}

		public void SetGold(int newGoldText, bool forceGold = false)
		{
			goldAmount = newGoldText;
			if (goldAmount == 0)
			{
				base.transform.parent.gameObject.SetActive(value: false);
				return;
			}
			base.transform.parent.gameObject.SetActive(value: true);
			goldText.text = goldAmount.ToString(CultureInfo.InvariantCulture);
			CheckForAffordability(forceGold);
		}

		public void SetMaxHPLoss(int maxHpLossAmount)
		{
			base.transform.parent.gameObject.SetActive(value: true);
			GetComponentInParent<HorizontalLayoutGroup>().spacing = 17f;
			goldText.text = $"-{maxHpLossAmount} Max HP";
			bool flag = (float)maxHpLossAmount < _playerHealthController.MaxHealth;
			goldText.color = (flag ? _defaultColor : _unaffordableColor);
		}

		private void CheckForAffordability(bool forceGold = false)
		{
			bool flag = ((forceGold || alwaysForceGold || !ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE) || !(_playerHealthController != null)) ? (CurrencyManager.Instance.GoldAmount >= goldAmount) : (_playerHealthController.CurrentHealth > (float)goldAmount));
			goldText.color = (flag ? _defaultColor : _unaffordableColor);
		}

		public void AttemptUnaffordablePurchase()
		{
			Transform parent = base.transform.parent;
			parent.DOKill();
			if (_initialParentPos == Vector3.zero)
			{
				_initialParentPos = parent.position;
			}
			else
			{
				parent.position = _initialParentPos;
			}
			parent.DOPunchPosition(new Vector3(parent.GetComponent<RectTransform>().rect.width / 10f, 0f, 0f), 0.08f);
			UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
		}
	}
}
