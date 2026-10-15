using UnityEngine;

namespace PeglinUtils
{
	public static class RectTransformExtensions
	{
		public static bool ContainsPoint(this RectTransform rt, Vector2 point)
		{
			Rect rect = rt.rect;
			float num = rt.anchoredPosition.x - rect.width / 2f;
			float num2 = rt.anchoredPosition.x + rect.width / 2f;
			float num3 = rt.anchoredPosition.y + rect.height / 2f;
			float num4 = rt.anchoredPosition.y - rect.height / 2f;
			if (point.x >= num && point.x <= num2 && point.y >= num4 && point.y <= num3)
			{
				return true;
			}
			return false;
		}
	}
}
