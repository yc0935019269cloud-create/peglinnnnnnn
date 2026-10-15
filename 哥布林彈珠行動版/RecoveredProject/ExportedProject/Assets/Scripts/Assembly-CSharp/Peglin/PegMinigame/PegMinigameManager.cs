using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Loading;
using Loading.Pegboards;
using Map;
using PeglinUI.PostBattle;
using PeglinUtils;
using Relics;
using Scenarios;
using UnityEngine;
using UnityEngine.UI;

namespace Peglin.PegMinigame
{
	public class PegMinigameManager : MonoBehaviour
	{
		[Header("Debug")]
		[SerializeField]
		private MapDataPegMinigame _mapDataDebug;

		[Header("Parameters")]
		[SerializeField]
		private float _minBoundaryX = -8f;

		[SerializeField]
		private float _maxBoundaryX = 8f;

		[SerializeField]
		private float _boundaryY = -10f;

		[SerializeField]
		private float _bouncerVerticalOffset = 2f;

		[SerializeField]
		public float _fadeTime = 0.6f;

		[Header("Prefabs")]
		[SerializeField]
		private GameObject _dynamicSlotPrefab;

		[Header("Scene Refs")]
		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private GameObject _navigationOrb;

		[SerializeField]
		private Transform _ballStartTransform;

		[SerializeField]
		private UpgradeConfirmationRelic _upgradeConfirmationRelic;

		[SerializeField]
		private UpgradeConfirmationOrbInspect _upgradeConfirmationOrbInspect;

		[SerializeField]
		private Image _fullCurtain;

		[SerializeField]
		private PredictionManager _predictionManager;

		[SerializeField]
		private GameObject _playfieldMouseDetection;

		public GameObject rewardInspectionButtonPrompt;

		public GameObject leftMostReward;

		public List<GameObject> allRewards = new List<GameObject>();

		private GameObject _ball;

		private MapDataPegMinigame _mapData;

		private int _indexSelected = -1;

		private bool _navigating;

		private bool _chooseSuccess;

		private float _boundaryLength;

		private float _bouncerOffset;

		private float _slotOffset;

		private List<FadeInOut> _fadeList = new List<FadeInOut>();

		private List<DynamicSlotTrigger> _slots = new List<DynamicSlotTrigger>();

		private PeglinSceneLoader.Scene _selectedScene;

		private bool _playerDied;

		private bool _navigationStarted;

		private int _remainingPachinkoBalls;

		private GameObject _layoutParent;

		private GameObject _pegBoard;

		private SeededPegMinigameNodeData _seededPegMinigameData;

		public bool chooseSuccess => _chooseSuccess;

		private void OnEnable()
		{
			NonBattleDeathHandler.NonBattleDeath = (Action)Delegate.Combine(NonBattleDeathHandler.NonBattleDeath, new Action(HandleDeath));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Combine(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(PachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PachinkoBallDestroyed));
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			Initialize();
		}

