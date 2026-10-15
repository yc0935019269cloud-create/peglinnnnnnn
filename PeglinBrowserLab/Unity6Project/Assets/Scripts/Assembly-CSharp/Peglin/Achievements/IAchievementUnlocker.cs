namespace Peglin.Achievements
{
	public interface IAchievementUnlocker
	{
		void SetupAchievementData();

		void UnlockAchievement(Achievement a);
	}
}
