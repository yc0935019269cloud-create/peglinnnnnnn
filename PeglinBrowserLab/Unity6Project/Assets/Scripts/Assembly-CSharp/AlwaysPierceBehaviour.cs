using UnityEngine;

public class AlwaysPierceBehaviour : MonoBehaviour
{
	private void OnTriggerExit2D(Collider2D collision)
	{
		Peg componentInParent = collision.GetComponentInParent<Peg>();
		if (!(componentInParent == null))
		{
			LongPeg longPeg = componentInParent as LongPeg;
			if (longPeg != null)
			{
				longPeg.EndPierce();
			}
		}
	}
}
