using System;
using UnityEngine;

namespace Battle.Enemies.Polish
{
	public class PaintingAttackAnimationTrigger : MonoBehaviour
	{
		public string endAttackTrigger = "end_absorb";

		private Enemy _painting;

		private Animator _anim;

		private void Awake()
		{
			_painting = GetComponent<Enemy>();
			_anim = GetComponent<Animator>();
		}

		private void OnEnable()
		{
			Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Combine(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(TriggerPaintingAttackEnd));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Remove(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(TriggerPaintingAttackEnd));
		}

		private void TriggerPaintingAttackEnd(float damage, bool melee = true, Enemy enemy = null, PlayerHealthController.DamageSource source = PlayerHealthController.DamageSource.UNSPECIFIED, bool forceMaxHPDamage = false)
		{
			if (enemy == _painting)
			{
				_anim.SetTrigger(endAttackTrigger);
			}
		}
	}
}
