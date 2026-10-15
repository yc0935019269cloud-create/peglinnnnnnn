using System;
using UnityEngine;

namespace Data
{
	[Serializable]
	public class WaveGroup
	{
		public enum WaveType
		{
			GROUND = 0,
			STATIONARY = 1,
			FLYING = 2
		}

		public WaveType waveType;

		public WaveData[] waveData;

		public GameObject spawnPoint;

		public WaveGroup(WaveData[] array)
		{
			waveData = array;
		}
	}
}
