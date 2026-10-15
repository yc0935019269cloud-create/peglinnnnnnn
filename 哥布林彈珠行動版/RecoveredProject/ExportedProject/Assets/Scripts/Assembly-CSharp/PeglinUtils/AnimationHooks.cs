using UnityEngine;

namespace PeglinUtils
{
	public class AnimationHooks : MonoBehaviour
	{
		public void EnableChildAnimator()
		{
			Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
			if (componentsInChildren.Length != 0)
			{
				componentsInChildren[^1].enabled = true;
			}
		}
	}
}
