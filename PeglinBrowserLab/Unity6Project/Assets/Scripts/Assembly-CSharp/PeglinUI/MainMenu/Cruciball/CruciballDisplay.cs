using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu.Cruciball
{
	public class CruciballDisplay : MonoBehaviour
	{
		public Image[] cruciballSlices;

		public CruciballNumberDisplayManager numberDisplay;

		public void Initialize(int cruciballLevel)
		{
			for (int i = 0; i < cruciballSlices.Length; i++)
			{
				float a = ((i < cruciballLevel) ? 1f : 0f);
				cruciballSlices[i].color = new Color(1f, 1f, 1f, a);
				numberDisplay.UpdateDisplay(cruciballLevel);
			}
		}
	}
}
