using UnityEngine;

namespace Battle.Enemies.Behaviours.EnemyStances
{
	public abstract class EnemyStance : MonoBehaviour
	{
		public abstract void ApplyStanceToEnemy(Enemy enemy, bool initial = false);

		public abstract void RemoveStanceOnEnemy(Enemy enemy);
	}
}
