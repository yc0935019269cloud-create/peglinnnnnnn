using Cruciball;
using I2.Loc;
using Peglin.Achievements;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class CruciballWinText : MonoBehaviour
	{
		public CruciballManager cruciballManager;

		public Button restartButton;

		private LocalizationParamsManager _locParams;

		private void OnEnable()
		{
			if (!AchievementManager.AchievementsOn)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			_locParams = GetComponent<LocalizationParamsManager>();
			if (cruciballManager.CruciballVictoryThisRun && !cruciballManager.AtMaxCruciballLevel())
			{
				_locParams.SetParameterValue("CRUCIBALL_LEVEL", (cruciballManager.currentCruciballLevel + 1).ToString());
				if (restartButton != null)
				{
					restartButton.gameObject.SetActive(value: false);
				}
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
