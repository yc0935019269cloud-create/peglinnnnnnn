using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.Encirclepedia
{
	public class ShowAchievementNameOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public GameObject nameRibbon;

		public void OnSelect(BaseEventData eventData)
		{
			nameRibbon.SetActive(value: true);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			nameRibbon.SetActive(value: false);
		}
	}
}
