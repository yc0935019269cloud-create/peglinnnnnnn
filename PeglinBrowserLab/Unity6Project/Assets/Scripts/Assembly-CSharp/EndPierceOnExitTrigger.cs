using UnityEngine;

public class EndPierceOnExitTrigger : MonoBehaviour
{
	private void OnTriggerExit2D(Collider2D collision)
	{
		LongPeg componentInParent = collision.gameObject.GetComponentInParent<LongPeg>();
		if (componentInParent != null)
		{
			componentInParent.EndPierce();
		}
	}
}
