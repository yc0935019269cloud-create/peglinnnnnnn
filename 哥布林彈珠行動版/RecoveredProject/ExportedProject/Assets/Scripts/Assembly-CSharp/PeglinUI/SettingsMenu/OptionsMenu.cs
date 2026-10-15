using System;
using System.Collections;
using I2.Loc;
using Loading;
using PeglinUI.ControllerSupport;
using PeglinUI.SettingsMenu.Options;
using Rewired;
using Saving;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PeglinUI.SettingsMenu
{
	public class OptionsMenu : MonoBehaviour
	{
		[Header("Parameters")]
		public bool unpauseOnExit = true;

		[Header("Prefab References")]
		public GameObject firstSelectable;

		public CarouselOptionsBoolean ControlModeOption;

		public CarouselOptionsBoolean SpeedupControlsOption;

		public CarouselOptionsBoolean VsyncOption;

		public CarouselOptionsFloat SpeedupAmountOption;

		public CarouselOptionsInt ScreenModeOption;

		public CarouselOptionsInt VignetteIntensity;

		public CarouselOptionsInt ControlTypeOption;

		public GameObject ResolutionOptions;

		public Slider BgmSlider;

		public Slider SfxSlider;

		public CarouselOptionsBoolean BatterySavingMode;

		public CarouselOptionsInt AnimateButtonPrompts;

		public CarouselOptionsBoolean UseHardwareCursor;

		public CarouselOptionsBoolean InstantLootAndCardDropsOption;

		public CarouselOptionsBoolean ShowCruciballCompletionOnTooltips;

		public ModalPanel modalPanel;

		public RewiredUIButtonTrigger acceptButton;

		public RewiredUIButtonTrigger applyButton;

		public RewiredUIButtonTrigger cancelButton;

		public RewiredUIButtonTrigger deleteButton;

		public OverrideButtonMapWhenSelected mappedButtonOverride;

		public Selectable deleteButtonSelectable;

		[FormerlySerializedAs("speedupWarning")]
		public GameObject SpeedupWarning;

		[Header("Scene Refs")]
		public HoverManager HoverManager;

		public AudioClip SfxTest;

		private PauseMenu _pauseMenu;

		private Player _player;

		private bool _hasUpdatedSfx;

		private float _timeToWaitSfx;

		private void OnEnable()
		{
			if (_pauseMenu == null)
			{
				_pauseMenu = UnityEngine.Object.FindObjectOfType<PauseMenu>();
			}
			ControlModeOption.ShowOption(SettingsManager.Instance.ClickToAim);
			SpeedupControlsOption.ShowOption(SettingsManager.Instance.SpeedupToggle);
			VsyncOption.ShowOption(SettingsManager.Instance.Vsync);
			SpeedupAmountOption.ShowOption(SettingsManager.Instance.SpeedupLevel);
			SpeedupWarning.SetActive(SettingsManager.Instance.SpeedupLevel >= 3f);
			ScreenModeOption.ShowOption(ScreenModeToDisplayIndex(SettingsManager.Instance.ScreenMode));
			VignetteIntensity.ShowOption((int)SettingsManager.Instance.VignetteIntensity);
			BatterySavingMode.ShowOption(SettingsManager.Instance.BatterySaveMode);
			BgmSlider.value = SettingsManager.Instance.BgmVol;
			SfxSlider.value = SettingsManager.Instance.SfxVol;
			ControlTypeOption.ShowOption((int)SettingsManager.Instance.ControllerInput);
			AnimateButtonPrompts.ShowOption((int)SettingsManager.Instance.AnimateButtonPrompts);
			UseHardwareCursor.ShowOption(SettingsManager.Instance.UseHardwareCursor);
			InstantLootAndCardDropsOption.ShowOption(SettingsManager.Instance.InstantLootAndCardDrops);
			ShowCruciballCompletionOnTooltips.ShowOption(SettingsManager.Instance.ShowCruciballCompletionOnTooltips);
			CarouselOptionsBoolean vsyncOption = VsyncOption;
			vsyncOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(vsyncOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyVsync));
			CarouselOptionsBoolean speedupControlsOption = SpeedupControlsOption;
			speedupControlsOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(speedupControlsOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplySpeedupControls));
			CarouselOptionsBoolean controlModeOption = ControlModeOption;
			controlModeOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(controlModeOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyControlMode));
			CarouselOptionsFloat speedupAmountOption = SpeedupAmountOption;
			speedupAmountOption.changeSettingAction = (CarouselOptionsFloat.FloatAction)Delegate.Combine(speedupAmountOption.changeSettingAction, new CarouselOptionsFloat.FloatAction(ApplySpeedupAmount));
			CarouselOptionsInt screenModeOption = ScreenModeOption;
			screenModeOption.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Combine(screenModeOption.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyScreenModeOptions));
			CarouselOptionsInt vignetteIntensity = VignetteIntensity;
			vignetteIntensity.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Combine(vignetteIntensity.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyVignetteIntensity));
			CarouselOptionsInt controlTypeOption = ControlTypeOption;
			controlTypeOption.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Combine(controlTypeOption.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyControllerType));
			CarouselOptionsInt animateButtonPrompts = AnimateButtonPrompts;
			animateButtonPrompts.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Combine(animateButtonPrompts.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyAnimateButtonPrompts));
			CarouselOptionsBoolean useHardwareCursor = UseHardwareCursor;
			useHardwareCursor.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(useHardwareCursor.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyHardwareCursor));
			CarouselOptionsBoolean instantLootAndCardDropsOption = InstantLootAndCardDropsOption;
			instantLootAndCardDropsOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(instantLootAndCardDropsOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyInstantLootAndCardDrops));
			CarouselOptionsBoolean showCruciballCompletionOnTooltips = ShowCruciballCompletionOnTooltips;
			showCruciballCompletionOnTooltips.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Combine(showCruciballCompletionOnTooltips.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyShowCruciballCompletionOnTooltips));
			modalPanel.gameObject.SetActive(value: false);
			BatterySavingMode.gameObject.SetActive(value: false);
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(0);
			}
			if (HoverManager != null)
			{
				HoverManager.SetHover(status: false);
			}
			EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
			_timeToWaitSfx = SfxTest.length;
		}

		private void ApplyHardwareCursor(bool value)
		{
			SettingsManager.Instance.UseHardwareCursor = value;
		}

		private void ApplyInstantLootAndCardDrops(bool value)
		{
			SettingsManager.Instance.InstantLootAndCardDrops = value;
		}

		private void ApplyShowCruciballCompletionOnTooltips(bool value)
		{
			SettingsManager.Instance.ShowCruciballCompletionOnTooltips = value;
		}

		private void ApplyAnimateButtonPrompts(int value)
		{
			SettingsManager.Instance.AnimateButtonPrompts = (SettingsManager.ButtonPromptsStyle)value;
		}

		private void OnDisable()
		{
			CarouselOptionsBoolean vsyncOption = VsyncOption;
			vsyncOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(vsyncOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyVsync));
			CarouselOptionsBoolean speedupControlsOption = SpeedupControlsOption;
			speedupControlsOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(speedupControlsOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplySpeedupControls));
			CarouselOptionsBoolean controlModeOption = ControlModeOption;
			controlModeOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(controlModeOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyControlMode));
			CarouselOptionsFloat speedupAmountOption = SpeedupAmountOption;
			speedupAmountOption.changeSettingAction = (CarouselOptionsFloat.FloatAction)Delegate.Remove(speedupAmountOption.changeSettingAction, new CarouselOptionsFloat.FloatAction(ApplySpeedupAmount));
			CarouselOptionsInt screenModeOption = ScreenModeOption;
			screenModeOption.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Remove(screenModeOption.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyScreenModeOptions));
			CarouselOptionsInt vignetteIntensity = VignetteIntensity;
			vignetteIntensity.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Remove(vignetteIntensity.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyVignetteIntensity));
			CarouselOptionsInt animateButtonPrompts = AnimateButtonPrompts;
			animateButtonPrompts.changeSettingAction = (CarouselOptionsInt.IntAction)Delegate.Remove(animateButtonPrompts.changeSettingAction, new CarouselOptionsInt.IntAction(ApplyAnimateButtonPrompts));
			CarouselOptionsBoolean useHardwareCursor = UseHardwareCursor;
			useHardwareCursor.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(useHardwareCursor.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyHardwareCursor));
			CarouselOptionsBoolean instantLootAndCardDropsOption = InstantLootAndCardDropsOption;
			instantLootAndCardDropsOption.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(instantLootAndCardDropsOption.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyInstantLootAndCardDrops));
			CarouselOptionsBoolean showCruciballCompletionOnTooltips = ShowCruciballCompletionOnTooltips;
			showCruciballCompletionOnTooltips.changeSettingAction = (CarouselOptionsBoolean.BooleanAction)Delegate.Remove(showCruciballCompletionOnTooltips.changeSettingAction, new CarouselOptionsBoolean.BooleanAction(ApplyShowCruciballCompletionOnTooltips));
		}

		private void Update()
		{
			if (_player != null && _player.GetButtonDown("Pause") && !unpauseOnExit)
			{
				CancelOptions();
			}
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

		private void FixedUpdate()
		{
		}

		public void ApplyOptions()
		{
			SettingsManager.Instance.BgmVol = BgmSlider.value;
			SettingsManager.Instance.SfxVol = SfxSlider.value;
			SettingsManager.Instance.ApplySettings();
		}

		public void AcceptOptions()
		{
			ApplyOptions();
			if (unpauseOnExit)
			{
				_pauseMenu.Resume();
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			if (HoverManager != null)
			{
				HoverManager.SetHover(status: true);
			}
		}

		public void CancelOptions()
		{
			SettingsManager.Instance.RevertSettings();
			if (unpauseOnExit)
			{
				_pauseMenu.Resume();
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			if (HoverManager != null)
			{
				HoverManager.SetHover(status: true);
			}
		}

		public void ApplyControlMode(bool value)
		{
			SettingsManager.Instance.ClickToAim = value;
		}

		public void ApplySpeedupAmount(float value)
		{
			SpeedupWarning.SetActive(value >= 3f);
			SettingsManager.Instance.SpeedupLevel = value;
		}

		public void ApplySpeedupControls(bool value)
		{
			SettingsManager.Instance.SpeedupToggle = value;
		}

		public void ApplyScreenModeOptions(int value)
		{
			FullScreenMode fullScreenMode = (FullScreenMode)value;
			if (fullScreenMode == FullScreenMode.MaximizedWindow)
			{
				fullScreenMode = FullScreenMode.Windowed;
			}
			SettingsManager.Instance.ScreenMode = fullScreenMode;
		}

		public void ApplyVignetteIntensity(int value)
		{
			SettingsManager.Instance.VignetteIntensity = (SettingsManager.IntensityLevel)value;
		}

		public void ApplyVsync(bool value)
		{
			SettingsManager.Instance.Vsync = value;
		}

		public void ApplyControllerType(int value)
		{
			SettingsManager.Instance.ControllerInput = (SettingsManager.PCControllerInputTypes)value;
			ControllerInputManager.OnInputTypeChanged();
		}

		public void ApplyBatterySaveMode(bool value)
		{
			SettingsManager.Instance.BatterySaveMode = value;
		}

		public void UpdateBGMVol(float value)
		{
			if (SettingsManager.Instance != null)
			{
				SettingsManager.Instance.BgmVol = value;
			}
		}

		public void UpdateSfxVol(float value)
		{
			if (SettingsManager.Instance != null)
			{
				SettingsManager.Instance.SfxVol = value;
				_hasUpdatedSfx = true;
			}
		}

		private int ScreenModeToDisplayIndex(FullScreenMode screenMode)
		{
			switch (screenMode)
			{
			case FullScreenMode.ExclusiveFullScreen:
				return 0;
			case FullScreenMode.FullScreenWindow:
			case FullScreenMode.MaximizedWindow:
				return 1;
			case FullScreenMode.Windowed:
				return 2;
			default:
				return 0;
			}
		}

		public void AskDeleteSaveData()
		{
			SetButtonsActive(value: false);
			EventSystem.current.SetSelectedGameObject(null);
			modalPanel.Choice(LocalizationManager.GetTranslation("Menu/delete_save_confirm"), DeleteSaveFiles, () =>
			{
				StartCoroutine(DelayButtonsActive());
			});
		}

		private void DeleteSaveFiles()
		{
			PersistentPlayerData.Instance.DeleteSaveData();
			PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MAIN_MENU);
			SetButtonsActive(value: true);
		}

		private void SetButtonsActive(bool value)
		{
			RewiredUIButtonTrigger rewiredUIButtonTrigger = deleteButton;
			OverrideButtonMapWhenSelected overrideButtonMapWhenSelected = mappedButtonOverride;
			RewiredUIButtonTrigger rewiredUIButtonTrigger2 = acceptButton;
			RewiredUIButtonTrigger rewiredUIButtonTrigger3 = applyButton;
			bool flag = (cancelButton.enabled = value);
			bool flag3 = (rewiredUIButtonTrigger3.enabled = flag);
			bool flag5 = (rewiredUIButtonTrigger2.enabled = flag3);
			bool flag7 = (overrideButtonMapWhenSelected.enabled = flag5);
			rewiredUIButtonTrigger.enabled = flag7;
		}

		private IEnumerator DelayButtonsActive()
		{
			yield return new WaitForSecondsRealtime(0.1f);
			SetButtonsActive(value: true);
			EventSystem.current.SetSelectedGameObject(deleteButtonSelectable.gameObject);
		}
	}
}
