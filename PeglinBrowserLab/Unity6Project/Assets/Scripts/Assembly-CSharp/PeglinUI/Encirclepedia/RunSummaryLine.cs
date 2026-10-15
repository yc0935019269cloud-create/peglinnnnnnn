using System.Globalization;
using ClassSystem;
using I2.Loc;
using PeglinUI.MainMenu.Cruciball;
using PeglinUI.RunSummary;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class RunSummaryLine : MonoBehaviour
	{
		public TextMeshProUGUI headerText;

		public Image act4WinIcon;

		public TextMeshProUGUI dateTimeText;

		public TextMeshProUGUI customGameText;

		public Image classImage;

		public CruciballDisplay cruciballDisplay;

		public SeedDisplay seedDisplay;

		public Image backgroundImage;

		public Sprite lossSprite;

		public ChallengesDisplay challengesDisplay;

		private RunStatisticsDetails _details;

		private RunStats _stats;

		private RunSummaryList _parent;

		public void Initialize(RunStats stats, ClassInfo classInfo, RunStatisticsDetails details, RunSummaryList parent)
		{
			string text = (stats.hasWon ? LocalizationManager.GetTranslation("Menu/RunSummary/win_label") : LocalizationManager.GetTranslation("Menu/RunSummary/loss_label"));
			headerText.text = LocalizationManager.GetTranslation(classInfo.classNameLocKey) + " - " + text;
			act4WinIcon.enabled = false;
			if (stats.hasWonCore)
			{
				act4WinIcon.enabled = true;
			}
			dateTimeText.text = stats.endDate.ToString(CultureInfo.CurrentCulture);
			customGameText.gameObject.SetActive(stats.isCustomRun);
			classImage.sprite = classInfo.classSprite;
			challengesDisplay.SetChallenges(stats);
			if (stats.cruciballLevel > 0)
			{
				cruciballDisplay.Initialize(stats.cruciballLevel);
			}
			else
			{
				cruciballDisplay.gameObject.SetActive(value: false);
			}
			_details = details;
			_stats = stats;
			if (!stats.hasWon)
			{
				backgroundImage.sprite = lossSprite;
			}
			GetComponentInChildren<Button>().interactable = true;
			_parent = parent;
		}

		public void ShowLineDetails()
		{
			_parent.SetPreviouslySelected(this);
			_details.Initialize(_stats);
		}
	}
}
