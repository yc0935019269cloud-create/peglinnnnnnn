using Cruciball;
using DG.Tweening;
using I2.Loc;
using Peglin.ClassSystem;
using Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.RunSummary
{
	public class FirstWinOnEachClassManager : MonoBehaviour
	{
		[SerializeField]
		private float fadeInLength = 4f;

		[SerializeField]
		private float fadeOutLength = 4f;

		[SerializeField]
		private Ease fadeInEase = Ease.Linear;

		[SerializeField]
		private Ease fadeOutEase = Ease.Linear;

		[SerializeField]
		private Button _moveToRunSummaryDetailsButton;

		public Button dismissButton;

		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private ClassWinPrefab _classWinPrefab;

		[SerializeField]
		private Localize _textI2l;

		[SerializeField]
		private string wonOnAllClassesTextKey;

		private CanvasGroup _canvasGroup;

		private Image _blackBackground;

		private bool _panelActive;

		public bool panelActive => _panelActive;

		private void Awake()
		{
			_blackBackground = GetComponent<Image>();
			_canvasGroup = GetComponent<CanvasGroup>();
			_blackBackground.enabled = false;
			_canvasGroup.alpha = 0f;
			dismissButton.enabled = false;
			if (_cruciballManager.CruciballVictoryThisRun && PersistentPlayerData.Instance.CruciballLevels[_cruciballManager.currentClass] <= 0)
			{
				DoFirstWinOnClassAnimation(_cruciballManager.currentClass);
			}
		}

		private void DoFirstWinOnClassAnimation(Class firstWinForClass)
		{
			_panelActive = true;
			_blackBackground.enabled = true;
			_moveToRunSummaryDetailsButton.enabled = false;
			_classWinPrefab.SetShinySliceForClassWeJustWonWith(firstWinForClass);
			if (PersistentPlayerData.Instance.CruciballLevels[Class.Peglin] >= 0 && PersistentPlayerData.Instance.CruciballLevels[Class.Roundrel] >= 0 && PersistentPlayerData.Instance.CruciballLevels[Class.Balladin] >= 0 && PersistentPlayerData.Instance.CruciballLevels[Class.Spinventor] >= 0)
			{
				_textI2l.Term = wonOnAllClassesTextKey;
			}
			Sequence s = DOTween.Sequence();
			s.Append(_canvasGroup.DOFade(1f, fadeInLength).SetEase(fadeInEase));
			s.AppendCallback(() =>
			{
				dismissButton.enabled = true;
				EventSystem.current.SetSelectedGameObject(dismissButton.gameObject);
			});
		}

		public void FadeOutAndContinue()
		{
			Sequence s = DOTween.Sequence();
			s.Append(_canvasGroup.DOFade(0f, fadeOutLength).SetEase(fadeOutEase));
			s.AppendCallback(() =>
			{
				_blackBackground.enabled = false;
				_moveToRunSummaryDetailsButton.enabled = true;
				EventSystem.current.SetSelectedGameObject(_moveToRunSummaryDetailsButton.gameObject);
				_panelActive = false;
			});
		}
	}
}
