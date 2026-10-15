using Battle.Attacks.Interfaces;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class GrantStatusAfterAttack : MonoBehaviour, PostAttackOperation
	{
		public StatusEffect[] effects;

		public float multiplyOnCrit = 1f;

		public bool multiplyByCritCount;

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			if (BattleController.criticalActive)
			{
				if (multiplyByCritCount)
				{
					StatusEffect[] array = effects;
					foreach (StatusEffect statusEffect in array)
					{
						statusController.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, Mathf.RoundToInt(statusEffect.Intensity * (BattleController.criticalHitCount + 1))), StatusEffectSource.PLAYER);
					}
				}
				else
				{
					StatusEffect[] array = effects;
					foreach (StatusEffect statusEffect2 in array)
					{
						statusController.ApplyStatusEffect(new StatusEffect(statusEffect2.EffectType, Mathf.RoundToInt((float)statusEffect2.Intensity * multiplyOnCrit)), StatusEffectSource.PLAYER);
					}
				}
			}
			else
			{
				statusController.ApplyStatusEffects(effects, StatusEffectSource.PLAYER);
			}
		}
	}
}
