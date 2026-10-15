using UnityEngine;

namespace Battle.Enemies
{
	public class SpecialMovementEnemy : Enemy
	{
		public bool isMoving;

		public virtual void StartSpecialMove(EnemyManager.EnemySlot[] groundSlots, EnemyManager.EnemySlot[] flyingSlots)
		{
			Debug.LogWarning("SpecialMove not implemented in base class.");
		}
	}
}
