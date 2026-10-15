using System;
using System.Collections;
using Battle.StatusEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.Enemies
{
	public class AlternateFlyingEnemy : SpecialMovementEnemy
	{
		[Header("Alternate Flying Params")]
		public float MoveSpeed = 1f;

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
		}

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
			isMoving = true;
			bool flag = false;
			float num = 0f;
			int num2 = 0;
			EnemyManager.EnemySlot[] array = groundSlots;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Enemy == this)
				{
					flag = true;
					num = _enemyManager.flyingEnemiesHeight;
					break;
				}
				num2++;
			}
			if (!flag)
			{
				num2 = 0;
				array = flyingSlots;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Enemy == this)
					{
						num = _enemyManager.groundEnemiesHeight;
						break;
					}
					num2++;
				}
			}
			if (flag)
			{
				if (!(flyingSlots[num2].Enemy == null))
				{
					isMoving = false;
					return;
				}
				enemyTypes |= EnemyType.Flying;
				flyingSlots[num2].Enemy = this;
				groundSlots[num2].Enemy = null;
				IsFlying = true;
			}
			else
			{
				if (!(groundSlots[num2].Enemy == null))
				{
					isMoving = false;
					return;
				}
				enemyTypes &= ~EnemyType.Flying;
				flyingSlots[num2].Enemy = null;
				groundSlots[num2].Enemy = this;
				IsFlying = false;
			}
			float duration = Mathf.Abs(num - base.transform.position.y) / MoveSpeed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveY(num, duration);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(VerticalMoveComplete));
		}

		private void VerticalMoveComplete()
		{
			isMoving = false;
		}

		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			return false;
		}

		protected override void OnVinesAnimationEnded()
		{
			OneShotAnimation vineAnimation = _vineAnimation;
			vineAnimation.OnAnimationEnded = (OneShotAnimation.AnimationEnded)Delegate.Remove(vineAnimation.OnAnimationEnded, new OneShotAnimation.AnimationEnded(OnVinesAnimationEnded));
			_vineAnimation.gameObject.SetActive(value: false);
			EndAction();
		}

		public override IEnumerator RangedAttack(Vector3 targetPos, float damage = -1f, GameObject projectileOverride = null, string animTriggerOverride = "")
		{
			if (IsFlying)
			{
				StartCoroutine(base.RangedAttack(targetPos, damage, projectileOverride, animTriggerOverride));
			}
			else
			{
				EndAction();
			}
			yield break;
		}
	}
}
