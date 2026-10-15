using System;
using System.Collections;
using Battle;
using Battle.StatusEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Loading;
using PeglinUtils;
using Relics;
using Scenarios;
using UnityEngine;
using UnityEngine.UI;
using Worldmap;

public class NavOnlyController : MonoBehaviour
{
	public static Action NavigationTriggered = () =>
	{
	};

	public static Action NavigationStarted = () =>
	{
	};

	public static Action NavigationReset = () =>
	{
	};

	public bool isTreasureScene;

	[SerializeField]
	private RelicManager _relicManager;

	[SerializeField]
	private GameObject _navigationOrb;

	[SerializeField]
	private Transform _ballStartTransform;

	[SerializeField]
	private SlotManager _leftSlotManager;

	[SerializeField]
	private SlotManager _rightSlotManager;

	[SerializeField]
	private SlotManager _centreSlotManager;

	[SerializeField]
	private float _fadeTime = 0.65f;

	private GameObject _ball;

	private bool _navStarted;

	private PeglinSceneLoader.Scene _selectedScene;

	private PredictionManager _predictionManager;

	private int _remainingPachinkoBalls;

	private Bomb[] _bombs;

	private void OnEnable()
	{
		PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Combine(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(PachinkoBallCreated));
		PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PachinkoBallDestroyed));
		SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Combine(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(HandleSlotTriggerActivated));
	}

	private void OnDisable()
	{
		PachinkoBall.OnAdditionalPachinkoBallCreated = (PachinkoBall.PachinkoBallCreated)Delegate.Remove(PachinkoBall.OnAdditionalPachinkoBallCreated, new PachinkoBall.PachinkoBallCreated(PachinkoBallCreated));
		PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PachinkoBallDestroyed));
		SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Remove(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(HandleSlotTriggerActivated));
	}

	public void Initialize()
	{
		CreateOrb(Vector2.zero);
	}

	public void SetUpTreasureBombs(Bomb[] bombs)
	{
		_bombs = bombs;
	}

	private void CreateOrb(Vector2 prevAim)
	{
		_ball = UnityEngine.Object.Instantiate(_navigationOrb, _ballStartTransform.position, Quaternion.identity);
		_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
		_ball.GetComponent<PachinkoBall>().Init(_relicManager, null, prevAim, _predictionManager, null);
		_remainingPachinkoBalls++;
	}

	public void PrepareForNavigation()
	{
		_ball.GetComponent<PachinkoBall>().Arm();
		_ball.GetComponent<TrajectorySimulation>().enabled = true;
		NavigationStarted?.Invoke();
		_predictionManager.CopyAllPegs();
		PlayerStatusEffectController playerStatusEffectController = UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>();
		if (playerStatusEffectController != null)
		{
			playerStatusEffectController.UpdatePlayerBlindness();
		}
		if (StaticGameData.currentNode == null || StaticGameData.currentNode.ChildNodes.Length < 1)
		{
			Debug.LogWarning("In Nav with no node data.");
			LoadSelectedScene();
			return;
		}
		MapNode mapNode = StaticGameData.currentNode.ChildNodes[0];
		_leftSlotManager.gameObject.SetActive(value: true);
		_leftSlotManager.ConfigureForNavigation(mapNode.activeIcon, MapNode.GetColorForNodeType(mapNode.RoomType));
		bool num = StaticGameData.currentNode.ChildNodes.Length == 1;
		mapNode = (num ? StaticGameData.currentNode.ChildNodes[0] : StaticGameData.currentNode.RightChild);
		_rightSlotManager.gameObject.SetActive(value: true);
		_rightSlotManager.ConfigureForNavigation(mapNode.activeIcon, MapNode.GetColorForNodeType(mapNode.RoomType));
		if (num)
		{
			_leftSlotManager.ToggleNavigationIconVisibility(visible: false);
			_rightSlotManager.ToggleNavigationIconVisibility(visible: false);
			_centreSlotManager.gameObject.SetActive(value: true);
			_centreSlotManager.ConfigureForIconOnly(mapNode.activeIcon);
		}
	}

	private void PachinkoBallCreated()
	{
		_remainingPachinkoBalls++;
	}

	private void PachinkoBallDestroyed(PachinkoBall pachinkoBall)
	{
		_remainingPachinkoBalls--;
		StartCoroutine(WaitAndSpawn(pachinkoBall.aimVector));
	}

	private IEnumerator WaitAndSpawn(Vector2 aimVector)
	{
		yield return WaitObjectAllocator.waitForEndOfFrame;
		if (!_navStarted && _remainingPachinkoBalls <= 0)
		{
			NavigationReset();
			CreateOrb(aimVector);
			_ball.GetComponent<PachinkoBall>().Arm();
			_ball.GetComponent<TrajectorySimulation>().enabled = true;
		}
	}

	private void HandleSlotTriggerActivated(int index, PachinkoBall pBall)
	{
		if (_navStarted)
		{
			return;
		}
		_navStarted = true;
		NavigationTriggered?.Invoke();
		if (isTreasureScene && _bombs != null)
		{
			bool flag = true;
			Bomb[] bombs = _bombs;
			for (int i = 0; i < bombs.Length; i++)
			{
				if (!bombs[i].detonated)
				{
					flag = false;
				}
			}
			if (flag)
			{
				UnityEngine.Object.FindObjectOfType<ChestScenarioController>().DoBonusChestStuff();
				FadeAndLoad(PeglinSceneLoader.Scene.TREASURE);
				return;
			}
		}
		if (index < 2)
		{
			StaticGameData.chosenNextNodeIndex = 0;
		}
		else if (index > 2)
		{
			StaticGameData.chosenNextNodeIndex = StaticGameData.currentNode.ChildNodes.Length - 1;
		}
		FadeAndLoad();
	}

	public void FadeAndLoad(PeglinSceneLoader.Scene selectedScene = PeglinSceneLoader.Scene.NONE, float fadeTime = -1f)
	{
		_selectedScene = selectedScene;
		fadeTime = ((fadeTime > 0f) ? fadeTime : _fadeTime);
		Image image = GameObject.FindGameObjectWithTag("Curtain")?.GetComponent<Image>();
		if (image == null)
		{
			Debug.LogWarning("Nav controller trying to navigate with a missing curtain fade!");
			return;
		}
		image.enabled = true;
		image.DOKill();
		PauseMenu.PauseBlock = true;
		TweenerCore<Color, Color, ColorOptions> tweenerCore = image.DOFade(1f, fadeTime).From(0f);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadSelectedScene));
	}

	public void LoadSelectedScene()
	{
		PauseMenu.PauseBlock = false;
		if (_selectedScene != PeglinSceneLoader.Scene.NONE)
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
}
