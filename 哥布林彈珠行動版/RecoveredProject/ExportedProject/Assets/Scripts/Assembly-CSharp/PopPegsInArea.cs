using System;
using Battle.Pachinko.Obstacles;
using I2.Loc;
using UnityEngine;

public abstract class PopPegsInArea : MonoBehaviour
{
	public PachinkoBall pachinkoBall;

	public int hitsUntilActivation = 8;

	public bool activateOnFirstPegHit;

	private int _currentHits;

	private bool _hasHitPeg;

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("HITS_UNTIL_ACTIVATION", hitsUntilActivation.ToString());
		}
		else
		{
			Debug.Log(base.gameObject.name + " does not have loc param manager   ");
		}
	}

	private void OnEnable()
	{
		PachinkoBall obj = pachinkoBall;
		obj.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(obj.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
	}

	private void OnDisable()
	{
		PachinkoBall obj = pachinkoBall;
		obj.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(obj.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
	}

	private void PegHit(Peg peg, PachinkoBall pBall)
	{
		if (!pachinkoBall.IsDummy)
		{
			if (activateOnFirstPegHit && !_hasHitPeg)
			{
				PopPegs();
				_hasHitPeg = true;
			}
			else if (++_currentHits >= hitsUntilActivation)
			{
				PopPegs();
				_currentHits = 0;
			}
		}
	}

	protected abstract void PopPegs();

	protected void ActivatePegsFromCollision(Collider2D[] collisions)
	{
		foreach (Collider2D obj in collisions)
		{
			PegBoardBramballVine component = obj.GetComponent<PegBoardBramballVine>();
			Peg component2 = obj.GetComponent<Peg>();
			if ((component2 != null || component != null) && (!(component2 is LongPeg) || !(component2 as LongPeg).hit))
			{
				if (component2 != null && !component2.IsDisabled() && (!component2.IsDelayedDeath() || !component2.IsWaitingForDeath()))
				{
					component2.PegActivated();
				}
				else if (component != null)
				{
					component.DestroyVine();
				}
			}
		}
	}
}
