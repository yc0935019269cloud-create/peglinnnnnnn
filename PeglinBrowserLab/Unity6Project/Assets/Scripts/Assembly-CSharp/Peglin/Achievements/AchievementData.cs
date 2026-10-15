using System;
using System.Collections.Generic;

namespace Peglin.Achievements
{
	public class AchievementData
	{
		public enum AchievementId
		{
			DEFEAT_MOLE_BOSS = 0,
			DEFEAT_SLIME_BOSS = 1,
			DEFEAT_WALL_BOSS = 2,
			DEFEAT_BALLISTA_BOSS = 3,
			DEFEAT_ANY_BOSS_FIRST_TURN = 4,
			DIE_IN_TEXT_SCENARIO = 5,
			DEAL_EXACTLY_ONE_DAMAGE = 6,
			DEAL_X_DAMAGE_IN_ONE_SHOT = 7,
			DEAL_EXACT_DAMAGE_TO_ENEMY = 8,
			CLEAR_PEGBOARD = 9,
			HIT_X_PEGS_IN_ONE_SHOT = 10,
			LEAVE_ENEMY_WITH_ONE_HEALTH = 11,
			BRAMBLE_ENEMY_FOR_X_TURNS = 12,
			DEFEAT_MOLE_TREE = 13,
			DEFEAT_BRICK_BOSS = 14,
			DEFEAT_SAPPER_BOSS = 15,
			REACH_CRUCIBALL_5 = 16,
			REACH_CRUCIBALL_10 = 17,
			REACH_CRUCIBALL_15 = 18,
			MULTIPLE_SLIME_BOUNCES = 19,
			POISON_AMOUNT_ON_ENEMY = 20,
			SHIELD_AMOUNT_AT_END_OF_BATTLE = 21,
			DEFEAT_LESHY_BOSS = 22,
			DEFEAT_PAINTER_BOSS = 23,
			DEFEAT_DRAGON_BOSS = 24,
			ASSEMBLE_ASSEMBALL = 25,
			MULTIPLE_CRITS = 26,
			SPAWN_ALL_MINIONS = 27,
			CLEAN_SHOP = 28,
			MULTIPLE_MULTIBALLS = 29,
			THREE_BRAMBALLS_STUCK_IN_VINES = 30,
			FINISH_RUN_WITH_TWO_EGGS = 31,
			REACH_CRUCIBALL_20 = 32,
			REFUSE_INGOT = 33,
			DIE_WHILE_KILLING_MINES_BOSS = 34,
			BRING_EGG_TO_PEGLIN_CHEF = 35,
			KILL_RAINBOW_SLIME = 36,
			WIN_RUN_WITH_ONLY_ONE_RELIC = 37,
			WIN_RUN_WITH_ONLY_LEVEL_ONE_ORBS = 38,
			FULLY_UNLOCK_MONSTER_IN_BESTIARY = 39,
			CARD_DROP_OBTAINED = 40,
			RIGGED_BOMB_WHILE_SWALLOWED = 41,
			DEFEAT_PAINTER_WITHOUT_KILLING_PAINTINGS = 42,
			CONVERT_OR_DESTROY_DULL_PEGS_VS_DEMON_WALL = 43,
			KILL_ALL_RUINS_IN_ONE_ATTACK = 44,
			DEAL_1000_DAMAGE_TO_SAPPER_WITH_SAPPERS = 45,
			COLLECT_ALL_DRAGON_COINS = 46,
			SEND_DEMON_SQUIRREL_TO_HOLE = 47,
			DEFEAT_PAINBOW = 48,
			DEFEAT_ORBSERVER = 49,
			END_FIRST_ORBSERVER_PHASE_EARLY = 50,
			APPLY_YOLK_TO_ORBSERVER = 51
		}

		public const int POISON_UNLOCK_AMOUNT = 40;

		public const int SLIME_HITS_IN_A_ROW_FOR_ACHIEVEMENT = 7;

		public static Dictionary<AchievementId, Achievement> Achievements = new Dictionary<AchievementId, Achievement>();

		public static void InitializeAchievements()
		{
			if (Achievements.Count <= 0)
			{
				Array values = Enum.GetValues(typeof(AchievementId));
				for (int i = 0; i < values.Length; i++)
				{
					AchievementId achievementId = (AchievementId)values.GetValue(i);
					Achievements.Add(achievementId, new Achievement(achievementId));
				}
				AchievementManager.Instance.SetupAchievementData();
			}
		}
	}
}
