using System;
using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class AddFireToSlot : AlternateEnemyAction
	{
		public string effectPegString = "";

		public string animOverride = "";

		public int fireToAddPerTurn = 1;

		public bool addFlamePegsOnceBottomsSlotsFull;

		public int flamePegsToAdd = 2;

		public bool addOnSpawn;

		private SpecialSlotController _specialSlotController;

		private Animator _anim;

		private bool moveAndShoot;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private EffectPeg _effectPeg;

		private void Start()
		{
			_specialSlotController = UnityEngine.Object.FindObjectOfType<SpecialSlotController>();
			_anim = _enemy.GetComponent<Animator>();
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			WalkEnemy walkEnemy = _enemy as WalkEnemy;
			if (walkEnemy != null && walkEnemy.MoveAndShoot)
			{
				Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Combine(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(OnEnemyAttack));
				moveAndShoot = true;
			}
			if (addOnSpawn)
			{
				SpecialSlotController.OnAddStaticDamageSlotsRequest(fireToAddPerTurn);
			}
		}

		private void OnEnable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
		}

		private void OnDisable()
		{
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
			if (moveAndShoot)
			{
				SpecialSlotController.OnClearStaticDamageSlotsRequest();
			}
		}

		private void OnEffectPegCreated(EffectPeg effectPeg)
		{
			if (_spiritOfRadia != null && _effectPeg == null && effectPeg.effectName == effectPegString)
			{
				_effectPeg = effectPeg;
				EffectPeg effectPeg2 = _effectPeg;
				effectPeg2.OnEffectPegToggled = (EffectPeg.EffectPegToggled)Delegate.Combine(effectPeg2.OnEffectPegToggled, new EffectPeg.EffectPegToggled(OnEffectPegToggled));
			}
		}

		private void OnEffectPegToggled(bool state)
		{
			if (!state)
			{
				SpecialSlotController.OnClearStaticDamageSlotsRequest();
			}
		}

		private void OnDestroy()
		{
			if (moveAndShoot)
			{
				Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Remove(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(OnEnemyAttack));
			}
		}

		private void OnEnemyAttack(float damage, bool melee, Enemy enemy, PlayerHealthController.DamageSource source, bool forceMaxHPDamage = false)
		{
			if (!(_enemy != enemy) && ConditionsMet())
			{
				SpecialSlotController.OnAddStaticDamageSlotsRequest(fireToAddPerTurn);
			}
		}

		public override bool ConditionsMet()
		{
			if (_spiritOfRadia != null && _effectPeg != null)
			{
				if (_effectPeg.pegActive)
				{
					if (!addFlamePegsOnceBottomsSlotsFull)
					{
						if (_specialSlotController != null)
						{
							return _specialSlotController.staticDamageSlotCount < 5;
						}
						return false;
					}
					return true;
				}
				return false;
			}
			if (_specialSlotController != null)
			{
				return _specialSlotController.staticDamageSlotCount < 5;
			}
			return false;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			PlaySFX();
			if (_specialSlotController != null && _specialSlotController.staticDamageSlotCount >= 5)
			{
				if (addFlamePegsOnceBottomsSlotsFull)
				{
					PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(new StatusEffect[1]
					{
						new StatusEffect(StatusEffectType.FlamePegs, flamePegsToAdd)
					}, StatusEffectSource.ENEMY);
				}
			}
			else
			{
				SpecialSlotController.OnAddStaticDamageSlotsRequest(fireToAddPerTurn);
			}
			if (animOverride != "")
			{
				_anim.SetTrigger(animOverride);
				yield return WaitObjectAllocator.waitForEndOfFrame;
				float timer = 0f;
				AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
				while (timer <= stateInfo.length)
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
				}
			}
			_enemy.CurrentlyActing = false;
		}
	}
}
