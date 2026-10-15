using System.Collections;
using System.Collections.Generic;
using PeglinUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Act4WinCutsceneController : MonoBehaviour
{
	public delegate void CutsceneFinished();

	public static CutsceneFinished OnCutsceneFinished = () =>
	{
	};

	public List<Sprite> pictureSequence = new List<Sprite>();

	public float[] customTimePerPicture = new float[10] { 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f };

	public float[] customFadeTimeAfterPicture = new float[10] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };

	public FadeCurtain fadeCurtain;

	public Image background;

	public GameObject skipButton;

	public AudioClip cutsceneMusic;

	public AudioClip creditsMusic;

	private bool _playing;

	private bool _creditsMusicPlaying;

	private void Awake()
	{
		EventSystem.current.SetSelectedGameObject(skipButton);
	}

	private void Start()
	{
		StartCoroutine(PlaySlideshow());
		BGMController.instance.ClearAfterJingleQueue();
	}

	private IEnumerator PlaySlideshow()
	{
		_playing = true;
		for (int i = 0; i < pictureSequence.Count; i++)
		{
			if (!_playing)
			{
				yield break;
			}
			background.sprite = pictureSequence[i];
			yield return new WaitForSecondsRealtime(customFadeTimeAfterPicture[Mathf.Max(i - 1, 0)] / 2f);
			if (!_playing)
			{
				yield break;
			}
			yield return new WaitForSecondsRealtime(customTimePerPicture[i]);
			if (!_playing)
			{
				yield break;
			}
			if (i >= pictureSequence.Count - 1)
			{
				break;
			}
			switch (i)
			{
			case 1:
				BGMController.instance.ForceStopOneTimeJingle();
				BGMController.instance.PlayOneTimeJingle(cutsceneMusic);
				break;
			case 7:
				if (!_creditsMusicPlaying)
				{
					BGMController.instance.ForceStopOneTimeJingle();
					BGMController.instance.PlayOneTimeJingle(creditsMusic);
					_creditsMusicPlaying = true;
				}
				break;
			}
			fadeCurtain.FadeInThenOut(1f, customFadeTimeAfterPicture[i] / 2f, useUnscaledTime: true);
			yield return new WaitForSecondsRealtime(customFadeTimeAfterPicture[i] / 2f);
		}
		EndCutscene();
	}

	public void SkipCutscene()
	{
		if (!_creditsMusicPlaying)
		{
			BGMController.instance.ForceStopOneTimeJingle();
			BGMController.instance.PlayOneTimeJingle(creditsMusic);
			_creditsMusicPlaying = true;
		}
		EndCutscene();
	}

	private void EndCutscene()
	{
		if (_playing)
		{
			StopCoroutine(PlaySlideshow());
			_playing = false;
			skipButton.GetComponent<Button>().onClick.RemoveListener(SkipCutscene);
			skipButton.SetActive(value: false);
			OnCutsceneFinished?.Invoke();
		}
	}
}
