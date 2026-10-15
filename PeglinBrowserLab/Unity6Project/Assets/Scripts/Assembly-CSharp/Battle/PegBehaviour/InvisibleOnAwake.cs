using UnityEngine;

namespace Battle.PegBehaviour
{
	public class InvisibleOnAwake : MonoBehaviour
	{
		private void Awake()
		{
			SpriteRenderer component = GetComponent<SpriteRenderer>();
			Color color = component.color;
			color.a = 0f;
			component.color = color;
		}
	}
}
