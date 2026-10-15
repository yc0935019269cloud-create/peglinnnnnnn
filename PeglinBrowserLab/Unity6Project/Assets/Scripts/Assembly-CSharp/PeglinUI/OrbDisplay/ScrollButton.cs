using UnityEngine;

namespace PeglinUI.OrbDisplay
{
	public class ScrollButton : MonoBehaviour
	{
		public OrbInfoWidget orbInfoWidget;

		private void OnMouseDown()
		{
			orbInfoWidget.ShowLargeOrbPreview();
		}
	}
}
