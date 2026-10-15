using System;
using System.Collections.Generic;
using I2.Loc;
using PeglinUI.ControllerSupport;
using Rewired;
using Rewired.Integration.UnityUI;
using TMPro;
using UI;
using UnityEngine;

namespace PeglinUI
{
	public class ResolutionModeEnumerator : MonoBehaviour
	{
		public static readonly Resolution[] SupportedWindowedResolutions = new Resolution[8]
		{
			GetResolution(1024, 576),
			GetResolution(1152, 648),
			GetResolution(1280, 720),
			GetResolution(1366, 768),
			GetResolution(1600, 900),
			GetResolution(1920, 1080),
			GetResolution(2560, 1440),
			GetResolution(3840, 2160)
		};

		public TextMeshProUGUI ResolutionText;

		public GameObject RightArrowButton;

		public GameObject LeftArrowButton;

		private TMP_Dropdown _dropdown;

		private List<Resolution> _activeResolutions = new List<Resolution>();

		private List<Resolution> _windowedResolutions = new List<Resolution>();

		private int _displayedResolutionIndex;

		private RewiredEventSystem _eventSystem;

		private Player _player;

		private bool _horizontalAxisDetected;

		private bool _isUsingCustomResolution;

		private void OnEnable()
		{
			if (_dropdown == null)
			{
				_dropdown = GetComponent<TMP_Dropdown>();
			}
			if (_eventSystem == null)
			{
				_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			}
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(0);
			}
			SetupWindowedResolutions();
			SetUpResolutions();
			DisplayManager.OnDisplayChanged = (DisplayManager.DisplayChanged)Delegate.Combine(DisplayManager.OnDisplayChanged, new DisplayManager.DisplayChanged(SetUpResolutions));
		}

		private void OnDisable()
		{
			DisplayManager.OnDisplayChanged = (DisplayManager.DisplayChanged)Delegate.Remove(DisplayManager.OnDisplayChanged, new DisplayManager.DisplayChanged(SetUpResolutions));
		}

		private void Update()
		{
			if (_eventSystem.currentSelectedGameObject == base.gameObject && !ControllerInputManager.IsPointerBased)
			{
				float axis = _player.GetAxis("Horizontal");
				if (axis == 0f)
				{
					axis = _player.GetAxis("UIHorizontal");
				}
				if (axis < 0f && !_horizontalAxisDetected && _displayedResolutionIndex > 0)
				{
					_horizontalAxisDetected = true;
					ScrollResolutionLeft();
				}
				else if (axis > 0f && !_horizontalAxisDetected && _displayedResolutionIndex < _activeResolutions.Count - 1)
				{
					_horizontalAxisDetected = true;
					ScrollResolutionRight();
				}
				if (axis == 0f)
				{
					_horizontalAxisDetected = false;
				}
			}
		}

