using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AttackFlatDamageModifier : MonoBehaviour
	{
		public bool addOnlyForAttack = true;

		public virtual int GetFlatDamageMod(DeckManager deckManager, RelicManager relicManager, int critCount = 0)
		{
			return 0;
		}
	}
}
