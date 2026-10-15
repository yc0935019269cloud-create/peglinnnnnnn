using System;
using MoreMountains.Tools;
using RNG.Scenarios;
using UnityEngine;

namespace Scenarios.DoodadInteractions
{
	public class UpdateAnimatorFromScenarioEvent : MonoBehaviour
	{
		private const string _orbAcceptedTrigger = "OrbAccepted";

		private Animator _animator;

		private static readonly int Accepted = Animator.StringToHash("OrbAccepted");

		private void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		private void OnEnable()
		{
			DialogueSystemScenario.OnOrbAccepted = (Action)Delegate.Combine(DialogueSystemScenario.OnOrbAccepted, new Action(OrbAccepted));
		}

		private void OnDisable()
		{
			DialogueSystemScenario.OnOrbAccepted = (Action)Delegate.Remove(DialogueSystemScenario.OnOrbAccepted, new Action(OrbAccepted));
		}

		private void OrbAccepted()
		{
			if (_animator.MMHasParameterOfType("OrbAccepted", AnimatorControllerParameterType.Trigger))
			{
				_animator.SetTrigger(Accepted);
			}
		}
	}
}
