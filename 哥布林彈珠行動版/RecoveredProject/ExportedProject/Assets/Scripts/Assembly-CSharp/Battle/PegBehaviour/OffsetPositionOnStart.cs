using UnityEngine;

namespace Battle.PegBehaviour
{
	public class OffsetPositionOnStart : NonDummyPegboardBehaviour
	{
		private void Start()
		{
			if (!isDummy)
			{
				base.transform.position = base.transform.position + (Vector3)(Random.insideUnitCircle * 0.25f);
			}
		}
	}
}
