using UnityEngine;

public class ConcentricationPachinko : PierceBehavior
{
	protected override void OnEnable()
	{
		base.OnEnable();
		base.gameObject.layer = 22;
	}

	protected override int GetStartingLayer()
	{
		return 22;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PierceStart(collision.GetComponentInParent<Peg>());
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		PierceEnd(collision.GetComponentInParent<Peg>());
	}

	protected override void ReturnToCollision()
	{
	}
}
