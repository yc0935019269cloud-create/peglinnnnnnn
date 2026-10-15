using System;
using System.Collections;
using Battle.Enemies.C20Upgrades;
using UnityEngine;

namespace Battle.Enemies
{
	public class BallistaBoss : StationaryEnemy
	{
		private BossUpgrade _bossUpgrade;

		[Header("Ballista Boss References")]
		public Animator FloorKnight;

		public Animator ArcherKnight;

		public Animator LadderKnight;

		[Header("Ballista Boss Properties")]
		[SerializeField]
		private float _minionKnightAttackDelay = 0.5f;

		[SerializeField]
		private float _delayBetweenShoutAndFire = 0.25f;

		[SerializeField]
		private float _minionKnightEmoteTime = 0.25f;

		private Enemy _enemy;

		private bool _dead;

		private static readonly int AttackParam = Animator.StringToHash("attack");

		private static readonly int DeathParam = Animator.StringToHash("death");

		protected override void Awake()
		{
			_bossUpgrade = GetComponent<BossUpgrade>();
			base.Awake();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_enemy = GetComponent<Enemy>();
			Enemy.OnEnemyDamaged = (EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new EnemyDamaged(HandleEnemyDamaged));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Enemy.OnEnemyDamaged = (EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new EnemyDamaged(HandleEnemyDamaged));
		}

		public override IEnumerator RangedAttack(Vector3 targetPos, float damage = -1f, GameObject projectileOverride = null, string animTriggerOverride = "")
		{
			CurrentlyActing = true;
			LadderKnight.SetTrigger(AttackParam);
			yield return new WaitForSeconds(_minionKnightAttackDelay);
			FloorKnight.SetTrigger(AttackParam);
			FloorKnight.GetComponent<Emoter>().ShowEmote(Emoter.Emote.EXCLAMATION, _minionKnightEmoteTime);
			ArcherKnight.SetTrigger(AttackParam);
			FloorKnight.GetComponent<Emoter>().ShowEmote(Emoter.Emote.EXCLAMATION, _minionKnightEmoteTime);
			yield return new WaitForSeconds(_delayBetweenShoutAndFire);
			CurrentlyActing = false;
			yield return base.RangedAttack(targetPos);
			if (_bossUpgrade != null && _bossUpgrade.isUpgraded)
			{
				currentChargeTime++;
			}
		}

		private void HandleEnemyDamaged(Enemy enemy, long damage, EnemyDamageSource damageSource)
		{
			if (enemy == _enemy && enemy.CurrentHealth <= 0f && !_dead)
			{
				_dead = true;
				LadderKnight.SetTrigger(DeathParam);
				FloorKnight.SetTrigger(DeathParam);
				ArcherKnight.SetTrigger(DeathParam);
			}
		}
	}
}
