using System;
using DG.Tweening;
using UnityEngine;

namespace Battle.Enemies.Polish
{
	public class CardDropAnimation : MonoBehaviour
	{
		public delegate void CardAnimationFinished(EnemyData enemyData);

		private Enemy _enemy;

		private Vector3 _target;

		public static CardAnimationFinished OnCardAchievementShouldPop = delegate
		{
		};

		private bool _shouldPopAchievement;

		private EnemyData _enemyData;

		public void SetupAnimation(Vector3 target, Enemy enemy, EnemyData data, bool shouldPopAchievement)
		{
			_target = target;
			_enemy = enemy;
			_enemyData = data;
			_shouldPopAchievement = shouldPopAchievement;
		}

		private void DoAnimation()
		{
			Sequence sequence = DOTween.Sequence().Append(base.transform.DOMoveX(_target.x, 1f).SetEase(Ease.OutCubic));
			sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, (TweenCallback)(() =>
			{
				_enemy.ExternalAnimationHasFinished();
				LootDropAnimation.OnLootDropAnimationFinished(100);
				if (_shouldPopAchievement)
				{
					OnCardAchievementShouldPop?.Invoke(_enemyData);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}));
		}
	}
}
