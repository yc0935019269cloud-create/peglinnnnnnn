using System;
using Battle;
using UnityEngine;

namespace Peglin.Achievements
{
	public class UnlockAchievementOnBattleWin : MonoBehaviour
	{
		public AchievementData.AchievementId AchievementToUnlock;

		public void OnEnable()
		{
			BattleController.OnVictory = (Action)Delegate.Combine(BattleController.OnVictory, new Action(UnlockAchievement));
		}

		private void UnlockAchievement()
		{
			AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementToUnlock]);
		}

		private void OnDisable()
		{
			BattleController.OnVictory = (Action)Delegate.Remove(BattleController.OnVictory, new Action(UnlockAchievement));
		}
	}
}
