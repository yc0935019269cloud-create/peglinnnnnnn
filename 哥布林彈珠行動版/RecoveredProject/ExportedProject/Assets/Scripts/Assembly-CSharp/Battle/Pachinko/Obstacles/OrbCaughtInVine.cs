using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class OrbCaughtInVine : MonoBehaviour
	{
		public enum OrbSize
		{
			SMALL = 0,
			REGULAR = 1,
			LARGE = 2,
			CUSTOM = 3
		}

		public SpriteRenderer spriteRenderer;

		public Vector3 orbScale;

		public Vector3 spriteRendererScale;

		public float ppu;

		public float colliderRadius;

		public OrbSize orbSize = OrbSize.REGULAR;

		public bool isBramball;
	}
}
