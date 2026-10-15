using Rewired.Integration.UnityUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
	public class TutorialWindow : MonoBehaviour
	{
		public delegate void TutorialFinished();

		public TutorialFinished OnFinishedTutorial;

		public static bool isOpen;

		public static bool wasOpenThisFrame;

		[SerializeField]
		private TextMeshProUGUI _mainText;

		[SerializeField]
		private TextMeshProUGUI _pagesText;

		[SerializeField]
		private Button _nextPageButton;

		[SerializeField]
		private Button _previousPageButton;

		[SerializeField]
		private Button _finishTutorialButton;

		private TutorialElement _tutorialData;

		private string[] _tutorialText;

		private int _currentPage;

		private int _maxPage;

		public void SetData(TutorialElement element)
		{
			_tutorialData = element;
			_currentPage = 0;
			_tutorialText = element.TutorialText;
			_maxPage = _tutorialText.Length - 1;
			SetMainText();
			SetPagesText();
			SetButtonsStatus();
			GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>().SetSelectedGameObject(_nextPageButton.gameObject);
			PauseMenu.PauseBlock = true;
			isOpen = true;
			wasOpenThisFrame = true;
		}

		private void SetMainText()
		{
			_mainText.text = _tutorialText[_currentPage];
		}

		private void SetPagesText()
		{
			if (_pagesText != null)
			{
				_pagesText.text = _currentPage + 1 + "/" + (_maxPage + 1);
			}
		}

		public void NextPage()
		{
			if (_currentPage < _maxPage)
			{
				_currentPage++;
				SetMainText();
				SetPagesText();
				SetButtonsStatus();
			}
		}

		public void PreviousPage()
		{
			if (_currentPage > 0)
			{
				_currentPage--;
				SetMainText();
				SetPagesText();
				SetButtonsStatus();
			}
		}

		public void FinishTutorial()
		{
			PauseMenu.PauseBlock = false;
			isOpen = false;
			OnFinishedTutorial?.Invoke();
			base.gameObject.SetActive(value: false);
		}

		private void SetButtonsStatus()
		{
			_nextPageButton.gameObject.SetActive(_currentPage < _maxPage);
			_previousPageButton.gameObject.SetActive(_currentPage > 0);
			_finishTutorialButton.gameObject.SetActive(_currentPage == _maxPage);
		}
	}
}
