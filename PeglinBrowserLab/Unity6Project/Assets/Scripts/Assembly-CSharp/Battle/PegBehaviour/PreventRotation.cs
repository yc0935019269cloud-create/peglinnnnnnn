using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PreventRotation : MonoBehaviour
	{
		private void Update()
		{
			base.transform.rotation = Quaternion.identity;
		}
	}
}
