using System;
using System.Collections;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public abstract class EffectPassingEnemiesBehaviour : MonoBehaviour
	{
		protected Enemy _enemy;

		private EnemyManager _enemyManager;

		protected virtual void Awake()
		{
			_enemy = GetComponent<Enemy>();
			_enemyManager = UnityEngine.Object.FindObjectOfType<EnemyManager>();
		}

		private void OnEnable()
		{
			EnemyManager.OnEnemyMoved = (EnemyManager.EnemyMoved)Delegate.Combine(EnemyManager.OnEnemyMoved, new EnemyManager.EnemyMoved(OnEnemyMoved));
		}

		private void OnDisable()
		{
			EnemyManager.OnEnemyMoved = (EnemyManager.EnemyMoved)Delegate.Remove(EnemyManager.OnEnemyMoved, new EnemyManager.EnemyMoved(OnEnemyMoved));
		}

		private void OnEnemyMoved(int fromSlot, int toSlot, Enemy otherEnemy)
		{
			if (_enemyManager == null || _enemy == null)
			{
				Debug.LogWarning("Null reference in EffectPassingEnemiesBehaviour. Will not effect any enemies.");
			}
			else if (!(_enemy.CurrentHealth <= 0f))
			{
				int slotIndexForEnemy = _enemyManager.GetSlotIndexForEnemy(_enemy, out var _);
				bool flag = (fromSlot > slotIndexForEnemy && toSlot <= slotIndexForEnemy) || (fromSlot == slotIndexForEnemy && toSlot == slotIndexForEnemy);
				WalkEnemy walkEnemy = otherEnemy as WalkEnemy;
				if ((walkEnemy != null) & flag)
				{
					float waitTime = Mathf.Abs(walkEnemy.transform.position.x - base.transform.position.x) / walkEnemy.MoveSpeed;
					StartCoroutine(WaitAndApplyEffect(waitTime, otherEnemy, fromSlot, toSlot, slotIndexForEnemy));
				}
				else if (flag)
				{
					ApplyEffect(otherEnemy);
				}
			}
		}

		private IEnumerator WaitAndApplyEffect(float waitTime, Enemy enemy, int fromSlot, int toSlot, int mySlot)
		{
			yield return new WaitForSeconds(waitTime);
			ApplyEffect(enemy);
		}

		protected abstract void ApplyEffect(Enemy enemy);
	}
}
