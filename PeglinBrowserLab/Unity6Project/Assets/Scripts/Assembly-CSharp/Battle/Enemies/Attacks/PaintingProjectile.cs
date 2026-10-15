using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class PaintingProjectile : StraightProjectile
	{
		public GameObject _particlePrefab;

		public int numberOfParticles = 10;

		public float radius = 1f;

		public float spreadTime = 0.2f;

		public float postSpreadFireDelay = 0.6f;

		public float spreadingRotationSpeed = 100f;

		public float fastRotationSpeed = 200f;

		public float dispearseRotationSpeed = 400f;

		public float deathDisperseTime = 0.2f;

		private bool _spreading;

		private bool _dispersing;

		private List<GameObject> _particles;

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			base.transform.position += new Vector3(0f, 1f, 0f);
			StartCoroutine(StartFiring(target, projectileStartOffset));
		}

		private IEnumerator StartFiring(Vector3 target, Vector3 projectileStartOffset)
		{
			_spreading = true;
			_particles = new List<GameObject>();
			for (int i = 0; i < numberOfParticles; i++)
			{
				float f = (float)i * 2f * MathF.PI / (float)numberOfParticles;
				float x = Mathf.Sin(f) * radius;
				float y = Mathf.Cos(f) * radius;
				GameObject gameObject = UnityEngine.Object.Instantiate(_particlePrefab, base.transform);
				gameObject.transform.DOLocalMove(new Vector3(x, y, 0f), spreadTime).SetEase(Ease.InSine);
				gameObject.transform.DOScale(gameObject.transform.localScale, deathDisperseTime).SetEase(Ease.OutSine).From(Vector3.zero);
				_particles.Add(gameObject);
			}
			yield return new WaitForSeconds(spreadTime + postSpreadFireDelay);
			_spreading = false;
			base.Fire(target, projectileStartOffset);
		}

		private new void Update()
		{
			if (_spreading)
			{
				base.transform.Rotate(0f, 0f, spreadingRotationSpeed * Time.deltaTime);
				return;
			}
			if (_dispersing)
			{
				base.transform.Rotate(0f, 0f, dispearseRotationSpeed * Time.deltaTime);
				return;
			}
			base.transform.Rotate(0f, 0f, fastRotationSpeed * Time.deltaTime);
			if (base.HasHitDestination && !delayDeathUntilAnimationEnds)
			{
				StartCoroutine(StartDestruction());
			}
			else if (base.HasHitDestination && delayDeathUntilAnimationEnds && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				StartCoroutine(StartDestruction());
			}
		}

		private IEnumerator StartDestruction()
		{
			_dispersing = true;
			foreach (GameObject particle in _particles)
			{
				particle.transform.DOLocalMove(particle.transform.localPosition * 1.3f, deathDisperseTime).SetEase(Ease.OutSine);
				particle.transform.DOScale(Vector3.zero, deathDisperseTime).SetEase(Ease.OutSine);
			}
			yield return new WaitForSeconds(deathDisperseTime);
			_dispersing = false;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
