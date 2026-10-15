using MoreMountains.Tools;
using NaughtyAttributes;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class SpacedPegSpawner : NonDummyPegboardBehaviour
	{
		public GameObject pegPrefab;

		public int numPegs = 12;

		public float distanceChange = 1.5f;

		public float initialOffset;

		[Button(null, EButtonEnableMode.Always)]
		private void CreatePegs()
		{
			base.transform.MMDestroyAllChildren();
			for (int i = 0; i < numPegs; i++)
			{
				Object.Instantiate(pegPrefab, base.transform).transform.position = base.transform.position + Vector3.right * (initialOffset + distanceChange * (float)i);
			}
		}
	}
}
