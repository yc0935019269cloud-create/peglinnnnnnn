using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Cruciball;
using PeglinUtils;
using Relics;
using UnityEngine;

namespace Battle.Attacks
{
	public class ProjectileAttack : Attack
	{
		private struct WaitAndFireData
		{
			public long Damage;

			public int DmgValuesLength;

			public int CritCount;

			public ShotBehavior ReverseShot;

			public Vector3 ReverseAttackPos;

			public WaitAndFireData(long damage, int dmgValuesLength, int critCount, ShotBehavior reverseShot, Vector3 reverseAttackPos)
			{
				Damage = damage;
				DmgValuesLength = dmgValuesLength;
				CritCount = critCount;
				ReverseShot = reverseShot;
				ReverseAttackPos = reverseAttackPos;
			}
		}

		[SerializeField]
		protected GameObject _shotPrefab;

		[SerializeField]
		protected GameObject _criticalShotPrefab;

		protected ShotBehavior _shot;

		protected ShotBehavior _criticalShot;

		protected ShotBehavior _reverseShot;

		protected ShotBehavior _reverseCriticalShot;

		protected long _hitDamage;

		private WaitAndFireData _secondPincerManeuverShotData;

		private bool _firstPincerManeuverShotFinished = true;

		private float _accumulatedDamageMods = 1f;

		private AudioSource _sfxAudioSource;

		private int _critCount;

		private List<ShotBehavior> _shotsFired = new List<ShotBehavior>();

		public long shotDamage
		{
			get
			{
				return _hitDamage;
			}
			set
			{
				_hitDamage = value;
			}
		}

		public float shotDamageMod => _accumulatedDamageMods;

