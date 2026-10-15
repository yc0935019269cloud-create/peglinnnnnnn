using Peglin.Achievements;
using Peglin.ClassSystem;
using Saving;
using UnityEngine;

namespace ClassSystem
{
	[CreateAssetMenu(fileName = "ClassInfo", menuName = "Classes/ClassInfo")]
	public class ClassInfo : ScriptableObject
	{
		public Class characterClass;

		public bool startsUnlocked;

		public string classNameLocKey;

		public string classDescriptionLocKey;

		public string classRelicNameLocKey;

		public string classRelicDescriptionLocKey;

		public string classUnlockMethodLocKey;

		public Sprite classSprite;

		public Sprite classRelicIcon;

		public Sprite nameScrollImage;

		public Sprite lockedSprite;

		public Sprite tabSprite;

		public AchievementData.AchievementId unlockAchievementId = AchievementData.AchievementId.POISON_AMOUNT_ON_ENEMY;

		public bool ClassUnlocked()
		{
			if (startsUnlocked)
			{
				return true;
			}
			if (!AchievementData.Achievements[unlockAchievementId].Unlocked)
			{
				return PersistentPlayerData.Instance.IsClassUnlockedByProgress(characterClass);
			}
			return true;
		}
	}
}
