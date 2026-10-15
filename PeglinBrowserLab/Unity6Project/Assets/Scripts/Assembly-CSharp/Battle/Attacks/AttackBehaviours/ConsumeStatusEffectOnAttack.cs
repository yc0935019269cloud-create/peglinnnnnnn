using Battle.Attacks.Interfaces;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class ConsumeStatusEffectOnAttack : MonoBehaviour, PostAttackOperation
	{
		public delegate void StatusEffectConsumed(StatusEffectType effect);

		public static StatusEffectConsumed OnStatusEffectConsumed = (StatusEffectType statusEffect) =>
		{
		};

		public StatusEffectType effect;

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			if (statusController.HasEffect(effect))
			{
				statusController.UpdateStatusEffect(new StatusEffect(effect, 0));
				OnStatusEffectConsumed?.Invoke(effect);
			}
		}
	}
}
