using UnityEngine;

public class PopPegOnTrigger : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		Peg component = other.gameObject.GetComponent<Peg>();
		if (component != null && !component.IsDisabled())
		{
			component.PegActivated();
		}
	}
}
