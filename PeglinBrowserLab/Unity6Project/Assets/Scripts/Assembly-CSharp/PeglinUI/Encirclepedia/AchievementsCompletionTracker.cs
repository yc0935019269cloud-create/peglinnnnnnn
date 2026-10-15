using Achievements;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;

namespace PeglinUI.Encirclepedia
{
	public class AchievementsCompletionTracker : MonoBehaviour
	{
		public AchievementDatabase database;

		public UIEffect backgroundGrayscale;

		private TextMeshProUGUI text;

		private void OnEnable()
		{
			UpdateInfo();
		}

		public void UpdateInfo()
		{
			if (text == null)
			{
				text = GetComponent<TextMeshProUGUI>();
			}
			(int, int) progression = database.GetProgression();
			text.text = $"{progression.Item1}/{progression.Item2}";
			backgroundGrayscale.enabled = progression.Item1 < progression.Item2;
		}
	}
}
