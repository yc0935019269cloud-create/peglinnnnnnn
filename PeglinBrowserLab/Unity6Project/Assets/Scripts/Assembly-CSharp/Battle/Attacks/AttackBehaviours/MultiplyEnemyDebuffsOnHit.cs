using System;
using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class MultiplyEnemyDebuffsOnHit : MonoBehaviour, IAffectEnemyOnHit
	{
		public int multiplier = 2;

		public void AffectEnemy(Enemy enemy, RelicManager rm, int critCount)
		{
			foreach (StatusEffectType value in Enum.GetValues(typeof(StatusEffectType)))
			{
				if (StatusEffect.IsStatusEffectDebuff(value))
				{
					enemy.IncrementStatusEffectMultiplier(value, multiplier);
				}
			}
		}
	}
}
