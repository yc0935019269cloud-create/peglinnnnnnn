using Battle.Pachinko.Obstacles;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegShieldOverlay : DisableFromPrediction
	{
		public delegate void ShieldPegDestroyedEvent(PegShieldOverlay pegShield);

		public static ShieldPegDestroyedEvent OnShieldPegDestroyed = delegate
		{
		};

		public int hitCount;

		public int hitLimit = 4;

		public bool isClaimed;

		private Animator _anim;

		private SpriteRenderer _rend;

		private static readonly int HitCountParam = Animator.StringToHash("HitCount");

		private void Awake()
		{
			_anim = GetComponent<Animator>();
			_rend = GetComponent<SpriteRenderer>();
		}

		private void OnEnable()
		{
			UpdateVisuals();
		}

		public void UpdateAlpha(float a)
		{
			if (_rend != null)
			{
				Color color = _rend.color;
				color.a = a;
				_rend.color = color;
			}
		}

		public void Reset()
		{
			hitCount = 0;
			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			_anim.SetInteger(HitCountParam, hitCount);
			_rend.enabled = hitCount < hitLimit;
		}

		public bool TestShieldedOnActivated()
		{
			if (hitCount <= hitLimit)
			{
				hitCount++;
				_anim.SetInteger(HitCountParam, hitCount);
				if (hitCount == hitLimit)
				{
					OnShieldPegDestroyed?.Invoke(this);
				}
			}
			return hitCount <= hitLimit;
		}

		public void BreakShieldPegImmediately()
		{
			hitCount = hitLimit;
			_anim.SetInteger(HitCountParam, hitCount);
			OnShieldPegDestroyed?.Invoke(this);
		}

		public bool CheckShieldActive()
		{
			return hitCount < hitLimit;
		}
	}
}
