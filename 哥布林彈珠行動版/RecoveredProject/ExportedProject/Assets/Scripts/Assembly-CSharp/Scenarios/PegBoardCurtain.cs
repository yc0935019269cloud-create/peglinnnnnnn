using Map;
using UnityEngine;
using UnityEngine.UI;

namespace Scenarios
{
	public class PegBoardCurtain : MonoBehaviour
	{
		private Color coreBackgroundColor = new Color(0.0588f, 0.0588f, 0.1215f);

		public Image[] images;

		private void Awake()
		{
			if (MapController.instance.Act == 4)
			{
				Image[] array = images;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = coreBackgroundColor;
				}
			}
		}
	}
}
