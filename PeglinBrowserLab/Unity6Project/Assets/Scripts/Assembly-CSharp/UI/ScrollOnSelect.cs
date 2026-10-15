using PeglinUI.ControllerSupport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class ScrollOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		public int ScrollValue;

		private Scrollbar _scrollbar;

		private void OnEnable()
		{
			_scrollbar = GetComponentInParent<ScrollRect>().verticalScrollbar;
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (ControllerInputManager.primaryInputType == InputType.Controller)
			{
				_scrollbar.value = ScrollValue;
			}
		}
	}
}
