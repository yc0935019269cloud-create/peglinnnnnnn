using System.Collections;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
	public float seconds = 1f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(seconds);
		Object.Destroy(base.gameObject);
	}
}
