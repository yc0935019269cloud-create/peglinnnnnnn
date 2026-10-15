using UnityEngine;

namespace PeglinUI.ControllerSupport
{
	public class SelectableObjectContainer : MonoBehaviour
	{
		[HideInInspector]
		public GameObject selectableObject;

		public GameObject GetSelectableObject()
		{
			return selectableObject;
		}
	}
}
