using System;
using Battle.Enemies;
using UnityEngine;

namespace Battle.Attacks
{
	public class SelfDamagePerEnemyHit : MonoBehaviour
	{
		public int selfDamage;

		private ShotBehavior _shot;

		private PlayerHealthController _healthController;

		private void OnEnable()
		{
			if (_shot == null)
			{
				_shot = GetComponent<ShotBehavior>();
			}
			if (_healthController == null)
			{
				_healthController = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
			}
			ShotBehavior shot = _shot;
			shot.OnShotHit = (ShotBehavior.ShotHit)Delegate.Combine(shot.OnShotHit, new ShotBehavior.ShotHit(DealSelfDamage));
		}

		private void OnDisable()
		{
			ShotBehavior shot = _shot;
			shot.OnShotHit = (ShotBehavior.ShotHit)Delegate.Remove(shot.OnShotHit, new ShotBehavior.ShotHit(DealSelfDamage));
		}

		private void DealSelfDamage(Enemy enemy, ShotBehavior behavior, bool attackfinished)
		{
			if (enemy != null)
			{
				_healthController.DealSelfDamage(selfDamage, blockable: true, PlayerHealthController.DamageSource.ORB_SELF);
			}
		}
	}
}
