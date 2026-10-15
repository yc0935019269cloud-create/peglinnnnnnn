using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
	public void DestroyThisObject()
	{
		Object.Destroy(base.gameObject);
	}
}
