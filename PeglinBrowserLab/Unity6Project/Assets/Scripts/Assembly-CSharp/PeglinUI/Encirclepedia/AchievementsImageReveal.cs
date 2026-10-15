using Peglin.Achievements;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class AchievementsImageReveal : MonoBehaviour
	{
		public Image[] AchievementArt;

		private void OnEnable()
		{
			int numberOfAchievementsUnlocked = AchievementManager.Instance.GetNumberOfAchievementsUnlocked();
			Image[] achievementArt = AchievementArt;
			for (int i = 0; i < achievementArt.Length; i++)
			{
				achievementArt[i].gameObject.SetActive(value: false);
			}
			for (int j = 0; j < numberOfAchievementsUnlocked / 4; j++)
			{
				if (AchievementArt.Length > j)
				{
					AchievementArt[j].gameObject.SetActive(value: true);
				}
			}
		}
	}
}
