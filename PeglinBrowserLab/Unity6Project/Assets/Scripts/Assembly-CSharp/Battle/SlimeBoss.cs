using System;
using Battle.Attacks;
using Battle.Enemies;
using DG.Tweening;
using Peglin.Achievements;
using UnityEngine;

namespace Battle
{
	public class SlimeBoss : MonoBehaviour
	{
		[Header("Alternating Aim Positions")]
		public Vector2[] regularAimPositions;

		public Vector2 eatenAimPosition;

		private int _lastRegularPositionIndex = -1;

		[Header("Peglin Eating Mechanics")]
		public float goblinEatTime = 0.6f;

		public Vector3 goblinOffset = new Vector3(0.8f, 1.2f);

		public Vector3 goblinRotation = new Vector3(0f, 25f, 0f);

		private float _originalY = float.MinValue;

		private Enemy _enemy;

		private bool _dead;

		private bool _eatingPlayer;

		private static readonly int Eaten = Animator.StringToHash("eaten");

		private BattleController _battleController;

		private EnemyManager _enemyManager;

		private void OnEnable()
		{
			_enemy = GetComponent<Enemy>();
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Combine(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(HandleEnemyDamaged));
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
			BattleController.OnBombLobFinished = (Action<BombLob>)Delegate.Combine(BattleController.OnBombLobFinished, new Action<BombLob>(OnBombLobFinished));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDamaged = (Enemy.EnemyDamaged)Delegate.Remove(Enemy.OnEnemyDamaged, new Enemy.EnemyDamaged(HandleEnemyDamaged));
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
			BattleController.OnBombLobFinished = (Action<BombLob>)Delegate.Remove(BattleController.OnBombLobFinished, new Action<BombLob>(OnBombLobFinished));
		}

		private void Start()
		{
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			_enemyManager = UnityEngine.Object.FindObjectOfType<EnemyManager>();
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.CompareTag("Player") && !_eatingPlayer && !_dead)
			{
				_eatingPlayer = true;
				_originalY = collision.transform.position.y;
				Vector3 endValue = collision.transform.position + goblinOffset;
				collision.transform.DOMove(endValue, goblinEatTime);
				collision.transform.DORotate(goblinRotation, goblinEatTime);
				collision.GetComponent<Animator>().SetBool(Eaten, value: true);
				collision.GetComponent<PeglinBattleAnimationController>().EatenBySlimeBoss = true;
				UpdatePachinkoPos();
			}
		}

		private void HandleEnemyDamaged(Enemy enemy, long damage, Enemy.EnemyDamageSource damageSource)
		{
			if (enemy == _enemy && enemy.CurrentHealth <= 0f && !_dead && _eatingPlayer)
			{
				_dead = true;
				_eatingPlayer = false;
				GameObject obj = GameObject.FindGameObjectWithTag("Player");
				float delay = 0.3f;
				obj.transform.DORotate(new Vector3(0f, 0f, 0f), 0.3f).SetDelay(delay);
				obj.transform.DOLocalMoveY(_originalY, 0.4f).SetDelay(delay);
				GetComponent<SpriteRenderer>().DOFade(0f, 1.3f).SetDelay(0.5f);
				obj.GetComponent<Animator>().SetBool(Eaten, value: false);
			}
		}

		private void HandleEnemyTurnEnd()
		{
			UpdatePachinkoPos();
		}

		private void OnBombLobFinished(BombLob obj)
		{
			if (obj.isRigged && _eatingPlayer)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.RIGGED_BOMB_WHILE_SWALLOWED);
			}
		}

		private void UpdatePachinkoPos()
		{
			if (_eatingPlayer)
			{
				_battleController.pachinkoBallSpawnLocation = eatenAimPosition;
				return;
			}
			_lastRegularPositionIndex++;
			_lastRegularPositionIndex %= regularAimPositions.Length;
			_battleController.pachinkoBallSpawnLocation = regularAimPositions[_lastRegularPositionIndex];
		}
	}
}
