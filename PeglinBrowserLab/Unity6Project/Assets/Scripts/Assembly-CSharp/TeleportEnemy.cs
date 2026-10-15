using System;
using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using MoreMountains.Tools;
using PeglinUtils;
using UnityEngine;

public class TeleportEnemy : Enemy
{
	public Action<int> TeleportingForward;

	public Action<int> TeleportingBack;

	private bool _chargingMove;

	private bool _meleeAttackedThisTurn;

	private bool _preMoveVanishFinished;

	private int _targetSlot = -1;

	protected override void OnEnable()
	{
		base.OnEnable();
		FreeMovement = true;
		MaxMovement = 99;
	}

	public override bool Move(Vector3 targetPos, int slotIndex)
	{
		if (HasEffect(StatusEffectType.Thorned))
		{
			PlayThornedAnimation();
			return false;
		}
		if (HasEffect(StatusEffectType.Stunned))
		{
			PlayStunnedAnimation();
			return false;
		}
		if (slotIndex != 0)
		{
			return false;
		}
		if (!_chargingMove)
		{
			_chargingMove = true;
			_anim.SetBool("move_charge", value: true);
			return false;
		}
		_targetSlot = _enemyManager.GetSlotIndexForGroundEnemy(this);
		_chargingMove = false;
		_anim.SetTrigger("vanish");
		_anim.SetBool("move_charge", value: false);
		StartCoroutine(MoveAtEndOfVanishAnimation(targetPos));
		return true;
	}

	public override void Attack(int range, Vector3 targetPos)
	{
		if (HasEffect(StatusEffectType.Stunned))
		{
			PlayStunnedAnimation();
			return;
		}
		base.Attack(range, targetPos);
		_chargingMove = false;
		if (range <= 1 && !_meleeAttackedThisTurn)
		{
			_meleeAttackedThisTurn = true;
		}
	}

	public override void DoEndOfTurnCleanup()
	{
		base.DoEndOfTurnCleanup();
		if (_meleeAttackedThisTurn)
		{
			_meleeAttackedThisTurn = false;
			if (HasEffect(StatusEffectType.Thorned))
			{
				PlayThornedAnimation();
			}
			else
			{
				StartCoroutine(MoveBack());
			}
		}
	}

	public void PreMoveVanishFinished()
	{
		_preMoveVanishFinished = true;
	}

	private IEnumerator MoveAtEndOfVanishAnimation(Vector3 targetPos)
	{
		CurrentlyActing = true;
		_preMoveVanishFinished = false;
		while (!_preMoveVanishFinished)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
		}
		TeleportingForward?.Invoke(_targetSlot);
		base.transform.position = targetPos;
		if (_anim.MMHasParameterOfType("is_emerging_enemy", AnimatorControllerParameterType.Bool) && _anim.GetBool("is_emerging_enemy"))
		{
			_animationHasFinished = false;
			while (!_animationHasFinished)
			{
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
		}
		EndAction();
	}

	private IEnumerator MoveBack()
	{
		CurrentlyActing = true;
		PlayAnimation("vanish_back");
		base.CleanupHasFinished = false;
		_preMoveVanishFinished = false;
		while (!_preMoveVanishFinished)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
		}
		int slotIndexForGroundEnemy = _enemyManager.GetSlotIndexForGroundEnemy(this);
		Vector3 position = _enemyManager.MoveEnemyToFarthestEmptyGroundSlot(this);
		int slotIndexForGroundEnemy2 = _enemyManager.GetSlotIndexForGroundEnemy(this);
		if (slotIndexForGroundEnemy != slotIndexForGroundEnemy2)
		{
			TeleportingBack?.Invoke(slotIndexForGroundEnemy);
			base.transform.position = position;
		}
		_animationHasFinished = false;
		while (!_animationHasFinished)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
		}
		EndAction(endTurn: false);
		base.CleanupHasFinished = true;
	}
}
