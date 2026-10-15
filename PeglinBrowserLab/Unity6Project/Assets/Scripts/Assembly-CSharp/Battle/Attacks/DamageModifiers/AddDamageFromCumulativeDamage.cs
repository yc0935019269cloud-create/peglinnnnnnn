using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageFromCumulativeDamage : AttackBaseDamageModifier
	{
		public float damageAt100DamageTaken = 10f;

		public float critAt100DamageTaken = 15f;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			int result = 0;
			if (healthController != null)
			{
				result = Mathf.CeilToInt(((critCount > 0) ? critAt100DamageTaken : damageAt100DamageTaken) * (healthController.totalDamageTakenThisBattle / 100f));
			}
			return result;
		}
	}
}
