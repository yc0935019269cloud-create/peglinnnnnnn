using System.Collections.Generic;
using Battle.StatusEffects;

namespace Battle.Attacks
{
	public class BlindAttack : ProjectileAttack
	{
		public int BlindIntensity = 1;

		public override List<StatusEffect> GetStatusEffects(int critCount)
		{
			List<StatusEffect> statusEffects = base.GetStatusEffects(critCount);
			statusEffects.Add(new StatusEffect(StatusEffectType.Blind, BlindIntensity));
			return statusEffects;
		}
	}
}
