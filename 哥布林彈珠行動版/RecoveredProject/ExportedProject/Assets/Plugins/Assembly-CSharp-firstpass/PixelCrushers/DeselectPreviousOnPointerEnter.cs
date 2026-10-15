using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(Selectable))]
	public class DeselectPreviousOnPointerEnter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IDeselectHandler
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			GetComponent<Selectable>().OnPointerExit(null);
		}
	}
}
