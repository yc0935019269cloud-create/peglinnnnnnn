using System;
using System.Collections;
using Battle;
using Battle.Attacks;
using UnityEngine;

public class PeglinBattleAnimationController : MonoBehaviour
{
	public delegate void FirePoint();

	public delegate void LobPoint();

	[HideInInspector]
	public bool EatenBySlimeBoss;

	public static FirePoint OnFirePoint = () =>
	{
	};

	public static LobPoint OnLobPoint = () =>
	{
	};

	private Animator _animator;

	private readonly float ANIMATION_TIMEOUT_TIME = 1.1f;

	private bool _animationEndCalled;

	private static readonly int Lob = Animator.StringToHash("lob");

	private void OnEnable()
	{
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		AttackManager.OnAttackPerformed = (AttackManager.AttackPerformed)Delegate.Combine(AttackManager.OnAttackPerformed, new AttackManager.AttackPerformed(DoAttackAnimation));
		BattleController.OnBombThrown = (Action)Delegate.Combine(BattleController.OnBombThrown, new Action(DoLobAnimation));
	}

	private void DoAttackAnimation(string trigger)
	{
		_animator.SetTrigger(trigger);
		StartCoroutine(CheckForAnimationTimeout());
	}

	private void DoLobAnimation()
	{
		_animator.SetTrigger(Lob);
		StartCoroutine(CheckForAnimationTimeout());
	}

	private IEnumerator CheckForAnimationTimeout()
	{
		yield return new WaitForSeconds(ANIMATION_TIMEOUT_TIME);
		if (!_animationEndCalled)
		{
			Debug.LogWarning("Animation Event for Attack Animation wasn't called, calling it");
			OnFirePoint?.Invoke();
		}
		else
		{
			_animationEndCalled = false;
		}
	}

	private void AttackPointReached()
	{
		_animationEndCalled = true;
		OnFirePoint?.Invoke();
	}

	private void LobPointReached()
	{
		_animationEndCalled = true;
		OnLobPoint?.Invoke();
	}

	private void OnDisable()
	{
		AttackManager.OnAttackPerformed = (AttackManager.AttackPerformed)Delegate.Remove(AttackManager.OnAttackPerformed, new AttackManager.AttackPerformed(DoAttackAnimation));
		BattleController.OnBombThrown = (Action)Delegate.Remove(BattleController.OnBombThrown, new Action(DoLobAnimation));
	}
}
