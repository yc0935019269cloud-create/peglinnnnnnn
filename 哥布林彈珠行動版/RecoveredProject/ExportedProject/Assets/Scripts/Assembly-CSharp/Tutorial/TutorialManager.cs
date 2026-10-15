using System;
using System.Collections;
using PeglinUtils;
using UnityEngine;

namespace Tutorial
{
	public class TutorialManager : MonoBehaviour
	{
		[SerializeField]
		private Canvas _tutorialCanvas;

		[SerializeField]
		private GameObject _tutorialWindowPrefab;

		[SerializeField]
		private GameObject _fadePanel;

		private TutorialWindow _tutorialWindow;

		private bool _paused;

		public static bool WasPausedAtStartOfFrame { get; protected set; }

		private void OnEnable()
		{
			DeckManager.onBallDrawn = (DeckManager.BallDrawn)Delegate.Combine(DeckManager.onBallDrawn, new DeckManager.BallDrawn(HandleBallDrawn));
		}

		private void LateUpdate()
		{
			WasPausedAtStartOfFrame = _paused;
		}

		public bool ShouldPopupTutorial(TutorialElement element)
		{
			if (PlayerPrefs.HasKey(element.prefSaveKey))
			{
				return false;
			}
			if (_tutorialWindow == null)
			{
				_tutorialWindow = UnityEngine.Object.Instantiate(_tutorialWindowPrefab, _tutorialCanvas.transform).GetComponent<TutorialWindow>();
			}
			_tutorialWindow.SetData(element);
			_tutorialCanvas.gameObject.SetActive(value: true);
			_tutorialWindow.gameObject.SetActive(value: true);
			TutorialWindow tutorialWindow = _tutorialWindow;
			tutorialWindow.OnFinishedTutorial = (TutorialWindow.TutorialFinished)Delegate.Combine(tutorialWindow.OnFinishedTutorial, new TutorialWindow.TutorialFinished(HandleTutorialFinished));
			PlayerPrefs.SetInt(element.prefSaveKey, 1);
			return true;
		}

		private void HandleBallDrawn(GameObject ball)
		{
			TutorialElement component = ball.GetComponent<TutorialElement>();
			if (component != null && component.enabled && ShouldPopupTutorial(component))
			{
				TimescaleManager.Instance.HandlePauseToggle(paused: true);
				_paused = true;
				_fadePanel.SetActive(value: true);
			}
		}

		private void HandleTutorialFinished()
		{
			TimescaleManager.Instance.HandlePauseToggle(paused: false);
			_paused = false;
			_fadePanel.SetActive(value: false);
			TutorialWindow tutorialWindow = _tutorialWindow;
			tutorialWindow.OnFinishedTutorial = (TutorialWindow.TutorialFinished)Delegate.Remove(tutorialWindow.OnFinishedTutorial, new TutorialWindow.TutorialFinished(HandleTutorialFinished));
			StartCoroutine(MarkClosedAtEndOfFrame());
		}

		private IEnumerator MarkClosedAtEndOfFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			TutorialWindow.wasOpenThisFrame = false;
		}

		private void OnDisable()
		{
			DeckManager.onBallDrawn = (DeckManager.BallDrawn)Delegate.Remove(DeckManager.onBallDrawn, new DeckManager.BallDrawn(HandleBallDrawn));
		}
	}
}
