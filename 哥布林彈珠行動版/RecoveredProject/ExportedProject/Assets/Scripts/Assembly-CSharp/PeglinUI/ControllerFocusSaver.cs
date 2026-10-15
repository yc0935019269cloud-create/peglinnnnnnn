using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ControllerFocusSaver : MonoBehaviour
	{
		public GameObject FocusObject;

		private CanvasGroup canvasGroup;

		private void Start()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		private void Update()
		{
			if (EventSystem.current.currentSelectedGameObject == null && canvasGroup.interactable)
			{
				EventSystem.current.SetSelectedGameObject(FocusObject);
			}
		}
	}
}
