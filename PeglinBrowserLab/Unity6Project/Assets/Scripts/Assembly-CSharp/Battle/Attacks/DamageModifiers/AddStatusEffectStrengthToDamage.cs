using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddStatusEffectStrengthToDamage : AttackBaseDamageModifier
	{
		public float damagePerStack = 1f;

		public float critPerStack = 2f;

		public int stackDivisor = 1;

		public StatusEffectType effectToConsume;

		private LocalizationParamsManager _paramsManager;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController sfController, int critCount = 0)
		{
			if (sfController != null)
			{
				int num = sfController.EffectStrength(effectToConsume) / stackDivisor;
				if (critCount != 0)
				{
					return Mathf.CeilToInt(critPerStack * (float)num);
				}
				return Mathf.CeilToInt(damagePerStack * (float)num);
			}
			return 0;
		}

		private void SetLocParams()
		{
			_paramsManager.SetParameterValue("STATUS_EFFECT_BONUS_DMG", damagePerStack.ToString());
			_paramsManager.SetParameterValue("STATUS_EFFECT_BONUS_CRIT", critPerStack.ToString());
			_paramsManager.SetParameterValue("STATUS_EFFECT_COUNT", stackDivisor.ToString());
		}
	}
}
