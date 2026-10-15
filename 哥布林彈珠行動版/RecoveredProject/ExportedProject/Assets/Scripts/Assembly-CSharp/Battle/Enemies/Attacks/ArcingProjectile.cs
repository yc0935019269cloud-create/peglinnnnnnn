using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class ArcingProjectile : Projectile
	{
		public float ArcHeight;

		public float xDistanceHeightMaxMod = 0.3f;

		public float Speed;

		public float MinFlightTime = 0.5f;

		public float MaxFlightTime = 1f;

		private Vector3 _startPos;

		private Vector2 _targetPos;

		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			Vector3 position = base.transform.position;
			_startPos = position + projectileStartOffset;
			_targetPos = target;
			float num = Mathf.Abs(_targetPos.x - _startPos.x);
			float num2 = Mathf.Min(ArcHeight, num * xDistanceHeightMaxMod);
			float num3 = Mathf.Clamp(num / Speed, MinFlightTime, MaxFlightTime);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(_targetPos.x, num3).SetEase(Ease.Linear);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(FireComplete));
			base.transform.DOMoveY(position.y + num2, num3 * 0.48f).SetEase(Ease.OutQuad);
			base.transform.DOMoveY(target.y, num3 * 0.48f).SetEase(Ease.InQuad).SetDelay(num3 * 0.48f);
			OnProjectileFired?.Invoke(num3);
		}

		private void FireComplete()
		{
			base.HasHitDestination = true;
			base.gameObject.SetActive(value: false);
		}
	}
}
