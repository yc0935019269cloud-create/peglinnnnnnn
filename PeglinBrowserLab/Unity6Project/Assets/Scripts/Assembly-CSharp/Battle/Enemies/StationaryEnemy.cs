using UnityEngine;

namespace Battle.Enemies
{
	public class StationaryEnemy : Enemy
	{
		[Space(10f)]
		public Vector2 spawnOffset;

		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			return false;
		}
	}
}
