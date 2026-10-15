using System.Globalization;
using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageRelativeToMissingHealth : AttackBaseDamageModifier
	{
		public float regularHitMultiplier = 0.05f;

		public float critHitMultiplier = 0.1f;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			int result = 0;
			if (healthController != null)
			{
				float num = Mathf.Max(healthController.MaxHealth, 100f) - healthController.CurrentHealth;
				result = ((critCount > 0) ? Mathf.FloorToInt(num * critHitMultiplier) : Mathf.FloorToInt(num * regularHitMultiplier));
			}
			return result;
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("TOMAHAWK_PERCENT", Mathf.RoundToInt(regularHitMultiplier * 100f).ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
