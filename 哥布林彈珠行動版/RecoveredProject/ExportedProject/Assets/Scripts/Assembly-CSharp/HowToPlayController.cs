using TMPro;
using UnityEngine;

public class HowToPlayController : MonoBehaviour
{
	public GameObject howToPlayContainer;

	public GameObject[] pages;

	public TMP_Text pageText;

	public GameObject prevButton;

	public GameObject nextButton;

	private int _activePage;

	public void ShowModal()
	{
		howToPlayContainer.SetActive(value: true);
		GameObject[] array = pages;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		pages[0].SetActive(value: true);
		_activePage = 0;
		prevButton.SetActive(value: false);
		UpdatePageText();
		UpdateButtons();
	}

	public void NextButtonClicked()
	{
		if (_activePage < pages.Length - 1)
		{
			pages[_activePage].SetActive(value: false);
			_activePage++;
			pages[_activePage].SetActive(value: true);
			UpdatePageText();
			UpdateButtons();
		}
	}

	public void NextButtonOrClose()
	{
		if (_activePage >= pages.Length - 1)
		{
			CloseModal();
		}
		else
		{
			NextButtonClicked();
		}
	}

	public void PrevButtonClicked()
	{
		if (_activePage > 0)
		{
			pages[_activePage].SetActive(value: false);
			_activePage--;
			pages[_activePage].SetActive(value: true);
			UpdatePageText();
			UpdateButtons();
		}
	}

	public void PrevButtonOrClose()
	{
		if (_activePage <= 0)
		{
			CloseModal();
		}
		else
		{
			PrevButtonClicked();
		}
	}

	private void UpdatePageText()
	{
		pageText.text = _activePage + 1 + "/" + pages.Length;
	}

	private void UpdateButtons()
	{
		if (_activePage >= pages.Length - 1)
		{
			nextButton.SetActive(value: false);
		}
		else
		{
			nextButton.SetActive(value: true);
		}
		if (_activePage == 0)
		{
			prevButton.SetActive(value: false);
		}
		else
		{
			prevButton.SetActive(value: true);
		}
	}

	public void CloseModal()
	{
		howToPlayContainer.SetActive(value: false);
	}
}
