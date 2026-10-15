using UnityEngine;

namespace PeglinUtils
{
	public class DisableGameObject : MonoBehaviour
	{
		public void Disable()
		{
			base.gameObject.SetActive(value: false);
		}

		public void DisableParent()
		{
			base.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
