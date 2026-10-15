using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Battle;
using Battle.Attacks;
using Battle.StatusEffects;
using Challenges;
using Coffee.UIEffects;
using Cruciball;
using Currency;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using I2.Loc;
using Map;
using PeglinUI.ControllerSupport;
using PeglinUtils;
using Relics;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Worldmap;

namespace PeglinUI.PostBattle
{
	public class BattleUpgradeCanvas : MonoBehaviour
	{
		public enum State
		{
			MAIN = 0,
			RELIC = 1,
			CHALLENGE = 2
		}

		public delegate void BattleUpgradeOver();

		private const int DEFAULT_ADD_COST = 25;

		private const int DEFAULT_HEAL_COST = 25;

		private const int DEFAULT_UPGRADE_COST = 25;

		private const int ADD_ORB_PRICE_INCREASE = 5;

		private const int UPGRADE_ORB_PRICE_INCREASE = 10;

		private const int SKIP_BOSS_RELIC_GOLD_ADD = 50;

		private const int BASALT_TOADEM_COST = 20;

		public static BattleUpgradeOver OnBattleUpgradeOver = () =>
		{
		};

		public static bool isOpen = false;

		[Header("External Panels")]
		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UpgradeConfirmationRelic _upgradeConfirmationRelic;

		[SerializeField]
		private UpgradeConfirmationOrbInspect _upgradeConfirmationOrbInspect;

		[SerializeField]
		private UpgradeConfirmationOrbUpgrade _upgradeConfirmationOrbUpgrade;

		[SerializeField]
		private UpgradeConfirmationChallenge _upgradeConfirmationChallenge;

		[SerializeField]
		private RectTransform _panelToScale;

		[SerializeField]
		private GameObject _ballInventoryPanel;

		[SerializeField]
		private GameObject _mainOptionsPanel;

		[SerializeField]
		private GameObject _relicPanel;

		[SerializeField]
		private GameObject _challengePanel;

		[SerializeField]
		private Canvas _playerHealthCanvas;

		[Header("Panel Elements")]
		[SerializeField]
		private PopulateSuggestionOrbs _suggestionOrbs;

		[SerializeField]
		private Button _upgradeButton;

		[SerializeField]
		private Button _healButton;

		[SerializeField]
		private Sprite _healButtonEmptyImage;

		[SerializeField]
		private Button _maxHPButton;

		[SerializeField]
		private Button _skipButton;

		[SerializeField]
		private GameObject _relicSkipButtonGoldContainer;

		[Header("Managers")]
		private DeckManager _deckManager;

		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private RelicManager _relicManager;

		private PlayerHealthController _playerHealthController;

		[Header("Variables")]
		[SerializeField]
		private float _minPanelWidth = 42f;

		private GameObject _stagedUpgrade;

		private State _state;

		private int _currentHealCost = 25;

		private int _currentAddOrbCost = 25;

		private int _currentUpgradeOrbCost = 25;

		private int _currentBasaltToademCost = 20;

		private bool _hasHealed;

		private bool _hasIncreasedMaxHP;

		private Color _upgradeButtonDefaultColor;

		private RelicRarity _relicGrantRarity;

		[HideInInspector]
		public bool afterMinesBoss;

		private SeededBattleNodeData _seededBattleData;

		private void OnEnable()
		{
			isOpen = true;
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
			PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
			PlayerHealthController.OnPlayerMaxHealthChanged = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerMaxHealthChanged, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
			_upgradeConfirmationOrbInspect.Init(_deckManager, _relicManager, _cruciballManager);
			if (afterMinesBoss && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_playerHealthCanvas.sortingLayerName = "MapBase";
			}
		}

		private void OnDisable()
		{
			isOpen = false;
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			PlayerHealthController.OnPlayerDamaged = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerDamaged, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
			PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
			PlayerHealthController.OnPlayerMaxHealthChanged = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerMaxHealthChanged, new PlayerHealthController.PlayerHealthEvent(OnPlayerHealthChanged));
		}

