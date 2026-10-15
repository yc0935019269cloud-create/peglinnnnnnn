using Map.MinimapView;
using UnityEngine;

namespace UI
{
	public class MapButtonCloseAction : MonoBehaviour
	{
		public void MapButtonPressed()
		{
			MapPreviewCameraMovement component = Camera.main.GetComponent<MapPreviewCameraMovement>();
			if (component != null)
			{
				component.MapButtonPressed();
			}
		}
	}
}
