using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.StatusEffects;
using Challenges;
using Cruciball;
using Currency;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using Map;
using Peglin.Achievements;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUI.LoadoutManager;
using PeglinUI.PostBattle;
using Relics;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenarios.Shop
{
	public class ShopManager : MonoBehaviour
	{
		public delegate void ShopEvent();

		public static ShopEvent OnShopEvent = () =>
		{
		};

		public static bool isOpen = false;

		[Header("Scene Refs")]
		[SerializeField]
		private LoadoutManager _loadoutManager;

		public GameObject relicContainer;

		public GameObject orbContainer;

		public Image fadeCurtain;

		public ShopUIMove shopUI;

		public GameObject pegLayout;

		public RemoveOrbButton removeOrbButton;

		public GameObject debugPegboardFrame;

		public NavOnlyController navController;

		public Image navCurtain;

		public PlayfieldMouseDetector playfieldMouseDetector;

		public Button exitStoreButton;

		public UpgradeConfirmationOrbInspect upgradeConfirmationOrbInspect;

		public UpgradeConfirmationRelic upgradeConfirmationRelic;

		[Header("Shop Parameters")]
		public int orbsOffered = 5;

		public int relicsOffered = 4;

		private IPurchasableItem[] _purchasableOrbs;

		private IPurchasableItem[] _purchasableRelics;

		private bool _orbRemoved;

		[Range(0f, 1f)]
		public float percentChanceOfRareRelic = 0.05f;

		public int removeOrbCostBase = 50;

		public int removeOrbCostIncrease = 10;

		public float introFadeTime = 0.6f;

		public float cameraNavY = 2f;

		public float cameraToNavMoveTime = 1f;

		[Header("Prefabs")]
		public GameObject purchasablePrefab;

		[Header("ScriptableObject Refs")]
		public RelicManager relicManager;

		public DeckManager deckManager;

		public CruciballManager cruciballManager;

		private PredictionManager _predictionManager;

		private PlayerHealthController _playerHealthController;

		private MapDataShop _activeMapData;

		private bool _isControllerActive;

		private bool _storeClosed;

		private Selectable _firstSelectable;

		private RewiredEventSystem _rewiredEventSystem;

		private List<ShopItem> orbItems = new List<ShopItem>();

		private List<ShopItem> relicItems = new List<ShopItem>();

		private bool inventoryOpen;

		private bool shopItemOpen;

		private SeededShopNodeData _seededShopNodeData;

		private GameObject _pegboardFrame;

		private bool _isPriceFixingRelicActive;

		private int RemoveOrbCost => removeOrbCostBase + ((!_isPriceFixingRelicActive) ? (StaticGameData.StoreNumberOfOrbsRemoved * removeOrbCostIncrease) : 0) + (cruciballManager.ShouldIncreaseShopPrices() ? 10 : 0);

		private void Awake()
		{
			shopUI.gameObject.SetActive(value: false);
			fadeCurtain.gameObject.SetActive(value: true);
			_seededShopNodeData = StaticGameData.seededNodeData as SeededShopNodeData;
			if (StaticGameData.dataToLoad != null)
			{
				_activeMapData = StaticGameData.dataToLoad as MapDataShop;
				if (_activeMapData != null && _activeMapData.background != null)
				{
					UnityEngine.Object.Instantiate(_activeMapData.background);
				}
			}
			_playerHealthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
			_purchasableOrbs = new IPurchasableItem[orbsOffered];
			_purchasableRelics = new IPurchasableItem[relicsOffered];
			if (_activeMapData != null)
			{
				_pegboardFrame = UnityEngine.Object.Instantiate((_activeMapData.pegboardFrame != null) ? _activeMapData.pegboardFrame : debugPegboardFrame);
			}
			else
			{
				_pegboardFrame = debugPegboardFrame;
			}
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Combine(InventoryViewController.onInventoryViewToggled, (InventoryViewController.InventoryViewToggle)((bool value) =>
			{
				inventoryOpen = value;
			}));
			ShopItem.OnShopItemViewToggle = (InventoryViewController.InventoryViewToggle)Delegate.Combine(ShopItem.OnShopItemViewToggle, (InventoryViewController.InventoryViewToggle)((bool value) =>
			{
				shopItemOpen = value;
			}));
		}

		private void Update()
		{
			if (!_storeClosed && !inventoryOpen && !shopItemOpen)
			{
				if (!ControllerInputManager.IsPointerBased && !_isControllerActive)
				{
					EventSystem.current.SetSelectedGameObject(_firstSelectable.gameObject);
					_isControllerActive = true;
				}
				else if (ControllerInputManager.IsPointerBased && _isControllerActive)
				{
					EventSystem.current.SetSelectedGameObject(null);
					_isControllerActive = false;
				}
			}
		}

		private void OnEnable()
		{
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = fadeCurtain.DOFade(0f, introFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				isOpen = true;
				shopUI.gameObject.SetActive(value: true);
				shopUI.MoveIn();
				fadeCurtain.enabled = false;
			}));
			_isPriceFixingRelicActive = relicManager != null && relicManager.RelicEffectActive(RelicEffect.PRICE_FIXING);
			int num = (relicManager.WandOfGreedEffectActive() ? (RemoveOrbCost * 2) : RemoveOrbCost);
			if (MapController.instance != null && MapController.instance.Act == 4)
			{
				num *= 2;
			}
			removeOrbButton.Initialize(this, num);
			CreateRandomOrbOffer();
			SetUpRelicOffer();
			SetupNavigation();
			upgradeConfirmationOrbInspect.Init(deckManager, relicManager, cruciballManager);
		}

		public void PurchaseItem(IPurchasableItem item)
		{
			if (_storeClosed)
			{
				return;
			}
			int cost = item.GetCost();
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				cost = ChallengeManager.GetHpCostForGold(cost);
				UnityEngine.Object.FindObjectOfType<PlayerHealthController>()?.DealUnblockableDamage(cost);
			}
			else
			{
				CurrencyManager.Instance.RemoveGold(cost);
			}
			item.PurchaseItem();
			_isPriceFixingRelicActive = relicManager.RelicEffectActive(RelicEffect.PRICE_FIXING);
			int removeOrbCost = (relicManager.WandOfGreedEffectActive() ? (RemoveOrbCost * 2) : RemoveOrbCost);
			removeOrbButton.UpdateRemoveCost(removeOrbCost);
			int num = Array.IndexOf(_purchasableOrbs, item);
			if (num >= 0)
			{
				_purchasableOrbs[num] = null;
			}
			else
			{
				num = Array.IndexOf(_purchasableRelics, item);
				if (num >= 0)
				{
					_purchasableRelics[num] = null;
				}
			}
			if (cruciballManager.ShouldIncreaseShopPrices() && item is PurchasableRelic)
			{
				IPurchasableItem[] purchasableRelics = _purchasableRelics;
				foreach (IPurchasableItem purchasableItem in purchasableRelics)
				{
					if (purchasableItem != null && purchasableItem is PurchasableRelic purchasableRelic)
					{
						if (_isPriceFixingRelicActive)
						{
							purchasableRelic.ResetPrice();
						}
						else
						{
							purchasableRelic.IncreasePrice();
						}
					}
				}
				foreach (ShopItem relicItem in relicItems)
				{
					relicItem.UpdateCostText();
				}
			}
			CheckCleanShopAchievement();
		}

		private void CheckCleanShopAchievement()
		{
			if (cruciballManager.currentCruciballLevel < 10 || !_orbRemoved)
			{
				return;
			}
			for (int i = 0; i < _purchasableOrbs.Length; i++)
			{
				if (_purchasableOrbs[i] != null)
				{
					return;
				}
			}
			for (int j = 0; j < _purchasableRelics.Length; j++)
			{
				if (_purchasableRelics[j] != null)
				{
					return;
				}
			}
			AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.CLEAN_SHOP);
		}

		public void OrbRemoved()
		{
			_orbRemoved = true;
			StaticGameData.StoreNumberOfOrbsRemoved++;
			CheckCleanShopAchievement();
		}

		private void SetUpRelicOffer()
		{
			Relic[] array;
			if (_seededShopNodeData != null)
			{
				List<Relic> list = new List<Relic>();
				List<RelicRarity> list2 = new List<RelicRarity>();
				int num = 5;
				if (_seededShopNodeData.rareRelicChanceRoll <= percentChanceOfRareRelic)
				{
					list.Add(relicManager.GetRelicOfRarityOffOfQueue(RelicRarity.RARE));
					list2.Add(RelicRarity.RARE);
					num--;
				}
				Relic relic = null;
				Relic[] extraRelicsToNotAllowInList = null;
				if (_seededShopNodeData.shopRelicChanceRoll <= relicManager.GetShopRelicChanceForCurrentClass())
				{
					relic = relicManager.GetShopOnlyRelicOffOfQueue();
					if (relic != null)
					{
						extraRelicsToNotAllowInList = new Relic[1] { relic };
						num--;
					}
				}
				list.AddRange(relicManager.GetMultipleRelicsOffOfQueue(num, RelicRarity.COMMON));
				for (int i = 0; i < num; i++)
				{
					list2.Add(RelicRarity.COMMON);
				}
				array = relicManager.CheckAndUpdateSeededRelicOffers(list.ToArray(), list2.ToArray(), extraRelicsToNotAllowInList);
				if (relic != null)
				{
					array = new List<Relic>(array) { relic }.ToArray();
				}
			}
			else
			{
				Debug.LogWarning("No seeded data for shop relic offer.");
				array = relicManager.GetMultipleCommonRelicsWithRareChance(relicsOffered, percentChanceOfRareRelic);
			}
			if (array.Length == 0)
			{
				return;
			}
			int num2 = 0;
			Relic[] array2 = array;
			foreach (Relic relic2 in array2)
			{
				if (!(relic2 == null))
				{
					ShopItem component = UnityEngine.Object.Instantiate(purchasablePrefab, relicContainer.transform).GetComponent<ShopItem>();
					int num3 = ((!relicManager.WandOfGreedEffectActive()) ? 1 : 2);
					if (MapController.instance != null && MapController.instance.Act == 4)
					{
						num3 *= 2;
					}
					PurchasableRelic purchasableRelic = new PurchasableRelic(relic2, relicManager, num3);
					component.Initialize(purchasableRelic, this, _playerHealthController, relicManager);
					component.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
					_purchasableRelics[num2++] = purchasableRelic;
					relicItems.Add(component);
				}
			}
		}

		private void CreateRandomOrbOffer()
		{
			GameObject[] array;
			if (_seededShopNodeData != null)
			{
				array = deckManager.CheckAndUpdateSeededOrbOffers(_seededShopNodeData.shopOrbs, allowDuplicates: true);
			}
			else
			{
				List<GameObject> list = new List<GameObject>();
				for (int i = 0; i < orbsOffered; i++)
				{
					List<GameObject> randomOrbPool = GetRandomOrbPool();
					list.Add(randomOrbPool[UnityEngine.Random.Range(0, randomOrbPool.Count)]);
				}
				array = list.ToArray();
			}
			for (int j = 0; j < orbsOffered; j++)
			{
				ShopItem component = UnityEngine.Object.Instantiate(purchasablePrefab, orbContainer.transform).GetComponent<ShopItem>();
				int num = ((!relicManager.WandOfGreedEffectActive()) ? 1 : 2);
				if (MapController.instance != null && MapController.instance.Act == 4)
				{
					num *= 2;
				}
				PurchasableOrb purchasableOrb = new PurchasableOrb(array[j], deckManager, num, relicManager, cruciballManager.ShouldIncreaseShopPrices());
				component.Initialize(purchasableOrb, this, _playerHealthController, relicManager);
				if (j == 0)
				{
					_firstSelectable = component.GetComponentInChildren<Selectable>(includeInactive: true);
				}
				component.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
				_purchasableOrbs[j] = purchasableOrb;
				orbItems.Add(component);
			}
		}

		private void SetupNavigation()
		{
			for (int i = 0; i < orbItems.Count; i++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (i > 0)
				{
					navigation.selectOnLeft = orbItems[i - 1].selectable;
				}
				if (orbItems.Count > i + 1)
				{
					navigation.selectOnRight = orbItems[i + 1].selectable;
				}
				else
				{
					navigation.selectOnRight = exitStoreButton;
				}
				int index = Mathf.Clamp(i, 0, relicItems.Count - 1);
				navigation.selectOnDown = relicItems[index].selectable;
				orbItems[i].selectable.navigation = navigation;
			}
			for (int j = 0; j < relicItems.Count; j++)
			{
				Navigation navigation2 = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (j > 0)
				{
					navigation2.selectOnLeft = relicItems[j - 1].selectable;
				}
				if (relicItems.Count > j + 1)
				{
					navigation2.selectOnRight = relicItems[j + 1].selectable;
				}
				else
				{
					navigation2.selectOnRight = exitStoreButton;
				}
				int index2 = Mathf.Clamp(j, 0, orbItems.Count - 1);
				navigation2.selectOnUp = orbItems[index2].selectable;
				navigation2.selectOnDown = removeOrbButton.selectable;
				relicItems[j].selectable.navigation = navigation2;
			}
			Navigation navigation3 = new Navigation
			{
				mode = Navigation.Mode.Explicit
			};
			navigation3.selectOnRight = exitStoreButton;
			if (relicItems.Count > 0)
			{
				navigation3.selectOnLeft = relicItems[0].selectable;
				if (relicItems.Count >= 3)
				{
					navigation3.selectOnUp = relicItems[2].selectable;
				}
				else
				{
					navigation3.selectOnUp = relicItems[relicItems.Count - 1].selectable;
				}
			}
			else if (orbItems.Count > 0)
			{
				navigation3.selectOnLeft = orbItems[0].selectable;
				if (orbItems.Count >= 3)
				{
					navigation3.selectOnUp = orbItems[2].selectable;
				}
				else
				{
					navigation3.selectOnUp = orbItems[0].selectable;
				}
			}
			removeOrbButton.selectable.navigation = navigation3;
			UpdateButtonNavsOnToggle updateButtonNavsOnToggle = removeOrbButton.gameObject.AddComponent<UpdateButtonNavsOnToggle>();
			List<ButtonNavigationUpdateData> list = new List<ButtonNavigationUpdateData>();
			foreach (ShopItem relicItem in relicItems)
			{
				if (relicItem.selectable.navigation.selectOnDown == removeOrbButton.selectable)
				{
					ButtonNavigationUpdateData item = new ButtonNavigationUpdateData(relicItem.button, null, relicItem.selectable);
					list.Add(item);
				}
			}
			updateButtonNavsOnToggle.allButtonsToUpdateOnDiable = list.ToArray();
			Navigation navigation4 = new Navigation
			{
				mode = Navigation.Mode.Explicit
			};
			if (relicItems.Count > 0)
			{
				navigation4.selectOnLeft = relicItems[relicItems.Count - 1].selectable;
			}
			else if (orbItems.Count > 0)
			{
				navigation4.selectOnLeft = orbItems[orbItems.Count - 1].selectable;
			}
			if (orbItems.Count > 0)
			{
				navigation4.selectOnUp = orbItems[orbItems.Count - 1].selectable;
			}
			else if (relicItems.Count > 0)
			{
				navigation4.selectOnUp = relicItems[relicItems.Count - 1].selectable;
			}
			exitStoreButton.navigation = navigation4;
		}

		public void CloseStore()
		{
			_predictionManager.Initialize(pegLayout, _pegboardFrame, relicManager, navigation: true);
			navController.Initialize();
			OnShopEvent?.Invoke();
			StaticGameData.dataToLoad = null;
			_storeClosed = true;
			removeOrbButton.DisablePurchase();
			exitStoreButton.interactable = false;
			EventSystem.current.SetSelectedGameObject(null);
			SpawnBlocksToMatchShopItems component = pegLayout.GetComponent<SpawnBlocksToMatchShopItems>();
			if ((bool)component)
			{
				component.SpawnOrbBlocks(_purchasableOrbs);
				component.SpawnRelicBlocks(_purchasableRelics);
				component.ToggleRemoveOrbRow(_orbRemoved);
			}
			StartCoroutine(UpdatePrediction());
			_playerHealthController.GetComponent<PlayerStatusEffectController>().UpdatePlayerBlindness();
			PauseMenu.PauseBlock = true;
			Sequence sequence = DOTween.Sequence();
			sequence.Append(shopUI.MoveBack());
			sequence.AppendCallback(() =>
			{
				shopUI.gameObject.SetActive(value: false);
				isOpen = false;
			});
			if (StaticGameData.currentNode != null || StaticGameData.currentNode.ChildNodes.Length > 1)
			{
				sequence.Append(Camera.main.transform.DOMoveY(cameraNavY, cameraToNavMoveTime));
			}
			Sequence sequence2 = sequence.AppendInterval(0.1f);
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(StartNavigation));
			sequence.Play();
		}

		public void SetShopItemsInteractable(bool value)
		{
			foreach (ShopItem orbItem in orbItems)
			{
				orbItem.SetInteractable(!value);
				orbItem.enabled = !value;
			}
			foreach (ShopItem relicItem in relicItems)
			{
				relicItem.SetInteractable(!value);
				relicItem.enabled = !value;
			}
		}

		private IEnumerator UpdatePrediction()
		{
			yield return null;
			_predictionManager.CopyAllPegs();
		}

		private void LoadSelectedScene()
		{
			PauseMenu.PauseBlock = false;
			navController.GetComponent<NavOnlyController>().LoadSelectedScene();
		}

		private void StartNavigation()
		{
			if (StaticGameData.currentNode == null || StaticGameData.currentNode.ChildNodes.Length == 1)
			{
				StaticGameData.chosenNextNodeIndex = 0;
				navController.gameObject.SetActive(value: true);
				fadeCurtain.enabled = true;
				PauseMenu.PauseBlock = true;
				TweenerCore<Color, Color, ColorOptions> tweenerCore = fadeCurtain.DOFade(1.2f, introFadeTime).From(0f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadSelectedScene));
			}
			else
			{
				PauseMenu.PauseBlock = false;
				navController.gameObject.SetActive(value: true);
				navController.PrepareForNavigation();
				TweenerCore<Color, Color, ColorOptions> tweenerCore2 = navCurtain.DOFade(0f, introFadeTime);
				tweenerCore2.onComplete = (TweenCallback)Delegate.Combine(tweenerCore2.onComplete, new TweenCallback(DisableCurtain));
			}
		}

		private void DisableCurtain()
		{
			playfieldMouseDetector.gameObject.SetActive(value: true);
			navCurtain.enabled = false;
		}

		private List<GameObject> GetRandomOrbPool()
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.6f)
			{
				return deckManager.CommonOrbPool;
			}
			if (value <= 0.9f)
			{
				return deckManager.UncommonOrbPool;
			}
			return deckManager.RareOrbPool;
		}
	}
}
