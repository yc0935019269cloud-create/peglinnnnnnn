using System;
using System.Collections;
using Battle.StatusEffects;
using DG.Tweening;
using Relics;
using UnityEngine;

namespace Battle.Enemies
{
	public class StompEnemy : Enemy
	{
		private enum StompState
		{
			JumpingToMove = 0,
			JumpingToAttack = 1,
			JumpingToReturn = 2
		}

		private static readonly int StartJump = Animator.StringToHash("StartJump");

		private static readonly int StartLand = Animator.StringToHash("StartLand");

		private static readonly int Jump = Animator.StringToHash("Jump");

		private static readonly int Land = Animator.StringToHash("Land");

		[Header("Stomp Refs")]
		[SerializeField]
		private Animator _dustEffectAnim;

		[Header("Stomp Params")]
		public float moveTime = 1f;

		public float attackTime = 0.65f;

		public float jumpForce = 4f;

		public float landingScreenshakeWeight = 0.5f;

		public float landingScreenshakeTime = 0.25f;

		private Vector2 _returnToPos;

		private Vector2 _targetPos;

		private Vector3 _dustEffectOffset;

		private bool _tweenCompleted;

		private StompState _stompState;

		private new void Start()
		{
			_dustEffectOffset = _dustEffectAnim.transform.localPosition;
		}

		public override bool Move(Vector3 position, int slotIndex = -1)
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
			_stompState = StompState.JumpingToMove;
			StartMove(position);
			return true;
		}

		private void StartMove(Vector3 targetPos)
		{
			CurrentlyActing = true;
			_targetPos = targetPos;
			_anim.SetTrigger(StartJump);
		}

		private void StartJumpDust()
		{
			Transform obj = _dustEffectAnim.transform;
			obj.SetParent(null);
			obj.position = base.transform.position + _dustEffectOffset;
			_dustEffectAnim.SetTrigger(Jump);
			JumpToLocation(_targetPos);
		}

		private void JumpToLocation(Vector3 targetPos)
		{
			if (CurrentHealth <= 0f)
			{
				return;
			}
			float duration = ((_stompState == StompState.JumpingToMove) ? moveTime : attackTime);
			_tweenCompleted = false;
			Sequence sequence = base.transform.DOJump(targetPos, jumpForce, 1, duration).SetEase(Ease.Linear);
			sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, (TweenCallback)(() =>
			{
				if (!_tweenCompleted)
				{
					_tweenCompleted = true;
					JumpComplete(targetPos);
				}
			}));
			sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)(() =>
			{
				if (!_tweenCompleted)
				{
					_tweenCompleted = true;
					JumpComplete(targetPos);
				}
			}));
		}

		private void JumpComplete(Vector2 targetPos)
		{
			base.transform.position = targetPos;
			_anim.SetTrigger(StartLand);
			switch (_stompState)
			{
			case StompState.JumpingToMove:
				StartCoroutine(WaitForLandingAndFinishMove());
				break;
			case StompState.JumpingToAttack:
				StartCoroutine(WaitForLandingAndAttack());
				break;
			case StompState.JumpingToReturn:
				StartCoroutine(WaitForLandingAndFinishMove());
				break;
			}
		}

		private void StartLandingDust()
		{
			_dustEffectAnim.transform.position = base.transform.position + _dustEffectOffset;
			_dustEffectAnim.SetTrigger(Land);
			if (_stompState == StompState.JumpingToMove)
			{
				LandingScreenshake();
			}
		}

		private IEnumerator WaitForLandingAndAttack()
		{
			while (!_animationHasFinished)
			{
				yield return null;
			}
			_animationHasFinished = false;
			_audioSource?.PlayOneShot(_enemySfx.MeleeAttackClip);
			Enemy.OnEnemyAttack?.Invoke(DamagePerMeleeAttack, melee: true, this);
			if (_meleeAppliedStatusEffects.Length != 0 && !lastAttackDodged)
			{
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(_meleeAppliedStatusEffects, StatusEffectSource.ENEMY);
			}
			if (!lastAttackDodged && _relicManager.AttemptUseRelic(RelicEffect.BLIND_WHEN_HIT))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 15), StatusEffectSource.PLAYER);
			}
			_stompState = StompState.JumpingToReturn;
			StartMove(_returnToPos);
		}

		private IEnumerator WaitForLandingAndFinishMove()
		{
			while (!_animationHasFinished)
			{
				yield return null;
			}
			_animationHasFinished = false;
			base.transform.position = _targetPos;
			EndAction();
		}

		private void LandingScreenshake()
		{
			if (landingScreenshakeWeight > 0f && !base.screenShakeBlocked)
			{
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				_camera.DOShakePosition(landingScreenshakeTime, Vector3.down * landingScreenshakeWeight * num);
			}
		}

		public override void Attack(int range, Vector3 targetPos)
		{
			if (HasEffect(StatusEffectType.Thorned))
			{
				PlayThornedAnimation();
			}
			else if (HasEffect(StatusEffectType.Stunned))
			{
				PlayStunnedAnimation();
			}
			else if (range <= _enemyAttack.AttackRange)
			{
				StartCoroutine(RangedAttack(targetPos));
			}
		}

		public override IEnumerator RangedAttack(Vector3 vector3, float damage = -1f, GameObject projectileOverride = null, string animTriggerOverride = "")
		{
			CurrentlyActing = true;
			_returnToPos = base.enemyManager.GetSpawnPointForGroundSlot(base.enemyManager.GetSlotIndexForGroundEnemy(this));
			_stompState = StompState.JumpingToAttack;
			StartMove(vector3);
			yield return null;
		}

		protected void OnDestroy()
		{
			if (_dustEffectAnim != null)
			{
				UnityEngine.Object.Destroy(_dustEffectAnim.gameObject);
				_dustEffectAnim = null;
			}
		}
	}
}
