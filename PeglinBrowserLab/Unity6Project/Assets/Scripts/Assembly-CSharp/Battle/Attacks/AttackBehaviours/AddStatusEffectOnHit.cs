using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battle.Attacks.AttackBehaviours
{
	public class AddStatusEffectOnHit : MonoBehaviour, IAffectEnemyOnHit
	{
		public StatusEffectType type;

		public int intensity;

		public int critCountThreshold;

		public int critAddIntensity;

		[FormerlySerializedAs("critWindowCumulative")]
		[Tooltip("If this is off, the status effect bonus will be applied once if the crit count is over the threshold. If it's on, it will be applied once every X crits hit where X is the threshold.")]
		public bool isCritThresholdCumulative;

		public void AffectEnemy(Enemy enemy, RelicManager rm, int critCount)
		{
			int num = intensity;
			if (isCritThresholdCumulative && critCountThreshold > 0)
			{
				num += critCount / critCountThreshold * critAddIntensity;
			}
			else if (critCountThreshold <= critCount)
			{
				num += critAddIntensity;
			}
			enemy.ApplyStatusEffect(new StatusEffect(type, num), StatusEffectSource.PLAYER);
		}
	}
}
