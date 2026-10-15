using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.ControllerSupport
{
	public class CallRunSummaryLineParentOnSelection : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		private Button _button;

		private void OnEnable()
		{
			_button = GetComponent<Button>();
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				_button.onClick.Invoke();
			}
		}
	}
}
