using System;
using System.Collections;
using Battle.Enemies.C20Upgrades;
using Battle.StatusEffects;
using DG.Tweening;
using MoreMountains.Tools;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies
{
	public class MinesDragonBoss : StationaryEnemy
	{
		private enum DragonState
		{
			Charging = 0,
			BreathFire = 1,
			MeleeAttack = 2
		}

		private BossUpgrade _bossUpgrade;

		private DragonPegLayoutSimulation _dragonPegLayoutSimulation;

		[Header("Dragon Parameters")]
		public StatusEffect[] flameTickStatusEffects;

		public StatusEffect[] flameEndStatusEffects;

		public Transform peglinInhaleTarget;

		private Transform _player;

		private DragonState _dragonState;

		private float _playerInitialPosition;

		private float _playerTargetPosition;

		private float _fallbackTimer;

		protected override void Awake()
		{
			_bossUpgrade = GetComponent<BossUpgrade>();
			base.Awake();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SpecialSlotController.OrbInhaled = (Action)Delegate.Combine(SpecialSlotController.OrbInhaled, new Action(HandleOrbInhaled));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			SpecialSlotController.OrbInhaled = (Action)Delegate.Remove(SpecialSlotController.OrbInhaled, new Action(HandleOrbInhaled));
		}

		protected override void Start()
		{
			_player = GameObject.FindWithTag("Player").transform;
			_playerInitialPosition = base.enemyManager.playerPosition.x;
			_playerTargetPosition = peglinInhaleTarget.position.x;
			_dragonPegLayoutSimulation = UnityEngine.Object.FindObjectOfType<DragonPegLayoutSimulation>();
			if (_bossUpgrade.isUpgraded)
			{
				currentChargeTime++;
			}
		}

		protected override void Update()
		{
			if (CurrentlyActing && _fallbackTimer <= 5f)
			{
				_fallbackTimer += Time.deltaTime;
				if (_fallbackTimer > 5f)
				{
					Debug.LogWarning("Timed out on our attack start wait");
					_fallbackTimer = 0f;
					CurrentlyActing = false;
				}
			}
			else if (!CurrentlyActing)
			{
				_fallbackTimer = 0f;
			}
		}

		public override void Attack(int range, Vector3 targetPos)
		{
			if (HasEffect(StatusEffectType.Stunned))
			{
				PlayStunnedAnimation();
			}
			else
			{
				if (CurrentHealth <= 0f || _enemyAttack == null)
				{
					return;
				}
				if (_dragonState == DragonState.Charging && AttackChargeLength > 0 && currentChargeTime < AttackChargeLength)
				{
					_anim.SetBool("charge", value: true);
					currentChargeTime++;
					if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
					{
						_anim.SetInteger("charge_count", currentChargeTime);
					}
					CurrentlyActing = true;
					if (_bossUpgrade.isUpgraded)
					{
						_player.DOMoveX(Mathf.Lerp(_playerInitialPosition, _playerTargetPosition, (float)(currentChargeTime - 1) / (float)(AttackChargeLength - 1)), 0.65f);
					}
					else
					{
						_player.DOMoveX(Mathf.Lerp(_playerInitialPosition, _playerTargetPosition, (float)currentChargeTime / (float)AttackChargeLength), 0.65f);
					}
					if (currentChargeTime >= AttackChargeLength)
					{
						_anim.SetBool("charge_to_fire", value: true);
						SpecialSlotController.RemoveInhaleZoneRequest?.Invoke();
					}
					else
					{
						SpecialSlotController.PlaceInhaleZoneRequest?.Invoke();
					}
					return;
				}
				if (_dragonState == DragonState.Charging && AttackChargeLength > 0 && currentChargeTime >= AttackChargeLength)
				{
					_dragonState = DragonState.BreathFire;
					_anim.SetBool("charge", value: false);
					currentChargeTime = 0;
					if (_bossUpgrade.isUpgraded)
					{
						currentChargeTime++;
					}
					if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
					{
						_anim.SetInteger("charge_count", currentChargeTime);
					}
				}
				if (_dragonState == DragonState.BreathFire)
				{
					_anim.SetBool("charge_to_fire", value: false);
					Debug.Log("Calling fire attack animation");
					StartCoroutine(FireBreathAttack());
				}
				else if (_dragonState == DragonState.MeleeAttack)
				{
					_anim.SetBool("charge_to_fire", value: false);
					Debug.Log("Calling melee attack animation");
					StartCoroutine(_dragonPegLayoutSimulation.WaitAndUpdateBoardAfterResimulation());
					StartCoroutine(MeleeAttack());
					_dragonState = DragonState.Charging;
				}
			}
		}

		public override void MeleeAttackMadeContact()
		{
			base.MeleeAttackMadeContact();
			_player.DOMoveX(_playerInitialPosition, 0.8f);
			ScreenShake();
			ControllerVibration.DoVibration(0.95f, 0.3f, 0.05f, 0.05f);
		}

		public void ScreenShake()
		{
			if (_screenShaker == null || !_screenShaker.blocked)
			{
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				float duration = Mathf.Lerp(screenShakeDurationMin, screenShakeDurationMax, num);
				_camera.DOShakePosition(duration, Mathf.Min(screenShakeIntensityBase * num, screenShakeIntensityMax));
			}
		}

		private IEnumerator FireBreathAttack()
		{
			float timer = 0f;
			queuedAnimations.Enqueue(AnimationTypes.MELEE_ATTACK);
			while (timer <= 5f && (CurrentlyActing || queuedAnimations.Peek() != AnimationTypes.MELEE_ATTACK))
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 5f)
				{
					Debug.LogWarning("Timed out on our attack start wait");
				}
			}
			CurrentlyActing = true;
			if (_enemyAttack != null && !string.IsNullOrEmpty(_enemyAttack.RangedAttackAnimation))
			{
				_anim.SetTrigger(_enemyAttack.RangedAttackAnimation);
			}
			if (!(CurrentHealth > 0f))
			{
				yield break;
			}
			Debug.Log("Waiting for fire attack anim to finish");
			_animationHasFinished = false;
			timer = 0f;
			while (!_animationHasFinished)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 2f)
				{
					Debug.LogWarning("Timed out on our attack finish end");
					_animationHasFinished = true;
				}
			}
			_dragonState = DragonState.MeleeAttack;
			EndAction();
			if (queuedAnimations.Count > 0)
			{
				queuedAnimations.Dequeue();
			}
			else
			{
				Debug.LogWarning("Found no animation to dequeue from MeleeAttack end");
			}
		}

		private void HandleOrbInhaled()
		{
			int intensity = ((!(_cruciballManager != null) || !_cruciballManager.ShouldUpgradeBosses()) ? 1 : 3);
			ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, intensity), StatusEffectSource.ENEMY);
		}

		public void EndChargingAction()
		{
			CurrentlyActing = false;
		}

		public void FlameDamageTick()
		{
			Enemy.OnEnemyAttack?.Invoke(DamagePerRangedAttack, melee: false, this);
			PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(flameTickStatusEffects, StatusEffectSource.ENEMY);
		}

		public void FinalFlameDamageTick()
		{
			FlameDamageTick();
			PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(flameEndStatusEffects, StatusEffectSource.ENEMY);
			if (_bossUpgrade.isUpgraded)
			{
				DragonBossUpgrade dragonBossUpgrade = _bossUpgrade as DragonBossUpgrade;
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(dragonBossUpgrade.extraFlamePegsToAdd, StatusEffectSource.ENEMY);
			}
		}
	}
}
