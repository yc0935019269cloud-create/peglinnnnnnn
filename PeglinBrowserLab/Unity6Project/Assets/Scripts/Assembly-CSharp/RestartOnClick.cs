using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using Loading;
using Map;
using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RestartOnClick : MonoBehaviour
{
	private delegate void LoadMethod();

	[FormerlySerializedAs("LoadDebug")]
	public bool LoadSceneDirect;

	[FormerlySerializedAs("DebugScene")]
	public PeglinSceneLoader.Scene directScene = PeglinSceneLoader.Scene.MAIN_MENU;

	public bool additive;

	public bool fade;

	public Image fadePanel;

	public float fadeTime = 0.6f;

	public bool newGame = true;

	[Header("Save system")]
	public LoadMapData LoadMap;

	[Header("Prefab references")]
	[SerializeField]
	private GameObject _modalPanelPrefab;

	private MapControllerSaveData _mapControllerSaveData;

	private ModalPanel _modalPanel;

	private RewiredEventSystem _rewiredEventSystem;

	private void Awake()
	{
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		if (_modalPanel == null)
		{
			_modalPanel = UnityEngine.Object.FindObjectOfType<ModalPanel>();
		}
		if (_modalPanel == null)
		{
			Canvas componentInParent = GetComponentInParent<Canvas>();
			_modalPanel = UnityEngine.Object.Instantiate(_modalPanelPrefab, componentInParent.transform).GetComponent<ModalPanel>();
		}
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
	}

	private void OnEnable()
	{
		if (SaveManager.Instance.HasRunSaveData())
		{
			_mapControllerSaveData = DataSerializer.Load<SaveObjectData>("MapController", DataSerializer.SaveType.RUN) as MapControllerSaveData;
		}
	}

	public void DeleteSaveAndLoadScene()
	{
		SaveManager.RequestDelete();
		ButtonClicked();
	}

	public void ButtonClicked()
	{
		if (LoadSceneDirect)
		{
			LoadMap.SceneToLoad = directScene;
			LoadMap.NewGame = true;
			LoadMap.ContinueGame = false;
			Load();
		}
		else if (_mapControllerSaveData != null)
		{
			_modalPanel.Choice(LocalizationManager.GetTranslation("Menu/continue_game"), ContinueClicked, ContinueCanceled);
		}
		else
		{
			SetUpNewGameOnLoadMapData();
			Load();
		}
	}

	public void LoadNextScene()
	{
		Load();
	}

	private void ContinueClicked()
	{
		LoadMap.SceneToLoad = (PeglinSceneLoader.Scene)_mapControllerSaveData.Scene;
		LoadMap.NewGame = false;
		LoadMap.ContinueGame = true;
		Load();
	}

	private void ContinueCanceled()
	{
		_modalPanel.Choice(LocalizationManager.GetTranslation("Menu/start_new_game_delete_old_data"), StartNewGame, SaveModalCanceled);
	}

	private void SaveModalCanceled()
	{
		_rewiredEventSystem.GetComponent<ControllerMenuHookup>().LastSelected = base.gameObject;
		_rewiredEventSystem.SetSelectedGameObject(base.gameObject);
	}

	private void StartNewGame()
	{
		SaveManager.RequestDelete();
		SetUpNewGameOnLoadMapData();
		Load();
	}

	private void SetUpNewGameOnLoadMapData()
	{
		LoadMap.NewGame = true;
		LoadMap.ContinueGame = false;
		LoadMap.SceneToLoad = LoadMap.NewGameScene;
	}

	public void Load()
	{
		if (fade)
		{
			fadePanel.gameObject.SetActive(value: true);
			fadePanel.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = fadePanel.DOFade(1f, fadeTime).From(0f);
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

	public void StartNewGameFromGameOverScreen()
	{
		SetUpNewGameOnLoadMapData();
		if (fade)
		{
			fadePanel.gameObject.SetActive(value: true);
			fadePanel.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = fadePanel.DOFade(1f, fadeTime).From(0f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				DOTween.KillAll();
				DoLoadGameInit();
			}));
		}
		else
		{
			DoLoadGameInit();
		}
	}

	public void StartNewGameFromWinScreen()
	{
		StartNewGameFromGameOverScreen();
	}

	private void DoLoad()
	{
		LoadSceneMode mode = (additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		if (LoadSceneDirect)
		{
			PeglinSceneLoader.Instance.LoadScene(directScene, mode);
		}
		else
		{
			Debug.LogError("Attempted to load scenes by build index!");
		}
	}

	private void DoLoadGameInit()
	{
		LoadSceneMode mode = (additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		PeglinSceneLoader.Instance.LoadScene(directScene, mode);
	}
}