		public void ScrollResolutionRight()
		{
			_displayedResolutionIndex = Mathf.Clamp(_displayedResolutionIndex + 1, 0, _activeResolutions.Count - 1);
			ShowActiveResolution();
			UpdateResolution();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		public void ScrollResolutionLeft()
		{
			_displayedResolutionIndex = Mathf.Clamp(_displayedResolutionIndex - 1, 0, _activeResolutions.Count - 1);
			ShowActiveResolution();
			UpdateResolution();
			ButtonHandleHover.OnUIButtonHover?.Invoke(2);
		}

		public void UpdateResolution()
		{
			SettingsManager.Instance.Resolution = GetSelectedResolution();
		}

		public static Resolution GetResolution(int width, int height)
		{
			return new Resolution
			{
				width = width,
				height = height,
				refreshRate = 60
			};
		}

		public void SetAvailableResolutionOptions(FullScreenMode screenMode)
		{
			_activeResolutions.Clear();
			if (screenMode == FullScreenMode.FullScreenWindow || screenMode == FullScreenMode.ExclusiveFullScreen)
			{
				SetUpResolutions(Screen.resolutions);
			}
			else
			{
				SetUpResolutions(_windowedResolutions.ToArray());
			}
		}

		private void SetUpResolutions()
		{
			SetupWindowedResolutions();
			SetAvailableResolutionOptions(SettingsManager.Instance.ScreenMode);
		}

		private void SetupWindowedResolutions()
		{
			_windowedResolutions.Clear();
			Resolution res = Screen.resolutions[Screen.resolutions.Length - 1];
			int maxRefreshRateForResolution = SettingsManager.GetMaxRefreshRateForResolution(res, reported: false);
			Resolution[] supportedWindowedResolutions = SupportedWindowedResolutions;
			for (int i = 0; i < supportedWindowedResolutions.Length; i++)
			{
				Resolution resolution = supportedWindowedResolutions[i];
				if (resolution.width <= res.width && resolution.height <= res.height)
				{
					Resolution resolution2 = GetResolution(resolution.width, resolution.height);
					resolution2.refreshRate = maxRefreshRateForResolution;
					_windowedResolutions.Add(resolution2);
				}
			}
		}

		private void SetUpResolutions(Resolution[] resolutions)
		{
			int num = -1;
			bool flag = false;
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution resolution = resolutions[i];
				int maxRefreshRateForResolution = SettingsManager.GetMaxRefreshRateForResolution(resolution, reported: false);
				while (resolution.refreshRate < maxRefreshRateForResolution && i + 1 < resolutions.Length)
				{
					i++;
					resolution = resolutions[i];
				}
				if (!ResolutionExists(resolution))
				{
					num++;
					_activeResolutions.Add(resolution);
					if (Screen.width == resolution.width && Screen.height == resolution.height)
					{
						_displayedResolutionIndex = num;
						_isUsingCustomResolution = false;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Resolution resolution2 = GetResolution(Screen.width, Screen.height);
				resolution2.refreshRate = Screen.currentResolution.refreshRate;
				_activeResolutions.Add(resolution2);
				_displayedResolutionIndex = _activeResolutions.Count - 1;
				_isUsingCustomResolution = true;
			}
			ShowActiveResolution();
		}

		private void ShowActiveResolution()
		{
			SetResolutionText();
			CheckShowArrows();
			CheckHideArrows();
		}

		private Resolution GetSelectedResolution()
		{
			if (_activeResolutions.Count > _displayedResolutionIndex)
			{
				return _activeResolutions[_displayedResolutionIndex];
			}
			if (IsWindowed() && _displayedResolutionIndex == _activeResolutions.Count)
			{
				return Screen.currentResolution;
			}
			Debug.LogWarning("Null selected option!");
			return default;
		}

		private bool ResolutionExists(Resolution res)
		{
			foreach (Resolution activeResolution in _activeResolutions)
			{
				if (AreResolutionsEqual(activeResolution, res))
				{
					return true;
				}
			}
			return false;
		}

		private void SetResolutionText()
		{
			if (_isUsingCustomResolution && _displayedResolutionIndex == _activeResolutions.Count - 1)
			{
				ResolutionText.text = LocalizationManager.GetTranslation("Menu/custom_windowed_res_label");
			}
			else
			{
				ResolutionText.text = $"{_activeResolutions[_displayedResolutionIndex].width}x{_activeResolutions[_displayedResolutionIndex].height}";
			}
		}

		private void CheckShowArrows()
		{
			if (_displayedResolutionIndex < _activeResolutions.Count - 1)
			{
				RightArrowButton.SetActive(value: true);
			}
			if (_displayedResolutionIndex > 0)
			{
				LeftArrowButton.SetActive(value: true);
			}
		}

		private void CheckHideArrows()
		{
			if (_displayedResolutionIndex == _activeResolutions.Count - 1)
			{
				RightArrowButton.SetActive(value: false);
			}
			if (_displayedResolutionIndex == 0)
			{
				LeftArrowButton.SetActive(value: false);
			}
		}

		private static bool AreResolutionsEqual(Resolution r1, Resolution r2)
		{
			if (r1.width == r2.width)
			{
				return r1.height == r2.height;
			}
			return false;
		}

		private bool IsWindowed()
		{
			return SettingsManager.Instance.ScreenMode == FullScreenMode.Windowed;
		}
	}
}
