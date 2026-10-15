using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loading
{
	public class PeglinSceneLoader : MonoBehaviour
	{
		public enum Scene
		{
			NONE = 0,
			MAIN_MENU = 1,
			POST_MAIN_MENU = 2,
			FOREST_MAP = 3,
			FOREST_WIN = 4,
			CASTLE_MAP = 5,
			CASTLE_WIN = 6,
			MINES_MAP = 7,
			MINES_WIN = 8,
			CORE_MAP = 15,
			CORE_WIN = 16,
			BATTLE = 9,
			TEXT_SCENARIO = 10,
			TREASURE = 11,
			PEG_MINIGAME = 12,
			SHOP_SCENARIO = 13,
			RUN_SUMMARY = 14
		}

		public const string POST_MAIN_MENU_SCENE = "PostMainMenu";

		public Image fadePanel;

		public GameObject animatingSprite;

		public GameObject slider;

		public GameObject logger;

		public GraphicRaycaster rc;

		public bool isLoading;

		private Scene _queuedScene;

		private bool _isSceneQueued;

		private static PeglinSceneLoader _instance;

		public static PeglinSceneLoader Instance
		{
			get
			{
				if (_instance == null)
				{
					return new GameObject("PeglinSceneLoader").AddComponent<PeglinSceneLoader>();
				}
				return _instance;
			}
		}

		private void OnEnable()
		{
			Object.DontDestroyOnLoad(this);
			_instance = this;
		}

		private void Update()
		{
			if (_isSceneQueued && !isLoading)
			{
				_isSceneQueued = false;
				LoadScene(_queuedScene);
			}
		}

		public void LoadScene(Scene scene, LoadSceneMode mode = LoadSceneMode.Single, bool queueScene = false, float fadeDuration = 0.5f)
		{
			if (isLoading & queueScene)
			{
				_queuedScene = scene;
				_isSceneQueued = true;
			}
			else
			{
				if (isLoading)
				{
					return;
				}
				PauseMenu.PauseBlock = true;
				isLoading = true;
				Sequence sequence = DOTween.Sequence();
				sequence.Append(fadePanel.DOFade(1f, fadeDuration)).AppendCallback(() =>
				{
					if (animatingSprite != null)
					{
						animatingSprite.SetActive(value: true);
					}
				}).AppendCallback(() =>
				{
					StartCoroutine(LoadSceneAsync(SceneToString(scene), mode));
				});
				sequence.Play();
			}
		}

		public void LoadScene(int sceneId, LoadSceneMode mode = LoadSceneMode.Single)
		{
			if (!isLoading)
			{
				PauseMenu.PauseBlock = true;
				Sequence sequence = DOTween.Sequence();
				sequence.Append(fadePanel.DOFade(1f, 0.5f)).AppendCallback(() =>
				{
					animatingSprite.SetActive(value: true);
				}).AppendCallback(() =>
				{
					StartCoroutine(LoadSceneAsync(sceneId, mode));
				});
				sequence.Play();
			}
		}

		private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode)
		{
			AsyncOperationHandle<SceneInstance> asyncOperationHandle = Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad: false);
			asyncOperationHandle.Completed += LoadSceneInstance;
			yield return null;
		}

		private IEnumerator LoadSceneAsync(int sceneID, LoadSceneMode mode)
		{
			AsyncOperationHandle<SceneInstance> operation = Addressables.LoadSceneAsync(sceneID, mode, activateOnLoad: false);
			operation.Completed += LoadSceneInstance;
			LoadSceneInstance(operation);
			yield return null;
		}

		private void LoadSceneInstance(AsyncOperationHandle<SceneInstance> operation)
		{
			operation.Result.ActivateAsync().completed += (AsyncOperation asyncOperation) =>
			{
				if (asyncOperation.isDone)
				{
					HideLoadingScreen();
				}
			};
		}

		private void HideLoadingScreen()
		{
			if (animatingSprite != null)
			{
				animatingSprite.SetActive(value: false);
			}
			if (fadePanel != null)
			{
				fadePanel.DOFade(0f, 0.5f);
			}
			if (slider != null)
			{
				Object.Destroy(slider.gameObject);
			}
			if (logger != null)
			{
				Object.Destroy(logger);
			}
			if (rc != null)
			{
				Object.Destroy(rc);
			}
			isLoading = false;
			PauseMenu.PauseBlock = false;
		}

		private static string SceneToString(Scene scene)
		{
			if (scene == Scene.NONE)
			{
				Debug.LogError("A scene value was not set properly");
				return "MainMenu";
			}
			return scene switch
			{
				Scene.BATTLE => "Battle", 
				Scene.MAIN_MENU => "MainMenu", 
				Scene.MINES_MAP => "MinesMap", 
				Scene.MINES_WIN => "FinalWinScene", 
				Scene.CORE_MAP => "CoreMap", 
				Scene.CORE_WIN => "CoreWinScene", 
				Scene.TREASURE => "Treasure", 
				Scene.CASTLE_MAP => "CastleMap", 
				Scene.CASTLE_WIN => "CastleWinScene", 
				Scene.FOREST_MAP => "ForestMap", 
				Scene.FOREST_WIN => "ForestWinScene", 
				Scene.RUN_SUMMARY => "RunSummary", 
				Scene.PEG_MINIGAME => "PegMinigame", 
				Scene.NONE => "PreMainMenu", 
				Scene.POST_MAIN_MENU => "PostMainMenu", 
				Scene.SHOP_SCENARIO => "ShopScenario", 
				Scene.TEXT_SCENARIO => "TextScenario", 
				_ => "MainMenu", 
			};
		}
	}
}