		private void Start()
		{
			_upgradeButtonDefaultColor = _upgradeButton.GetComponent<Image>().color;
			if (_state == State.MAIN)
			{
				SetUpPostBattleOptions();
			}
			if (_state == State.RELIC)
			{
				RelicButtonHandleInput componentInChildren = GetComponentInChildren<RelicButtonHandleInput>();
				if (componentInChildren != null && componentInChildren.selectionArrow != null)
				{
					componentInChildren.selectionArrow.gameObject.SetActive(value: false);
				}
			}
			if (_panelToScale != null)
			{
				Vector2 sizeDelta = _panelToScale.sizeDelta;
				sizeDelta.x = _minPanelWidth;
				TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = _panelToScale.DOSizeDelta(_panelToScale.sizeDelta, 0.65f).From(sizeDelta).SetUpdate(isIndependentUpdate: true);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(FinishIntroLerp));
			}
			else
			{
				FinishIntroLerp();
			}
			GetComponent<Canvas>().worldCamera = Camera.main;
		}

		private void FinishIntroLerp()
		{
			if (_state != State.RELIC)
			{
				return;
			}
			RelicButtonHandleInput[] componentsInChildren = GetComponentsInChildren<RelicButtonHandleInput>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				componentsInChildren[0].selectionArrow.gameObject.SetActive(value: true);
				RelicButtonHandleInput[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].StartFixAlignmentRoutine();
				}
			}
		}

		public void ConfigureForPostBattleRewards(DeckManager deckManager, PlayerHealthController playerHealthController, SeededBattleNodeData seededBattleData)
		{
			_deckManager = deckManager;
			_playerHealthController = playerHealthController;
			_seededBattleData = seededBattleData;
		}

		public void ConfigureForRelicGrantOnly(RelicRarity rarity)
		{
			SetupRelicGrant(rarity, isTreasure: true);
		}

		public void SetUpPostBattleOptions()
		{
			if (_deckManager == null)
			{
				Debug.LogWarning("BattleUpgradeCanvas::OnEnable(): No DeckManager set. Did you leave this enabled when it shouldn't be?");
				base.gameObject.SetActive(value: false);
				return;
			}
			if (_playerHealthController == null)
			{
				Debug.LogError("BattleUpgradeCanvas::OnEnable(): No PlayerHealthController set");
				return;
			}
			if (ReleaseDemoManager.ReleaseDemoActive && StaticGameData.currentNode != null && StaticGameData.currentNode.RoomType == RoomType.BOSS)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			PlayerHealthController playerHealthController = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealthController>();
			if (playerHealthController != null)
			{
				if (_relicManager.AttemptUseRelic(RelicEffect.HEAL_WITH_BALLWARK))
				{
					PlayerStatusEffectController componentInChildren = playerHealthController.GetComponentInChildren<PlayerStatusEffectController>();
					if (componentInChildren != null && componentInChildren.HasEffect(StatusEffectType.Ballwark))
					{
						playerHealthController.AdjustMaxHealth(2f);
					}
				}
				if (StaticGameData.currentNode != null && StaticGameData.currentNode.RoomType == RoomType.BOSS)
				{
					if (_cruciballManager.LessHealingFromBosses())
					{
						playerHealthController.Heal(Mathf.Ceil(playerHealthController.MaxHealth / 2f));
					}
					else
					{
						playerHealthController.HealToFull();
					}
				}
			}
			else
			{
				Debug.LogError("PlayerHealthController reference missing after battle!");
			}
			if (_relicManager.WandOfGreedEffectActive())
			{
				_currentHealCost *= 2;
				_currentAddOrbCost *= 2;
				_currentUpgradeOrbCost *= 2;
				_currentBasaltToademCost *= 2;
			}
			else if (_relicManager.RelicEffectActive(RelicEffect.DOUBLE_COINS_AND_PRICES) && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_currentHealCost *= 2;
				_currentBasaltToademCost *= 2;
			}
			_suggestionOrbs.Initialize(_currentAddOrbCost, _seededBattleData);
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_upgradeButton.GetComponentInChildren<SimpleCostText>().SetGold(ChallengeManager.GetHpCostForGold(_currentUpgradeOrbCost));
			}
			else
			{
				_upgradeButton.GetComponentInChildren<SimpleCostText>().SetGold(_currentUpgradeOrbCost);
			}
			CheckForUpgradeButtonEnabled();
			_skipButton.gameObject.SetActive(value: true);
			_maxHPButton.transform.parent.gameObject.SetActive(_relicManager.RelicEffectActive(RelicEffect.INC_MAX_HP_IF_FULL_HP));
			_maxHPButton.GetComponentInChildren<SimpleCostText>(includeInactive: true).SetGold(_currentBasaltToademCost, forceGold: true);
			OnPlayerHealthChanged();
		}

		public void ShowInventoryForUpgrade()
		{
			if (_deckManager.GetUpgradeableOrbs().Count <= 0)
			{
				UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
				return;
			}
			if (!ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE) && !CurrencyManager.Instance.CanAfford(_currentUpgradeOrbCost))
			{
				_upgradeButton.GetComponentInChildren<SimpleCostText>()?.AttemptUnaffordablePurchase();
				return;
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE) && _playerHealthController.CurrentHealth <= (float)ChallengeManager.GetHpCostForGold(_currentUpgradeOrbCost))
			{
				_upgradeButton.GetComponentInChildren<SimpleCostText>()?.AttemptUnaffordablePurchase();
				return;
			}
			_mainOptionsPanel.SetActive(value: false);
			_ballInventoryPanel.GetComponentInChildren<InventoryGridPopulator>(includeInactive: true).onlyShowUpgrades = true;
			_ballInventoryPanel.SetActive(value: true);
		}

		public void BackFromInventory()
		{
			if (!ControllerInspection.CurrentlyInspecting && !PauseMenu.Paused)
			{
				_mainOptionsPanel.gameObject.SetActive(value: true);
				_ballInventoryPanel.SetActive(value: false);
			}
		}

		private void CloseAdditionalWindows()
		{
			_ballInventoryPanel.SetActive(value: false);
		}

		private void HandleUpgradeOptionClicked(UpgradeOption.UpgradeType type, GameObject clickedButton = null, GameObject chosen = null)
		{
			if (PauseMenu.Paused || InventoryViewController.isOpen)
			{
				return;
			}
			switch (type)
			{
			case UpgradeOption.UpgradeType.HEALTH_HEAL:
			{
				if (!CurrencyManager.Instance.CanAfford(_currentHealCost))
				{
					_healButton.GetComponentInChildren<SimpleCostText>()?.AttemptUnaffordablePurchase();
					break;
				}
				if (_hasHealed)
				{
					UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
					break;
				}
				if (_playerHealthController.CurrentHealth >= _playerHealthController.MaxHealth)
				{
					UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
					break;
				}
				_hasHealed = true;
				_playerHealthController.HealEndOfBattleAmount();
				CurrencyManager.Instance.RemoveGold(_currentHealCost);
				CloseAdditionalWindows();
				float duration = 0.35f;
				_healButton.GetComponent<Button>().transition = Selectable.Transition.None;
				_healButton.GetComponent<Image>().sprite = _healButtonEmptyImage;
				_healButton.GetComponentInChildren<CanvasGroup>().DOFade(0f, duration);
				break;
			}
			case UpgradeOption.UpgradeType.MAX_HP_INC:
			{
				if (!CurrencyManager.Instance.CanAfford(_currentBasaltToademCost))
				{
					_maxHPButton.GetComponentInChildren<SimpleCostText>()?.AttemptUnaffordablePurchase();
					break;
				}
				if (_hasIncreasedMaxHP)
				{
					UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
					break;
				}
				_hasIncreasedMaxHP = true;
				_relicManager.AttemptUseRelic(RelicEffect.INC_MAX_HP_IF_FULL_HP);
				_playerHealthController.AdjustMaxHealth(5f);
				CurrencyManager.Instance.RemoveGold(_currentBasaltToademCost);
				CloseAdditionalWindows();
				_maxHPButton.GetComponent<Button>().transition = Selectable.Transition.None;
				float duration = 0.35f;
				UIEffect buttonUIEffect = _maxHPButton.GetComponent<UIEffect>();
				DOTween.To(() => buttonUIEffect.effectFactor, (float x) =>
				{
					buttonUIEffect.effectFactor = x;
				}, 1f, duration);
				_maxHPButton.GetComponent<Image>().DOColor(Color.white, duration);
				_maxHPButton.GetComponentInChildren<CanvasGroup>().DOFade(0f, duration);
				break;
			}
			case UpgradeOption.UpgradeType.INSPECT_ORB_FOR_UPGRADE:
				if (_deckManager.GetUpgradeableOrbs().Count <= 0)
				{
					UIAudioManager.InvalidSelectionSfxRequested?.Invoke();
					break;
				}
				_stagedUpgrade = chosen;
				_upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: true);
				_upgradeConfirmationOrbUpgrade.Populate(_stagedUpgrade, _currentUpgradeOrbCost);
				_upgradeConfirmationOrbUpgrade.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = clickedButton;
				break;
			case UpgradeOption.UpgradeType.UPGRADE_CONFIRMED:
				_deckManager.UpgradeSpecificOrb(_stagedUpgrade);
				CloseAdditionalWindows();
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
				{
					int hpCostForGold = ChallengeManager.GetHpCostForGold(_currentUpgradeOrbCost);
					_playerHealthController.DealUnblockableDamage(hpCostForGold);
				}
				else
				{
					CurrencyManager.Instance.RemoveGold(_currentUpgradeOrbCost);
				}
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
				{
					if (!_relicManager.AttemptUseRelic(RelicEffect.PRICE_FIXING))
					{
						_currentUpgradeOrbCost += 10;
					}
					_upgradeButton.GetComponentInChildren<SimpleCostText>().SetGold(ChallengeManager.GetHpCostForGold(_currentUpgradeOrbCost));
				}
				else if (!_relicManager.AttemptUseRelic(RelicEffect.PRICE_FIXING))
				{
					_currentUpgradeOrbCost += (_relicManager.WandOfGreedEffectActive() ? 20 : 10);
					_upgradeButton.GetComponentInChildren<SimpleCostText>().SetGold(_currentUpgradeOrbCost);
				}
				UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.UpgradeOrb);
				CheckForUpgradeButtonEnabled();
				_mainOptionsPanel.SetActive(value: true);
				_upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: false);
				break;
			case UpgradeOption.UpgradeType.INSPECT_NEW_ORB:
			{
				_stagedUpgrade = chosen;
				_upgradeConfirmationOrbInspect.gameObject.SetActive(value: true);
				int num2 = (_relicManager.WandOfGreedEffectActive() ? (_stagedUpgrade.GetComponent<Attack>().PostBattleAddLevelCostMod * 2) : _stagedUpgrade.GetComponent<Attack>().PostBattleAddLevelCostMod);
				int gold = _currentAddOrbCost + num2;
				_upgradeConfirmationOrbInspect.PopulateForNewOrb(_stagedUpgrade, gold);
				_upgradeConfirmationOrbInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = clickedButton;
				break;
			}
			case UpgradeOption.UpgradeType.NEW_ORB_CONFIRMED:
			{
				int postBattleAddLevelCostMod = _stagedUpgrade.GetComponent<Attack>().PostBattleAddLevelCostMod;
				int num = _currentAddOrbCost + postBattleAddLevelCostMod;
				if (_relicManager.WandOfGreedEffectActive())
				{
					num += postBattleAddLevelCostMod;
				}
				bool flag = false;
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
				{
					num = ChallengeManager.GetHpCostForGold(num);
					if (_playerHealthController.CurrentHealth > (float)num)
					{
						flag = true;
					}
				}
				else
				{
					flag = CurrencyManager.Instance.CanAfford(num);
				}
				if (flag)
				{
					_deckManager.AddOrbToDeck(_stagedUpgrade);
					CloseAdditionalWindows();
					UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddOrb);
					CheckForUpgradeButtonEnabled();
					if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
					{
						_playerHealthController.DealUnblockableDamage(num);
					}
					else
					{
						CurrencyManager.Instance.RemoveGold(num);
					}
					if (!_relicManager.AttemptUseRelic(RelicEffect.PRICE_FIXING))
					{
						_currentAddOrbCost += (_relicManager.WandOfGreedEffectActive() ? 10 : 5);
					}
					_suggestionOrbs.UpdateOrbAdditionCosts(_currentAddOrbCost);
					_suggestionOrbs.DisableAddButton(_stagedUpgrade);
					_mainOptionsPanel.SetActive(value: true);
					_upgradeConfirmationOrbInspect.gameObject.SetActive(value: false);
				}
				else if ((bool)clickedButton)
				{
					clickedButton.GetComponentInChildren<SimpleCostText>()?.AttemptUnaffordablePurchase();
				}
				break;
			}
			case UpgradeOption.UpgradeType.INSPECT_RELIC:
				if (_state == State.RELIC && (bool)clickedButton)
				{
					_stagedUpgrade = clickedButton;
					Relic relic2 = clickedButton.GetComponentInChildren<RelicIcon>().relic;
					RelicIcon.HideTooltip?.Invoke();
					_upgradeConfirmationRelic.gameObject.SetActive(value: true);
					_upgradeConfirmationRelic.Populate(relic2, 0);
					_upgradeConfirmationRelic.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = clickedButton;
				}
				break;
			case UpgradeOption.UpgradeType.ACCEPT_RELIC:
				if (_state == State.RELIC && (bool)clickedButton)
				{
					UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddRelic);
					Relic relic = _stagedUpgrade.GetComponentInChildren<RelicIcon>().relic;
					RelicIcon.HideTooltip?.Invoke();
					UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddRelic);
					AcceptRelic(relic);
				}
				break;
			case UpgradeOption.UpgradeType.SKIP:
				if (_state == State.RELIC)
				{
					if (_relicGrantRarity == RelicRarity.BOSS)
					{
						CurrencyManager.Instance.AddGold(50);
					}
					MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
					if (mapDataBattle != null && mapDataBattle.grantChallenge)
					{
						_relicPanel.SetActive(value: false);
						SetupChallengeGrant();
					}
					else
					{
						CloseAndMoveOn();
					}
				}
				else
				{
					FinishUpgradeStep();
				}
				break;
			case UpgradeOption.UpgradeType.INSPECT_CHALLENGE:
				if ((bool)clickedButton)
				{
					_stagedUpgrade = clickedButton;
					ChallengeEffect challenge2 = clickedButton.GetComponentInChildren<ChallengeIcon>().challenge;
					ChallengeIcon.HideTooltip?.Invoke();
					_upgradeConfirmationChallenge.gameObject.SetActive(value: true);
					_upgradeConfirmationChallenge.Populate(challenge2);
					_upgradeConfirmationChallenge.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = clickedButton;
				}
				break;
			case UpgradeOption.UpgradeType.ADD_CHALLENGE:
				if ((bool)clickedButton)
				{
					UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddChallenge);
					ChallengeEffect challenge = _stagedUpgrade.GetComponentInChildren<ChallengeIcon>().challenge;
					ChallengeIcon.HideTooltip?.Invoke();
					AcceptChallenge(challenge);
				}
				break;
			case UpgradeOption.UpgradeType.RANDOM_ORB:
			case UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED:
			case UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED:
				break;
			}
		}

		private void OnPlayerHealthChanged(float amount = 0f)
		{
			if (_playerHealthController == null)
			{
				return;
			}
			if (_playerHealthController.CurrentHealth >= _playerHealthController.MaxHealth)
			{
				if (!_hasHealed)
				{
					_healButton.GetComponent<UIEffect>().effectFactor = 1f;
				}
			}
			else
			{
				_healButton.GetComponent<UIEffect>().effectFactor = 0f;
			}
			_healButton.GetComponentInChildren<Localize>().Term = "Menu/post_battle_heal_percent";
			LocalizationParamsManager componentInChildren = _healButton.GetComponentInChildren<LocalizationParamsManager>();
			componentInChildren.SetParameterValue("POST_BATTLE_HEAL_PERCENT", Mathf.RoundToInt(_playerHealthController.endOfBattleHealPercent * 100f).ToString(CultureInfo.InvariantCulture));
			componentInChildren.SetParameterValue("POST_BATTLE_HEAL_AMOUNT", _playerHealthController.endOfBattleHealAmount.ToString());
			SimpleCostText componentInChildren2 = _healButton.GetComponentInChildren<SimpleCostText>();
			componentInChildren2.SetGold(_currentHealCost, forceGold: true);
			componentInChildren2.alwaysForceGold = true;
		}

		private void CheckForUpgradeButtonEnabled()
		{
			UIEffect buttonUIEffect = _upgradeButton.GetComponent<UIEffect>();
			Image component = _upgradeButton.GetComponent<Image>();
			float duration = 0.35f;
			if (_deckManager.GetUpgradeableOrbs().Count <= 0)
			{
				DOTween.To(() => buttonUIEffect.effectFactor, (float x) =>
				{
					buttonUIEffect.effectFactor = x;
				}, 1f, duration);
				component.DOColor(Color.white, duration);
			}
			else
			{
				DOTween.To(() => buttonUIEffect.effectFactor, (float x) =>
				{
					buttonUIEffect.effectFactor = x;
				}, 0f, duration);
				component.DOColor(_upgradeButtonDefaultColor, duration);
			}
		}

		public void FinishUpgradeStep(bool forceSkipRelic = false)
		{
			CloseAdditionalWindows();
			if (!forceSkipRelic && StaticGameData.dataToLoad != null && _state != State.CHALLENGE)
			{
				MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
				RelicRarity relicRarity = RelicRarity.NONE;
				if (StaticGameData.relicRarityOverride != RelicRarity.NONE)
				{
					relicRarity = StaticGameData.relicRarityOverride;
					StaticGameData.relicRarityOverride = RelicRarity.NONE;
				}
				else if (mapDataBattle != null && mapDataBattle.grantedRelicRarity != RelicRarity.NONE)
				{
					relicRarity = mapDataBattle.grantedRelicRarity;
				}
				if (relicRarity != RelicRarity.NONE)
				{
					SetupRelicGrant(relicRarity);
					return;
				}
			}
			CloseAndMoveOn();
		}

		private void SetupRelicGrant(RelicRarity rarity, bool isTreasure = false)
		{
			if (afterMinesBoss && ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.HP_TO_PURCHASE))
			{
				_playerHealthCanvas.sortingLayerName = "SidescrollerUI";
			}
			_state = State.RELIC;
			_mainOptionsPanel.gameObject.SetActive(value: false);
			_relicPanel.SetActive(value: true);
			RelicIcon[] componentsInChildren = _relicPanel.GetComponentsInChildren<RelicIcon>();
			int num = 1;
			if (isTreasure)
			{
				num = 1;
			}
			else
			{
				switch (rarity)
				{
				case RelicRarity.BOSS:
					num = 3;
					break;
				case RelicRarity.RARE:
					num = 2;
					break;
				}
			}
			_relicGrantRarity = rarity;
			if (rarity == RelicRarity.BOSS)
			{
				_relicSkipButtonGoldContainer.SetActive(value: true);
			}
			MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
			if (mapDataBattle != null && mapDataBattle.name == "MimicMinibossMapData")
			{
				num++;
			}
			if (!isTreasure && _relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_ORB_RELIC_OPTIONS))
			{
				num++;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_PEGLIN_CHOICES))
			{
				num++;
			}
			Relic[] array;
			if (StaticGameData.seedSet)
			{
				Relic[] multipleRelicsOffOfQueue = _relicManager.GetMultipleRelicsOffOfQueue(num, rarity);
				List<RelicRarity> list = new List<RelicRarity>();
				for (int i = 0; i < num; i++)
				{
					list.Add(rarity);
				}
				array = _relicManager.CheckAndUpdateSeededRelicOffers(multipleRelicsOffOfQueue, list.ToArray());
			}
			else
			{
				Debug.LogWarning("No seeded data for relic offer.");
				array = _relicManager.GetMultipleRelicsOfRarity(num, rarity);
			}
			int num2 = 0;
			RelicIcon[] array2 = componentsInChildren;
			foreach (RelicIcon relicIcon in array2)
			{
				if (num2 < array.Length)
				{
					relicIcon.SetRelic(array[num2]);
					relicIcon.shouldShowTooltip = false;
				}
				else
				{
					componentsInChildren[num2].transform.parent.gameObject.SetActive(value: false);
				}
				num2++;
			}
		}

		private void AcceptRelic(Relic relic)
		{
			_relicManager.AddRelic(relic);
			MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
			if (mapDataBattle != null && mapDataBattle.grantChallenge)
			{
				_upgradeConfirmationRelic.gameObject.SetActive(value: false);
				SetupChallengeGrant();
			}
			else
			{
				StartCoroutine(WaitAndClose());
			}
		}

		private void SetupChallengeGrant()
		{
			RelicIcon.HideTooltip?.Invoke();
			if (!ChallengeManager.Instance.AreChallengesAvailable() || ChallengeManager.Instance.challengesOfEachAct[MapController.instance.Act + 1] != ChallengeEffect.NONE || MapController.instance.Act == 3)
			{
				StartCoroutine(WaitAndClose());
				return;
			}
			_state = State.CHALLENGE;
			ChallengeIcon[] componentsInChildren = _challengePanel.GetComponentsInChildren<ChallengeIcon>();
			ChallengeEffect[] array;
			if (StaticGameData.seedSet)
			{
				array = ChallengeManager.Instance.GetMultipleChallengesOffOfQueue(3);
			}
			else
			{
				Debug.LogWarning("No seeded data for challenge offer.");
				array = ChallengeManager.Instance.GetRandomUnseededChallenges(3);
			}
			_mainOptionsPanel.gameObject.SetActive(value: false);
			if (array != null && array.Length > 0)
			{
				_relicPanel.SetActive(value: false);
				_challengePanel.SetActive(value: true);
				int num = 0;
				ChallengeIcon[] array2 = componentsInChildren;
				foreach (ChallengeIcon challengeIcon in array2)
				{
					if (array[num] != ChallengeEffect.NONE)
					{
						challengeIcon.SetChallenge(array[num]);
					}
					else
					{
						componentsInChildren[num].transform.parent.gameObject.SetActive(value: false);
					}
					num++;
				}
			}
			else
			{
				StartCoroutine(WaitAndClose());
			}
		}

		private void AcceptChallenge(ChallengeEffect effect)
		{
			ChallengeManager.Instance.AddChallenge(effect, MapController.instance.Act + 1);
			StartCoroutine(WaitAndClose());
		}

		private IEnumerator WaitAndClose()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			CloseAndMoveOn();
		}

		private void CloseAndMoveOn()
		{
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			_mainCanvasGroup.interactable = false;
			Sequence sequence = DOTween.Sequence();
			if (_panelToScale != null)
			{
				Vector2 sizeDelta = _panelToScale.sizeDelta;
				sizeDelta.x = _minPanelWidth;
				sequence.Append(_panelToScale.DOSizeDelta(sizeDelta, 0.5f).SetUpdate(isIndependentUpdate: true));
			}
			Sequence sequence2 = sequence.Insert(0.2f, _mainCanvasGroup.DOFade(0f, 0.35f).SetUpdate(isIndependentUpdate: true));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(SignalMoveOn));
			sequence.Play();
		}

		private void SignalMoveOn()
		{
			OnBattleUpgradeOver?.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
