using System;
using System.Collections.Generic;
using PeglinUI;
using Saving;
using UnityEngine;
using UnityEngine.Purchasing;

public class DemoEndingManager : MonoBehaviour
{
	public GameObject DemoCanvas;

	public Canvas UnlockFullGameCanvas;

	public GameObject ReleaseCanvas;

	public FadeCurtain fadeCurtain;

	private static readonly List<string> _mapNames = new List<string> { "ForestMap", "CastleMap", "MinesMap" };

	private void OnEnable()
	{
		IAPManager.OnPurchaseFullGameSuccessful = (IAPManager.PurchaseFullGameSuccessful)Delegate.Combine(IAPManager.OnPurchaseFullGameSuccessful, new IAPManager.PurchaseFullGameSuccessful(FullGamePurchaseSuccessful));
		IAPManager.OnPurchaseFullGameFailure = (IAPManager.PurchaseFullGameFailure)Delegate.Combine(IAPManager.OnPurchaseFullGameFailure, new IAPManager.PurchaseFullGameFailure(PurchaseFailed));
		IAPManager.OnRefreshPurchaseSuccessful = (IAPManager.PurchaseFullGameSuccessful)Delegate.Combine(IAPManager.OnRefreshPurchaseSuccessful, new IAPManager.PurchaseFullGameSuccessful(RefreshPurchase));
		if (ReleaseDemoManager.ReleaseDemoActive)
		{
			SetUpDemo();
		}
		else if (IAPManager.PlatformRequiresIAPUnlock && !IAPManager.FullGamePurchased)
		{
			SetUpFullGameUnlock();
		}
		else if (ReleaseDemoManager.ReleaseDemoActive)
		{
			SetUpDemo();
		}
		else
		{
			SetUpRelease();
		}
	}

	private void OnDisable()
	{
		IAPManager.OnPurchaseFullGameSuccessful = (IAPManager.PurchaseFullGameSuccessful)Delegate.Remove(IAPManager.OnPurchaseFullGameSuccessful, new IAPManager.PurchaseFullGameSuccessful(FullGamePurchaseSuccessful));
		IAPManager.OnPurchaseFullGameFailure = (IAPManager.PurchaseFullGameFailure)Delegate.Remove(IAPManager.OnPurchaseFullGameFailure, new IAPManager.PurchaseFullGameFailure(PurchaseFailed));
	}

	private void SetUpDemo()
	{
		DemoCanvas.SetActive(value: true);
		UnlockFullGameCanvas.gameObject.SetActive(value: false);
		ReleaseCanvas.SetActive(value: false);
		SaveManager.RequestDelete();
	}

	private void SetUpFullGameUnlock()
	{
		DemoCanvas?.SetActive(value: false);
		UnlockFullGameCanvas.gameObject.SetActive(value: true);
		ReleaseCanvas.SetActive(value: false);
	}

	private void SetUpRelease()
	{
		if (DemoCanvas != null)
		{
			DemoCanvas.SetActive(value: false);
		}
		if (UnlockFullGameCanvas != null)
		{
			UnlockFullGameCanvas.gameObject.SetActive(value: false);
		}
		if (ReleaseCanvas != null)
		{
			ReleaseCanvas.SetActive(value: true);
		}
	}

	public void FullGamePurchaseSuccessful()
	{
		IAPManager.FullGamePurchased = true;
		SetUpRelease();
		PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
		fadeCurtain.FadeOutAndDisableObj();
	}

	public void PurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		if (failureReason == PurchaseFailureReason.DuplicateTransaction)
		{
			IAPManager.FullGamePurchased = true;
			SetUpRelease();
			PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
		}
		fadeCurtain.FadeOutAndDisableObj();
	}

	private void RefreshPurchase()
	{
		IAPManager.FullGamePurchased = true;
		SetUpRelease();
		PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
		fadeCurtain.FadeOutAndDisableObj();
	}
}
