using System;
using PeglinUI;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimescaleManager : MonoBehaviour
{
	public delegate void SpeedupStateChanged();

	public static SpeedupStateChanged OnSpeedupStateChanged = () =>
	{
	};

	public static TimescaleManager Instance = null;

	private Player _player;

	private bool _isSpedUp;

	private bool _spedUpPrevRun;

	private bool _speedupToggleBlocked = true;

	public bool isSpedUp
	{
		get
		{
			return _isSpedUp;
		}
		set
		{
			_isSpedUp = value;
			_spedUpPrevRun = value;
			OnSpeedupStateChanged?.Invoke();
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			_player = ReInput.players.GetPlayer(0);
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Combine(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(HandlePauseToggle));
		SceneManager.sceneLoaded += HandleSceneLoaded;
	}

	private void OnDisable()
	{
		PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Remove(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(HandlePauseToggle));
		SceneManager.sceneLoaded += HandleSceneLoaded;
	}

	public void HandlePauseToggle(bool paused)
	{
		if (paused)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = (isSpedUp ? SettingsManager.Instance.SpeedupLevel : 1f);
		}
	}

	public void ManualSpeedupToggle()
	{
		if (!_speedupToggleBlocked && !PauseMenu.Paused)
		{
			if (!isSpedUp)
			{
				Time.timeScale = SettingsManager.Instance.SpeedupLevel;
				isSpedUp = true;
			}
			else
			{
				Time.timeScale = 1f;
				isSpedUp = false;
			}
		}
	}

	public void SetSpeedupToggleBlock(bool value)
	{
		_speedupToggleBlocked = value;
	}

	private void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
	{
		if (scene.name == "MainMenu")
		{
			Time.timeScale = 1f;
			isSpedUp = false;
			_speedupToggleBlocked = true;
		}
		else if (scene.name == "PostMainMenu")
		{
			if (ReleaseDemoManager.ReleaseDemoActive)
			{
				Time.timeScale = Mathf.Max(SettingsManager.Instance.SpeedupLevel, 2f);
				isSpedUp = true;
			}
			else if (_spedUpPrevRun)
			{
				Time.timeScale = SettingsManager.Instance.SpeedupLevel;
				isSpedUp = true;
			}
			_speedupToggleBlocked = false;
		}
		else
		{
			_speedupToggleBlocked = false;
		}
	}

	private void Update()
	{
		if (_speedupToggleBlocked || PauseMenu.Paused)
		{
			return;
		}
		if (_player.GetButtonDown(24) && SettingsManager.Instance.SpeedupToggle)
		{
			if (isSpedUp)
			{
				Time.timeScale = 1f;
				isSpedUp = false;
			}
			else
			{
				Time.timeScale = SettingsManager.Instance.SpeedupLevel;
				isSpedUp = true;
			}
		}
		else if (_player.GetButton(24) && !SettingsManager.Instance.SpeedupToggle && !isSpedUp)
		{
			Time.timeScale = SettingsManager.Instance.SpeedupLevel;
			isSpedUp = true;
		}
		else if (!_player.GetButton(24) && !SettingsManager.Instance.SpeedupToggle && isSpedUp)
		{
			Time.timeScale = 1f;
			isSpedUp = false;
		}
	}
}
