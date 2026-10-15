using System.Collections;
using DG.Tweening;
using PeglinUtils;
using Relics;
using TMPro;
using UnityEngine;

namespace Battle.Enemies
{
	public class DemonWallBossWalkEnemy : WalkEnemy
	{
		[Header("Demon Wall Boss Properties")]
		[Tooltip("Don't eat enemies further than this, move closer first.")]
		[SerializeField]
		private float _enemyEatDistance = 2f;

		[SerializeField]
		private float _enemyCanvasFadeTime = 0.5f;

		[SerializeField]
		private float _emoteDisplayDelay = 0.3f;

		[SerializeField]
		private float _enemyEmoteTime = 1.2f;

		[SerializeField]
		private float _timeFromEmoteStartToFlip = 0.65f;

		[SerializeField]
		private float _peglinStartWalkDelay = 0.25f;

		[SerializeField]
		private float _peglinWalkIntoMouthTime = 0.85f;

		private static readonly int OpenMouth = Animator.StringToHash("open_mouth");

		private static readonly int CloseMouth = Animator.StringToHash("close_mouth");

		private static readonly int Moving = Animator.StringToHash("moving");

		private SpriteMask _spriteMask;

		private DemonWallBossUpgrade _demonWallBossUpgrade;

		protected override void Awake()
		{
			base.Awake();
			_spriteMask = GetComponentInChildren<SpriteMask>();
			_demonWallBossUpgrade = GetComponent<DemonWallBossUpgrade>();
		}

		public override bool Move(Vector3 position, int slotIndex = -1)
		{
			FodderEnemy[] array = Object.FindObjectsOfType<FodderEnemy>();
			FodderEnemy fodderEnemy = null;
			float num = float.MaxValue;
			if (array != null && array.Length != 0)
			{
				FodderEnemy[] array2 = array;
				foreach (FodderEnemy fodderEnemy2 in array2)
				{
					float num2 = Mathf.Abs(fodderEnemy2.transform.position.x - base.transform.position.x);
					if (num2 <= _enemyEatDistance && num2 < num)
					{
						num = num2;
						fodderEnemy = fodderEnemy2;
					}
				}
			}
			if (fodderEnemy != null)
			{
				CurrentlyActing = true;
				fodderEnemy.FadeOutCanvas(_enemyCanvasFadeTime);
				Emoter component = fodderEnemy.GetComponent<Emoter>();
				_anim.SetTrigger(OpenMouth);
				StartCoroutine(WaitAndDisplayEmote(component));
				StartCoroutine(WaitAndFlipEnemy(fodderEnemy, component._emoteText));
				StartCoroutine(WaitForOpenMouth());
				return false;
			}
			return base.Move(position, slotIndex);
		}

		protected override IEnumerator MeleeAttack()
		{
			if (!base.relicManager.AttemptUseRelic(RelicEffect.IMMORTAL))
			{
				CurrentlyActing = true;
				PlayerHealthController playerHealthController = Object.FindObjectOfType<PlayerHealthController>();
				Animator component = playerHealthController.GetComponent<Animator>();
				Emoter component2 = playerHealthController.GetComponent<Emoter>();
				_anim.SetTrigger(OpenMouth);
				StartCoroutine(WaitAndDisplayEmote(component2));
				StartCoroutine(PeglinStartRunning(component));
				yield return StartCoroutine(WaitForOpenMouth());
				yield return new WaitForSeconds(_peglinWalkIntoMouthTime - _peglinStartWalkDelay);
				playerHealthController.DealUnblockableDamage(2000f, displayFloatingDamageText: false, instakill: true);
				_anim.SetTrigger(CloseMouth);
			}
		}

		private IEnumerator PeglinStartRunning(Animator playerAnimator)
		{
			yield return new WaitForSeconds(_peglinStartWalkDelay);
			Vector3 localScale = playerAnimator.transform.localScale;
			localScale.x *= -1f;
			playerAnimator.transform.localScale = localScale;
			playerAnimator.SetBool(Moving, value: true);
			Canvas componentInChildren = playerAnimator.GetComponentInChildren<Canvas>();
			if (componentInChildren != null)
			{
				localScale = componentInChildren.transform.localScale;
				localScale.x *= -1f;
				componentInChildren.transform.localScale = localScale;
			}
			playerAnimator.transform.DOMoveX(_spriteMask.transform.position.x, _peglinWalkIntoMouthTime).SetEase(Ease.InBack);
		}

		private IEnumerator WaitAndDisplayEmote(Emoter emoter)
		{
			yield return new WaitForSeconds(_emoteDisplayDelay);
			emoter.ShowEmote(Emoter.Emote.EXCLAMATION, _enemyEmoteTime);
		}

		private IEnumerator WaitAndFlipEnemy(FodderEnemy fodderEnemy, TMP_Text emoteText)
		{
			yield return new WaitForSeconds(_timeFromEmoteStartToFlip);
			yield return new WaitForSeconds(1f);
			StartCoroutine(EatFodderEnemy(fodderEnemy));
		}

		private IEnumerator WaitForOpenMouth()
		{
			while (!_animationHasFinished)
			{
				yield return null;
			}
			_animationHasFinished = true;
		}

		private IEnumerator EatFodderEnemy(FodderEnemy enemy)
		{
			enemy.ForceMoveToPoint(_spriteMask.transform.position);
			float timer = 0f;
			while (enemy.CurrentlyActing && timer <= 300f)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 300f)
				{
					Debug.LogWarning("Timed out on waiting to eat fodder enemy");
				}
			}
			_enemyManager.RemoveEnemy(enemy);
			Object.Destroy(enemy.gameObject);
			_anim.SetTrigger(CloseMouth);
			StartCoroutine(WaitForCloseMouth());
		}

		private IEnumerator WaitForCloseMouth()
		{
			while (!_animationHasFinished)
			{
				yield return null;
			}
			_animationHasFinished = false;
			CurrentlyActing = false;
		}
	}
}
