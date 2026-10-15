using System;

namespace Data
{
	[Serializable]
	public class StarterSpawn
	{
		public WaveGroup.WaveType spawnerType;

		public SpawnData spawnData;

		public int slot;
	}
}
