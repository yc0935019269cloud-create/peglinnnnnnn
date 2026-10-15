using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class AddRandomStatusEffectOnHit : MonoBehaviour, IAffectEnemyOnHit
	{
		public static readonly StatusEffectType[] RoundreloquencePotentialEffects = new StatusEffectType[5]
		{
			StatusEffectType.Thorned,
			StatusEffectType.Poison,
			StatusEffectType.Blind,
			StatusEffectType.Exploitaball,
			StatusEffectType.Transpherency
		};

		public static readonly int[] RoundreloquenceEffectIntensity = new int[5] { 1, 2, 4, 3, 2 };

		public void AffectEnemy(Enemy enemy, RelicManager rm, int critCount)
		{
			int num = Random.Range(0, RoundreloquencePotentialEffects.Length);
			StatusEffectType effectType = RoundreloquencePotentialEffects[num];
			rm.AttemptUseRelic(RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT);
			enemy.ApplyStatusEffect(new StatusEffect(effectType, RoundreloquenceEffectIntensity[num]), StatusEffectSource.PLAYER);
		}
	}
}
