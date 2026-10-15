using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegTriggerActivator : MonoBehaviour
	{
		private Peg _peg;

		private void OnEnable()
		{
			if (_peg == null)
			{
				_peg = GetComponentInParent<Peg>();
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (_peg == null)
			{
				_peg = GetComponentInParent<Peg>();
			}
			_peg.DoPegCollision(collision.GetComponentInParent<PachinkoBall>(), Peg.CollisionType.TRIGGER);
		}
	}
}
