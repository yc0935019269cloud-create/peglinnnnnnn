using System.Collections;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class RandomPaintColorProjectile : StraightProjectile
	{
		public Color[] possibleColors;

		public GameObject splatterParticles;

		private SpriteRenderer _spriteRenderer;

		private ParticleSystem _particles;

		private void Awake()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_particles = GetComponent<ParticleSystem>();
			Color color = possibleColors[Random.Range(0, possibleColors.Length)];
			_spriteRenderer.color = color;
			ParticleSystem.MainModule main = _particles.main;
			main.startColor = color;
		}

		private new void Update()
		{
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
			_spriteRenderer.enabled = false;
			_particles.Stop();
			yield return new WaitForSecondsRealtime(5f);
			Object.Destroy(base.gameObject);
		}
	}
}
