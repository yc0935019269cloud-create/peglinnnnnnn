using UnityEngine;

namespace Battle.Attacks
{
	public class SimpleShotAll : MonoBehaviour
	{
		public delegate void AttackEvent();

		public AttackEvent OnHitPointReached = () =>
		{
		};

		public AttackEvent OnAttackAnimationEnded = () =>
		{
		};

		[Header("Audio")]
		public AttackSfx AttackSfx;

		private float _defaultTimer = 3f;

		private float _timer;

		private bool _hitPointReached;

		private bool _attackAnimationEnded;

		private AudioSource _audioSource;

		public void Initialize(AudioSource audioSource)
		{
			_audioSource = audioSource;
			if (AttackSfx.SlowAttackClip != null)
			{
				audioSource.PlayOneShot(AttackSfx.SlowAttackClip);
			}
			else if (AttackSfx.FastAttackClip != null)
			{
				audioSource.PlayOneShot(AttackSfx.FastAttackClip);
			}
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer >= _defaultTimer)
			{
				if (!_hitPointReached)
				{
					OnHitPointReached?.Invoke();
				}
				if (!_attackAnimationEnded)
				{
					OnAttackAnimationEnded?.Invoke();
				}
			}
		}

		public void HitPointReached()
		{
			if (!_hitPointReached)
			{
				OnHitPointReached?.Invoke();
				_audioSource.PlayOneShot(AttackSfx.OnHitClip);
				_hitPointReached = true;
			}
		}

		public void AttackAnimationEnded()
		{
			if (!_attackAnimationEnded)
			{
				OnAttackAnimationEnded?.Invoke();
				_attackAnimationEnded = true;
				Object.Destroy(base.transform.parent.gameObject);
			}
		}
	}
}
