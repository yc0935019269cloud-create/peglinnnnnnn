using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data;
using Data.Scenarios;
using Loading.Pegboards;
using Map;
using Peglin.Achievements;
using PeglinUI;
using PixelCrushers.DialogueSystem;
using Relics;
using Scenarios;
using UnityEngine;
using UnityEngine.UI;

namespace RNG.Scenarios
{
	public class DialogueSystemScenario : MonoBehaviour
	{
		public static Action OnOrbAccepted = () =>
		{
		};

		public static Action DialogueCurtainRemoved = () =>
		{
		};

		public MapDataScenario debugMapData;

		public GameObject debugPegboardFrame;

		private MapDataScenario _activeMapData;

		public float introCurtainFadeTime = 0.65f;

		public float curtainFadeTime = 1f;

		[Header("Nav Objs")]
		public InventoryViewController inventoryView;

		public GameObject navController;

		public GameObject playfieldMouseDetection;

		public Transform navOrbSpawnTransform;

		public Image fullCurtain;

		public Image navCurtain;

		[Header("Other Refs")]
		public CanvasGroup mainTextAnimatorCanvas;

		public TextScenarioInteractions textScenarioInteractions;

		public RelicManager relicManager;

		private Image _fadeCurtain;

		private PegLayoutLoader _pegLayoutLoader;

		private bool _navBlock;

		private bool _randomNav;

		private UnityEngine.Random.State _previousRandomState;

		private SeededTextScenarioNodeData _seededTextScenerioData;

		private void OnEnable()
		{
			NonBattleDeathHandler.NonBattleDeath = (Action)Delegate.Combine(NonBattleDeathHandler.NonBattleDeath, new Action(HandleDeath));
		}

		private void OnDisable()
		{
			NonBattleDeathHandler.NonBattleDeath = (Action)Delegate.Remove(NonBattleDeathHandler.NonBattleDeath, new Action(HandleDeath));
		}

		private void Awake()
		{
			_seededTextScenerioData = StaticGameData.seededNodeData as SeededTextScenarioNodeData;
			if (_seededTextScenerioData != null)
			{
				_previousRandomState = UnityEngine.Random.state;
				UnityEngine.Random.state = _seededTextScenerioData.randomState.randomState;
				MapController.CheckAndFixRandomState();
			}
			if (StaticGameData.dataToLoad == null)
			{
				_activeMapData = debugMapData;
			}
			else
			{
				_activeMapData = StaticGameData.dataToLoad as MapDataScenario;
				if (_activeMapData.background != null)
				{
					UnityEngine.Object.Instantiate(_activeMapData.background);
				}
			}
			_fadeCurtain = GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>();
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeCurtain.DOFade(0f, introCurtainFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableFadeCurtain));
			_pegLayoutLoader = base.gameObject.AddComponent<PegLayoutLoader>();
			LoadData();
		}

		private void DisableFadeCurtain()
		{
			PauseMenu.PauseBlock = false;
			_fadeCurtain.GetComponent<Image>().enabled = false;
			DialogueManager.StartConversation("Scenario/" + _activeMapData.scenarioName);
		}

		private void LoadData()
		{
			GameObject[] scenarioDoodads = _activeMapData.scenarioDoodads;
			for (int i = 0; i < scenarioDoodads.Length; i++)
			{
				UnityEngine.Object.Instantiate(scenarioDoodads[i]);
			}
			if (_activeMapData != null && _activeMapData.navigationPegLayout != null)
			{
				GameObject pegboard = UnityEngine.Object.Instantiate((_activeMapData.pegboardFrame != null) ? _activeMapData.pegboardFrame : debugPegboardFrame);
				navOrbSpawnTransform.position = _activeMapData.navigationPegLayout.BallStartingPosition;
				GameObject gameObject = null;
				if (!ShouldSkipNavigation())
				{
					gameObject = _pegLayoutLoader.TryLoadPegLayout(_activeMapData.navigationPegLayout.PegLayoutData, _activeMapData.navigationPegLayout.layoutDataFilename, base.transform.position);
					UnityEngine.Object.FindObjectOfType<PredictionManager>().Initialize(gameObject, pegboard, relicManager, navigation: true);
					navController.GetComponent<NavOnlyController>().Initialize();
				}
			}
		}

