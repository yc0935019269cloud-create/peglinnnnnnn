using DG.Tweening;
using UnityEngine;

namespace Scenarios.Shop
{
	public class ShopUIMove : MonoBehaviour
	{
		public float animationDuration = 2.5f;

		private float startingPos;

		public Tween MoveIn()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			startingPos = rectTransform.anchoredPosition.y;
			return DOTween.To(() => rectTransform.anchoredPosition, (Vector2 v) =>
			{
				rectTransform.anchoredPosition = v;
			}, new Vector2(rectTransform.anchoredPosition.x, -10f), animationDuration).SetEase(Ease.OutQuint);
		}

		public Tween MoveBack()
		{
			RectTransform rectTransform = GetComponent<RectTransform>();
			return DOTween.To(() => rectTransform.anchoredPosition, (Vector2 v) =>
			{
				rectTransform.anchoredPosition = v;
			}, new Vector2(rectTransform.anchoredPosition.x, startingPos), animationDuration).SetEase(Ease.InQuint);
		}
	}
}
