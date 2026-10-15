using UnityEngine;

public class DestroyObjectsWithTag : MonoBehaviour
{
	public string tagToDestroy = "";

	private void Start()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(tagToDestroy);
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
	}
}
