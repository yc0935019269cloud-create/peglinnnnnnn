using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public abstract class AttackBaseDamageModifier : MonoBehaviour
	{
		public virtual int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			return 0;
		}

		public virtual int GetStaticDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			return 0;
		}
	}
}
