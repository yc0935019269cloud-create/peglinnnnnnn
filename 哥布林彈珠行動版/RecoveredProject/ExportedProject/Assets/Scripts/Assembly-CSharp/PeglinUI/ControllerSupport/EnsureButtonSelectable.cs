using Rewired.Integration.UnityUI;
using UnityEngine;

namespace PeglinUI.ControllerSupport
{
	public class EnsureButtonSelectable : MonoBehaviour
	{
		private RewiredEventSystem _rewiredEventSystem;

		private ControllerMenuHookup _controllerMenuHookup;

		private void Awake()
		{
			GameObject gameObject = GameObject.FindWithTag("InputSystem");
			_rewiredEventSystem = gameObject.GetComponent<RewiredEventSystem>();
			_controllerMenuHookup = gameObject.GetComponent<ControllerMenuHookup>();
		}

		private void Update()
		{
			if (!PauseMenu.Paused && _rewiredEventSystem != null && (_rewiredEventSystem.currentSelectedGameObject == null || !_rewiredEventSystem.currentSelectedGameObject.activeInHierarchy) && !ControllerInputManager.IsPointerBased)
			{
				_rewiredEventSystem.SetSelectedGameObject(base.gameObject);
				_controllerMenuHookup.LastSelected = base.gameObject;
			}
		}
	}
}
