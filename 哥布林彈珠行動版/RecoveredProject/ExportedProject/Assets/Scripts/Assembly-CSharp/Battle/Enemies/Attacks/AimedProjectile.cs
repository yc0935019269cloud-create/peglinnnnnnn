using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class AimedProjectile : StraightProjectile
	{
		public override void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			base.HasHitDestination = false;
			base.transform.position += FiringOffset + projectileStartOffset;
			_target = target;
			base.transform.right = base.transform.position - _target;
			float duration = (base.transform.position.x - target.x) / Speed;
			base.transform.DOMoveY(target.y, duration).SetEase(Ease.Linear);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(target.x, duration).SetEase(Ease.Linear);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(base.MoveComplete));
		}
	}
}
