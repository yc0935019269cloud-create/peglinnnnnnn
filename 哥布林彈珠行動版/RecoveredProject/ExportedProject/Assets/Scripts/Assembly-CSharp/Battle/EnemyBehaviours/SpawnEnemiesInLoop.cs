using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class SpawnEnemiesInLoop : AlternateEnemyAction
	{
		public Enemy[] enemiesToSpawn;

		public int[] damageTakenOnSpawn = new int[6] { 40, 40, 40, 60, 80, 100 };

		public StatusEffect statusToAddOnLoopFinished;

		public string summonAnimationTrigger = "summon";

		private Animator _anim;

		private int _slotToSpawnIn;

		private int _currentSpawnIndex;

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
		}

		private void Start()
		{
			_slotToSpawnIn = _enemy.enemyManager.GetSlotIndexForEnemy(_enemy, out var _);
		}

		public override bool ConditionsMet()
		{
			return true;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			_anim.SetTrigger(summonAnimationTrigger);
			yield return WaitObjectAllocator.waitForEndOfFrame;
			float timer = 0f;
			AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			yield return StartCoroutine(InstantiateEnemy());
			_enemy.CurrentlyActing = false;
		}

		private IEnumerator InstantiateEnemy()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (_enemy.enemyManager.IsGroundSlotClear(_slotToSpawnIn))
			{
				Vector3 spawnPointForGroundSlot = _enemy.enemyManager.GetSpawnPointForGroundSlot(_slotToSpawnIn);
				Enemy component = Object.Instantiate(enemiesToSpawn[_currentSpawnIndex], spawnPointForGroundSlot, Quaternion.identity).GetComponent<Enemy>();
				component.Initialize(_enemy.GetComponentInChildren<StatusEffectIconManager>().EffectData, _enemy.enemyManager, _enemy.relicManager, enemiesToSpawn[_currentSpawnIndex].name);
				StatusEffect[] statusEffects = _enemy.StatusEffects;
				foreach (StatusEffect statusEffect in statusEffects)
				{
					component.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, statusEffect.Intensity), StatusEffectSource.ENEMY);
				}
				_enemy.enemyManager.AddEnemy(component, _slotToSpawnIn);
				_enemy.Damage(damageTakenOnSpawn[_currentSpawnIndex]);
				if (_currentSpawnIndex == enemiesToSpawn.Length - 1)
				{
					_enemy.ApplyStatusEffect(new StatusEffect(statusToAddOnLoopFinished.EffectType, statusToAddOnLoopFinished.Intensity), StatusEffectSource.ENEMY);
				}
				_currentSpawnIndex = (_currentSpawnIndex + 1) % enemiesToSpawn.Length;
			}
		}
	}
}
