using System;
using System.Collections;
using Battle.StatusEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MoreMountains.Tools;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies
{
	public class WalkEnemy : Enemy
	{
		[Header("Walk Enemy Params")]
		public float MoveSpeed = 1f;

		public float MoveScreenShakeWeight;

		[Tooltip("Should this enemy attempt a ranged attack after moving?\nIgnores range values. The intention is moving + shooting until melee range.")]
		public bool MoveAndShoot;

		private int _targetSlotIndex = -1;

		[SerializeField]
		private string _moveAnimName = "Running";

		private IExtraTurnTrigger[] _extraTurnTriggers;

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
			_extraTurnTriggers = GetComponents<IExtraTurnTrigger>();
		}

		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			_targetSlotIndex = slotIndex;
			if (HasEffect(StatusEffectType.Thorned))
			{
				PlayThornedAnimation();
				if (MoveAndShoot)
				{
					StartCoroutine(DoMoveAndShootAttack());
				}
				return false;
			}
			if (HasEffect(StatusEffectType.Stunned))
			{
				PlayStunnedAnimation();
				return false;
			}
			if (_skipActionThisTurn)
			{
				_skipActionThisTurn = false;
				return false;
			}
			StartCoroutine(StartMove(position));
			return true;
		}

		protected IEnumerator StartMove(Vector3 targetPosition)
		{
			float timer = 0f;
			queuedAnimations.Enqueue(AnimationTypes.MOVEMENT);
			while (timer <= 300f && (CurrentlyActing || queuedAnimations.Peek() != AnimationTypes.MOVEMENT))
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 300f)
				{
					Debug.LogWarning("Timed out on our move start wait");
				}
			}
			CurrentlyActing = true;
			if (_anim.MMHasParameterOfType(_moveAnimName, AnimatorControllerParameterType.Bool))
			{
				_anim.SetBool(_moveAnimName, value: true);
			}
			float duration = Mathf.Abs(base.transform.position.x - targetPosition.x) / MoveSpeed;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(targetPosition.x, duration).SetEase(Ease.InOutSine);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveComplete));
			if (_lineRenderer != null)
			{
				DOTween.To(base.SetLifelinkRendererPosition, base.transform.position.x, targetPosition.x, duration);
			}
			if (MoveScreenShakeWeight > 0f && !base.screenShakeBlocked)
			{
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				_camera.DOShakePosition(duration, MoveScreenShakeWeight * num);
			}
		}

		private void MoveLifelinkRenderer(float positionX)
		{
		}

		private void MoveComplete()
		{
			if (_anim.MMHasParameterOfType(_moveAnimName, AnimatorControllerParameterType.Bool))
			{
				_anim.SetBool(_moveAnimName, value: false);
			}
			if (MoveAndShoot && _targetSlotIndex > 0)
			{
				StartCoroutine(DoMoveAndShootAttack());
			}
			else
			{
				EndAction();
			}
			if (queuedAnimations.Count > 0)
			{
				queuedAnimations.Dequeue();
			}
			else
			{
				Debug.LogWarning("Found no animations to dequeue from MoveComplete");
			}
		}

		protected override void OnVinesAnimationEnded()
		{
			OneShotAnimation vineAnimation = _vineAnimation;
			vineAnimation.OnAnimationEnded = (OneShotAnimation.AnimationEnded)Delegate.Remove(vineAnimation.OnAnimationEnded, new OneShotAnimation.AnimationEnded(OnVinesAnimationEnded));
			_vineAnimation.gameObject.SetActive(value: false);
			if (!MoveAndShoot)
			{
				EndAction();
			}
		}

		public IEnumerator DoMoveAndShootAttack()
		{
			if (CurrentHealth <= 0f)
			{
				yield break;
			}
			if (AttackChargeLength > 0 && currentChargeTime < AttackChargeLength)
			{
				PlayAnimation("charge_trigger");
				if (_anim.MMHasParameterOfType("charge", AnimatorControllerParameterType.Bool))
				{
					_anim.SetBool("charge", value: true);
				}
				currentChargeTime++;
				if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
				{
					_anim.SetInteger("charge_count", currentChargeTime);
				}
				EndAction();
				yield break;
			}
			if (AttackChargeLength > 0 && currentChargeTime >= AttackChargeLength)
			{
				if (_anim.MMHasParameterOfType("charge", AnimatorControllerParameterType.Bool))
				{
					_anim.SetBool("charge", value: false);
				}
				currentChargeTime = 0;
				if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
				{
					_anim.SetInteger("charge_count", currentChargeTime);
				}
			}
			CurrentlyActing = false;
			if (_extraTurnTriggers != null)
			{
				for (int i = 0; i < _extraTurnTriggers.Length; i++)
				{
					_extraTurnTriggers[i].DoExtraTurnBehavior(this);
				}
			}
			yield return StartCoroutine(RangedAttack(_enemyManager.playerPosition));
			if (!TryGetComponent<SlimeBossUpgrade>(out var component) || !component.isUpgraded || CurrentHealth <= 0f)
			{
				yield break;
			}
			CurrentlyActing = false;
			if (_extraTurnTriggers != null)
			{
				for (int j = 0; j < _extraTurnTriggers.Length; j++)
				{
					_extraTurnTriggers[j].DoExtraTurnBehavior(this);
				}
			}
			StartCoroutine(RangedAttack(_enemyManager.playerPosition));
		}
	}
}
