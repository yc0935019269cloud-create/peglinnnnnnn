using System;
using System.Collections;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies
{
	public class RotatingSquareEnemy : SpecialMovementEnemy
	{
		[Header("Rotating Square Params")]
		public float MoveSpeed = 1f;

		public float FlyingYOffset = 1f;

		private int _currentRotation = -1;

		private int _previousSlotIndex = -1;

		[SerializeField]
		private string _moveAnim = "moving";

		public int currentRotation => _currentRotation;

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
			if (UnityEngine.Object.FindObjectOfType<UnlockRuinsBossOneShotAchievement>() == null)
			{
				new GameObject("RuinsAchievementChecker").AddComponent<UnlockRuinsBossOneShotAchievement>();
			}
		}

		private new IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			base.Start();
			if (_currentRotation >= 0)
			{
				yield break;
			}
			int slotIndexForFlyingEnemy = _enemyManager.GetSlotIndexForFlyingEnemy(this);
			if (slotIndexForFlyingEnemy >= 0)
			{
				_previousSlotIndex = slotIndexForFlyingEnemy;
				IsFlying = true;
				enemyTypes |= EnemyType.Flying;
				_currentRotation = 0;
				float z = -0.5f;
				if (slotIndexForFlyingEnemy > 0)
				{
					Enemy flyingEnemyAtSlot = _enemyManager.GetFlyingEnemyAtSlot(slotIndexForFlyingEnemy - 1);
					if (flyingEnemyAtSlot != null && flyingEnemyAtSlot is RotatingSquareEnemy)
					{
						_currentRotation = 1;
						z = -1f;
					}
				}
				base.transform.position += new Vector3(0f, FlyingYOffset, z);
				yield break;
			}
			slotIndexForFlyingEnemy = _enemyManager.GetSlotIndexForGroundEnemy(this);
			if (slotIndexForFlyingEnemy < 0)
			{
				yield break;
			}
			_previousSlotIndex = slotIndexForFlyingEnemy;
			IsFlying = false;
			enemyTypes &= ~EnemyType.Flying;
			_currentRotation = 3;
			float z2 = 0f;
			if (slotIndexForFlyingEnemy > 0)
			{
				Enemy groundEnemyAtSlot = _enemyManager.GetGroundEnemyAtSlot(slotIndexForFlyingEnemy - 1);
				if (groundEnemyAtSlot != null && groundEnemyAtSlot is RotatingSquareEnemy)
				{
					_currentRotation = 2;
					z2 = -0.1f;
				}
			}
			base.transform.position += new Vector3(0f, 0f, z2);
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
			Vector2 vector = Vector2.zero;
			switch (_currentRotation)
			{
			case 0:
				if (flyingSlots[_previousSlotIndex].Enemy == this)
				{
					flyingSlots[_previousSlotIndex].Enemy = null;
				}
				_previousSlotIndex++;
				flyingSlots[_previousSlotIndex].Enemy = this;
				vector = flyingSlots[_previousSlotIndex].Position + new Vector3(0f, FlyingYOffset, -1f);
				IsFlying = true;
				enemyTypes |= EnemyType.Flying;
				_currentRotation = 1;
				break;
			case 1:
				if (flyingSlots[_previousSlotIndex].Enemy == this)
				{
					flyingSlots[_previousSlotIndex].Enemy = null;
				}
				groundSlots[_previousSlotIndex].Enemy = this;
				vector = groundSlots[_previousSlotIndex].Position + new Vector3(0f, 0f, -0.1f);
				IsFlying = false;
				enemyTypes &= ~EnemyType.Flying;
				_currentRotation = 2;
				break;
			case 2:
				if (groundSlots[_previousSlotIndex].Enemy == this)
				{
					groundSlots[_previousSlotIndex].Enemy = null;
				}
				_previousSlotIndex--;
				groundSlots[_previousSlotIndex].Enemy = this;
				vector = groundSlots[_previousSlotIndex].Position + new Vector3(0f, 0f, 0f);
				IsFlying = false;
				enemyTypes &= ~EnemyType.Flying;
				_currentRotation = 3;
				break;
			case 3:
				if (groundSlots[_previousSlotIndex].Enemy == this)
				{
					groundSlots[_previousSlotIndex].Enemy = null;
				}
				flyingSlots[_previousSlotIndex].Enemy = this;
				vector = flyingSlots[_previousSlotIndex].Position + new Vector3(0f, FlyingYOffset, -0.5f);
				IsFlying = true;
				enemyTypes |= EnemyType.Flying;
				_currentRotation = 0;
				break;
			}
			_anim.SetBool(_moveAnim, value: true);
			float duration = Mathf.Abs(Vector2.Distance(vector, base.transform.position)) / MoveSpeed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMove(vector, duration);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(SpecialMoveComplete));
		}

		private void SpecialMoveComplete()
		{
			_anim.SetBool(_moveAnim, value: false);
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
		}
	}
}
