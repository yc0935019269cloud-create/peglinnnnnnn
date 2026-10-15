using System;
using NaughtyAttributes;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class SpiralPegSpawner : NonDummyPegboardBehaviour
	{
		private const float GOLDEN_RATIO = 1.618034f;

		public GameObject pegPrefab;

		public int numArms = 5;

		public int numPegsPerArm = 20;

		public float angleChange = 6f;

		public float distanceChange = 0.4f;

		[Button(null, EButtonEnableMode.Always)]
		private void CreatePegs()
		{
			for (int i = 0; i < numArms; i++)
			{
				float num = (float)(i * 2) * MathF.PI / (float)numArms;
				float num2 = 0.5f;
				float num3 = angleChange;
				for (int j = 0; j < numPegsPerArm; j++)
				{
					UnityEngine.Object.Instantiate(pegPrefab, base.transform).transform.position = base.transform.position + new Vector3(Mathf.Sin(num), Mathf.Cos(num)) * num2;
					num += num3;
					num2 += distanceChange;
				}
			}
		}
	}
}
