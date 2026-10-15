using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class SummonedAttack : Projectile
	{
		public Vector3 SummonOffset = new Vector3(-1f, 0.5f);

		protected Vector3 _target;

		protected Animator _animator;

		private void OnEnable()
		{
			_animator = GetComponentInChildren<Animator>();
		}

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			_target = target;
			base.transform.position = _target + SummonOffset;
			OnProjectileFired?.Invoke();
		}

		private new void Update()
		{
			if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				base.HasHitDestination = true;
				Object.Destroy(base.gameObject);
			}
		}
	}
}
