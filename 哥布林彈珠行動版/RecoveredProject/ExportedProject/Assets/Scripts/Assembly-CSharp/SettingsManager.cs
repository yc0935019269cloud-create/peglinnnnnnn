using System;
using I2.Loc;
using PeglinUI.ControllerSupport;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
	public delegate void SettingsChangedEvent();

	public enum IntensityLevel
	{
		OFF = 0,
		LOW = 1,
		HIGH = 2
	}

	public enum ButtonPromptsStyle
	{
		Off = 0,
		Static = 1,
		Animated = 2
	}

	public enum PCControllerInputTypes
	{
		Auto = 0,
		Xbox = 1,
		Playstation = 2,
		Switch = 3
	}

	private static SettingsManager _instance;

	public static SettingsChangedEvent OnSettingsChangedEvent;

	private Resolution _resolution;

	private FullScreenMode _screenMode;

	private int _targetFramerate;

	private bool _batterySaveMode;

	private bool _vsync;

	private bool _clickToAim;

	private string _language;

	private float _sfxVol;

	private float _bgmVol;

	private float _screenshakeIntensity;

	private float _rumbleIntensity;

	private IntensityLevel _vignetteLevel;

	public IntensityLevel _screenshakeLevel;

	private ButtonPromptsStyle _animateButtonPrompts;

	private PCControllerInputTypes _controllerInput;

	private Resolution _lastResolutionUsed;

	private FullScreenMode _lastScreenModeUsed = FullScreenMode.FullScreenWindow;

	private int _lastTargetFramerateUsed;

	private bool _lastVsyncUsed;

	private bool _lastClickToAimUsed;

	private string _lastLanguageUsed;

	private float _lastSFXVol;

	private float _lastBGMVol;

	private float _lastScreenshakeIntensity;

	private bool _lastSpeedupToggle;

	private float _lastSpeedupLevel;

	private bool _lastBatterySaveMode;

	private ButtonPromptsStyle _lastAnimateButtonPrompts;

	private IntensityLevel _lastVignetteIntensity;

	private PCControllerInputTypes _lastControllerInput;

	private bool _lastUseHardwareCursor;

	private bool _lastInstantLootAndCardDrops;

	private bool _lastShowCruciballCompletionOnTooltips;

	public const string KEY_CLICK_TO_AIM = "PauseOnFocusLost";

	public const string KEY_SFX_VOL = "SFXVol";

	public const string KEY_BGM_VOL = "BGMVol";

	private const string KEY_RES_X = "ResolutionX";

	private const string KEY_RES_Y = "ResolutionY";

	private const string KEY_SCREEN_MODE = "ScreenMode";

	private const string KEY_VSYNC = "VSync";

	private const string KEY_SCREENSHAKE_INTENSITY = "ScreenshakeIntensity";

	private const string KEY_RUMBLE_INTENSITY = "RumbleIntensity";

	private const string KEY_VIGNETTE_INTENSITY = "VignetteIntensity";

	private const string KEY_SPEEDUP_LEVEL = "SpeedupLevel";

	private const string KEY_SPEEDUP_TOGGLE = "SpeedupToggle";

	private const string KEY_CONTROLLER_TYPE = "ControllerType";

	private const string KEY_ANIMATE_BUTTON_PROMPTS = "AnimateButtonPrompts";

	private const string KEY_USE_HARDWARE_CURSOR = "UseHardwareCursor";

	private const string KEY_INSTANT_LOOT_AND_CARD_DROPS = "InstantLootAndCardDrops";

	private const string KEY_SHOW_CRUCIBALL_COMPLETION_ON_TOOLTIPS = "ShowCruciballCompletionOnTooltips";

	private const string KEY_BATTERY_SAVE_MODE = "BatterySaveMode";

	public static SettingsManager Instance => _instance;

	public Resolution Resolution
	{
		get
		{
			return _resolution;
		}
		set
		{
			_resolution = value;
		}
	}

	public FullScreenMode ScreenMode
	{
		get
		{
			return _screenMode;
		}
		set
		{
			_screenMode = value;
		}
	}

	public int TargetFramerate
	{
		get
		{
			return _targetFramerate;
		}
		set
		{
			_targetFramerate = value;
		}
	}

	public bool BatterySaveMode
	{
		get
		{
			return _batterySaveMode;
		}
		set
		{
			_batterySaveMode = value;
		}
	}

	public bool Vsync
	{
		get
		{
			return _vsync;
		}
		set
		{
			_vsync = value;
		}
	}

	public bool ClickToAim
	{
		get
		{
			return _clickToAim;
		}
		set
		{
			_clickToAim = value;
		}
	}

	public string Language
	{
		get
		{
			return _language;
		}
		set
		{
			_language = value;
		}
	}

	public float SfxVol
	{
		get
		{
			return _sfxVol;
		}
		set
		{
			_sfxVol = value;
			BGMController.instance.sfxVol = _sfxVol;
		}
	}

	public float BgmVol
	{
		get
		{
			return _bgmVol;
		}
		set
		{
			_bgmVol = value;
			BGMController.instance.bgmVol = _bgmVol;
		}
	}

	public float ScreenshakeInstensity
	{
		get
		{
			return _screenshakeIntensity;
		}
		set
		{
			_screenshakeIntensity = value;
		}
	}

	public float RumbleInstensity
	{
		get
		{
			return _rumbleIntensity;
		}
		set
		{
			_rumbleIntensity = value;
		}
	}

	public IntensityLevel VignetteIntensity
	{
		get
		{
			return _vignetteLevel;
		}
		set
		{
			_vignetteLevel = value;
		}
	}

	public IntensityLevel ScreenshakeLevel
	{
		get
		{
			return _screenshakeLevel;
		}
		set
		{
			_screenshakeLevel = value;
		}
	}

	public ButtonPromptsStyle AnimateButtonPrompts
	{
		get
		{
			return _animateButtonPrompts;
		}
		set
		{
			_animateButtonPrompts = value;
		}
	}

	public float SpeedupLevel { get; set; }

	public bool SpeedupToggle { get; set; }

	public bool UseHardwareCursor { get; set; }

	public bool InstantLootAndCardDrops { get; set; }

	public bool ShowCruciballCompletionOnTooltips { get; set; }

	public PCControllerInputTypes ControllerInput
	{
		get
		{
			return _controllerInput;
		}
		set
		{
			_controllerInput = value;
		}
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		int num = PlayerPrefs.GetInt("ResolutionX", 0);
		int num2 = PlayerPrefs.GetInt("ResolutionY", 0);
		int screenMode = PlayerPrefs.GetInt("ScreenMode", 0);
		int num3 = PlayerPrefs.GetInt("VSync", 1);
		float screenshakeInstensity = PlayerPrefs.GetFloat("ScreenshakeIntensity", 0.8f);
		float rumbleInstensity = PlayerPrefs.GetFloat("RumbleIntensity", 1f);
		int vignetteIntensity = Mathf.Clamp(PlayerPrefs.GetInt("VignetteIntensity", 2), 0, 2);
		float speedupLevel = Mathf.Max(PlayerPrefs.GetFloat("SpeedupLevel", 2f), 1.5f);
		float num4 = PlayerPrefs.GetInt("SpeedupToggle", 1);
		int controllerInput = PlayerPrefs.GetInt("ControllerType", 0);
		int animateButtonPrompts = PlayerPrefs.GetInt("AnimateButtonPrompts", 2);
		int num5 = PlayerPrefs.GetInt("UseHardwareCursor", 0);
		int num6 = PlayerPrefs.GetInt("InstantLootAndCardDrops", 0);
		int num7 = PlayerPrefs.GetInt("ShowCruciballCompletionOnTooltips", 1);
		if (num == 0 && num2 == 0)
		{
			_resolution = Screen.resolutions[Screen.resolutions.Length - 1];
			_screenMode = FullScreenMode.FullScreenWindow;
		}
		else
		{
			_resolution.width = num;
			_resolution.height = num2;
			_screenMode = (FullScreenMode)screenMode;
			int refreshRate = ((_screenMode == FullScreenMode.FullScreenWindow) ? GetMaxRefreshRateForResolution(_resolution, reported: true) : Screen.currentResolution.refreshRate);
			_resolution.refreshRate = refreshRate;
		}
		SetResolutionIfNeeded();
		ClickToAim = PlayerPrefs.GetInt("PauseOnFocusLost", 0) == 1;
		BatterySaveMode = false;
		Application.targetFrameRate = -1;
		TargetFramerate = Application.targetFrameRate;
		Vsync = num3 == 1;
		QualitySettings.vSyncCount = num3;
		ScreenshakeInstensity = screenshakeInstensity;
		RumbleInstensity = rumbleInstensity;
		VignetteIntensity = (IntensityLevel)vignetteIntensity;
		Language = LocalizationManager.CurrentLanguage;
		SpeedupLevel = speedupLevel;
		SpeedupToggle = (double)Math.Abs(num4 - 1f) < 0.01;
		ControllerInput = (PCControllerInputTypes)controllerInput;
		AnimateButtonPrompts = (ButtonPromptsStyle)animateButtonPrompts;
		UseHardwareCursor = num5 > 0;
		InstantLootAndCardDrops = num6 > 0;
		ShowCruciballCompletionOnTooltips = num7 > 0;
	}

	private void Start()
	{
		float bgmVol = PlayerPrefs.GetFloat("BGMVol", 1f);
		float sfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);
		BgmVol = bgmVol;
		SfxVol = sfxVol;
		InitializePreviousSettings();
		SaveSettings();
	}

	public static int GetMaxRefreshRateForResolution(Resolution res, bool reported)
	{
		int num = -1;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution r = resolutions[i];
			if (r.width != res.width || r.height != res.height)
			{
				continue;
			}
			if (reported)
			{
				int reportedRefreshRate = GetReportedRefreshRate(r);
				if (reportedRefreshRate >= num)
				{
					num = reportedRefreshRate;
				}
			}
			else if (res.refreshRate >= num)
			{
				num = r.refreshRate;
			}
		}
		return num;
	}

	public static int GetReportedRefreshRate(Resolution r)
	{
		int num = r.refreshRate;
		if (num % 2 != 0)
		{
			num++;
		}
		return num;
	}

	private void SetResolutionIfNeeded()
	{
		Resolution r = default;
		if (_screenMode == FullScreenMode.Windowed)
		{
			r.width = Screen.width;
			r.height = Screen.height;
			r.refreshRate = Screen.currentResolution.refreshRate;
		}
		else
		{
			r = Screen.currentResolution;
		}
		if (DisplayManager.AreResolutionsDifferent(r, _resolution))
		{
			Screen.SetResolution(_resolution.width, _resolution.height, _screenMode);
		}
	}

	public void ApplySettings()
	{
		_screenMode = ScreenMode;
		Screen.fullScreenMode = ScreenMode;
		QualitySettings.vSyncCount = (Vsync ? 1 : 0);
		SetResolutionIfNeeded();
		ControllerInputManager.RequireConfirmationToFireSetting = ClickToAim;
		LocalizationManager.SetLanguageAndCode(Language, LocalizationManager.GetLanguageCode(Language));
		SaveSettings();
		InitializePreviousSettings();
	}

	public void SaveSettings()
	{
		PlayerPrefs.SetInt("ResolutionX", Resolution.width);
		PlayerPrefs.SetInt("ResolutionY", Resolution.height);
		PlayerPrefs.SetInt("ScreenMode", (int)_screenMode);
		PlayerPrefs.SetInt("VSync", Vsync ? 1 : 0);
		PlayerPrefs.SetInt("PauseOnFocusLost", _clickToAim ? 1 : 0);
		PlayerPrefs.SetFloat("SFXVol", _sfxVol);
		PlayerPrefs.SetFloat("BGMVol", _bgmVol);
		PlayerPrefs.SetFloat("ScreenshakeIntensity", ScreenshakeInstensity);
		PlayerPrefs.SetFloat("RumbleIntensity", RumbleInstensity);
		PlayerPrefs.SetInt("VignetteIntensity", (int)VignetteIntensity);
		PlayerPrefs.SetFloat("SpeedupLevel", SpeedupLevel);
		PlayerPrefs.SetInt("SpeedupToggle", SpeedupToggle ? 1 : 0);
		PlayerPrefs.SetInt("BatterySaveMode", BatterySaveMode ? 1 : 0);
		PlayerPrefs.SetInt("ControllerType", (int)_controllerInput);
		PlayerPrefs.SetInt("AnimateButtonPrompts", (int)_animateButtonPrompts);
		PlayerPrefs.SetInt("UseHardwareCursor", UseHardwareCursor ? 1 : 0);
		PlayerPrefs.SetInt("InstantLootAndCardDrops", InstantLootAndCardDrops ? 1 : 0);
		PlayerPrefs.SetInt("ShowCruciballCompletionOnTooltips", ShowCruciballCompletionOnTooltips ? 1 : 0);
		OnSettingsChangedEvent?.Invoke();
		PlayerPrefs.Save();
	}

	public void SaveSoundSettings()
	{
		PlayerPrefs.SetFloat("SFXVol", _sfxVol);
		PlayerPrefs.SetFloat("BGMVol", _bgmVol);
		PlayerPrefs.Save();
	}

	public void InitializePreviousSettings()
	{
		_lastResolutionUsed = Resolution;
		_lastScreenModeUsed = Screen.fullScreenMode;
		_lastClickToAimUsed = ClickToAim;
		_lastLanguageUsed = Language;
		_lastBGMVol = BgmVol;
		_lastSFXVol = SfxVol;
		_lastVsyncUsed = Vsync;
		_lastScreenshakeIntensity = ScreenshakeInstensity;
		_lastVignetteIntensity = VignetteIntensity;
		_lastSpeedupLevel = SpeedupLevel;
		_lastSpeedupToggle = SpeedupToggle;
		_lastBatterySaveMode = BatterySaveMode;
		_lastControllerInput = ControllerInput;
		_lastAnimateButtonPrompts = AnimateButtonPrompts;
		_lastUseHardwareCursor = UseHardwareCursor;
		_lastInstantLootAndCardDrops = InstantLootAndCardDrops;
		_lastShowCruciballCompletionOnTooltips = ShowCruciballCompletionOnTooltips;
	}

	public void RevertSettings()
	{
		if (_lastResolutionUsed.width != 0 && _lastResolutionUsed.height != 0)
		{
			Resolution = _lastResolutionUsed;
		}
		if (_screenMode != _lastScreenModeUsed)
		{
			ScreenMode = _lastScreenModeUsed;
		}
		TargetFramerate = _lastTargetFramerateUsed;
		Vsync = _lastVsyncUsed;
		ClickToAim = _lastClickToAimUsed;
		Language = _lastLanguageUsed;
		LocalizationManager.SetLanguageAndCode(Language, LocalizationManager.GetLanguageCode(Language));
		BgmVol = _lastBGMVol;
		SfxVol = _lastSFXVol;
		VignetteIntensity = _lastVignetteIntensity;
		ScreenshakeInstensity = _lastScreenshakeIntensity;
		SpeedupToggle = _lastSpeedupToggle;
		SpeedupLevel = _lastSpeedupLevel;
		BatterySaveMode = _lastBatterySaveMode;
		ControllerInput = _lastControllerInput;
		AnimateButtonPrompts = _lastAnimateButtonPrompts;
		UseHardwareCursor = _lastUseHardwareCursor;
		InstantLootAndCardDrops = _lastInstantLootAndCardDrops;
		ShowCruciballCompletionOnTooltips = _lastShowCruciballCompletionOnTooltips;
		ApplySettings();
		ClearLastUsedValues();
		OnSettingsChangedEvent?.Invoke();
	}

	public void ClearLastUsedValues()
	{
		_lastResolutionUsed = _resolution;
		if (_screenMode != FullScreenMode.Windowed && _screenMode != FullScreenMode.MaximizedWindow)
		{
			_lastScreenModeUsed = _screenMode;
		}
		_lastClickToAimUsed = _clickToAim;
	}

	public static float VignetteLevelToIntensity(IntensityLevel vignetteLevel)
	{
		return vignetteLevel switch
		{
			IntensityLevel.HIGH => 1f, 
			IntensityLevel.LOW => 0.75f, 
			IntensityLevel.OFF => 0f, 
			_ => 0.75f, 
		};
	}

	private float ScreenshakeIntensityToValue(IntensityLevel level)
	{
		return level switch
		{
			IntensityLevel.OFF => 0f, 
			IntensityLevel.LOW => 0.25f, 
			IntensityLevel.HIGH => 1f, 
			_ => 0.25f, 
		};
	}
}
