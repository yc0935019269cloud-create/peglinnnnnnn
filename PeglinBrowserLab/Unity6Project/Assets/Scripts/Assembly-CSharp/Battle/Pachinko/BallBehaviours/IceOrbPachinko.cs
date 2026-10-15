using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class IceOrbPachinko : PierceBehavior
	{
		private const int POPPED_PEG_TRIGGER_LAYER = 19;

		public Sprite pierceSprite;

		private Sprite _originalSprite;

		private Animator _anim;

		private SpriteRenderer _renderer;

		[HideInInspector]
		public bool donePierce;

		protected override void OnEnable()
		{
			base.OnEnable();
			_anim = GetComponentInChildren<Animator>();
			_anim.enabled = false;
			_renderer = GetComponentInChildren<SpriteRenderer>();
			_originalSprite = _renderer.sprite;
			_renderer.sprite = pierceSprite;
			CanPierce = true;
			ShouldPierceOnRestart = true;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (!donePierce && collision.gameObject.layer != 19)
			{
				PierceStart(collision.GetComponentInParent<Peg>());
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (!donePierce && collision.gameObject.layer != 19 && PierceEnd(collision.GetComponentInParent<Peg>()))
			{
				donePierce = true;
				if (TryGetComponent<GhostOrbPachinko>(out var component))
				{
					component.iceComponentHasPierced = true;
					component.ForceUpdateGhostEffect(GetComponent<Rigidbody2D>().linearVelocity);
				}
			}
		}

		protected override void ReturnToCollision()
		{
			base.ReturnToCollision();
			CanPierce = false;
			if (!_ball.IsDummy)
			{
				_renderer.sprite = _originalSprite;
				_anim.enabled = true;
			}
		}

		public override void RestartPierceForSimulation()
		{
			base.RestartPierceForSimulation();
			if (_ball.IsDummy)
			{
				donePierce = false;
			}
		}
	}
}
