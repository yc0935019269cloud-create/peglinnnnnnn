using System;
using DG.Tweening;
using UnityEngine;

namespace Battle.Enemies.Polish
{
	public class LootDropAnimation : MonoBehaviour
	{
		public delegate void LootDropAnimationFinished(int lootXp);

		public static LootDropAnimationFinished OnLootDropAnimationFinished = delegate
		{
		};

		public float initialJumpHeight = 3f;

		public Transform debugTarget;

		public void DoAnimation(Vector3 target, Enemy enemy, int lootXp)
		{
			Sequence s = DOTween.Sequence();
			Vector3 position = base.transform.position;
			float endValue = position.y + initialJumpHeight + UnityEngine.Random.Range(-0.05f, 0.05f);
			float num = UnityEngine.Random.Range(-1f, 1f);
			float x = (target.x - position.x + num) / 2f;
			Vector3 vector = new Vector3(x, UnityEngine.Random.Range(position.y + 3.5f, position.y + 4.5f));
			Sequence sequence = s.Append(base.transform.DOMoveY(endValue, 0.5f).SetEase(Ease.OutExpo)).Append(base.transform.DOMoveY(target.y - 0.5f, 1f).SetEase(Ease.OutBounce)).AppendInterval(1f)
				.Append(base.transform.DOMoveY(vector.y, 0.5f).SetEase(Ease.OutQuad))
				.Append(base.transform.DOMoveY(target.y - 0.5f, 0.5f).SetEase(Ease.InQuad))
				.Insert(0f, base.transform.DOMoveX(position.x + num, 1.5f).SetEase(Ease.Linear))
				.Insert(2.5f, base.transform.DOMoveX(target.x, 1f).SetEase(Ease.Linear));
			sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, (TweenCallback)(() =>
			{
				if (enemy != null)
				{
					enemy.ExternalAnimationHasFinished();
				}
				OnLootDropAnimationFinished(lootXp);
				UnityEngine.Object.Destroy(base.gameObject);
			}));
		}
	}
}
