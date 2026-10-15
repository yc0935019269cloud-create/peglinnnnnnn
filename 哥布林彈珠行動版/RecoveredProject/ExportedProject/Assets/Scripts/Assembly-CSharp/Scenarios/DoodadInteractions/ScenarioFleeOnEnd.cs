using System;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;

namespace Scenarios.DoodadInteractions
{
	public class ScenarioFleeOnEnd : MonoBehaviour
	{
		public float fleeTargetX = 20f;

		public float MoveSpeed = 4.5f;

		[SerializeField]
		private string _moveAnimName = "Running";

		private void OnEnable()
		{
			DialogueScriptableObjectInteractions.OnScenarioEnded = (Action)Delegate.Combine(DialogueScriptableObjectInteractions.OnScenarioEnded, new Action(BeginFleeing));
		}

		private void OnDisable()
		{
			DialogueScriptableObjectInteractions.OnScenarioEnded = (Action)Delegate.Remove(DialogueScriptableObjectInteractions.OnScenarioEnded, new Action(BeginFleeing));
		}

		private void BeginFleeing()
		{
			if (TryGetComponent<Animator>(out var component) && component.MMHasParameterOfType(_moveAnimName, AnimatorControllerParameterType.Bool))
			{
				component.SetBool(_moveAnimName, value: true);
			}
			if (TryGetComponent<SpriteRenderer>(out var component2))
			{
				component2.flipX = !component2.flipX;
			}
			float duration = Mathf.Abs(base.transform.position.x - fleeTargetX) / MoveSpeed;
			base.transform.DOMoveX(fleeTargetX, duration).SetEase(Ease.InOutSine);
		}
	}
}
