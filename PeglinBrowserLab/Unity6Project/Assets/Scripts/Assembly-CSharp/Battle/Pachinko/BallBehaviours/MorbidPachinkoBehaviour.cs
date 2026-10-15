using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class MorbidPachinkoBehaviour : MonoBehaviour
	{
		[SerializeField]
		public int morbidLevel = 1;

		private void Start()
		{
			if (base.transform.Find("PoppedPegTrigger")?.gameObject == null)
			{
				GameObject obj = new GameObject("PoppedPegTrigger");
				obj.transform.parent = base.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
				obj.layer = 19;
				CircleCollider2D circleCollider2D = obj.AddComponent<CircleCollider2D>();
				circleCollider2D.isTrigger = true;
				circleCollider2D.radius = GetComponent<CircleCollider2D>().radius;
			}
		}
	}
}