		private void OnDisable()
		{
			NonBattleDeathHandler.NonBattleDeath = (Action)Delegate.Remove(NonBattleDeathHandler.NonBattleDeath, new Action(HandleDeath));
			PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Remove(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(PachinkoBallCreated));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PachinkoBallDestroyed));
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
		}

		public void Initialize()
		{
			_mapData = StaticGameData.dataToLoad as MapDataPegMinigame;
			_seededPegMinigameData = StaticGameData.seededNodeData as SeededPegMinigameNodeData;
			if (_mapData == null && _mapDataDebug == null)
			{
				Debug.LogError("MapData is not MapDataPegMinigame!");
				return;
			}
			if (_mapData == null && (bool)_mapDataDebug)
			{
				_mapData = _mapDataDebug;
			}
			_mapData.PopulateRewards(_seededPegMinigameData);
			UnityEngine.Object.Instantiate(_mapData.background);
			PegLayoutLoader pegLayoutLoader = base.gameObject.AddComponent<PegLayoutLoader>();
			_pegBoard = UnityEngine.Object.Instantiate(_mapData.pegboardFrame);
			_layoutParent = pegLayoutLoader.TryLoadPegLayout(_mapData.pegLayoutData, _mapData.layoutDataFilename, base.transform.position);
			_predictionManager.Initialize(_layoutParent, _pegBoard, _relicManager, navigation: true);
			_fullCurtain = GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>();
			_fullCurtain.enabled = true;
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fullCurtain.DOFade(0f, _fadeTime).From(1f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(HideMainCurtain));
			CreateRewardBouncersAndSlots();
			CreateOrb();
			StartCoroutine(UpdatePrediction());
		}

		private IEnumerator UpdatePrediction()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_predictionManager.CopyAllPegs();
		}

		private void CreateOrb()
		{
			_ball = UnityEngine.Object.Instantiate(_navigationOrb, _ballStartTransform.position, Quaternion.identity);
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			_ball.GetComponent<PachinkoBall>().Init(_relicManager, null, Vector2.zero, _predictionManager, null);
			_remainingPachinkoBalls++;
		}

		private void CalculateBouncerOffsets(int numberOfSlots)
		{
			_boundaryLength = Mathf.Abs(_minBoundaryX) + Mathf.Abs(_maxBoundaryX);
			_bouncerOffset = _boundaryLength / (float)numberOfSlots;
			_slotOffset = _bouncerOffset / 2f;
		}

		private void CreateRewardBouncersAndSlots()
		{
			_fadeList.Clear();
			CalculateBouncerOffsets(_mapData.Rewards.Count);
			CreateBouncers(_mapData.Rewards.Count);
			_slots = CreateSlots(_mapData.Rewards.Count);
			for (int i = 0; i < _slots.Count; i++)
			{
				_slots[i].Initialize(i, HandleRewardSlotTriggerActivated, _bouncerOffset / 2f, _mapData.Rewards[i]);
			}
		}

		private void CreateNavigationBouncersAndSlots()
		{
			_predictionManager.Initialize(_layoutParent, _pegBoard, _relicManager, navigation: true);
			_fadeList.Clear();
			CalculateBouncerOffsets(2);
			CreateBouncers(2);
			List<DynamicSlotTrigger> list = CreateSlots(2);
			for (int i = 0; i < list.Count; i++)
			{
				DynamicSlotTrigger dynamicSlotTrigger = list[i];
				dynamicSlotTrigger.Initialize(i, HandleNavigationSlotTriggerActivated, _bouncerOffset / 2f, null);
				dynamicSlotTrigger.SetSprite(StaticGameData.currentNode.ChildNodes[i].activeIcon, Color.white, Vector3.one);
			}
		}

		private void CreateBouncers(int numberOfSlots)
		{
			for (int i = 1; i < numberOfSlots; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(position: new Vector2(_bouncerOffset * (float)i + _minBoundaryX, _boundaryY + _bouncerVerticalOffset), original: _mapData.bouncerPrefab, rotation: Quaternion.identity);
				_predictionManager.AddObjectToSimulationScene(gameObject);
				_fadeList.Add(gameObject.GetComponent<FadeInOut>());
			}
		}

		private List<DynamicSlotTrigger> CreateSlots(int numberOfSlots)
		{
			List<DynamicSlotTrigger> list = new List<DynamicSlotTrigger>();
			List<Selectable> list2 = new List<Selectable>();
			for (int i = 0; i < numberOfSlots; i++)
			{
				DynamicSlotTrigger componentInChildren = UnityEngine.Object.Instantiate(position: new Vector2(_bouncerOffset * (float)i + _minBoundaryX + _slotOffset, _boundaryY), original: _dynamicSlotPrefab, rotation: Quaternion.identity).GetComponentInChildren<DynamicSlotTrigger>();
				if (componentInChildren == null)
				{
					Debug.LogError("A DynamicSlotTrigger was not in the prefab instantiated");
				}
				else
				{
					list.Add(componentInChildren);
				}
				_fadeList.Add(componentInChildren.gameObject.GetComponent<FadeInOut>());
				list2.Add(componentInChildren.GetComponentInChildren<Selectable>());
			}
			if (list2.Count > 0)
			{
				leftMostReward = list2[0].gameObject;
			}
			allRewards.Clear();
			foreach (Selectable item in list2)
			{
				allRewards.Add(item.gameObject);
			}
			if (list2.Count >= 2)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnRight = list2[1]
				};
				list2[0].navigation = navigation;
				Navigation navigation2 = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnLeft = list2[list2.Count - 2]
				};
				list2[list2.Count - 1].navigation = navigation2;
				for (int j = 1; j < list2.Count - 1; j++)
				{
					Navigation navigation3 = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnLeft = list2[j - 1],
						selectOnRight = list2[j + 1]
					};
					list2[j].navigation = navigation3;
				}
			}
			return list;
		}

		private void PachinkoBallCreated()
		{
			_remainingPachinkoBalls++;
		}

		private void PachinkoBallDestroyed(PachinkoBall pBall)
		{
			_remainingPachinkoBalls--;
			if ((!_chooseSuccess || _navigating) && _remainingPachinkoBalls <= 0)
			{
				CreateOrb();
				PrepareNavigationOrbForFiring();
			}
		}

		private void HandleRewardSlotTriggerActivated(int index)
		{
			_indexSelected = index;
			_chooseSuccess = true;
			NavOnlyController.NavigationTriggered?.Invoke();
			Reward reward = _mapData.Rewards[index];
			if (reward is OrbReward)
			{
				_upgradeConfirmationOrbInspect.gameObject.SetActive(value: true);
				_upgradeConfirmationOrbInspect.PopulateForNewOrb(((OrbReward)reward).Orb, 0);
			}
			else
			{
				_upgradeConfirmationRelic.gameObject.SetActive(value: true);
				_upgradeConfirmationRelic.Populate(((RelicReward)reward).Relic, 0);
			}
			foreach (DynamicSlotTrigger slot in _slots)
			{
				slot.DisableTooltips();
			}
			if (_ball != null && _ball.TryGetComponent<PachinkoBall>(out var component))
			{
				component.DestroyMultiballsAndSelf();
				_remainingPachinkoBalls = 0;
			}
		}

		private void HandleNavigationSlotTriggerActivated(int index)
		{
			if (!_navigationStarted)
			{
				_navigationStarted = true;
				NavOnlyController.NavigationTriggered?.Invoke();
				if (index < 1)
				{
					StaticGameData.chosenNextNodeIndex = 0;
				}
				else
				{
					StaticGameData.chosenNextNodeIndex = StaticGameData.currentNode.ChildNodes.Length - 1;
				}
				FadeAndLoad();
			}
		}

		private void HandleUpgradeOptionClicked(UpgradeOption.UpgradeType type, GameObject clickedButton, GameObject chosen)
		{
			if (_indexSelected >= 0)
			{
				switch (type)
				{
				case UpgradeOption.UpgradeType.INSPECT_ORB_FOR_UPGRADE:
					return;
				default:
					_mapData.Rewards[_indexSelected].Grant();
					break;
				case UpgradeOption.UpgradeType.SKIP:
					break;
				}
				_indexSelected = -1;
				_upgradeConfirmationRelic.gameObject.SetActive(value: false);
				_upgradeConfirmationOrbInspect.gameObject.SetActive(value: false);
				if (StaticGameData.currentNode == null || StaticGameData.currentNode.ChildNodes.Length == 1)
				{
					StaticGameData.chosenNextNodeIndex = 0;
					FadeAndLoad();
				}
				else
				{
					StartCoroutine(WaitAndSpawnNav(0.5f));
				}
			}
		}

		private void HideMainCurtain()
		{
			_fullCurtain.enabled = false;
			PauseMenu.PauseBlock = false;
			PrepareNavigationOrbForFiring();
		}

		private void PrepareNavigationOrbForFiring()
		{
			_ball.GetComponent<PachinkoBall>().Arm();
			_ball.GetComponent<TrajectorySimulation>().enabled = true;
			_playfieldMouseDetection.SetActive(value: true);
		}

		public void FadeAndLoad(PeglinSceneLoader.Scene selectedScene = PeglinSceneLoader.Scene.NONE, float fadeTime = -1f)
		{
			_selectedScene = selectedScene;
			fadeTime = ((fadeTime > 0f) ? fadeTime : _fadeTime);
			Image obj = GameObject.FindGameObjectWithTag("Curtain")?.GetComponent<Image>();
			if (obj == null)
			{
				Debug.LogWarning("PegMinigame Manager trying to navigate with a missing curtain fade!");
			}
			obj.enabled = true;
			obj.DOKill();
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = obj.DOFade(1f, fadeTime).From(0f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadSelectedScene));
		}

		private void LoadSelectedScene()
		{
			PauseMenu.PauseBlock = false;
			if (_playerDied)
			{
				_fullCurtain.gameObject.SetActive(value: false);
			}
			else if (_selectedScene != PeglinSceneLoader.Scene.NONE)
			{
				PeglinSceneLoader.Instance.LoadScene(_selectedScene);
			}
			else if (StaticGameData.activeMapScene == PeglinSceneLoader.Scene.NONE)
			{
				Debug.LogWarning("No active map set in treasure/scenario room!");
				PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.FOREST_MAP);
			}
			else
			{
				PeglinSceneLoader.Instance.LoadScene(StaticGameData.activeMapScene);
			}
		}

		private IEnumerator WaitAndSpawnNav(float waitTime)
		{
			foreach (DynamicSlotTrigger slot in _slots)
			{
				slot.IgnoreSelectionHidingAndShowing();
			}
			if (_fadeList != null && _fadeList.Count > 0)
			{
				foreach (FadeInOut fade in _fadeList)
				{
					if (fade != null)
					{
						fade.FadeOut(disable: true);
					}
				}
			}
			yield return new WaitForSeconds(waitTime);
			_navigating = true;
			CreateOrb();
			PrepareNavigationOrbForFiring();
			CreateNavigationBouncersAndSlots();
		}

		private void HandleDeath()
		{
			_playerDied = true;
		}
	}
}
