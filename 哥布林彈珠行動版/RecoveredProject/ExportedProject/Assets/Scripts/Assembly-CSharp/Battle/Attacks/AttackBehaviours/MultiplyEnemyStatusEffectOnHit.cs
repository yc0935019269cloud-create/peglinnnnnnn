using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class MultiplyEnemyStatusEffectOnHit : MonoBehaviour, IAffectEnemyOnHit
	{
		public StatusEffectType[] types;

		public int multiplier = 2;

		public void AffectEnemy(Enemy enemy, RelicManager rm, int critCount)
		{
			StatusEffectType[] array = types;
			foreach (StatusEffectType type in array)
			{
				enemy.IncrementStatusEffectMultiplier(type, multiplier);
			}
		}
	}
}
