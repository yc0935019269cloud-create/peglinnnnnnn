using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class OrbserverSwordProjectile : Projectile
	{
		public AudioSource audioSource;

		public Transform swordTransform;

		public float explodeOutDistance = 1f;

		public float explodeOutTime = 1f;

		public float minExplodeOutPostWaitTime = 0.5f;

		public float rotateToTargetTime = 0.5f;

		public float postRotateWaitTime = 0.5f;

		public float rotateAroundCenterAngle = 80f;

		public float Speed = 1f;

		public Vector3 FiringOffset = new Vector3(-1f, 0.5f);

		public bool tweenYAxis;

		public Ease EaseType = Ease.Linear;

		public GameObject spawnOnEnable;

		public Vector3 spawnOffset = Vector3.zero;

		protected Vector3 _target;

		protected GameObject _spawnedElement;

		private bool _destroying;

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			base.transform.position += FiringOffset + projectileStartOffset;
			if (spawnOnEnable != null)
			{
				_spawnedElement = UnityEngine.Object.Instantiate(spawnOnEnable, base.transform.position + spawnOffset, Quaternion.identity);
			}
			_target = target;
			StartCoroutine(DoFiringSequence(target));
		}

		private IEnumerator DoFiringSequence(Vector3 target)
		{
			float num = MathF.PI * 2f / (float)(totalExtraProjectiles + 1) * (float)extraProjectileForAttackNumber + MathF.PI / 2f;
			float z = num * 57.29578f;
			swordTransform.localRotation = Quaternion.Euler(0f, 0f, z);
			base.transform.DORotate(new Vector3(0f, 0f, rotateAroundCenterAngle), explodeOutTime + minExplodeOutPostWaitTime, RotateMode.FastBeyond360).SetEase(Ease.InOutSine).SetDelay(extraProjectileTotalSpawnTime - extraProjectileTotalSpawnTime / (float)(totalExtraProjectiles + 1) * (float)extraProjectileForAttackNumber);
			yield return StartCoroutine(ExplodeOutFromPoint(num));
			yield return StartCoroutine(RotateToTarget(target));
			FlyToTarget(target);
		}

		private IEnumerator ExplodeOutFromPoint(float angle)
		{
			Vector2 vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * explodeOutDistance;
			swordTransform.DOLocalMove(vector, explodeOutTime).SetEase(Ease.OutQuart);
			yield return new WaitForSeconds(explodeOutTime);
			yield return new WaitForSeconds(minExplodeOutPostWaitTime);
		}

		private IEnumerator RotateToTarget(Vector3 target)
		{
			Vector3 normalized = (target - swordTransform.position).normalized;
			Quaternion.Euler(0f, 0f, Mathf.Atan2(normalized.y, normalized.x) * 57.29578f);
			swordTransform.DORotate(new Vector3(0f, 0f, Mathf.Atan2(normalized.y, normalized.x) * 57.29578f), rotateToTargetTime).SetEase(Ease.InCirc);
			yield return new WaitForSeconds(rotateToTargetTime + postRotateWaitTime);
		}

		private void FlyToTarget(Vector3 target)
		{
			audioSource.Play();
			float num = (swordTransform.position.x - target.x) / Speed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = swordTransform.DOMoveX(target.x, num).SetEase(EaseType);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveComplete));
			if (tweenYAxis)
			{
				swordTransform.DOMoveY(target.y, num).SetEase(EaseType);
			}
			OnProjectileFired?.Invoke(num);
		}

		protected override void Update()
		{
		}

		public void MoveComplete()
		{
			base.HasHitDestination = true;
			GetComponentInChildren<SpriteRenderer>().enabled = false;
			StartCoroutine(WaitAndDestroy());
		}

		private IEnumerator WaitAndDestroy()
		{
			yield return new WaitForSeconds(1f);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void OnDisable()
		{
			UnityEngine.Object.Destroy(_spawnedElement);
		}
	}
}
