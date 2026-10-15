using System;
using Battle.EnemyBehaviours;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.Enemies
{
	public class PainterBoss : StationaryEnemy
	{
		private ContinuouslySummonEnemies _summonEnemies;

		public Sprite specialBackgroundWall;

		public Sprite specialBackgroundLight;

		private bool _anySummonsDied;

		private string SUMMON_KEY = "painting_name";

		protected override void Awake()
		{
			_summonEnemies = GetComponent<ContinuouslySummonEnemies>();
			SpriteRenderer spriteRenderer = GameObject.FindWithTag("CastleBgLight")?.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				spriteRenderer.sprite = specialBackgroundLight;
			}
			SpriteRenderer spriteRenderer2 = GameObject.FindWithTag("CastleBgWall")?.GetComponent<SpriteRenderer>();
			if ((bool)spriteRenderer2)
			{
				spriteRenderer2.sprite = specialBackgroundWall;
			}
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			foreach (Enemy enemy in _enemyManager.Enemies)
			{
				if (enemy != this)
				{
					enemy.enemyTypes |= EnemyType.Minion;
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Enemy.OnEnemyKilled = (EnemyDataEvent)Delegate.Combine(Enemy.OnEnemyKilled, new EnemyDataEvent(CheckForSummonKilled));
		}

		private void CheckForSummonKilled(string lockey)
		{
			if (lockey == SUMMON_KEY)
			{
				_anySummonsDied = true;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (CurrentHealth <= 0f && !_anySummonsDied)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DEFEAT_PAINTER_WITHOUT_KILLING_PAINTINGS);
			}
		}
	}
}
