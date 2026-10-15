using UnityEngine;

public class PegColliderActivator : MonoBehaviour
{
	private Peg _peg;

	private void OnEnable()
	{
		_peg = GetComponentInParent<Peg>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		_peg.DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		LongPeg longPeg = _peg as LongPeg;
		if (longPeg != null)
		{
			longPeg.EndPierce();
		}
	}
}
