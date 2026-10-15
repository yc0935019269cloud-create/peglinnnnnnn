using System.Collections;
using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using UnityEngine;

public class SetSelectedButton : MonoBehaviour
{
	public GameObject _objectToSet;

	private void OnEnable()
	{
		StartCoroutine(SetSelected());
	}

	private IEnumerator SetSelected()
	{
		yield return null;
		if (_objectToSet == null)
		{
			_objectToSet = base.gameObject;
		}
		GameObject obj = GameObject.FindGameObjectWithTag("InputSystem");
		RewiredEventSystem component = obj.GetComponent<RewiredEventSystem>();
		ControllerMenuHookup component2 = obj.GetComponent<ControllerMenuHookup>();
		if (_objectToSet != null && component != null)
		{
			component.SetSelectedGameObject(_objectToSet);
			component2.LastSelected = _objectToSet;
		}
	}
}
