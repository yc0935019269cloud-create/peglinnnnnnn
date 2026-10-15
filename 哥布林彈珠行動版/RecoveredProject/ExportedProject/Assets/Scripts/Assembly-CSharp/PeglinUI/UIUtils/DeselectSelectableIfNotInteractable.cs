using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.UIUtils
{
	public class DeselectSelectableIfNotInteractable : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		private Selectable _selectable;

		private void OnEnable()
		{
			_selectable = GetComponent<Selectable>();
		}

		public void OnSelect(BaseEventData eventData)
		{
		}
	}
}
