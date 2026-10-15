using UnityEngine;
using UnityEngine.EventSystems;

public class DebugCheckSelectedObject : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		Debug.Log(EventSystem.current.currentSelectedGameObject);
	}
}