		public void LoadEliteBattle()
		{
			LoadBattle(elite: true);
		}

		public void LoadRandomBattle()
		{
			LoadBattle();
		}

		public void ChooseOrbForRemoval()
		{
			inventoryView.ShowInventoryForOrbRemoval();
		}

		public void ChooseMutlipleOrbsForRemoval(int amount)
		{
			inventoryView.ShowInventoryForMultipleOrbRemovals(amount);
		}

		public void ChooseOrbForRemovalUnskippable()
		{
			inventoryView.ShowInventoryForOrbRemoval(skippable: false);
		}

		public void ChooseOrbForUpgrade()
		{
			inventoryView.ShowInventoryForOrbUpgrade();
		}

		public void ChooseOrbForDuplication()
		{
			inventoryView.ShowInventoryForOrbDuplication();
		}

		public void ChooseOrbForDuplicationUnskippable()
		{
			inventoryView.ShowInventoryForOrbDuplication(skippable: false);
		}

		public void ChooseOrbForUpgradeUnskippable()
		{
			inventoryView.ShowInventoryForOrbUpgrade(skippable: false);
		}

		public void LoadSpecificBattle(MapDataBattle battle)
		{
			_navBlock = true;
			PauseMenu.PauseBlock = true;
			_fadeCurtain.gameObject.SetActive(value: true);
			_fadeCurtain.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeCurtain.DOFade(1f, curtainFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				FinishLoadSpecificBattle(battle);
			}));
		}

		private void FinishLoadSpecificBattle(MapDataBattle battle)
		{
			MapController mapController = UnityEngine.Object.FindObjectOfType<MapController>();
			if (mapController == null)
			{
				Debug.LogWarning("MapController null when attempting to start specific Battle from Scenario. Elite?");
				StartNavigation();
			}
			else
			{
				mapController.LoadSpecificBattleFromScenario(battle);
			}
		}

		public void SetRandomNav()
		{
			_randomNav = true;
		}

		public void OfferCommonRelic()
		{
			OfferRelic(RelicRarity.COMMON);
		}

		public void OfferRareRelic()
		{
			OfferRelic(RelicRarity.RARE);
		}

		public void OfferRareRelics(int amount)
		{
			OfferRelic(RelicRarity.RARE, amount);
		}

		public void OfferCommonOrRareRelic(float rareChance)
		{
			RelicRarity rarity = ((!(UnityEngine.Random.value < rareChance)) ? RelicRarity.COMMON : RelicRarity.RARE);
			textScenarioInteractions.gameObject.SetActive(value: true);
			textScenarioInteractions.OfferRelic(rarity);
		}

		public void OfferSpecificRelic(Relic relic)
		{
			textScenarioInteractions.gameObject.SetActive(value: true);
			textScenarioInteractions.OfferSpecificRelic(relic);
		}

		public void OfferSpecificRelicUnskippable(Relic relic)
		{
			textScenarioInteractions.gameObject.SetActive(value: true);
			textScenarioInteractions.OfferSpecificRelicNonSkippable(relic);
		}

		public void UnlockCakeAchievement()
		{
			AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.BRING_EGG_TO_PEGLIN_CHEF);
		}

		private void OfferRelic(RelicRarity rarity, int quantity = 1)
		{
			textScenarioInteractions.gameObject.SetActive(value: true);
			textScenarioInteractions.OfferRelic(rarity, quantity);
		}

		public void LoadRandomBossBattle()
		{
			_navBlock = true;
			if (_fadeCurtain.color.a < 1f)
			{
				PauseMenu.PauseBlock = true;
				_fadeCurtain.gameObject.SetActive(value: true);
				_fadeCurtain.enabled = true;
				TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeCurtain.DOFade(1f, curtainFadeTime);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadRandomBossBattle));
				return;
			}
			PauseMenu.PauseBlock = false;
			MapController mapController = UnityEngine.Object.FindObjectOfType<MapController>();
			if (mapController == null)
			{
				Debug.LogWarning("MapController null when attempting to start Boss Battle from Scenario.");
				StartNavigation();
			}
			else
			{
				mapController.LoadBossBattleFromScenario();
			}
		}

		private void LoadBattle(bool elite = false)
		{
			_navBlock = true;
			PauseMenu.PauseBlock = true;
			_fadeCurtain.gameObject.SetActive(value: true);
			_fadeCurtain.enabled = true;
			if (elite)
			{
				TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeCurtain.DOFade(1f, curtainFadeTime);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(FinishLoadEliteBattle));
			}
			else
			{
				TweenerCore<Color, Color, ColorOptions> tweenerCore2 = _fadeCurtain.DOFade(1f, curtainFadeTime);
				tweenerCore2.onComplete = (TweenCallback)Delegate.Combine(tweenerCore2.onComplete, new TweenCallback(FinishLoadRegularBattle));
			}
		}

		public void ActivateSecretTunnel()
		{
			_navBlock = true;
			PauseMenu.PauseBlock = true;
			_fadeCurtain.gameObject.SetActive(value: true);
			_fadeCurtain.enabled = true;
			StaticGameData.specificNodeOverride = StaticGameData.currentNode.secretTunnelConnection;
			navController.SetActive(value: true);
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeCurtain.DOFade(1f, curtainFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadSelectedScene));
		}

		private void FinishLoadRegularBattle()
		{
			PauseMenu.PauseBlock = false;
			MapController mapController = UnityEngine.Object.FindObjectOfType<MapController>();
			if (mapController == null)
			{
				Debug.LogWarning("MapController null when attempting to start non-elite Battle from Scenario. ");
				StartNavigation();
			}
			else
			{
				mapController.LoadBattleFromScenario();
			}
		}

		private void FinishLoadEliteBattle()
		{
			PauseMenu.PauseBlock = false;
			MapController mapController = UnityEngine.Object.FindObjectOfType<MapController>();
			if (mapController == null)
			{
				Debug.LogWarning("MapController null when attempting to start Elite Battle from Scenario. ");
				StartNavigation();
			}
			else
			{
				mapController.LoadBattleFromScenario(elite: true);
			}
		}

		public void OrbAccepted()
		{
			OnOrbAccepted?.Invoke();
		}

		public void ConversationEnded()
		{
			if (!_navBlock)
			{
				StartNavigation();
			}
		}

		private void StartNavigation()
		{
			TextScenarioInteractions.OnInteractionComplete = (Action)Delegate.Remove(TextScenarioInteractions.OnInteractionComplete, new Action(StartNavigation));
			if (_randomNav || ShouldSkipNavigation())
			{
				if (_randomNav)
				{
					StaticGameData.chosenNextNodeIndex = UnityEngine.Random.Range(0, StaticGameData.currentNode.ChildNodes.Length);
				}
				else
				{
					StaticGameData.chosenNextNodeIndex = 0;
				}
				navController.SetActive(value: true);
				fullCurtain.enabled = true;
				PauseMenu.PauseBlock = true;
				TweenerCore<Color, Color, ColorOptions> tweenerCore = fullCurtain.DOFade(1.2f, curtainFadeTime).From(0f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadSelectedScene));
			}
			else
			{
				if (_seededTextScenerioData != null)
				{
					UnityEngine.Random.state = _previousRandomState;
				}
				StartCoroutine(NavigationDelay());
			}
		}

		private bool ShouldSkipNavigation()
		{
			if (!_activeMapData.forceNavigation)
			{
				if (!(StaticGameData.currentNode == null))
				{
					return StaticGameData.currentNode.ChildNodes.Length == 1;
				}
				return true;
			}
			return false;
		}

		private void LoadSelectedScene()
		{
			PauseMenu.PauseBlock = false;
			navController.GetComponent<NavOnlyController>().LoadSelectedScene();
		}

		private IEnumerator NavigationDelay()
		{
			yield return null;
			navController.SetActive(value: true);
			navController.GetComponent<NavOnlyController>().PrepareForNavigation();
			TweenerCore<Color, Color, ColorOptions> tweenerCore = navCurtain.DOFade(0f, curtainFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableCurtain));
		}

		private void HandleDeath()
		{
			AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.DIE_IN_TEXT_SCENARIO]);
			HideTextCanvas();
		}

		private void HideTextCanvas()
		{
			mainTextAnimatorCanvas.gameObject.SetActive(value: false);
		}

		private void DisableCurtain()
		{
			DialogueCurtainRemoved?.Invoke();
			playfieldMouseDetection.SetActive(value: true);
			navCurtain.enabled = false;
		}
	}
}
