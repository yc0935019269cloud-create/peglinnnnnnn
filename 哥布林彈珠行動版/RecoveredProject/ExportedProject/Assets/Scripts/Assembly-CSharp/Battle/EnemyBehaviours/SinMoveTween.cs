using DG.Tweening;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class SinMoveTween : MonoBehaviour
	{
		public float moveHeight = 2f;

		public float topToBottomDuration = 1f;

		private void Start()
		{
			float num = base.transform.localPosition.y + moveHeight / 2f;
			base.transform.DOLocalMoveY(num - moveHeight, topToBottomDuration).From(num).SetEase(Ease.Linear)
				.SetLoops(-1, LoopType.Yoyo);
		}

		private void OnDisable()
		{
			base.transform.DOKill();
		}
	}
}
