using UnityEngine;

namespace PeglinUtils
{
	public static class ObjectUtils
	{
		public static bool IsTransformAChildOfTransformRecursive(Transform child, Transform parent)
		{
			if (child.parent == null)
			{
				return false;
			}
			if (child.parent == parent)
			{
				return true;
			}
			return IsTransformAChildOfTransformRecursive(child.parent, parent);
		}

		public static bool Contains(this Rect rect, Rect targetRect)
		{
			if (rect.Contains(targetRect.max))
			{
				return rect.Contains(targetRect.min);
			}
			return false;
		}
	}
}
