using UnityEngine;
using UnityEngine.Events;

namespace Battle.Attacks
{
	public class TargetedSpellStartup : MonoBehaviour
	{
		public Vector3 peglinPositionStartupOffset = new Vector3(1f, 0f, 0f);

		public bool doYLock;

		public float yLock = 6f;

		public UnityEvent maxPointReached;

		private void Awake()
		{
			GameObject gameObject = GameObject.FindWithTag("Player");
			base.gameObject.transform.parent.position = gameObject.transform.position + peglinPositionStartupOffset;
			if (doYLock)
			{
				base.gameObject.transform.parent.position = new Vector3(base.gameObject.transform.parent.position.x, yLock, base.gameObject.transform.parent.position.z);
			}
		}

		public void OnMaxPointReached()
		{
			maxPointReached?.Invoke();
		}

		public void AnimationEnded()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
