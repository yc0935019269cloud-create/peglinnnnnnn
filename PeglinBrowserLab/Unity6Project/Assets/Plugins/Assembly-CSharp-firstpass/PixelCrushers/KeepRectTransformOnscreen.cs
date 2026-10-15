using UnityEngine;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class KeepRectTransformOnscreen : MonoBehaviour
	{
		private Vector3 originalLocalPosition;

		private RectTransform rectTransform;

		private void Awake()
		{
			originalLocalPosition = base.transform.localPosition;
			rectTransform = GetComponent<RectTransform>();
		}

		private void LateUpdate()
		{
			base.transform.localPosition = originalLocalPosition;
			Vector3[] array = new Vector3[4];
			rectTransform.GetWorldCorners(array);
			float num = Mathf.Abs(array[2].x - array[0].x);
			float num2 = Mathf.Abs(array[2].y - array[0].y);
			Vector3 vector = Camera.main.ScreenToWorldPoint(Vector3.zero);
			Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight));
			Rect rect = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
			Rect rect2 = new Rect(rect.x + num / 2f, rect.y, rect.width - num, rect.height - num2);
			base.transform.position = new Vector3(Mathf.Clamp(base.transform.position.x, rect2.xMin, rect2.xMax), Mathf.Clamp(base.transform.position.y, rect2.yMin, rect2.yMax), base.transform.position.z);
		}
	}
}
