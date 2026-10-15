using System.Collections;
using Loading;
using PeglinUI.ControllerSupport;
using Rewired;
using Rewired.Integration.UnityUI;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	public delegate void PauseToggle(bool pauseState);

	public static PauseToggle OnPauseToggled = (bool pauseState) =>
	{
	};

	public string mainMenuScene = "MainMenu";

	[Tooltip("Are we controlling this from another menu? It does not play nicely with submenu")]
	[SerializeField]
	private bool _isSubMenu;

	[SerializeField]
	private GameObject _pausePanel;

	[SerializeField]
	private Slider _bgmSlider;

	[SerializeField]
	private Slider _sfxSlider;

	[SerializeField]
	private Toggle _clickToAimToggle;

	[SerializeField]
	private AudioClip SfxTest;

	public static bool Paused = false;

	public static bool WasPausedAtStartOfFrame = false;

	public static bool PauseBlock = false;

	private bool pauseOnFocusLost;

	private Player _player;

	private RewiredEventSystem _rewiredEventSystem;

	private GameObject _previouslySelected;

	private bool _hasUpdatedSfx;

	private float _timeToWaitSfx;

	private void Awake()
	{
		PauseBlock = false;
		_player = ReInput.players.GetPlayer(0);
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		pauseOnFocusLost = false;
		_clickToAimToggle?.SetIsOnWithoutNotify(ControllerInputManager.RequireConfirmationToFireSetting);
		_timeToWaitSfx = SfxTest.length;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus && pauseOnFocusLost)
		{
			SetPaused(paused: true);
		}
	}

	private void Update()
	{
		if (!_isSubMenu && _player.GetButtonDown("Pause"))
		{
			SetPaused(!Paused);
		}
		if (Paused)
		{
			bool flag = false;
			if (_hasUpdatedSfx)
			{
				_hasUpdatedSfx = false;
				flag = true;
			}
			if (_timeToWaitSfx > SfxTest.length)
			{
				_timeToWaitSfx -= Time.unscaledDeltaTime;
				UIButtonClickEventDispatcher.OnUIButtonCustomSfx(SfxTest);
			}
			else if (_timeToWaitSfx <= SfxTest.length && _timeToWaitSfx > 0f)
			{
				_timeToWaitSfx -= Time.unscaledDeltaTime;
			}
			else if ((_timeToWaitSfx < 0f) & flag)
			{
				_timeToWaitSfx = SfxTest.length + 0.01f;
			}
		}
	}

	private void LateUpdate()
	{
		WasPausedAtStartOfFrame = Paused;
	}

	private void SetPaused(bool paused)
	{
		if (paused && PauseBlock)
		{
			return;
		}
		Paused = paused;
		OnPauseToggled?.Invoke(paused);
		if (paused && (bool)SettingsManager.Instance && _bgmSlider != null && _sfxSlider != null)
		{
			_bgmSlider.SetValueWithoutNotify(SettingsManager.Instance.BgmVol);
			_sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.SfxVol);
		}
		if (Paused)
		{
			SettingsManager.Instance?.InitializePreviousSettings();
		}
		_pausePanel.SetActive(Paused);
		if (_isSubMenu)
		{
			return;
		}
		if (Paused)
		{
			if (ControllerInspection.CurrentlyInspecting)
			{
				_previouslySelected = ControllerInspection.LastSelectedObject;
			}
			else
			{
				_previouslySelected = _rewiredEventSystem.currentSelectedGameObject;
			}
			_rewiredEventSystem.SetSelectedGameObject(_bgmSlider.gameObject);
		}
		else if (_previouslySelected != null)
		{
			_rewiredEventSystem.SetSelectedGameObject(_previouslySelected);
		}
	}

	public void Pause()
	{
		SetPaused(paused: true);
	}

	public void Resume()
	{
		SetPaused(paused: false);
	}

	public void ResumeViaButton()
	{
		StartCoroutine(WaitAndResume());
	}

	private IEnumerator WaitAndResume()
	{
		yield return null;
		Resume();
	}

	public void ToggleFocusPause(bool value)
	{
		pauseOnFocusLost = value;
		PlayerPrefs.SetInt("PauseOnFocusLost", value ? 1 : 0);
	}

	public void ToggleClickToFire(bool value)
	{
		ControllerInputManager.RequireConfirmationToFireSetting = value;
	}

	public void AbandonRun()
	{
		if (!StaticGameData.hasReachedBoss)
		{
			PlayerPrefs.SetInt(GameInit.BOSS_STREAK_PREF_STRING, 0);
		}
		SetPaused(paused: false);
		SaveManager.RequestDelete();
		PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MAIN_MENU);
	}

	public void QuitToMenu()
	{
		SetPaused(paused: false);
		PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MAIN_MENU);
	}

	public void QuitToDesktop()
	{
		Application.Quit();
	}

	private void OnApplicationQuit()
	{
		if (!StaticGameData.hasReachedBoss)
		{
			PlayerPrefs.SetInt(GameInit.BOSS_STREAK_PREF_STRING, 0);
		}
	}

	public void UpdateBGMVol(float val)
	{
		if (SettingsManager.Instance != null)
		{
			SettingsManager.Instance.BgmVol = val;
			SettingsManager.Instance.SaveSoundSettings();
		}
	}

	public void UpdateSFXVol(float val)
	{
		if (SettingsManager.Instance != null)
		{
			SettingsManager.Instance.SfxVol = val;
			SettingsManager.Instance.SaveSoundSettings();
			_hasUpdatedSfx = true;
		}
	}
}
