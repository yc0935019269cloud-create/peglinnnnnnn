using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CruciballNumberDisplayManager : MonoBehaviour
{
	public GameObject DigitOne;

	public GameObject DigitTwo;

	public bool SpriteDigits = true;

	public Sprite[] NumberSprites;

	private Image DigitImageOne;

	private Image DigitImageTwo;

	private TextMeshProUGUI _levelText;

	private void OnEnable()
	{
		DigitImageOne = DigitOne.GetComponent<Image>();
		DigitImageTwo = DigitTwo.GetComponent<Image>();
		if (!SpriteDigits)
		{
			DigitOne.SetActive(value: false);
			DigitTwo.SetActive(value: false);
		}
	}

	public void UpdateDisplay(int level)
	{
		if (SpriteDigits)
		{
			int num = level / 10 % 10;
			int num2 = level % 10;
			DigitOne.SetActive(num > 0);
			if (DigitImageOne == null)
			{
				DigitImageOne = DigitOne.GetComponent<Image>();
			}
			if (DigitImageTwo == null)
			{
				DigitImageTwo = DigitTwo.GetComponent<Image>();
			}
			DigitImageOne.sprite = NumberSprites[num];
			DigitImageTwo.sprite = NumberSprites[num2];
		}
		else
		{
			_levelText = GetComponent<TextMeshProUGUI>();
			_levelText.enabled = true;
			_levelText.gameObject.SetActive(value: true);
			_levelText.text = level.ToString();
		}
	}
}
