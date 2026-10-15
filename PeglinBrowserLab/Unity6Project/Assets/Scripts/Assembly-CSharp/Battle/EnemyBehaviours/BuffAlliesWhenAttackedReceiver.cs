using Battle.Enemies;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class BuffAlliesWhenAttackedReceiver : MonoBehaviour
	{
		private Enemy _enemy;

		public int buffAmount = 1;

		private void Awake()
		{
			_enemy = GetComponent<Enemy>();
		}

		public void OnAllyAttacked()
		{
			_enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, buffAmount), StatusEffectSource.ENEMY);
		}
	}
}
