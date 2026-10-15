using System.Collections.Generic;
using Battle.Attacks;
using Battle.StatusEffects;

namespace Battle
{
	public class ThornAttack : ProjectileAttack
	{
		public int ThornIntensity = 1;

		public override List<StatusEffect> GetStatusEffects(int critCount)
		{
			List<StatusEffect> statusEffects = base.GetStatusEffects(critCount);
			statusEffects.Add(new StatusEffect(StatusEffectType.Thorned, ThornIntensity));
			return statusEffects;
		}
	}
}
