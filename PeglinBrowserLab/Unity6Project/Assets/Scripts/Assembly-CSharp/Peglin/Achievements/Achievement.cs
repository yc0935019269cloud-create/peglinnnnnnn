using System;
using I2.Loc;
using UnityEngine;

namespace Peglin.Achievements
{
	[Serializable]
	public class Achievement
	{
		public AchievementData.AchievementId Id;

		private static readonly string LOC_PREFIX = "Achievements/NEW_ACHIEVEMENT_16_";

		private static readonly string LOC_NAME_SUFFIX = "_NAME";

		private static readonly string LOC_DESC_SUFFIX = "_DESC";

		public bool Unlocked;

		public Sprite UnlockedImage;

		public Sprite LockedImage;

		public string Name => LocalizationManager.GetTranslation($"{LOC_PREFIX}{(int)Id}{LOC_NAME_SUFFIX}");

		public string Description => LocalizationManager.GetTranslation($"{LOC_PREFIX}{(int)Id}{LOC_DESC_SUFFIX}");

		public Achievement(AchievementData.AchievementId id)
		{
			Id = id;
			Unlocked = false;
		}
	}
}
