using System;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ChangeSpriteOnCrit : MonoBehaviour
	{
		[Tooltip("Should this orb swap to the dull sprite right away, and swap back to the main sprite on crit?")]
		public bool startsDull;

		public Sprite originalSprite;

		public Sprite dullSprite;

		private PachinkoBall _pachinkoBall;

		private SpriteRenderer _renderer;

		private bool _hasCrit;

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_renderer = GetComponentInChildren<SpriteRenderer>();
		}

		protected void OnEnable()
		{
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(HandleCriticalHit));
		}

		protected void OnDisable()
		{
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(HandleCriticalHit));
		}

		private void Start()
		{
			if (!BattleController.criticalActive)
			{
				if (startsDull)
				{
					_renderer.sprite = dullSprite;
				}
				return;
			}
			_hasCrit = true;
			if (!startsDull)
			{
				_renderer.sprite = dullSprite;
			}
		}

		protected void HandleCriticalHit()
		{
			if (!_hasCrit && !_pachinkoBall.IsDummy)
			{
				if (startsDull)
				{
					_renderer.sprite = originalSprite;
				}
				else
				{
					_renderer.sprite = dullSprite;
				}
				_hasCrit = true;
			}
		}
	}
}