		public override void Initialize(Vector2 playerPosition, AttackManager attackManager, RelicManager relicManager, DeckManager deckManager, CruciballManager cruciballManager, PlayerHealthController healthController, PlayerStatusEffectController playerStatusEffectController)
		{
			base.Initialize(playerPosition, attackManager, relicManager, deckManager, cruciballManager, healthController, playerStatusEffectController);
			_sfxAudioSource = attackManager.SfxAudioSource;
			_shot = UnityEngine.Object.Instantiate(_shotPrefab, _playerPosition, Quaternion.identity).GetComponent<ShotBehavior>();
			_criticalShot = UnityEngine.Object.Instantiate(_criticalShotPrefab, _playerPosition, Quaternion.identity).GetComponent<ShotBehavior>();
			_shot.Initialize(this, attackManager.SfxAudioSource, _relicManager);
			_criticalShot.Initialize(this, attackManager.SfxAudioSource, _relicManager);
			bool flag = GetComponent<ReverseAttack>() != null;
			if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK) | flag)
			{
				Vector2 vector = new Vector2(16f, _playerPosition.y);
				_reverseShot = UnityEngine.Object.Instantiate(_shotPrefab, vector, Quaternion.identity).GetComponent<ShotBehavior>();
				_reverseCriticalShot = UnityEngine.Object.Instantiate(_criticalShotPrefab, vector, Quaternion.identity).GetComponent<ShotBehavior>();
				_reverseShot.Initialize(this, attackManager.SfxAudioSource, _relicManager);
				_reverseCriticalShot.Initialize(this, attackManager.SfxAudioSource, _relicManager);
				_reverseShot.startupDelay += 0.5f;
				_reverseShot.invisibleDuringStartupDelay = true;
				_reverseCriticalShot.startupDelay += 0.5f;
				_reverseCriticalShot.invisibleDuringStartupDelay = true;
				_reverseShot.gameObject.SetActive(value: false);
				_reverseCriticalShot.gameObject.SetActive(value: false);
			}
			if (_shot == null)
			{
				Debug.LogError("ProjectileAttack::OnEnable(): Fireball shot prefab has no ShotBehavior component");
				return;
			}
			if (_criticalShot == null)
			{
				Debug.LogError("ProjectileAttack::OnEnable(): Ice Spike shot prefab has no ShotBehavior component");
				return;
			}
			_shot.gameObject.SetActive(value: false);
			_criticalShot.gameObject.SetActive(value: false);
		}

		public override void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long dmgBonus, int critCount = 0)
		{
			_critCount = critCount;
			_hitDamage = GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount);
			if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK))
			{
				long num = _hitDamage % 2;
				_hitDamage = _hitDamage / 2 + num;
			}
			ShotBehavior shotBehavior = ((critCount > 0 && _hitDamage > 0) ? _criticalShot : _shot);
			ShotBehavior shotBehavior2 = ((critCount > 0 && _hitDamage > 0) ? _reverseCriticalShot : _reverseShot);
			if (shotBehavior == null)
			{
				Debug.LogWarning("ProjectileAttack::Fire(): Somehow we got called after the shot itself has been destroyed, due to a dangling delegate call");
				OnEnemyHit(null, null, attackFinished: true);
				return;
			}
			ReverseAttack component = GetComponent<ReverseAttack>();
			bool num2 = component != null && (critCount > 0 || !component.onlyOnCrit);
			bool flag = _relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK);
			Vector2 vector = new Vector2(16f, _playerPosition.y);
			_checkForPostAttackOperations = true;
			if (num2)
			{
				SpawnAndFireProjectile(target, numPegsHit, critCount, shotBehavior, vector);
				if (flag)
				{
					shotBehavior2.isAPincerManeuverBackwardsShot = true;
					_shotsFired.Add(shotBehavior2);
					_secondPincerManeuverShotData = new WaitAndFireData(_hitDamage, numPegsHit, critCount, shotBehavior2, vector);
					_firstPincerManeuverShotFinished = false;
				}
			}
			else if (flag)
			{
				SpawnAndFireProjectile(target, numPegsHit, critCount, shotBehavior, _playerPosition);
				shotBehavior2.isAPincerManeuverBackwardsShot = true;
				_shotsFired.Add(shotBehavior2);
				_secondPincerManeuverShotData = new WaitAndFireData(_hitDamage, numPegsHit, critCount, shotBehavior2, vector);
				_firstPincerManeuverShotFinished = false;
			}
			else
			{
				SpawnAndFireProjectile(target, numPegsHit, critCount, shotBehavior, _playerPosition);
			}
		}

		private void SpawnAndFireProjectile(Enemy target, int dmgValuesLength, int critCount, ShotBehavior shot, Vector3 spawnPos)
		{
			shot.gameObject.transform.position = spawnPos;
			SnowballShotBehavior component = null;
			if (shot.TryGetComponent<SnowballShotBehavior>(out component))
			{
				component.SetAttackType();
			}
			if (target != null && target.enemyTypes.HasFlag(Enemy.EnemyType.Flying) && shot.canAimUp && _relicManager.AttemptUseRelic(RelicEffect.FLYING_HORIZONTAL_PIERCE))
			{
				Vector2 vector = shot.gameObject.transform.position;
				vector.y = target.transform.position.y;
				EnemyManager enemyManager = UnityEngine.Object.FindObjectOfType<EnemyManager>();
				if (enemyManager != null)
				{
					float num = 0f;
					float num2 = 0f;
					foreach (Enemy enemy in enemyManager.Enemies)
					{
						if (enemy.IsFlying)
						{
							Bounds bounds = enemy.GetComponent<Collider2D>().bounds;
							if (num == 0f || bounds.max.y < num)
							{
								num = bounds.max.y;
							}
							if (num2 == 0f || bounds.min.y > num2)
							{
								num2 = bounds.min.y;
							}
						}
					}
					if (num < num2)
					{
						Debug.LogError("There is no way to hit all flying enemies with this shot.");
					}
					else
					{
						vector.y = num2 + (num - num2) / 2f;
					}
				}
				shot.transform.position = vector;
			}
			shot.gameObject.SetActive(value: true);
			if (component != null)
			{
				shot.StartCoroutine(WaitAndSetSnowballAnim(component));
			}
			else
			{
				shot.initialPos = shot.transform.position;
			}
			shot.OnShotHit = (ShotBehavior.ShotHit)Delegate.Combine(shot.OnShotHit, new ShotBehavior.ShotHit(OnEnemyHit));
			shot.OnShotDisabled = (ShotBehavior.ShotDisabled)Delegate.Combine(shot.OnShotDisabled, new ShotBehavior.ShotDisabled(OnShotDisabled));
			if (_hitDamage > 0)
			{
				shot.Fire(dmgValuesLength, target, critCount);
			}
			else
			{
				shot.Whiff();
			}
			if (!_shotsFired.Contains(shot))
			{
				_shotsFired.Add(shot);
			}
		}

		public IEnumerator WaitAndSetSnowballAnim(SnowballShotBehavior snowballShotBehaviour)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			snowballShotBehaviour.anim.SetInteger(SnowballShotBehavior.SNOWBALL_SIZE_ANIM_INT_NAME, SnowballDmgAndSize.allSnowballsCurrentSizeIndex);
		}

		private void OnFirstPincerManeuverShotFinished()
		{
			_firstPincerManeuverShotFinished = true;
			_hitDamage = _secondPincerManeuverShotData.Damage;
			SpawnAndFireProjectile(_attackManager.enemyManager.GetFarthestEnemyFromPlayer(), _secondPincerManeuverShotData.DmgValuesLength, _secondPincerManeuverShotData.CritCount, _secondPincerManeuverShotData.ReverseShot, _secondPincerManeuverShotData.ReverseAttackPos);
		}

		private void OnShotDisabled(ShotBehavior shot)
		{
			CheckForPostAttackOperations();
			shot.OnShotDisabled = (ShotBehavior.ShotDisabled)Delegate.Remove(shot.OnShotDisabled, new ShotBehavior.ShotDisabled(OnShotDisabled));
		}

		public void SetShotPrefab(GameObject shot, GameObject crit)
		{
			_shotPrefab = shot;
			_criticalShotPrefab = crit;
		}

		protected virtual void OnEnemyHit(Enemy enemy, ShotBehavior shot, bool attackFinished)
		{
			if (enemy == null)
			{
				OnAttackFinished(shot);
				return;
			}
			StatusEffect effect = enemy.GetEffect(StatusEffectType.Transpherency);
			int num = -1;
			if (effect != null)
			{
				num = effect.Intensity;
			}
			float currentHealth = enemy.CurrentHealth;
			float num2 = 1f;
			if (enemy.IsFlying && shot.canAimUp && _relicManager.AttemptUseRelic(RelicEffect.FLYING_HORIZONTAL_PIERCE))
			{
				num2 = 1.5f;
			}
			float damageMod = GetDamageMod(enemy.projectileAttackDamageMod);
			damageMod *= num2;
			enemy.DealProjectileAttackDamage(_hitDamage, damageMod * _accumulatedDamageMods, _critCount, DealsMaxHPDamage);
			IAffectEnemyOnHit[] components = GetComponents<IAffectEnemyOnHit>();
			if (components != null && components.Length != 0)
			{
				IAffectEnemyOnHit[] array = components;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AffectEnemy(enemy, _relicManager, _critCount);
				}
			}
			if (shot.isOverflowShot || (shot.isPierceShot && enemy.pierceDamageReductionMod != 1f))
			{
				_accumulatedDamageMods *= enemy.pierceDamageReductionMod;
			}
			if (shot.isOverflowShot)
			{
				float num3 = currentHealth;
				if (damageMod < 1f && damageMod > 0f)
				{
					num3 /= damageMod;
				}
				else if (damageMod == 0f)
				{
					num3 = _hitDamage;
				}
				if (effect != null && (float)_hitDamage - num3 <= 0f)
				{
					_hitDamage = (long)((float)_hitDamage * ((float)num / 10f));
				}
				else if (effect != null && (float)_hitDamage - num3 > 0f)
				{
					_hitDamage = (long)((float)_hitDamage - num3 + num3 * ((float)num / 10f));
				}
				else
				{
					_hitDamage = (long)((float)_hitDamage - num3);
				}
				if (_hitDamage <= 0)
				{
					shot.DisableShot();
					attackFinished = true;
				}
				else if (!shot.isNaturalOverflow)
				{
					_relicManager.AttemptUseRelic(RelicEffect.NORMAL_ATTACKS_OVERFLOW);
				}
			}
			else if (effect != null && (!shot.isPierceShot || num >= 10))
			{
				_hitDamage = (long)((float)_hitDamage * ((float)num / 10f));
			}
			foreach (StatusEffect statusEffect in GetStatusEffects(_critCount))
			{
				if (statusEffect.EffectType != StatusEffectType.None)
				{
					enemy.ApplyStatusEffect(statusEffect, StatusEffectSource.PLAYER);
				}
			}
			_attackManager.WaitForEnemyDamageAnimationToEnd(enemy);
			if (attackFinished)
			{
				OnAttackFinished(shot);
			}
		}

		private void OnAttackFinished(ShotBehavior shot)
		{
			if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK) && !_firstPincerManeuverShotFinished)
			{
				OnFirstPincerManeuverShotFinished();
			}
			shot.OnShotHit = (ShotBehavior.ShotHit)Delegate.Remove(shot.OnShotHit, new ShotBehavior.ShotHit(OnEnemyHit));
			shot.gameObject.SetActive(value: false);
			_shotsFired.Remove(shot);
			if (_shotsFired.Count == 0)
			{
				_attackManager.AttackAnimationEnded();
				PachinkoBall component = base.gameObject.GetComponent<PachinkoBall>();
				if (IsSummonedBy != null)
				{
					IsSummonedBy.DestroyMultiballsAndSelf();
					IsSummonedBy = null;
				}
				else if (component != null)
				{
					component.DestroyMultiballsAndSelf();
				}
			}
		}

		private void OnDisable()
		{
			CheckForPostAttackOperations();
			if (_shot != null && _shot.gameObject != null)
			{
				UnityEngine.Object.Destroy(_shot.gameObject);
			}
			if (_criticalShot != null && _criticalShot.gameObject != null)
			{
				UnityEngine.Object.Destroy(_criticalShot.gameObject);
			}
		}

		private void OnDestroy()
		{
			if (_shot != null && _shot.gameObject != null)
			{
				UnityEngine.Object.Destroy(_shot.gameObject);
			}
			if (_criticalShot != null && _criticalShot.gameObject != null)
			{
				UnityEngine.Object.Destroy(_criticalShot.gameObject);
			}
		}
	}
}
