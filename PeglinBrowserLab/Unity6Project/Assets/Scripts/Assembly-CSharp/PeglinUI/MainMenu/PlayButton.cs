using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using Loading;
using Map;
using PeglinUI.ControllerSupport;
using PeglinUI.LoadoutManager;
using PeglinUtils;
using Rewired.Integration.UnityUI;
using Saving;
using Stats;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu
{
	public class PlayButton : MonoBehaviour
	{
		public bool additive;

		public bool fade;

		public Image fadePanel;

		public float screenFadeTime = 0.65f;

		public Transform buttonParent;

		[Header("Save system")]
		public LoadMapData LoadMap;

		[Header("Prefab references")]
		[SerializeField]
		private GameObject _modalPanelPrefab;

		private MapControllerSaveData _mapControllerSaveData;

		private ModalPanel _modalPanel;

		private RewiredEventSystem _rewiredEventSystem;

		[Header("GameObject Refs")]
		public CanvasGroup _mainCanvasGroup;

		public CanvasGroup _characterSelectGroup;

		public Button continueButton;

		public CruciballParticleToggle CruciballParticleSystem;

		public CharacterXpDisplayManager CharacterXpDisplayManager;

		[Header("Vars")]
		private float _mainCanvasCameraY;

		public float characterSelectCameraY = 2f;

		public float canvasFadeTime = 0.65f;

		public float characterSelectFadeTime = 0.35f;

		public float cameraMoveTime = 1.65f;

		private Button _button;

		private void Awake()
		{
			if (BGMController.instance != null)
			{
				BGMController.instance.ForceStopOneTimeJingle(fadeOut: true);
			}
			_button = GetComponent<Button>();
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			_mainCanvasCameraY = Camera.main.transform.position.y;
			if (_modalPanel == null)
			{
				_modalPanel = UnityEngine.Object.FindObjectOfType<ModalPanel>();
			}
			if (_modalPanel == null)
			{
				Canvas componentInParent = GetComponentInParent<Canvas>();
				_modalPanel = UnityEngine.Object.Instantiate(_modalPanelPrefab, componentInParent.transform).GetComponent<ModalPanel>();
			}
			StartCoroutine(WaitAndCheckForModalDialogs());
		}

		private IEnumerator WaitAndCheckForModalDialogs()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (SaveManager.SaveFileCorruptionDetector.Instance.SaveFileHasBeenCorrupted)
			{
				SaveManager.SaveFileCorruptionDetector.Instance.SaveFileHasBeenCorrupted = false;
				if (_modalPanel != null)
				{
					_modalPanel.Notify(LocalizationManager.GetTranslation("Menu/save_data_corrupted"));
				}
				else
				{
					Debug.LogWarning("Save data was corrupted. The previous save data was backed up and a new save file will be created.");
				}
			}
			if (SaveManager.SaveFileCorruptionDetector.Instance.CloudSavesDisabled)
			{
				SaveManager.SaveFileCorruptionDetector.Instance.CloudSavesDisabled = false;
				if (_modalPanel != null)
				{
					_modalPanel.NotifyWithDismiss(LocalizationManager.GetTranslation("Menu/cloud_saves_disabled"), "CLOUD_SAVES_DISMISS_CHECKBOX");
				}
				else
				{
					Debug.LogWarning("Cloud saves have been disabled because we've identified an issue with the Google API response");
				}
			}
		}

		private void OnEnable()
		{
			bool flag = SaveManager.Instance.HasRunSaveData();
			if (flag)
			{
				DataSerializer.LoadFile(DataSerializer.SaveType.RUN);
				_mapControllerSaveData = DataSerializer.Load<SaveObjectData>("MapController", DataSerializer.SaveType.RUN) as MapControllerSaveData;
				StaticGameData.CurrentRunStats = LoadRunStats();
			}
			else
			{
				_rewiredEventSystem.firstSelectedGameObject = base.gameObject;
			}
			continueButton.gameObject.SetActive(flag);
			GetComponent<DOTweenSinWaveText>().enabled = !flag;
		}

		private RunStats LoadRunStats()
		{
			CurrentRunStatsSaveData currentRunStatsSaveData = (CurrentRunStatsSaveData)DataSerializer.Load<SaveObjectData>(CurrentRunStatsSaveData.KEY, DataSerializer.SaveType.RUN);
			if (currentRunStatsSaveData == null)
			{
				Debug.LogWarning("No runstats save data found for run, creating new one from scratch...");
				return new RunStats();
			}
			return new RunStats(currentRunStatsSaveData.runStatsSaveData);
		}

		public void MovetoCharacterSelect()
		{
			Button[] componentsInChildren = buttonParent.GetComponentsInChildren<Button>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].interactable = false;
			}
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = sequence.Append(_mainCanvasGroup.DOFade(0f, canvasFadeTime));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(SwitchToRunConfigCanvas));
			sequence.Append(Camera.main.transform.DOMoveY(characterSelectCameraY, cameraMoveTime));
			sequence.Play();
			if (StaticGameData.CurrentRunStats != null)
			{
				Debug.Log("Transitioning to Character Select, updating persistent class unlock progress: ");
				if (PersistentPlayerData.Instance.BalladinProgress < PersistentPlayerData.BALLADIN_PROGRESS_MAX)
				{
					PersistentPlayerData.Instance.BalladinProgress += StaticGameData.CurrentRunStats.balladinProgress;
					CharacterXpDisplayManager.BalladinStagingProgress = StaticGameData.CurrentRunStats.balladinProgress;
					Debug.Log($"Updated Balladin Progress {PersistentPlayerData.Instance.BalladinProgress} / {PersistentPlayerData.BALLADIN_PROGRESS_MAX}");
				}
				if (PersistentPlayerData.Instance.RoundrelProgress < PersistentPlayerData.ROUNDREL_PROGRESS_MAX)
				{
					PersistentPlayerData.Instance.RoundrelProgress += StaticGameData.CurrentRunStats.roundrelProgress;
					CharacterXpDisplayManager.RoundrelStagingProgress = StaticGameData.CurrentRunStats.roundrelProgress;
					Debug.Log($"Updated Roundrel Progress {PersistentPlayerData.Instance.RoundrelProgress} / {PersistentPlayerData.ROUNDREL_PROGRESS_MAX}");
				}
				if (PersistentPlayerData.Instance.SpinventorProgress < PersistentPlayerData.SPINVENTOR_PROGRESS_MAX)
				{
					PersistentPlayerData.Instance.SpinventorProgress += StaticGameData.CurrentRunStats.spinventorProgress;
					CharacterXpDisplayManager.SpinventorStagingProgress = StaticGameData.CurrentRunStats.spinventorProgress;
					Debug.Log($"Updated Spinventor Progress {PersistentPlayerData.Instance.SpinventorProgress} / {PersistentPlayerData.SPINVENTOR_PROGRESS_MAX}");
				}
				StaticGameData.CurrentRunStats.balladinProgress = 0;
				StaticGameData.CurrentRunStats.roundrelProgress = 0;
				StaticGameData.CurrentRunStats.spinventorProgress = 0;
				PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
			}
		}

		public void ReturnFromCharacterSelect()
		{
			Button[] componentsInChildren = buttonParent.GetComponentsInChildren<Button>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].interactable = true;
			}
			CruciballParticleSystem.DisableSystem();
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = sequence.Append(_characterSelectGroup.DOFade(0f, characterSelectFadeTime));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(SwitchToMainCanvas));
			sequence.Append(Camera.main.transform.DOMoveY(_mainCanvasCameraY, cameraMoveTime));
			sequence.Play();
		}

		private void SwitchToRunConfigCanvas()
		{
			_mainCanvasGroup.gameObject.SetActive(value: false);
			_characterSelectGroup.gameObject.SetActive(value: true);
			_characterSelectGroup.GetComponent<CanvasGroup>().DOFade(1f, canvasFadeTime).From(0f);
			_rewiredEventSystem.SetSelectedGameObject(null);
			if (PersistentPlayerData.Instance.CruciballActive && !ReleaseDemoManager.ReleaseDemoActive)
			{
				CruciballParticleSystem.EnableSystem();
			}
		}

		private void SwitchToMainCanvas()
		{
			_mainCanvasGroup.gameObject.SetActive(value: true);
			_mainCanvasGroup.GetComponent<CanvasGroup>().DOFade(1f, canvasFadeTime).From(0f);
			_characterSelectGroup.gameObject.SetActive(value: false);
			_rewiredEventSystem.SetSelectedGameObject(base.gameObject);
		}

		public void LoadNextScene()
		{
			Load();
		}

		public void ConfirmRunConfigAndStartGame()
		{
			SaveManager.RequestDelete();
			SetUpNewGameOnLoadMapData();
			Load();
		}

		public void ContinueClicked()
		{
			LoadMap.SceneToLoad = (PeglinSceneLoader.Scene)_mapControllerSaveData.Scene;
			LoadMap.NewGame = false;
			LoadMap.ContinueGame = true;
			if (StaticGameData.CurrentRunStats == null)
			{
				StaticGameData.CurrentRunStats = new RunStats();
			}
			else
			{
				StaticGameData.CurrentRunStats.Reset();
			}
			PersistentPlayerData.Instance.ClobberBestiaryDataWithMemoryValues();
			Load();
		}

		private void SaveModalCanceled()
		{
			_rewiredEventSystem.GetComponent<ControllerMenuHookup>().LastSelected = base.gameObject;
			_rewiredEventSystem.SetSelectedGameObject(base.gameObject);
		}

		private void SetUpNewGameOnLoadMapData()
		{
			LoadMap.NewGame = true;
			LoadMap.ContinueGame = false;
			LoadMap.SceneToLoad = LoadMap.NewGameScene;
			if (StaticGameData.CurrentRunStats == null)
			{
				StaticGameData.CurrentRunStats = new RunStats();
			}
			else
			{
				StaticGameData.CurrentRunStats.Reset();
			}
			StaticGameData.CurrentRunStats.runTimerSw.Reset();
			StaticGameData.CurrentRunStats.runTimerSw.Start();
			StaticGameData.CurrentRunStats.startDate = DateTime.Now;
			PeglinUI.LoadoutManager.LoadoutManager loadoutManager = UnityEngine.Object.FindObjectOfType<PeglinUI.LoadoutManager.LoadoutManager>();
			StaticGameData.CurrentRunStats.isCustomRun = loadoutManager.hasCustomLoadout || loadoutManager.seedInputPopup.hasCustomSeed;
		}

		public void Load()
		{
			if (fade)
			{
				fadePanel.gameObject.SetActive(value: true);
				fadePanel.enabled = true;
				TweenerCore<Color, Color, ColorOptions> tweenerCore = fadePanel.DOFade(1f, screenFadeTime).From(0f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
				{
					DOTween.KillAll();
					DoLoad();
				}));
			}
			else
			{
				DoLoad();
			}
		}

		private void DoLoad()
		{
			_ = additive;
			PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.POST_MAIN_MENU);
		}
	}
}
