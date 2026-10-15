using Rewired.Integration.UnityUI;
using UI;
using UnityEngine;

namespace PeglinUI.ControllerSupport
{
	public class SubMenuControllerConnection : MonoBehaviour
	{
		[Tooltip("GameObject that we want to completely disable while this menu is active")]
		public GameObject prevMenuToHide;

		[Tooltip("CanvasGroup - Alternative if the prev menu is still visible and we just don't want it interactable")]
		public CanvasGroup prevMenuCanvasGroup;

		public GameObject firstActiveInMenu;

		public GameObject setActiveOnReturn;

		public bool forceFirstSelectionOnMouseInput;

		private ControllerMenuHookup _controllerMenuHookup;

		private RewiredEventSystem _rewiredEventSystem;

		private void OnEnable()
		{
			if (prevMenuToHide != null)
			{
				prevMenuToHide.SetActive(value: false);
			}
			if (prevMenuCanvasGroup != null)
			{
				prevMenuCanvasGroup.interactable = false;
			}
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			_controllerMenuHookup = _rewiredEventSystem.GetComponent<ControllerMenuHookup>();
			AssignActive(firstActiveInMenu);
		}

		private void OnDisable()
		{
			if (prevMenuToHide != null)
			{
				prevMenuToHide.SetActive(value: true);
			}
			if (prevMenuCanvasGroup != null)
			{
				prevMenuCanvasGroup.interactable = true;
			}
			_rewiredEventSystem.SetSelectedGameObject(null);
			AssignActive(setActiveOnReturn);
		}

		private void AssignActive(GameObject toAssign)
		{
			if (toAssign == null)
			{
				return;
			}
			_controllerMenuHookup.LastSelected = toAssign;
			if (!ControllerInputManager.IsPointerBased)
			{
				_rewiredEventSystem.SetSelectedGameObject(toAssign);
				ButtonHandleHover component = toAssign.GetComponent<ButtonHandleHover>();
				if (component != null)
				{
					component.Highlight();
				}
			}
			else if (forceFirstSelectionOnMouseInput)
			{
				_rewiredEventSystem.SetSelectedGameObject(toAssign);
			}
		}
	}
}
