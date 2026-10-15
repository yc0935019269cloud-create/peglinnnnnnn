using System;
using Battle.Enemies;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class LifestealOrbDamage : MonoBehaviour
	{
		public float lifestealPercent;

		private void OnEnable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(OnEnemyDamaged));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(OnEnemyDamaged));
		}

		private void OnEnemyDamaged(Enemy enemy, long damage = 0L, Enemy.EnemyDamageSource damageSource = Enemy.EnemyDamageSource.Unspecified)
		{
			if (damageSource == Enemy.EnemyDamageSource.ProjectileAttack || damageSource == Enemy.EnemyDamageSource.TargetedAttack || damageSource == Enemy.EnemyDamageSource.AOE)
			{
				PlayerHealthController.OnExternalHealRequested(Mathf.Max(Mathf.Ceil((float)damage * lifestealPercent), 0f));
			}
		}
	}
}
