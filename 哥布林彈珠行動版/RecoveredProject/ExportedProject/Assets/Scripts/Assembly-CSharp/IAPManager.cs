using System;
using System.Threading.Tasks;
using Steamworks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener, IStoreListener
{
	public delegate void PurchaseFullGameSuccessful();

	public delegate void PurchaseFullGameFailure(Product product, PurchaseFailureReason failureReason);

	public static PurchaseFullGameSuccessful OnPurchaseFullGameSuccessful;

	public static PurchaseFullGameSuccessful OnRefreshPurchaseSuccessful;

	public static PurchaseFullGameFailure OnPurchaseFullGameFailure;

	public static bool PlatformRequiresIAPUnlock = false;

	public static bool FullGamePurchased = false;

	public static bool IsSupporterDLCInstalled = false;

	private static IStoreController m_StoreController;

	private static IExtensionProvider m_StoreExtensionProvider;

	private static Product test_product = null;

	private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

	public static string PEGLIN_FULL_GAME = "peglin_full_game";

	private bool return_complete = true;

	private const string k_Environment = "production";

	public static bool IsFullGameLocked
	{
		get
		{
			if (PlatformRequiresIAPUnlock)
			{
				return !FullGamePurchased;
			}
			return false;
		}
	}

	private event Action<int> DebugLogAction;

	private void Awake()
	{
		PlatformRequiresIAPUnlock = Application.isMobilePlatform || Application.platform == RuntimePlatform.WebGLPlayer;
		InitializeGamingServices(OnInitializeGamingServicesSuccess, OnInitializeGamingServicesError);
		InitializePurchasing();
	}

	private void Start()
	{
		try
		{
			IsSupporterDLCInstalled = SteamApps.BIsDlcInstalled(new AppId_t(4528840u));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Unable to access SteamWorks DLC information: " + ex.Message);
		}
		DebugLogAction += DebugLogFunction;
	}

	private void InitializeGamingServices(Action onSuccess, Action<string> onError)
	{
		try
		{
			UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production")).ContinueWith((Task task) =>
			{
				onSuccess();
			});
		}
		catch (Exception ex)
		{
			onError(ex.Message);
		}
	}

	private void OnInitializeGamingServicesSuccess()
	{
		Debug.Log("Unity Gaming Services has been successfully initialized.");
	}

	private void OnInitializeGamingServicesError(string message)
	{
		Debug.LogError("Unity Gaming Services failed to initialize with error: " + message + ".");
	}

	public void InitializePurchasing()
	{
		if (!IsInitialized() && PlatformRequiresIAPUnlock)
		{
			ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			configurationBuilder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);
			configurationBuilder.Configure<IGooglePlayConfiguration>().SetQueryProductDetailsFailedListener(DebugLogAction);
			configurationBuilder.AddProduct(PEGLIN_FULL_GAME, ProductType.NonConsumable);
			UnityPurchasing.Initialize(this, configurationBuilder);
		}
	}

	private void DebugLogFunction(int myInt)
	{
		Debug.Log("Listener = " + myInt);
	}

	public bool IsInitialized()
	{
		if (m_StoreController != null)
		{
			return m_StoreExtensionProvider != null;
		}
		return false;
	}

	private void OnDeferredPurchase(Product product)
	{
		Debug.Log("Purchase of " + product.definition.id + " is deferred");
	}

	public void BuyFullGame()
	{
		BuyProductID(PEGLIN_FULL_GAME);
	}

	public bool HasProductBeenPurchased(string productId)
	{
		if (!IsInitialized())
		{
			return false;
		}
		return m_StoreController.products.WithID(productId).hasReceipt;
	}

	public void CompletePurchase()
	{
		if (test_product == null)
		{
			Debug.Log("Cannot complete purchase, product not initialized.");
			return;
		}
		m_StoreController.ConfirmPendingPurchase(test_product);
		Debug.Log("Completed purchase with " + test_product.transactionID.ToString());
	}

	public void ToggleComplete()
	{
		return_complete = !return_complete;
		Debug.Log("Complete = " + return_complete);
	}

	public void RestorePurchases()
	{
		m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((bool result, string str) =>
		{
			if (result)
			{
				Debug.Log("Restore purchases succeeded.");
			}
			else
			{
				Debug.Log("Restore purchases failed, reason " + str);
			}
		});
	}

	private void BuyProductID(string productId)
	{
		if (IsInitialized())
		{
			Product product = m_StoreController.products.WithID(productId);
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product:" + product.definition.id.ToString()));
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.LogError("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		else
		{
			Debug.LogError("BuyProductID FAIL. Not initialized.");
		}
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		m_StoreController = controller;
		m_StoreExtensionProvider = extensions;
		m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
		Debug.Log("IAPManager: OnInitialized Called");
		if (!FullGamePurchased && HasProductBeenPurchased(PEGLIN_FULL_GAME))
		{
			OnRefreshPurchaseSuccessful?.Invoke();
		}
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		Debug.LogError("IAP IStoreListener OnInitializeFailed InitializationFailureReason:" + error);
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
	{
		Debug.LogError("IAP IStoreListener OnInitializeFailed InitializationFailureReason:" + error.ToString() + " " + message);
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		test_product = args.purchasedProduct;
		if (m_GooglePlayStoreExtensions.IsPurchasedProductDeferred(test_product))
		{
			return PurchaseProcessingResult.Pending;
		}
		if (return_complete)
		{
			OnPurchaseFullGameSuccessful?.Invoke();
			return PurchaseProcessingResult.Complete;
		}
		return PurchaseProcessingResult.Pending;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		OnPurchaseFullGameFailure?.Invoke(product, failureReason);
	}

	void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
	{
		OnPurchaseFullGameFailure?.Invoke(product, failureDescription.reason);
	}

	public void ListPurchases()
	{
		Product[] all = m_StoreController.products.all;
		foreach (Product product in all)
		{
			if (product.hasReceipt)
			{
				Debug.Log("In list for  " + product.receipt.ToString());
			}
			else
			{
				Debug.Log("No receipt for " + product.definition.id.ToString());
			}
		}
	}
}
