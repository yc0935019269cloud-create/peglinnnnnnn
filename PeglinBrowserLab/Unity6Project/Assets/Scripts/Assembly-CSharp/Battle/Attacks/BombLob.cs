using System.Collections;
using Battle.Enemies;
using DG.Tweening;
using UnityEngine;

namespace Battle.Attacks
{
	public class BombLob : MonoBehaviour
	{
		public delegate void BombTossComplete(BombLob bomb);

		public delegate void BombExplosionPoint();

		public BombTossComplete OnBombTossComplete;

		public static BombExplosionPoint OnBombExplosionPoint;

		public Vector2 detonateVolMinMax = new Vector2(0.1f, 0.5f);

		public Vector2 bombThrowForceMin = new Vector2(160f, 180f);

		public Vector2 bombThrowForceMax = new Vector2(180f, 220f);

		public bool isRigged;

		protected Animator _animator;

		protected AudioSource _audioSource;

		protected Rigidbody2D _rb;

		protected Collider2D _collider;

		private Tween _rotateTween;

		private void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();
			_rb = GetComponent<Rigidbody2D>();
			_collider = GetComponent<Collider2D>();
		}

		public void Shoot(bool inBoss = false, int totalThrown = 1)
		{
			_audioSource.volume = Mathf.Clamp(1f / (float)totalThrown, detonateVolMinMax.x, detonateVolMinMax.y);
			Vector2 force = ((SpiritOfRadiaBoss.CurrentPhase == 2) ? new Vector2(Random.Range(bombThrowForceMin.x, bombThrowForceMax.x) * 0.7f, Random.Range(bombThrowForceMin.y, bombThrowForceMax.y) * 0.7f) : ((!inBoss) ? new Vector2(Random.Range(bombThrowForceMin.x, bombThrowForceMax.x), Random.Range(bombThrowForceMin.y, bombThrowForceMax.y)) : new Vector2(Random.Range(100f, 100f), Random.Range(120f, 120f))));
			_rb.AddForce(force);
			Rotate();
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			_rb.linearVelocity = Vector2.zero;
			_rb.angularVelocity = 0f;
			_rb.isKinematic = true;
			_collider.enabled = false;
			StartCoroutine(Explode());
		}

		private void Rotate()
		{
			_rotateTween = base.transform.DORotate(new Vector3(0f, 0f, -360f), Random.Range(0.5f, 0.75f), RotateMode.FastBeyond360).SetRelative().SetLoops(-1, LoopType.Incremental)
				.SetEase(Ease.Linear);
		}

		private IEnumerator Explode()
		{
			_rotateTween?.Kill();
			_rotateTween = null;
			_animator.SetTrigger("explode");
			yield return new WaitForSeconds(0.3f);
			OnBombExplosionPoint?.Invoke();
			OnBombTossComplete?.Invoke(this);
			yield return new WaitForSeconds(0.35f);
			Object.Destroy(base.gameObject);
		}
	}
}
