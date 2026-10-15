using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class BeamProjectile : Projectile
	{
		public float startDelay = 1f;

		public Vector2 startOffset = Vector2.zero;

		protected Animator _animator;

		private void OnEnable()
		{
			base.HasHitDestination = false;
			_animator = GetComponentInChildren<Animator>();
			base.transform.position += (Vector3)startOffset;
		}

		private new void Update()
		{
			if (base.HasHitDestination && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			base.gameObject.SetActive(value: true);
			OnProjectileFired?.Invoke();
		}

		protected void HitPointReached()
		{
			base.HasHitDestination = true;
		}
	}
}
