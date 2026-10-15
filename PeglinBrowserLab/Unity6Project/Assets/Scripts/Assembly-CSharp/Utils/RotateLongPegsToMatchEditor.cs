using UnityEngine;

namespace Utils
{
	public class RotateLongPegsToMatchEditor : MonoBehaviour
	{
		private void Start()
		{
			float z = base.transform.rotation.eulerAngles.z;
			base.transform.Rotate(Vector3.forward, z);
		}
	}
}
