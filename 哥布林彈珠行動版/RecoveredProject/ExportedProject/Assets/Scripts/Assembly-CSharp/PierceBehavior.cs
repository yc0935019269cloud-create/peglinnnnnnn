using System.Collections.Generic;
using UnityEngine;

public abstract class PierceBehavior : MonoBehaviour
{
	protected PachinkoBall _ball;

	public bool CanPierce;

	public bool ShouldPierceOnRestart;

	protected const int PACHINKO_COLLISION_LAYER = 13;

	protected const int PACHINKO_TRIGGER_LAYER = 18;

	protected const int PACHINKO_SPECIAL_COLLISION_LAYER = 22;

	protected const int PACHINKO_POPPED_PEGS_COLLISION_LAYER = 23;

	protected List<Collider2D> piercedPegs = new List<Collider2D>();

	protected virtual void OnEnable()
	{
		_ball = GetComponent<PachinkoBall>();
		base.gameObject.layer = GetStartingLayer();
	}

	protected virtual int GetStartingLayer()
	{
		return 18;
	}

	protected void PierceStart(Peg peg)
	{
		if (!(peg != null) || peg.IsDisabled() || !peg.ShouldEndPierce() || !(peg as LongPeg == null))
		{
			return;
		}
		if (!Peg.PegTypeIndestructible(peg.pegType))
		{
			ReturnToCollision();
		}
		if (_ball.IsDummy)
		{
			Collider2D component = peg.gameObject.GetComponent<Collider2D>();
			component.enabled = false;
			if (!piercedPegs.Contains(component))
			{
				piercedPegs.Add(component);
			}
		}
	}

	protected bool PierceEnd(Peg peg)
	{
		if (peg == null || !peg.ShouldEndPierce())
		{
			return false;
		}
		LongPeg longPeg = peg as LongPeg;
		if (longPeg != null)
		{
			ReturnToCollision();
			longPeg.EndPierce();
		}
		else if (Peg.PegTypeIndestructible(peg.pegType))
		{
			ReturnToCollision();
		}
		return true;
	}

	protected virtual void ReturnToCollision()
	{
		base.gameObject.layer = 13;
	}

	public virtual void RestartPierceForSimulation()
	{
		if (!_ball.IsDummy)
		{
			return;
		}
		if (ShouldPierceOnRestart)
		{
			CanPierce = true;
		}
		base.gameObject.layer = GetStartingLayer();
		foreach (Collider2D piercedPeg in piercedPegs)
		{
			if (piercedPeg != null)
			{
				piercedPeg.enabled = true;
			}
		}
		piercedPegs.Clear();
	}
}
