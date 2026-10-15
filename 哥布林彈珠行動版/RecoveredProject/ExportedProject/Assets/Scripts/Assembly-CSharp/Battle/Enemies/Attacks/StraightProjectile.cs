using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class StraightProjectile : Projectile
	{
		public float Speed = 1f;

		public Vector3 FiringOffset = new Vector3(-1f, 0.5f);

		public bool tweenYAxis;

		public Ease EaseType = Ease.Linear;

		public bool delayDeathUntilAnimationEnds;

		public GameObject spawnOnEnable;

		public Vector3 spawnOffset = Vector3.zero;

		public float rotateForce = -1f;

		protected Vector3 _target;

		protected Animator _animator;

		protected GameObject _spawnedElement;

		private void OnEnable()
		{
			_animator = GetComponentInChildren<Animator>();
		}

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			base.transform.position += FiringOffset + projectileStartOffset;
			if (spawnOnEnable != null)
			{
				_spawnedElement = UnityEngine.Object.Instantiate(spawnOnEnable, base.transform.position + spawnOffset, Quaternion.identity);
			}
			_target = target;
			float num = (base.transform.position.x - target.x) / Speed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(target.x, num).SetEase(EaseType);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveComplete));
			if (tweenYAxis)
			{
				base.transform.DOMoveY(target.y, num).SetEase(EaseType);
			}
			if (rotateForce > 0f)
			{
				Rigidbody2D component = GetComponent<Rigidbody2D>();
				if (component != null)
				{
					component.AddTorque(0f - rotateForce);
				}
			}
			OnProjectileFired?.Invoke(num);
		}

		private new void Update()
		{
			if (base.HasHitDestination && !delayDeathUntilAnimationEnds)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (base.HasHitDestination && delayDeathUntilAnimationEnds && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void MoveComplete()
		{
			base.HasHitDestination = true;
		}

		private void OnDisable()
		{
			UnityEngine.Object.Destroy(_spawnedElement);
		}
	}
}
