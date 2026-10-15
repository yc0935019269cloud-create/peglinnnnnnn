using Battle.StatusEffects;
using UnityEngine;

namespace Battle
{
	[CreateAssetMenu(menuName = "Battle/Status Effect Immunities")]
	public class StatusEffectImmunity : ScriptableObject
	{
		public StatusEffectType[] Immunities;
	}
}
