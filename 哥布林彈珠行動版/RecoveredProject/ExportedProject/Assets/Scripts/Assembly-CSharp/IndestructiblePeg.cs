using MoreMountains.Feedbacks;
using UnityEngine;

public class IndestructiblePeg : Peg
{
	private MMFeedbacks _feedbacks;

	protected override void Awake()
	{
		base.Awake();
		_feedbacks = GetComponent<MMFeedbacks>();
	}

	public override GameObject ConvertPegToType(PegType type)
	{
		Debug.LogError("Indestructible peg conversion not allowed!");
		return base.gameObject;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
	}

	public override void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
	{
		if (!(pachinko == null) && !pachinko.IsDummy)
		{
			if (cType == CollisionType.COLLIDER)
			{
				Peg.OnPegCollisionHit(pegType, this);
			}
			PegActivated();
		}
	}

	public override float GetMagnetForce()
	{
		return 0f;
	}

	public override bool IsDelayedDeath()
	{
		return false;
	}

	public override bool IsDisabled()
	{
		return false;
	}

	public override void PegActivated(bool playAudio = true, bool forcePop = false)
	{
		if (playAudio)
		{
			Peg.OnPegAudioRequest?.Invoke(PegType.REGULAR);
		}
		if (_feedbacks != null)
		{
			_feedbacks.PlayFeedbacks();
		}
	}

	public override void RemoveIfCleared()
	{
		Debug.LogError("RemoveIfCleared() not implemented for Indestructible Pegs");
	}

	public override void Reset(bool criticalHitActive = false)
	{
		Debug.LogError("Cannot reset IndestructiblePegs");
	}

	public override bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None)
	{
		return false;
	}

	public override void ResetHitCount()
	{
	}
}
