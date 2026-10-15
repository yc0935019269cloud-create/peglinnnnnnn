using Rewired.Integration.UnityUI;
using UnityEngine;

public class RewiredEventFinder : MonoBehaviour
{
	private void Awake()
	{
		RewiredEventSystem component = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		ArrowSelection[] componentsInChildren = GetComponentsInChildren<ArrowSelection>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].rewiredEventSystem = component;
		}
	}
}
