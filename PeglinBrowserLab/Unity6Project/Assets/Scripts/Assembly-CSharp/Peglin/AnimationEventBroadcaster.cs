using System;
using UnityEngine;

namespace Peglin
{
	public class AnimationEventBroadcaster : MonoBehaviour
	{
		public Action OnAnimationFinished = () =>
		{
		};

		public Action OnAnimationAction = () =>
		{
		};

		public void AnimationAction()
		{
			OnAnimationAction?.Invoke();
		}

		public void AnimationFinished()
		{
			OnAnimationFinished?.Invoke();
		}
	}
}
