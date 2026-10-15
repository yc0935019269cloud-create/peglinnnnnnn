using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Cruciball;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Loading;
using Peglin.ClassSystem;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using PeglinUtils;
using Relics;
using Saving;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInit : MonoBehaviour
{
	[Header("Player SO Vars")]
	public FloatVariable playerHealth;

	public FloatVariable maxPlayerHealth;

	public FloatVariable reductionToMaxPlayerHealth;

	[Header("Managers")]
	[SerializeField]
	private CruciballManager _cruciballManager;

	[SerializeField]
	private DeckData _initialDeck;

	[SerializeField]
	private DeckManager _deckManager;

	[SerializeField]
	private RelicManager _relicManager;

	[SerializeField]
	private SecretSeedManager _secretSeedManager;

	[Header("SceneObjects")]
	[SerializeField]
	private GameObject _chooseRelicCanvas;

	[SerializeField]
	private RelicIcon[] _chooseRelicIcons;

	[SerializeField]
	private UpgradeConfirmationRelic _upgradeConfirmationRelic;

	[Header("Load Data")]
	[SerializeField]
	private LoadMapData LoadData;

	[SerializeField]
	private float _chooseRelicCanvasFadeInTime = 2f;

	[SerializeField]
	private float _chooseRelicCanvasFadeOutTime = 1f;

	private List<Relic> _chosenRelics;

	private bool _relicsAvailable = true;

	private int _chosenIndex;

	public static string BOSS_STREAK_PREF_STRING = "ReachedBossStreak";

	private const int NUMBER_OF_RELICS_TO_OFFER = 3;

	private const RelicRarity RELICS_RARITY_TO_OFFER = RelicRarity.COMMON;

	private void Start()
	{
		Input.multiTouchEnabled = false;
		if (playerHealth == null || maxPlayerHealth == null)
		{
			Debug.LogError("Player health vars aren't set in GameInit!");
			return;
		}
		maxPlayerHealth.Reset();
		reductionToMaxPlayerHealth.Reset();
		float value = maxPlayerHealth.Value;
		if (_cruciballManager.ShouldReduceMaxHP())
		{
			maxPlayerHealth.Set(Mathf.RoundToInt(value * 0.85f));
		}
		playerHealth.Set(maxPlayerHealth.Value);
		CanvasGroup component = GameObject.FindGameObjectWithTag("PersistentUI").GetComponent<CanvasGroup>();
		StaticGameData.hasReachedBoss = false;
		StaticGameData.specificNodeOverride = null;
		StaticGameData.relicRarityOverride = RelicRarity.NONE;
		int num;
		int num2;
		if (LoadData.NewGame)
		{
			if (StaticGameData.currentSeed == string.Empty)
			{
				StaticGameData.currentSeed = SeedUtils.CreateNewRandomSeed(_secretSeedManager);
			}
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.seed = StaticGameData.currentSeed;
				StaticGameData.CurrentRunStats.isRunActive = true;
			}
			UnityEngine.Random.InitState(SeedUtils.ConvertSeedFromSymbolsToInt(StaticGameData.currentSeed));
			_relicManager.Reset();
			_relicManager.SetupInternalRelicPools();
			_deckManager.SeedOrbPools();
			_secretSeedManager.CheckAndSetUpSecretSeed();
			_deckManager.SeedHaglinsSatchel();
			_deckManager.SeedAssemballRerolls();
			_relicManager.SeedOrbertsStoryChests();
			PlayerHealthController.DamageEvasionRandomState = new SerializableRandomState(UnityEngine.Random.Range(int.MinValue, int.MaxValue), 0);
			StaticGameData.totalFloorCount = 0;
			if (StaticGameData.StartingOrbs != null)
			{
				if (StaticGameData.CurrentRunStats != null)
				{
					GameObject[] startingOrbs = StaticGameData.StartingOrbs;
					foreach (GameObject gameObject in startingOrbs)
					{
						if (!gameObject.GetComponent<AssemballComponent>())
						{
							StaticGameData.CurrentRunStats.AddOrbToDeck(gameObject.GetComponent<Attack>(), starting: true);
						}
					}
				}
				_deckManager.InstantiateDeck(new List<GameObject>(StaticGameData.StartingOrbs));
			}
			else
			{
				_deckManager.InstantiateDeck();
			}
			if (StaticGameData.StartingRelics != null)
			{
				Relic[] startingRelics = StaticGameData.StartingRelics;
				foreach (Relic relic in startingRelics)
				{
					_relicManager.AddRelic(relic);
				}
			}
			for (int j = 0; j < _cruciballManager.AdditionalStarterStones(); j++)
			{
				_deckManager.AddOrbToStarterDeck(_cruciballManager.stonePrefab.gameObject);
			}
			if (_cruciballManager.ShouldAddCursedOrb())
			{
				_deckManager.AddOrbToStarterDeck(_cruciballManager.dudOrb1Prefab.gameObject);
			}
			if (_cruciballManager.ShouldAddUnremovableCursedOrb())
			{
				_deckManager.AddOrbToStarterDeck(_cruciballManager.unremovalbeDudOrb1Prefab.gameObject);
			}
			if (HasPlayerBeatenTheGame())
			{
				num = (ReleaseDemoManager.ReleaseDemoActive ? 1 : 0);
				if (num == 0)
				{
					num2 = 3;
					goto IL_02bf;
				}
			}
			else
			{
				num = 1;
			}
			num2 = 1;
			goto IL_02bf;
		}
		_deckManager.InstantiateDeck(_initialDeck.Balls);
		_relicManager.Reset();
		component.alpha = 1f;
		LoadMapScene();
		return;
		IL_02bf:
		int num3 = num2;
		List<Relic> list = ((num != 0) ? new List<Relic> { _relicManager.GetRelicFromLimitedList() } : new List<Relic>(_relicManager.GetMultipleRelicsOffOfQueue(3, RelicRarity.COMMON)));
		List<RelicRarity> list2 = new List<RelicRarity>();
		for (int k = 0; k < num3; k++)
		{
			list2.Add(RelicRarity.COMMON);
		}
		_chosenRelics = new List<Relic>(_relicManager.CheckAndUpdateSeededRelicOffers(list.ToArray(), list2.ToArray()));
		if (_chosenRelics.Count <= 0 || (_chosenRelics.Count == 1 && _chosenRelics[0] == _relicManager.consolationPrize))
		{
			SkipRelic();
			StaticGameData.nextMapSeedState = UnityEngine.Random.state;
			return;
		}
		_chooseRelicCanvas.GetComponent<CanvasGroup>().DOFade(1f, _chooseRelicCanvasFadeInTime);
		component.DOFade(1f, _chooseRelicCanvasFadeInTime).From(0f);
		for (int l = 0; l < _chooseRelicIcons.Length; l++)
		{
			if (_chosenRelics.Count <= l || _chosenRelics[l].effect == RelicEffect.NONE)
			{
				_chooseRelicIcons[l].transform.parent.gameObject.SetActive(value: false);
			}
			else
			{
				_chooseRelicIcons[l].SetRelic(_chosenRelics[l]);
			}
		}
		StaticGameData.nextMapSeedState = UnityEngine.Random.state;
		StartCoroutine(WaitAndSelectFirstRelic());
	}

	private bool HasPlayerBeatenTheGame()
	{
		Dictionary<Class, int> cruciballLevels = PersistentPlayerData.Instance.CruciballLevels;
		if (cruciballLevels == null)
		{
			return false;
		}
		foreach (int value in cruciballLevels.Values)
		{
			if (value > 0)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator WaitAndSelectFirstRelic()
	{
		yield return null;
		if (!ControllerInputManager.IsPointerBased && _chooseRelicIcons.Count() > 0)
		{
			EventSystem.current.SetSelectedGameObject(_chooseRelicIcons[0].transform.parent.gameObject);
		}
	}

	public void ChooseRelic(int chosenIndex = 0)
	{
		if (_relicsAvailable)
		{
			_chosenIndex = chosenIndex;
			_upgradeConfirmationRelic.gameObject.SetActive(value: true);
			_upgradeConfirmationRelic.Populate(_chosenRelics[chosenIndex], 0);
			_upgradeConfirmationRelic.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = _chooseRelicIcons[chosenIndex].transform.parent.gameObject;
		}
	}

	public void AddRelic()
	{
		if (!ControllerInspection.CurrentlyInspecting && _relicsAvailable)
		{
			_upgradeConfirmationRelic.gameObject.SetActive(value: false);
			_relicManager.AddRelic(_chosenRelics[_chosenIndex]);
			UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddRelic);
			SkipRelic();
		}
	}

	public void SkipRelicButton()
	{
		if (!ControllerInspection.CurrentlyInspecting)
		{
			UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.Close);
			SkipRelic();
		}
	}

	private void SkipRelic()
	{
		RelicIcon.HideTooltip();
		_relicsAvailable = false;
		CanvasGroup component = _chooseRelicCanvas.GetComponent<CanvasGroup>();
		component.GetComponent<GraphicRaycaster>().enabled = false;
		TweenerCore<float, float, FloatOptions> tweenerCore = component.DOFade(0f, _chooseRelicCanvasFadeOutTime);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LoadMapScene));
	}

	private void LoadMapScene()
	{
		StaticGameData.seedSet = true;
		PeglinSceneLoader.Instance.LoadScene(LoadData.SceneToLoad);
	}
}
