using Coffee.UIEffects;
using Peglin.Achievements;
using PeglinUI.ControllerSupport;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class AchievementIconBig : MonoBehaviour
	{
		[Header("Scene Refs")]
		public TextMeshProUGUI nameText;

		public TextMeshProUGUI descriptionText;

		public Image image;

		public UIEffect achievementGrayscale;

		public UIEffect frameGrayscale;

		public UIEffect titleGrayscale;

		public MapButtonToInput mainCloseButton;

		public Image checkMark;

		private GameObject _previouslySelected;

		private void OnEnable()
		{
			mainCloseButton.enabled = false;
		}

		private void OnDisable()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				EventSystem.current.SetSelectedGameObject(_previouslySelected);
			}
			mainCloseButton.enabled = true;
		}

		public void Initialize(Achievement achievement, GameObject previouslySelected)
		{
			nameText.text = achievement.Name;
			descriptionText.text = achievement.Description;
			image.sprite = achievement.UnlockedImage;
			_previouslySelected = previouslySelected;
			UIEffect uIEffect = frameGrayscale;
			UIEffect uIEffect2 = achievementGrayscale;
			bool flag = (titleGrayscale.enabled = !achievement.Unlocked);
			bool flag3 = (uIEffect2.enabled = flag);
			uIEffect.enabled = flag3;
			checkMark.enabled = achievement.Unlocked;
		}
	}
}
