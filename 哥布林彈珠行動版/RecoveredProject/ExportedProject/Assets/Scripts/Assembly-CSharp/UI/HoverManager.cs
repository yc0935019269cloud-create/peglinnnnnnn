using UnityEngine;

namespace UI
{
	public class HoverManager : MonoBehaviour
	{
		public void SetHover(bool status)
		{
			ButtonHandleHover[] componentsInChildren = GetComponentsInChildren<ButtonHandleHover>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = status;
			}
		}
	}
}
