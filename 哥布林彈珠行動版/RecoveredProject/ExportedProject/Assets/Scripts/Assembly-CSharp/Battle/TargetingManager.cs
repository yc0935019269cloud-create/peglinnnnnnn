using System;
using System.Collections.Generic;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using Rewired;
using UnityEngine;

namespace Battle
{
	public class TargetingManager : MonoBehaviour
	{
		public delegate void TargetsSet(List<Enemy> targets);

		public delegate void DamageTargetedEnemyRequest(long damage, Enemy.EnemyDamageSource source = Enemy.EnemyDamageSource.Unspecified);

		public static TargetsSet OnTargetsSet;

		public static DamageTargetedEnemyRequest OnDamageTargetedEnemy;

		private bool _canTarget;

		private Enemy _target;

		private Vector2 _playerPosition;

		private TargetingType _targetingType;

		private EnemyManager _enemyManager;

		private RelicManager _relicManager;

		private Player _player;

		public Enemy currentTarget => _target;

		public bool canTarget => _canTarget;

		private void OnEnable()
		{
			Enemy.OnEnemyClicked = (Enemy.EnemyClicked)Delegate.Combine(Enemy.OnEnemyClicked, new Enemy.EnemyClicked(AttemptSetEnemyAsTarget));
			EnemyInfoElement.OnEnemyInfoElementClicked = (EnemyInfoElement.EnemyInfoElementClicked)Delegate.Combine(EnemyInfoElement.OnEnemyInfoElementClicked, new EnemyInfoElement.EnemyInfoElementClicked(AttemptSetEnemyAsTarget));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(StopTargetingOnFire));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegCollisionHit = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegCollisionHit, new Peg.PegHitEvent(HandlePegCollisionHit));
			PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(HandlePlayerHealed));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
			OnDamageTargetedEnemy = (DamageTargetedEnemyRequest)Delegate.Combine(OnDamageTargetedEnemy, new DamageTargetedEnemyRequest(ExternalDamageTargetedRequest));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyClicked = (Enemy.EnemyClicked)Delegate.Remove(Enemy.OnEnemyClicked, new Enemy.EnemyClicked(AttemptSetEnemyAsTarget));
			EnemyInfoElement.OnEnemyInfoElementClicked = (EnemyInfoElement.EnemyInfoElementClicked)Delegate.Remove(EnemyInfoElement.OnEnemyInfoElementClicked, new EnemyInfoElement.EnemyInfoElementClicked(AttemptSetEnemyAsTarget));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(StopTargetingOnFire));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegCollisionHit = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegCollisionHit, new Peg.PegHitEvent(HandlePegCollisionHit));
			PlayerHealthController.OnPlayerHealed = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerHealed, new PlayerHealthController.PlayerHealthEvent(HandlePlayerHealed));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
			OnDamageTargetedEnemy = (DamageTargetedEnemyRequest)Delegate.Remove(OnDamageTargetedEnemy, new DamageTargetedEnemyRequest(ExternalDamageTargetedRequest));
		}

		private void Start()
		{
			EnemyInfoElement.OnEnemyInfoElementClicked = (EnemyInfoElement.EnemyInfoElementClicked)Delegate.Combine(EnemyInfoElement.OnEnemyInfoElementClicked, new EnemyInfoElement.EnemyInfoElementClicked(AttemptSetEnemyAsTarget));
			Enemy.OnEnemyClicked = (Enemy.EnemyClicked)Delegate.Combine(Enemy.OnEnemyClicked, new Enemy.EnemyClicked(AttemptSetEnemyAsTarget));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(StopTargetingOnFire));
			_player = ReInput.players.GetPlayer(0);
			foreach (Enemy enemy in _enemyManager.Enemies)
			{
				if (enemy.targetLock)
				{
					SetEnemyAsTarget(enemy, force: true);
					break;
				}
				if (enemy.targetByDefault)
				{
					SetEnemyAsTarget(enemy, force: true);
				}
			}
		}

		public void Initialize(EnemyManager enemyManager, RelicManager relicManager, Vector3 playerPosition)
		{
			_enemyManager = enemyManager;
			_relicManager = relicManager;
			_playerPosition = playerPosition;
		}

		private void Update()
		{
			if (!canTarget)
			{
				return;
			}
			if (_player.GetButtonDown("TargetLeft"))
			{
				Enemy enemy = ((_target == null) ? GetClosestEnemy() : GetNextEnemy(_target, toTheRight: false));
				if (enemy != null)
				{
					SetEnemyAsTarget(enemy);
				}
			}
			else if (_player.GetButtonDown("TargetRight"))
			{
				Enemy enemy2 = ((_target == null) ? GetClosestEnemy() : GetNextEnemy(_target));
				if (enemy2 != null)
				{
					SetEnemyAsTarget(enemy2);
				}
			}
		}

		private void HandlePegCollisionHit(Peg.PegType pegType, Peg peg)
		{
		}

		private void HandlePegActivated(Peg.PegType pegType, Peg peg)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.DAMAGE_TARGETED_PEG_HITS))
			{
				AttemptDamageTargetedEnemy(1L, Enemy.EnemyDamageSource.RelicChip);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.LIFESTEAL_PEG_HITS) && AttemptDamageTargetedEnemy(1L, Enemy.EnemyDamageSource.RelicChip))
			{
				PlayerHealthController.OnExternalHealRequested?.Invoke(1f);
			}
		}

		private void HandlePlayerHealed(float healAmount)
		{
			if (healAmount > 0f && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_TARGETED_ON_HEAL))
			{
				AttemptDamageTargetedEnemy(Mathf.RoundToInt(healAmount * 3f));
			}
		}

		private void ExternalDamageTargetedRequest(long damage, Enemy.EnemyDamageSource source = Enemy.EnemyDamageSource.Unspecified)
		{
			AttemptDamageTargetedEnemy(damage, source);
		}

		private bool AttemptDamageTargetedEnemy(long damage, Enemy.EnemyDamageSource source = Enemy.EnemyDamageSource.Unspecified)
		{
			damage = Mathf.FloorToInt(damage);
			if (_target == null)
			{
				AutoSelect();
				if (_target == null)
				{
					return false;
				}
			}
			if (_target.CurrentHealth <= 0f)
			{
				AutoSelect();
				if (_target == null)
				{
					return false;
				}
			}
			_target.Damage(damage, screenshake: false, 0.25f, 1f, unblockable: false, source);
			if (_target.CurrentHealth == 0f)
			{
				ClearTarget();
			}
			return true;
		}

		private void ClearTarget()
		{
			if (_target != null)
			{
				_target.ToggleTargetedUI(on: false);
				_target = null;
			}
		}

		public void AutoSelect()
		{
			if ((_targetingType == TargetingType.SPELL && _target == null) || (_target == null && _enemyManager.FlyingEnemyCount > 0 && _enemyManager.GroundEnemyCount == 0))
			{
				SetEnemyAsTarget(GetClosestEnemy(), force: true);
			}
			else if ((_target != null && _targetingType == TargetingType.SPELL) || (_target != null && _enemyManager.FlyingEnemyCount > 0 && _enemyManager.GroundEnemyCount == 0))
			{
				if (_target == null || !currentTarget.gameObject.activeSelf)
				{
					ClearTarget();
					SetEnemyAsTarget(GetClosestEnemy(), force: true);
				}
			}
			else if (_target == null || _target.CurrentHealth <= 0f)
			{
				SetEnemyAsTarget(GetClosestEnemy(), force: true);
			}
		}

		public void SetTargetingStatus(bool canTarget, TargetingType targetingType)
		{
			_canTarget = canTarget;
			_targetingType = targetingType;
			AutoSelect();
		}

		public void AttemptSetEnemyAsTarget(Enemy enemy)
		{
			SetEnemyAsTarget(enemy);
		}

		public void SetEnemyAsTarget(Enemy enemy, bool force = false)
		{
			if (!canTarget && !force)
			{
				return;
			}
			if (_target != null && _target.CurrentHealth > 0f && (_target.targetLock || _target.untargetable))
			{
				_target.ShakeLockedTargetUI();
				return;
			}
			ClearTarget();
			if (enemy != null)
			{
				_target = enemy;
				enemy.ToggleTargetedUI(on: true);
			}
		}

		private void StopTargetingOnFire(Vector2 vector)
		{
			_canTarget = false;
		}

		private Enemy GetClosestEnemy()
		{
			Enemy enemy = null;
			foreach (Enemy enemy2 in _enemyManager.Enemies)
			{
				if (enemy2.CurrentHealth > 0f && !enemy2.ignoreAutoTarget && enemy2.targetLock && (enemy == null || Mathf.Abs(enemy2.transform.position.x - _playerPosition.x) < Mathf.Abs(enemy.transform.position.x - _playerPosition.x)))
				{
					enemy = enemy2;
				}
			}
			if (enemy != null)
			{
				return enemy;
			}
			foreach (Enemy enemy3 in _enemyManager.Enemies)
			{
				if (enemy3.CurrentHealth > 0f && !enemy3.ignoreAutoTarget && (enemy == null || Mathf.Abs(enemy3.transform.position.x - _playerPosition.x) < Mathf.Abs(enemy.transform.position.x - _playerPosition.x)))
				{
					enemy = enemy3;
				}
			}
			if (enemy != null)
			{
				return enemy;
			}
			bool flag = true;
			foreach (Enemy enemy4 in _enemyManager.Enemies)
			{
				if (!enemy4.ignoreAutoTarget)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (Enemy enemy5 in _enemyManager.Enemies)
				{
					if (enemy5.CurrentHealth > 0f && (enemy == null || Mathf.Abs(enemy5.transform.position.x - _playerPosition.x) < Mathf.Abs(enemy.transform.position.x - _playerPosition.x)))
					{
						enemy = enemy5;
					}
				}
			}
			return enemy;
		}

		private Enemy GetNextEnemy(Enemy targetEnemy, bool toTheRight = true)
		{
			List<Enemy> orderedTargetingList = _enemyManager.GetOrderedTargetingList();
			if (orderedTargetingList.Count <= 0)
			{
				return targetEnemy;
			}
			if (orderedTargetingList.Count == 1)
			{
				return orderedTargetingList[0];
			}
			for (int i = 0; i < orderedTargetingList.Count; i++)
			{
				if (!(orderedTargetingList[i] == targetEnemy))
				{
					continue;
				}
				if (toTheRight)
				{
					if (i == orderedTargetingList.Count - 1)
					{
						return orderedTargetingList[0];
					}
					return orderedTargetingList[i + 1];
				}
				if (i == 0)
				{
					return orderedTargetingList[orderedTargetingList.Count - 1];
				}
				return orderedTargetingList[i - 1];
			}
			return targetEnemy;
		}

		public void RemoveDeadEnemyFromTargets(Enemy enemy)
		{
			if (_target == enemy)
			{
				ClearTarget();
			}
		}

		private void OnCritActivated()
		{
			if (_relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.RANDOM_STATUS_EFFECT_ON_CRIT))
			{
				int num = UnityEngine.Random.Range(0, AddRandomStatusEffectOnHit.RoundreloquencePotentialEffects.Length);
				StatusEffectType effectType = AddRandomStatusEffectOnHit.RoundreloquencePotentialEffects[num];
				if (currentTarget != null)
				{
					currentTarget.ApplyStatusEffect(new StatusEffect(effectType, AddRandomStatusEffectOnHit.RoundreloquenceEffectIntensity[num]), StatusEffectSource.PLAYER);
				}
			}
		}
	}
}
