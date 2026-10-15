using System;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using UnityEngine;

public class GhostOrbPachinko : PierceBehavior, IUpdateableBySimulation
{
	public bool flipAxis;

	public bool bothAxis;

	private bool _onAscent;

	private Color _regularColour = Color.white;

	private Color _ghostColour = Color.white;

	private Rigidbody2D _rigid;

	private SpriteRenderer _renderer;

	private IceOrbPachinko _iceOrbPachinko;

	[HideInInspector]
	public bool iceComponentHasPierced = true;

	private int _nonTriggerCollisionLayer = 13;

	private bool _isDummy;

	private void Awake()
	{
		if (TryGetComponent<Attack>(out var component) && component.locNameString == "necorb")
		{
			_nonTriggerCollisionLayer = 23;
		}
	}

	private void Start()
	{
		_iceOrbPachinko = GetComponent<IceOrbPachinko>();
		iceComponentHasPierced = _iceOrbPachinko == null || _iceOrbPachinko.donePierce;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.gameObject.layer = GetStartingLayer();
		_rigid = GetComponent<Rigidbody2D>();
		_renderer = GetComponentInChildren<SpriteRenderer>();
		_regularColour = _renderer.color;
		_ghostColour = _regularColour;
		_ghostColour.a = 0.5f * _ghostColour.a;
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ForceUpdateGhostEffect));
	}

	private void OnDisable()
	{
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ForceUpdateGhostEffect));
	}

	protected override int GetStartingLayer()
	{
		if (TryGetComponent<IceOrbPachinko>(out var _))
		{
			return 18;
		}
		return _nonTriggerCollisionLayer;
	}

	private void Update()
	{
		if (!_isDummy)
		{
			DoUpdate(Physics2D.defaultPhysicsScene);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (iceComponentHasPierced)
		{
			PierceStart(collision.GetComponentInParent<Peg>());
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!iceComponentHasPierced)
		{
			PierceEnd(collision.GetComponentInParent<Peg>());
		}
		else if (_iceOrbPachinko != null)
		{
			_iceOrbPachinko.donePierce = true;
		}
	}

	protected override void ReturnToCollision()
	{
	}

	public void DoUpdate(PhysicsScene2D physicsScene2D)
	{
		if (!iceComponentHasPierced)
		{
			return;
		}
		if (bothAxis && !CanPierce)
		{
			StartPiercing();
		}
		else if (_rigid.velocity.y > 0.2f && !_onAscent)
		{
			if (!flipAxis)
			{
				StartPiercing();
			}
			else
			{
				StopPiercing();
			}
			_onAscent = true;
		}
		else if (_rigid.velocity.y < 0f && _onAscent)
		{
			if (!flipAxis)
			{
				StopPiercing();
			}
			else
			{
				StartPiercing();
			}
			_onAscent = false;
		}
	}

	private void StartPiercing()
	{
		base.gameObject.layer = 18;
		CanPierce = true;
		_renderer.color = _ghostColour;
	}

	private void StopPiercing()
	{
		base.gameObject.layer = _nonTriggerCollisionLayer;
		CanPierce = false;
		_renderer.color = _regularColour;
	}

	public void EnableIfDisabledDuringShot()
	{
	}

	public override void RestartPierceForSimulation()
	{
		base.RestartPierceForSimulation();
		if (_ball.IsDummy)
		{
			_onAscent = false;
			CanPierce = false;
			iceComponentHasPierced = _iceOrbPachinko == null;
		}
	}

	public void ForceUpdateGhostEffect(Vector2 movementDirection)
	{
		if (!iceComponentHasPierced)
		{
			return;
		}
		if (bothAxis && !CanPierce)
		{
			StartPiercing();
		}
		else if (movementDirection.y > 0f)
		{
			if (!flipAxis)
			{
				StartPiercing();
			}
			else
			{
				StopPiercing();
			}
			_onAscent = true;
		}
		else if (movementDirection.y < 0f)
		{
			if (!flipAxis)
			{
				StopPiercing();
			}
			else
			{
				StartPiercing();
			}
			_onAscent = false;
		}
	}

	public void Disable()
	{
	}

	public void SetDummy(bool value)
	{
		_isDummy = value;
	}
}
