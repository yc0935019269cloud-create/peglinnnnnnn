using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class PegBoardBramballVineTriggerCollider : MonoBehaviour
	{
		private PegBoardBramballVine parentVine;

		private void Awake()
		{
			parentVine = GetComponentInParent<PegBoardBramballVine>();
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			PachinkoBall component = collision.gameObject.GetComponent<PachinkoBall>();
			if (component != null && !component.IsDummy)
			{
				parentVine.DestroyVine();
			}
		}
	}
}
