using System;
using System.Collections;
using Battle;
using Data;
using Map;
using Scenarios.Shop;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class BGMController : MonoBehaviour
{
	public delegate void OneTimeJingleFinished();

	public static OneTimeJingleFinished OnOneTimeJingleFinished = () =>
	{
	};

	public bool keepPlayingMapMusic;

	public bool forceOverride;

	public static BGMController instance;

	public AudioClip mainMenuAudioClip;

	public AudioClip track;

	public AudioClip[] victoryTracks;

	public AudioClip[] bossVictoryTracks;

	public AudioClip[] defeatTracks;

	public AudioClip[] mapTracks;

	public AudioClip[] regularBattleTracks;

	public AudioClip shopExitJingle;

	public bool isBattleMusic;

	public AudioSource mapAudioSource;

	public AudioSource battleAudioSource;

	public AudioSource oneTimeJingleAudioSource;

	[SerializeField]
	private AudioMixer _bgmMixer;

	[SerializeField]
	private AudioMixer _sfxMixer;

	private AudioClip _audioToPlayAfterJingle;

	private AudioSource _afterJingleAudioSource;

	private float _bgmVol = 0.4f;

	private float _sfxVol = 0.4f;

	private float MAX_BGM_VALUE => 1f;

	private float MIN_BGM_VALUE => 0f;

	public float bgmVol
	{
		get
		{
			return _bgmVol;
		}
		set
		{
			_bgmVol = value;
			_bgmMixer.SetFloat("MasterBGMVolume", Mathf.Clamp(Mathf.Log10(_bgmVol) * 24f, -80f, -6f));
		}
	}

	public float sfxVol
	{
		get
		{
			return _sfxVol;
		}
		set
		{
			_sfxVol = value;
			_sfxMixer.SetFloat("SFXVolume", Mathf.Clamp(Mathf.Log10(_sfxVol) * 20f, -80f, -6f));
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			AudioClip audioClip = StaticGameData.dataToLoad?.GetMusicOverride();
			if (audioClip == null || forceOverride)
			{
				audioClip = track;
			}
			if (isBattleMusic)
			{
				MapDataBattle mapDataBattle = StaticGameData.dataToLoad as MapDataBattle;
				if (mapDataBattle == null || mapDataBattle.name != "MimicMinibossMapData")
				{
					QueueBattleAudio(audioClip);
				}
			}
			else if (keepPlayingMapMusic && (!(track == null) || !(audioClip != null)))
			{
				if (!instance.mapAudioSource.isPlaying)
				{
					QueueMapAudio(mapTracks[MapController.instance.Act - 1]);
				}
			}
			else
			{
				QueueMapAudio(audioClip);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (instance == null)
		{
			instance = this;
			BattleController.OnVictoryPlayJingle = (Action<int>)Delegate.Combine(BattleController.OnVictoryPlayJingle, (Action<int>)((int forAct) =>
			{
				PlayVictoryAudio(forAct);
			}));
			ShopManager.OnShopEvent = (ShopManager.ShopEvent)Delegate.Combine(ShopManager.OnShopEvent, (ShopManager.ShopEvent)(() =>
			{
				StartCoroutine(instance.StartOneTimeJingle(shopExitJingle));
			}));
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void OnEnable()
	{
		AudioSettings.OnAudioConfigurationChanged += HandleAudioConfigurationChange;
	}

	private void OnDisable()
	{
		AudioSettings.OnAudioConfigurationChanged -= HandleAudioConfigurationChange;
	}

	private void Start()
	{
		bgmVol = PlayerPrefs.GetFloat("BGMVol", _bgmVol);
		sfxVol = PlayerPrefs.GetFloat("SFXVol", _sfxVol);
		_bgmMixer.SetFloat("BGMVolume", Mathf.Log10(MAX_BGM_VALUE) * 24f);
		mapAudioSource.clip = track;
		mapAudioSource.Play();
	}

	private void HandleAudioConfigurationChange(bool something)
	{
		bgmVol = bgmVol;
		sfxVol = sfxVol;
	}

	public void QueueMapAudio(AudioClip track)
	{
		if (!instance.oneTimeJingleAudioSource.isPlaying)
		{
			instance.SetMapAudio(track);
			return;
		}
		instance._audioToPlayAfterJingle = track;
		instance._afterJingleAudioSource = instance.mapAudioSource;
	}

	public void QueueBattleAudio(AudioClip track)
	{
		if (!instance.oneTimeJingleAudioSource.isPlaying)
		{
			instance.SetBattleAudio(track);
			return;
		}
		instance._audioToPlayAfterJingle = track;
		instance._afterJingleAudioSource = instance.battleAudioSource;
	}

	public void SetNextQueuedTrack(AudioClip track, bool isBattleMusic)
	{
		instance._audioToPlayAfterJingle = track;
		if (isBattleMusic)
		{
			instance._afterJingleAudioSource = instance.battleAudioSource;
		}
		else
		{
			instance._afterJingleAudioSource = instance.mapAudioSource;
		}
	}

	public void SetMapAudio(AudioClip newTrack)
	{
		isBattleMusic = false;
		if (!mapAudioSource.isPlaying || !(mapAudioSource.clip != null) || !(mapAudioSource.clip == newTrack))
		{
			StartCoroutine(CrossFadeBGM(newTrack, battleAudioSource, mapAudioSource, 0.25f, 0.05f));
		}
	}

	public void SetBattleAudio(AudioClip newTrack)
	{
		isBattleMusic = true;
		StartCoroutine(CrossFadeBGM(newTrack, mapAudioSource, battleAudioSource, 0.25f, 0.05f));
	}

	public void SetMainMenuAudioClip(AudioClip clip)
	{
		StartCoroutine(CrossFadeBGM(clip, mapAudioSource, mapAudioSource, 0.25f, 0.25f));
	}

	public void SetDefeatAudio()
	{
		StartCoroutine(instance.StartOneTimeJingle(defeatTracks[MapController.instance.Act - 1]));
	}

	private void PlayVictoryAudio(int forAct)
	{
		if (!(MapController.instance == null))
		{
			if (BattleController.BattleActive && BattleController.IsBossBattle)
			{
				StartCoroutine(instance.StartOneTimeJingle(bossVictoryTracks[forAct - 1]));
			}
			else
			{
				StartCoroutine(instance.StartOneTimeJingle(victoryTracks[forAct - 1]));
			}
		}
	}

	public void PlayOneTimeJingle(AudioClip newTrack)
	{
		StartCoroutine(StartOneTimeJingle(newTrack));
	}

	private IEnumerator StartOneTimeJingle(AudioClip newTrack)
	{
		AudioSource fromAudioSource = battleAudioSource;
		if (mapAudioSource.isPlaying)
		{
			fromAudioSource = mapAudioSource;
		}
		yield return StartCoroutine(CrossFadeBGM(newTrack, fromAudioSource, oneTimeJingleAudioSource, 0.5f, 0.1f));
		StartCoroutine(WaitForJingleFinished());
	}

	public void ForceStopOneTimeJingle(bool fadeOut = false)
	{
		if (instance.oneTimeJingleAudioSource.isPlaying)
		{
			if (fadeOut)
			{
				instance.StartCoroutine(FadeOutJingle());
			}
			else
			{
				instance.oneTimeJingleAudioSource.Stop();
			}
		}
	}

	private IEnumerator FadeOutJingle(float duration = 1f)
	{
		float currentTime = 0f;
		_bgmMixer.GetFloat("BGMVolume", out var currentVol);
		currentVol = Mathf.Pow(10f, currentVol / 20f);
		float targetValue = Mathf.Clamp(MIN_BGM_VALUE, 0.0001f, 1f);
		while (currentTime < duration)
		{
			currentTime += Time.deltaTime;
			float f = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
			_bgmMixer.SetFloat("BGMVolume", Mathf.Log10(f) * 24f);
			yield return null;
		}
		instance.oneTimeJingleAudioSource.Stop();
	}

	public void ClearAfterJingleQueue()
	{
		instance._audioToPlayAfterJingle = null;
		instance._afterJingleAudioSource = null;
	}

	private IEnumerator CrossFadeBGM(AudioClip newTrack, AudioSource fromAudioSource, AudioSource toAudioSource, float fromDuration, float toDuration)
	{
		float currentTime = 0f;
		_bgmMixer.GetFloat("BGMVolume", out var currentVol);
		currentVol = Mathf.Pow(10f, currentVol / 20f);
		float targetValue = Mathf.Clamp(MIN_BGM_VALUE, 0.0001f, 1f);
		while (currentTime < fromDuration)
		{
			currentTime += Time.deltaTime;
			float f = Mathf.Lerp(currentVol, targetValue, currentTime / fromDuration);
			_bgmMixer.SetFloat("BGMVolume", Mathf.Log10(f) * 24f);
			yield return null;
		}
		fromAudioSource.Stop();
		toAudioSource.clip = newTrack;
		toAudioSource.Play();
		currentTime = 0f;
		while (currentTime < fromDuration)
		{
			currentTime += Time.deltaTime;
			float f2 = Mathf.Lerp(0f, MAX_BGM_VALUE, currentTime / toDuration);
			_bgmMixer.SetFloat("BGMVolume", Mathf.Log10(f2) * 24f);
			yield return null;
		}
	}

	public IEnumerator FadeOut(AudioSource source, float duration, float targetVolume)
	{
		float currentTime = 0f;
		float start = source.volume;
		while (currentTime < duration)
		{
			currentTime += Time.fixedDeltaTime;
			source.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
			yield return null;
		}
		source.Pause();
		source.volume = start;
	}

	public IEnumerator WaitAndPlayRunSummaryMapMusic(AudioClip mapAudio, float extraWaitTime = 0.5f)
	{
		while (oneTimeJingleAudioSource.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(extraWaitTime);
		SetMapAudio(mapAudio);
	}

	public IEnumerator WaitForJingleFinished()
	{
		yield return new WaitUntil(() => !instance.oneTimeJingleAudioSource.isPlaying);
		if (!(instance._audioToPlayAfterJingle == null) && !(instance._afterJingleAudioSource == null))
		{
			if (instance._afterJingleAudioSource == instance.mapAudioSource)
			{
				instance.SetMapAudio(instance._audioToPlayAfterJingle);
			}
			else if (instance._afterJingleAudioSource == instance.battleAudioSource)
			{
				instance.SetBattleAudio(instance._audioToPlayAfterJingle);
			}
			ClearAfterJingleQueue();
			OnOneTimeJingleFinished?.Invoke();
		}
	}
}
