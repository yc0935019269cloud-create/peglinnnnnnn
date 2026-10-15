using Battle.Attacks.Interfaces;
using Battle.Enemies;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks
{
	public class GrantStatusToEnemiesAfterAttack : MonoBehaviour, PostAttackOperation
	{
		public StatusEffect[] effects;

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			foreach (Enemy enemy in Object.FindObjectOfType<EnemyManager>().Enemies)
			{
				StatusEffect[] array = effects;
				foreach (StatusEffect statusEffect in array)
				{
					enemy.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType), StatusEffectSource.PLAYER);
				}
			}
		}
	}
}
