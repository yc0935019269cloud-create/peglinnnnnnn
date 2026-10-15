using System;
using Battle.Enemies;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class BuffAlliesWhenAttacked : MonoBehaviour
	{
		public int damageThreshold = 20;

		private Enemy _enemy;

		private BuffAlliesWhenAttackedReceiver[] _receivers;

		private void Awake()
		{
			_enemy = GetComponent<Enemy>();
		}

		private void Start()
		{
			_receivers = UnityEngine.Object.FindObjectsOfType<BuffAlliesWhenAttackedReceiver>();
		}

		private void OnEnable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(HandleEnemyDamaged));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(HandleEnemyDamaged));
		}

		private void HandleEnemyDamaged(Enemy enemy, long damage, Enemy.EnemyDamageSource damageSource)
		{
			if (enemy == _enemy && damage > damageThreshold && (damageSource == Enemy.EnemyDamageSource.ProjectileAttack || damageSource == Enemy.EnemyDamageSource.TargetedAttack || damageSource == Enemy.EnemyDamageSource.AOE))
			{
				BuffAlliesWhenAttackedReceiver[] receivers = _receivers;
				for (int i = 0; i < receivers.Length; i++)
				{
					receivers[i].OnAllyAttacked();
				}
			}
		}
	}
}
