using Battle.Enemies;
using Relics;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class IncreaseEnemyDebuffsOnHit : MonoBehaviour, IAffectEnemyOnHit
	{
		public int DebuffIncreaseAmt = 1;

		public void AffectEnemy(Enemy enemy, RelicManager rm, int critCount)
		{
			enemy.IncrementAllDebuffStatusEffects(DebuffIncreaseAmt);
		}
	}
}
