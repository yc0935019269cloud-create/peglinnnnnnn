using System.Collections;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu
{
	public class XpDisplay : MonoBehaviour
	{
		public Slider xpSlider;

		public TextMeshProUGUI currentXp;

		public TextMeshProUGUI totalXp;

		public void SetupXpBar(int currentXpValue, int xpAdd, int xpMax)
		{
			xpSlider.maxValue = xpMax;
			xpSlider.value = currentXpValue;
			currentXp.text = currentXpValue.ToString();
			totalXp.text = xpMax.ToString();
			if (xpAdd > 0)
			{
				StartCoroutine(DoXpAdd(currentXpValue, currentXpValue + xpAdd));
			}
		}

		private IEnumerator DoXpAdd(int currentXpValue, int finalXp)
		{
			yield return new WaitForSeconds(1f);
			xpSlider.DOValue(finalXp, 1f);
			currentXp.DOCounter(currentXpValue, finalXp, 1f, addThousandsSeparator: false, CultureInfo.InvariantCulture);
		}
	}
}
