using System;
using UnityEngine;

namespace Battle.Enemies.C20Upgrades
{
	public class MoleTreeBossUpgrade : BossUpgrade
	{
		private const string MOLE_BOSS_LOC_KEY = "mole_boss_name";

		public Sprite newTreeSpriteOn;

		public Sprite newTreeSpriteOff;

		private SpriteRenderer _renderer;

		private Enemy _moleBoss;

		private TargetingManager _targetingManager;

		private bool _targetLockingState;

		protected override void Awake()
		{
			base.Awake();
			_renderer = GetComponent<SpriteRenderer>();
		}

		private void Start()
		{
			if (!isUpgraded)
			{
				return;
			}
			_targetingManager = UnityEngine.Object.FindObjectOfType<TargetingManager>();
			foreach (Enemy enemy in _enemy.enemyManager.Enemies)
			{
				if (enemy.locKey == "mole_boss_name")
				{
					_moleBoss = enemy;
					break;
				}
			}
			SetTargetLockState(newValue: false);
		}

		private void OnEnable()
		{
			BattleController.OnStartedAwaitingShot = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnStartedAwaitingShot, new BattleController.BattleEvent(ToggleTargetLockState));
		}

		private void OnDisable()
		{
			BattleController.OnStartedAwaitingShot = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnStartedAwaitingShot, new BattleController.BattleEvent(ToggleTargetLockState));
		}

		public override void Upgrade()
		{
			_enemy.spellAttackDamageMod = 0.5f;
		}

		private void ToggleTargetLockState()
		{
			SetTargetLockState(!_targetLockingState);
		}

		private void SetTargetLockState(bool newValue)
		{
			if (isUpgraded)
			{
				_targetLockingState = newValue;
				if (_targetLockingState)
				{
					_targetingManager.SetEnemyAsTarget(_enemy);
					_enemy.targetLock = true;
					_renderer.sprite = newTreeSpriteOn;
				}
				else
				{
					_enemy.targetLock = false;
					_renderer.sprite = newTreeSpriteOff;
					_targetingManager.SetEnemyAsTarget(_moleBoss);
				}
				HideWhenCantRetarget.ExternalUpdateCurrentTargetingUI?.Invoke();
			}
		}
	}
}
