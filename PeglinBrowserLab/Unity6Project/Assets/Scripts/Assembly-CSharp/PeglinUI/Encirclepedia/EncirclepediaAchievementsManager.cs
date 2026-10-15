using System.Collections.Generic;
using Achievements;
using Peglin.Achievements;
using PeglinUI.LoadoutManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace PeglinUI.Encirclepedia
{
	public class EncirclepediaAchievementsManager : MonoBehaviour
	{
		[Header("Scene Refs")]
		public GameObject leftPageContainer;

		public GameObject rightPageContainer;

		[FormerlySerializedAs("achievementIcon")]
		[Header("External references")]
		public GameObject achievementIconPrefab;

		public AchievementDatabase achDb;

		public AchievementIconBig viewAchievementOverlay;

		private ImageCarousel _achievementsCarousel;

		private void OnEnable()
		{
			Initialize();
		}

		private void Initialize()
		{
			_achievementsCarousel = GetComponent<ImageCarousel>();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < achDb.achievementList.Count; i++)
			{
				GameObject gameObject = ((i / (_achievementsCarousel.objectsPerPage / 2) % 2 == 0) ? leftPageContainer : rightPageContainer);
				AchievementIcon component = Object.Instantiate(achievementIconPrefab, gameObject.transform).GetComponent<AchievementIcon>();
				component.Initialize(achDb.GetAchievement(achDb.achievementList[i].Id), this);
				list.Add(component.gameObject);
			}
			_achievementsCarousel.Initialize(list, autoNav: true);
			_achievementsCarousel.SelectFirstVisibleObject();
		}

		public void ViewAchievement(Achievement achievement)
		{
			viewAchievementOverlay.Initialize(achievement, EventSystem.current.currentSelectedGameObject);
			viewAchievementOverlay.gameObject.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
