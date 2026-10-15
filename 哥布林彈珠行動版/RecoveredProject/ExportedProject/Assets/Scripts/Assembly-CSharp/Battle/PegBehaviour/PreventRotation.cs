using Unity.Mathematics;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PreventRotation : MonoBehaviour
	{
		private void Update()
		{
			base.transform.rotation = quaternion.identity;
		}
	}
}
