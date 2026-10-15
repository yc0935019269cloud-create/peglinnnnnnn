using System;
using Battle.StatusEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MoreMountains.Tools;
using UnityEngine;

namespace Battle.Enemies
{
	public class FleeingEnemy : SpecialMovementEnemy
	{
		[Header("Fleeing Enemy Params")]
		public float MoveSpeed = 3.5f;

		[SerializeField]
		private string _moveAnimName = "Running";

		public override void StartSpecialMove(EnemyManager.EnemySlot[] groundSlots, EnemyManager.EnemySlot[] flyingSlots)
		{
			isMoving = false;
			if (HasEffect(StatusEffectType.Thorned))
			{
				PlayThornedAnimation();
				return;
			}
			if (HasEffect(StatusEffectType.Stunned))
			{
				PlayStunnedAnimation();
				return;
			}
			float targetX = groundSlots[^1].Position.x + 8.5f;
			int slotIndexForEnemy = _enemyManager.GetSlotIndexForEnemy(this, out var _);
			if (slotIndexForEnemy < 0 || slotIndexForEnemy + defaultMovement >= groundSlots.Length)
			{
				Flee(targetX);
				return;
			}
			int num = -1;
			for (int num2 = slotIndexForEnemy + defaultMovement; num2 > slotIndexForEnemy; num2--)
			{
				if (groundSlots[num2].Enemy == null)
				{
					num = num2;
					break;
				}
			}
			if (num < 0 && MaxMovement > defaultMovement)
			{
				for (int i = slotIndexForEnemy + defaultMovement + 1; i <= slotIndexForEnemy + MaxMovement; i++)
				{
					if (i >= groundSlots.Length || groundSlots[i].Enemy == null)
					{
						num = i;
						break;
					}
				}
			}
			if (num >= groundSlots.Length)
			{
				Flee(targetX);
			}
			else if (num >= 0 && num != slotIndexForEnemy)
			{
				CurrentlyActing = true;
				isMoving = true;
				groundSlots[slotIndexForEnemy].Enemy = null;
				groundSlots[num].Enemy = this;
				if (_anim.MMHasParameterOfType(_moveAnimName, AnimatorControllerParameterType.Bool))
				{
					_anim.SetBool(_moveAnimName, value: true);
				}
				Vector3 position = groundSlots[num].Position;
				float duration = Mathf.Abs(base.transform.position.x - position.x) / MoveSpeed;
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(position.x, duration).SetEase(Ease.InOutSine);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveComplete));
				EnemyManager.OnEnemyMoved?.Invoke(slotIndexForEnemy, num, this);
			}
		}

		private void MoveComplete()
		{
			if (_anim.MMHasParameterOfType(_moveAnimName, AnimatorControllerParameterType.Bool))
			{
				_anim.SetBool(_moveAnimName, value: false);
			}
			isMoving = false;
			EndAction();
		}

		private void Flee(float targetX)
		{
			CurrentlyActing = true;
			isMoving = true;
			float duration = 0.5f * Mathf.Abs(base.transform.position.x - targetX) / MoveSpeed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(targetX, duration).SetEase(Ease.InOutSine);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(FleeComplete));
		}

		private void FleeComplete()
		{
			isMoving = false;
			EndAction();
			ForceDestruction();
		}

		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			return false;
		}
	}
}
