using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.UIUtils
{
	public class MouseOverDetectorPointerHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public MouseOverDetector.MouseEvent OnPointerEntered;

		public MouseOverDetector.MouseEvent OnPointerExited;

		private bool pointerEnteredCalled;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!pointerEnteredCalled)
			{
				OnPointerEntered?.Invoke(base.gameObject);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!pointerEnteredCalled)
			{
				OnPointerExited?.Invoke(base.gameObject);
			}
		}
	}
}
