using System;
using System.Collections;
using PeglinUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Act4WinCreditsController : MonoBehaviour
{
	public float transitionIntroFadeTime = 3f;

	public FadeCurtain fadeCurtain;

	public Image background;

	public GameObject skipButton;

	public GameObject creditsParent;

	private void Awake()
	{
		creditsParent.SetActive(value: false);
	}

	private void OnEnable()
	{
		Act4WinCutsceneController.OnCutsceneFinished = (Act4WinCutsceneController.CutsceneFinished)Delegate.Combine(Act4WinCutsceneController.OnCutsceneFinished, new Act4WinCutsceneController.CutsceneFinished(TransitionToCreditsMinigame));
	}

	private void OnDisable()
	{
		Act4WinCutsceneController.OnCutsceneFinished = (Act4WinCutsceneController.CutsceneFinished)Delegate.Remove(Act4WinCutsceneController.OnCutsceneFinished, new Act4WinCutsceneController.CutsceneFinished(TransitionToCreditsMinigame));
	}

	public void TransitionToCreditsMinigame()
	{
		skipButton.SetActive(value: false);
		skipButton.GetComponent<Button>().onClick.AddListener(SkipCredtis);
		StartCoroutine(TransitionFade());
	}

	private IEnumerator TransitionFade()
	{
		fadeCurtain.FadeInThenOut(1f, transitionIntroFadeTime * Time.timeScale / 2f);
		yield return new WaitForSecondsRealtime(transitionIntroFadeTime / 2f);
		creditsParent.SetActive(value: true);
		background.enabled = false;
		skipButton.SetActive(value: true);
		EventSystem.current.SetSelectedGameObject(skipButton);
		yield return new WaitForSecondsRealtime(transitionIntroFadeTime / 2f);
		StartCreditsMinigame();
	}

	private void StartCreditsMinigame()
	{
	}

	public void SkipCredtis()
	{
		EndCredits();
	}

	private void EndCredits()
	{
		fadeCurtain.FadeInAndLoadRunSummary();
	}
}
