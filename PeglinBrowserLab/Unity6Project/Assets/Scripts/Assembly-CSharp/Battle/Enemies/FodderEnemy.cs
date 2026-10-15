using UnityEngine;

namespace Battle.Enemies
{
	public class FodderEnemy : WalkEnemy
	{
		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			return false;
		}

		public void ForceMoveToPoint(Vector3 position)
		{
			StartCoroutine(StartMove(position));
		}
	}
}
