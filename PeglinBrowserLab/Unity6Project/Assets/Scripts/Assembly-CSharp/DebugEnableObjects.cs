using UnityEngine;

public class DebugEnableObjects : MonoBehaviour
{
	public GameObject[] toEnable;

	private void Awake()
	{
		if (!Debug.isDebugBuild)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F6) && toEnable != null)
		{
			GameObject[] array = toEnable;
			foreach (GameObject obj in array)
			{
				obj.SetActive(!obj.activeSelf);
			}
		}
	}
}
