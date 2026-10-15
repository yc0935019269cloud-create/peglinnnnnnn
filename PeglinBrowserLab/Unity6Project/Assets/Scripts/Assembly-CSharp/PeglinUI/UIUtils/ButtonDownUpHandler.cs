using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeglinUI.UIUtils
{
	[RequireComponent(typeof(Button))]
	public class ButtonDownUpHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		public UnityEvent onPointerDown;

		public UnityEvent onPointerUp;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_button.interactable)
			{
				onPointerDown?.Invoke();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_button.interactable)
			{
				onPointerUp?.Invoke();
			}
		}
	}
}
