using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageFromDamageReceived : AttackBaseDamageModifier
	{
		public float multiplier = 1f;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			int result = 0;
			if (healthController != null)
			{
				result = ((critCount <= 0) ? Mathf.RoundToInt(multiplier * healthController.LastSourceOfDamage) : Mathf.RoundToInt(multiplier * (healthController.LastSourceOfDamage + healthController.NextToLastSourceOfDamage)));
			}
			return result;
		}

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("MULTIPLIER", (multiplier == 1f) ? "" : $"{multiplier}*");
		}
	}
}
