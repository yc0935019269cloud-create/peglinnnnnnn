using Challenges;
using Cruciball;
using Map;
using Relics;
using UnityEngine;

namespace Battle.Enemies
{
	public class PainbowSlimeEnemy : FleeingEnemy
	{
		public static readonly int FOREST_PAINBOW_STARTING_HEALTH = 2400;

		public static readonly int CRUCIBALL_FOREST_PAINBOW_STARTING_HEALTH = 2650;

		public static readonly int CASTLE_PAINBOW_STARTING_HEALTH = 6400;

		public static readonly int CRUCIBALL_CASTLE_PAINBOW_STARTING_HEALTH = 8000;

		public static readonly int MINES_PAINBOW_STARTING_HEALTH = 12000;

		public static readonly int CRUCIBALL_MINES_PAINBOW_STARTING_HEALTH = 18000;

		public static readonly int CORE_PAINBOW_STARTING_HEALTH = 5400;

		public static readonly int CRUCIBALL_CORE_PAINBOW_STARTING_HEALTH = 8100;

		public static int GetStartingHealth(int act, CruciballManager cruciballManager)
		{
			if (cruciballManager.EnemyShouldUseImprovedHealth())
			{
				switch (act)
				{
				case 1:
					return CRUCIBALL_FOREST_PAINBOW_STARTING_HEALTH;
				case 2:
					return CRUCIBALL_CASTLE_PAINBOW_STARTING_HEALTH;
				case 3:
					return CRUCIBALL_MINES_PAINBOW_STARTING_HEALTH;
				case 4:
					return CRUCIBALL_CORE_PAINBOW_STARTING_HEALTH;
				}
			}
			else
			{
				switch (act)
				{
				case 1:
					return FOREST_PAINBOW_STARTING_HEALTH;
				case 2:
					return CASTLE_PAINBOW_STARTING_HEALTH;
				case 3:
					return MINES_PAINBOW_STARTING_HEALTH;
				case 4:
					return CORE_PAINBOW_STARTING_HEALTH;
				}
			}
			return 1;
		}

		protected override void Start()
		{
			base.Start();
			if (MapController.instance != null)
			{
				_maxHealth = GetStartingHealth(MapController.instance.Act, _cruciballManager);
			}
			else if (_cruciballManager.EnemyShouldUseImprovedHealth())
			{
				_maxHealth = CRUCIBALL_FOREST_PAINBOW_STARTING_HEALTH;
			}
			else
			{
				_maxHealth = FOREST_PAINBOW_STARTING_HEALTH;
			}
			if (ChallengeManager.Instance.currentPainbowSlimeHealth == -1)
			{
				if (_relicManager.RelicEffectActive(RelicEffect.RANDOM_ENEMY_HEALTH))
				{
					_maxHealth = Mathf.Ceil(Random.Range(1f, _maxHealth * 2.5f));
				}
				ChallengeManager.Instance.currentPainbowSlimeHealth = (int)_maxHealth;
			}
			CurrentHealth = ChallengeManager.Instance.currentPainbowSlimeHealth;
			UpdateHealthBar();
		}

		protected override void OnDisable()
		{
			ChallengeManager.Instance.currentPainbowSlimeHealth = (int)CurrentHealth;
			base.OnDisable();
		}
	}
}
