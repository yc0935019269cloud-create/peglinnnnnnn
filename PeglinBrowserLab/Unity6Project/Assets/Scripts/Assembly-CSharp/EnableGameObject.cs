using UnityEngine;

public class EnableGameObject : MonoBehaviour
{
	public GameObject toEnable;

	public void Enable()
	{
		toEnable.SetActive(value: true);
	}
}
