using System;
using Saving;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace PeglinUI
{
	public class MainMenuIAPButtonController : MonoBehaviour
	{
		public Button FullGameUnlockButton;

		public Button RestorePurchasesButton;

		public FadeCurtain fadeCurtain;

		public IAPManager iapManager;

		private void OnEnable()
		{
			IAPManager.OnPurchaseFullGameSuccessful = (IAPManager.PurchaseFullGameSuccessful)Delegate.Combine(IAPManager.OnPurchaseFullGameSuccessful, new IAPManager.PurchaseFullGameSuccessful(PurchaseSuccessful));
			IAPManager.OnPurchaseFullGameFailure = (IAPManager.PurchaseFullGameFailure)Delegate.Combine(IAPManager.OnPurchaseFullGameFailure, new IAPManager.PurchaseFullGameFailure(PurchaseFailed));
			if (IAPManager.PlatformRequiresIAPUnlock)
			{
				IAPManager.FullGamePurchased = IAPManager.FullGamePurchased || PersistentPlayerData.Instance.HasPurchasedFullGameUnlock;
			}
			UpdatePurchaseButtonVisible();
		}

		private void OnDisable()
		{
			IAPManager.OnPurchaseFullGameSuccessful = (IAPManager.PurchaseFullGameSuccessful)Delegate.Remove(IAPManager.OnPurchaseFullGameSuccessful, new IAPManager.PurchaseFullGameSuccessful(PurchaseSuccessful));
			IAPManager.OnPurchaseFullGameFailure = (IAPManager.PurchaseFullGameFailure)Delegate.Remove(IAPManager.OnPurchaseFullGameFailure, new IAPManager.PurchaseFullGameFailure(PurchaseFailed));
		}

		private void UpdatePurchaseButtonVisible()
		{
			FullGameUnlockButton.gameObject.SetActive(IAPManager.IsFullGameLocked);
			bool active = false;
			RestorePurchasesButton.gameObject.SetActive(active);
		}

		public void PurchaseSuccessful()
		{
			IAPManager.FullGamePurchased = true;
			UpdatePurchaseButtonVisible();
			PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
			fadeCurtain.FadeOutAndDisableObj();
		}

		public void PurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			if (failureReason == PurchaseFailureReason.DuplicateTransaction)
			{
				IAPManager.FullGamePurchased = true;
				UpdatePurchaseButtonVisible();
			}
			fadeCurtain.FadeOutAndDisableObj();
		}
	}
}
