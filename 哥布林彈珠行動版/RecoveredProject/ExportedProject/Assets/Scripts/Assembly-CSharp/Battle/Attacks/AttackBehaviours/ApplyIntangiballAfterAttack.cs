using Battle.Attacks.Interfaces;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class ApplyIntangiballAfterAttack : MonoBehaviour, PostAttackOperation
	{
		public int startingPoint = 30;

		public int reductionPerAttack = 5;

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			int num = statusController.EffectStrength(StatusEffectType.Intangiball);
			if (num == 0)
			{
				statusController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, startingPoint - reductionPerAttack), StatusEffectSource.PLAYER);
				return;
			}
			int intensity = reductionPerAttack;
			if (num - reductionPerAttack <= 0)
			{
				intensity = num - 1;
			}
			statusController.RemoveStatusEffectIntensity(StatusEffectType.Intangiball, intensity);
		}
	}
}
