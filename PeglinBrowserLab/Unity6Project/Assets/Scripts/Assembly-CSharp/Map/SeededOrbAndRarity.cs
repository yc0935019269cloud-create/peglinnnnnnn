using UnityEngine;

namespace Map
{
	public struct SeededOrbAndRarity
	{
		public GameObject orb;

		public PachinkoBall.OrbRarity orbRarity;

		public SeededOrbAndRarity(GameObject seededOrb, PachinkoBall.OrbRarity seededOrbRarity)
		{
			orb = seededOrb;
			orbRarity = seededOrbRarity;
		}
	}
}
