using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.UIUtils
{
	public class SetObjSelectedOnEnable : MonoBehaviour
	{
		public GameObject objectToSelect;

		private void OnEnable()
		{
			EventSystem.current.SetSelectedGameObject(objectToSelect);
		}
	}
}
