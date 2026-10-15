using DG.Tweening;
using UnityEngine;

namespace PeglinUI.UIUtils
{
	public class ZoomOutEffect : MonoBehaviour
	{
		private void OnEnable()
		{
			RectTransform component = GetComponent<RectTransform>();
			Vector3 localScale = component.localScale;
			component.localScale *= 1.25f;
			component.DOScale(localScale, 5f);
		}
	}
}
