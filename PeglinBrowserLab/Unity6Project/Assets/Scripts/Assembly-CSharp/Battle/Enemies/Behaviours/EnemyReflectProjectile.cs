using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class EnemyReflectProjectile : MonoBehaviour
	{
		public float flatDamage = 5f;

		public int reflectPercent = 10;

		public int reflectIncrease = 2;

		private Enemy _enemy;

		private void Start()
		{
			_enemy = GetComponent<Enemy>();
			_enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Reflect, reflectPercent), StatusEffectSource.ENEMY);
		}
	}
}
