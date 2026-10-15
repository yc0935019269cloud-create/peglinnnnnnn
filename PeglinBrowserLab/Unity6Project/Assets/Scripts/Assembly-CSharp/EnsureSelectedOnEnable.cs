using UnityEngine;
using UnityEngine.EventSystems;

public class EnsureSelectedOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}
}
