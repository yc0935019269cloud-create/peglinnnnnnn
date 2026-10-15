using Coffee.UIEffects;
using Peglin.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class AchievementIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDeselectHandler, ISelectHandler
	{
		[Header("Scene Refs")]
		public Image image;

		public GameObject nameRibbon;

		public TextMeshProUGUI nameText;

		public UIEffect achievementGrayscale;

		public UIEffect frameGrayscale;

		private Achievement _achievement;

		public void Initialize(Achievement achievement, EncirclepediaAchievementsManager manager)
		{
			_achievement = achievement;
			image.sprite = (achievement.Unlocked ? achievement.UnlockedImage : achievement.LockedImage);
			nameText.text = achievement.Name;
			UIEffect uIEffect = frameGrayscale;
			bool flag = (achievementGrayscale.enabled = !achievement.Unlocked);
			uIEffect.enabled = flag;
			GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				manager.ViewAchievement(_achievement);
			});
		}

		private void OnDestroy()
		{
			GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			nameRibbon.SetActive(value: true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			nameRibbon.SetActive(value: false);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			nameRibbon.SetActive(value: false);
		}

		public void OnSelect(BaseEventData eventData)
		{
			nameRibbon.SetActive(value: true);
		}
	}
}
