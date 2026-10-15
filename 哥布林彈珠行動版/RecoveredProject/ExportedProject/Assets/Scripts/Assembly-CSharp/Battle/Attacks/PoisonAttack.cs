using System.Collections.Generic;
using Battle.StatusEffects;
using I2.Loc;

namespace Battle.Attacks
{
	public class PoisonAttack : ProjectileAttack
	{
		public int PoisonIntensity = 5;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("STATUS_EFFECT_AMOUNT", PoisonIntensity.ToString());
		}

		public override List<StatusEffect> GetStatusEffects(int critCount)
		{
			List<StatusEffect> statusEffects = base.GetStatusEffects(critCount);
			statusEffects.Add(new StatusEffect(StatusEffectType.Poison, PoisonIntensity));
			return statusEffects;
		}
	}
}
