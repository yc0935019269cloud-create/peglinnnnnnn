using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Enemies;
using Battle.StatusEffects;
using DG.Tweening;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ContinuouslySummonEnemies : AlternateEnemyAction
	{
		private class SummonedEnemyRespawnData
		{
			public Enemy enemy;

			public int slot;

			public bool isDead;

			public int roundsUntilRespawn;

			public SummonedEnemyRespawnData(Enemy Enemy, int Slot, bool IsDead, int RoundsUntilRespawn)
			{
				enemy = Enemy;
				slot = Slot;
				isDead = IsDead;
				roundsUntilRespawn = RoundsUntilRespawn;
			}
		}

		public Enemy[] enemiesToSpawn;

		public List<int> slotsToSpawnIn;

		public int roundsFromDeathToRespawn = 3;

		public int maxSummonsPerRound = 1;

		public string moveToSummonPositionAnimationTrigger = "flaotMoves";

		public float moveToSummonPosMoveTime = 0.6f;

		public string summonAnimationTrigger = "summon";

		public float timeBetweenSummonAnimations = 0.4f;

		[Space(15f)]
		public Vector3 summonPositionOffset;

		private Vector3 _homePostion;

		private Dictionary<int, SummonedEnemyRespawnData> _summonedEnemyData = new Dictionary<int, SummonedEnemyRespawnData>();

		private List<int> _slotsToSpawnInThisTurn = new List<int>();

		private Animator _anim;

		public bool HaveChildrenEnemyDied { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			_homePostion = _enemy.transform.position;
			_anim = GetComponent<Animator>();
			slotsToSpawnIn.Sort((int x, int y) => (int)Mathf.Sign(x - y));
		}

		private void Start()
		{
			foreach (int item in slotsToSpawnIn)
			{
				Enemy flyingEnemyAtSlot = _enemy.enemyManager.GetFlyingEnemyAtSlot(item);
				_summonedEnemyData.Add(item, new SummonedEnemyRespawnData(flyingEnemyAtSlot, item, flyingEnemyAtSlot == null, 0));
			}
		}

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyKilled));
		}

		public void DoPreMoveTurnAction()
		{
			_slotsToSpawnInThisTurn.Clear();
			foreach (SummonedEnemyRespawnData value in _summonedEnemyData.Values)
			{
				if (value.isDead)
				{
					HaveChildrenEnemyDied = true;
					value.roundsUntilRespawn--;
					if (value.roundsUntilRespawn <= 0)
					{
						_slotsToSpawnInThisTurn.Add(value.slot);
					}
				}
			}
		}

		private void OnEnemyKilled(Enemy killedEnemy)
		{
			foreach (SummonedEnemyRespawnData value in _summonedEnemyData.Values)
			{
				if (value.enemy != null && killedEnemy == value.enemy)
				{
					value.isDead = true;
					value.roundsUntilRespawn = roundsFromDeathToRespawn;
					break;
				}
			}
		}

		public override bool ConditionsMet()
		{
			if (!_enemy.HasEffect(StatusEffectType.Thorned))
			{
				return _slotsToSpawnInThisTurn.Count > 0;
			}
			return false;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			int numSummoned = 0;
			List<int> list = new List<int>(_slotsToSpawnInThisTurn.ToArray());
			foreach (int slotToSpawnIn in list)
			{
				if (numSummoned >= maxSummonsPerRound)
				{
					break;
				}
				_anim.SetTrigger(moveToSummonPositionAnimationTrigger);
				yield return _enemy.transform.DOMove(_enemy.enemyManager.GetSpawnPointForFlyingSlot(slotToSpawnIn) + summonPositionOffset, moveToSummonPosMoveTime).WaitForCompletion();
				_anim.SetTrigger(summonAnimationTrigger);
				yield return WaitObjectAllocator.waitForEndOfFrame;
				float timer = 0f;
				AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
				while (timer <= stateInfo.length)
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
				}
				StartCoroutine(InstantiateEnemy(slotToSpawnIn));
				yield return new WaitForSeconds(timeBetweenSummonAnimations);
				numSummoned++;
			}
			yield return _enemy.transform.DOMove(_homePostion, moveToSummonPosMoveTime).WaitForCompletion();
			_enemy.CurrentlyActing = false;
		}

		private IEnumerator InstantiateEnemy(int slot)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (slot > -1 && _enemy.enemyManager.IsFlyingSlotClear(slot))
			{
				Vector3 spawnPointForFlyingSlot = _enemy.enemyManager.GetSpawnPointForFlyingSlot(slot);
				Enemy enemy = enemiesToSpawn[UnityEngine.Random.Range(0, enemiesToSpawn.Length)];
				Enemy component = UnityEngine.Object.Instantiate(enemy, spawnPointForFlyingSlot, Quaternion.identity).GetComponent<Enemy>();
				component.Initialize(_enemy.GetComponentInChildren<StatusEffectIconManager>().EffectData, _enemy.enemyManager, _enemy.relicManager, enemy.name);
				component.enemyTypes |= Enemy.EnemyType.Minion;
				_enemy.enemyManager.AddEnemy(component, slot, isFlyingOverride: true);
				_summonedEnemyData[slot].enemy = component;
				_summonedEnemyData[slot].slot = slot;
				_summonedEnemyData[slot].isDead = false;
				_summonedEnemyData[slot].roundsUntilRespawn = -1;
			}
		}
	}
}
