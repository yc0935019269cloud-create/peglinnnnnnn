using System;
using Battle.Enemies;
using DG.Tweening;
using Loading;
using Peglin.Achievements;
using PeglinUI.ControllerSupport;
using Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class EncirclepediaManager : MonoBehaviour
	{
		[Header("Scene Refs")]
		public Camera cam;

		[FormerlySerializedAs("encirclopediaPosition")]
		public Transform encirclepediaPosition;

		[FormerlySerializedAs("encirclopedia")]
		public GameObject encirclepedia;

		public CanvasGroup mainCanvasGroup;

		public GameObject playButton;

		public GameObject menu;

		public GameObject closeButton;

		public GameObject achievementsParent;

		public GameObject overallProgressContainer;

		public GameObject runSummaryListParent;

		public GameObject buttonParent;

		public GameObject bestiary;

		public OverallProgressManager overallProgressManager;

		public MapButtonToInput closeButtonInputMapper;

		public Selectable firstButton;

		public Button achievementsButton;

		public Button runSummaryList;

		public Button bestiaryButton;

		public Button backButton;

		[Header("Parameters")]
		public AudioClip encirclepediaBgm;

		private Vector2 _originalPosition;

		private float _originalEncirclopediaPosition;

		private bool _callbackSet;

		private bool _encirclepediaActive;

		private void OnEnable()
		{
			if (!PersistentPlayerData.Instance.AchievementsLoaded)
			{
				PersistentPlayerData.OnAchievementsLoaded = (PersistentPlayerData.PersistentPlayerDataEvent)Delegate.Combine(PersistentPlayerData.OnAchievementsLoaded, new PersistentPlayerData.PersistentPlayerDataEvent(LoadEncirclepedia));
				_callbackSet = true;
			}
			else
			{
				LoadEncirclepedia();
			}
		}

		private void OnDisable()
		{
			if (_callbackSet)
			{
				PersistentPlayerData.OnAchievementsLoaded = (PersistentPlayerData.PersistentPlayerDataEvent)Delegate.Remove(PersistentPlayerData.OnAchievementsLoaded, new PersistentPlayerData.PersistentPlayerDataEvent(LoadEncirclepedia));
				_callbackSet = false;
			}
		}

		public void LoadEncirclepedia()
		{
			overallProgressManager.Initialize();
			runSummaryList.gameObject.SetActive(AssetLoading.Instance.RunStats.Count > 0);
			if (AssetLoading.Instance.RunStats.Count <= 0)
			{
				bestiaryButton.navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnDown = backButton,
					selectOnUp = achievementsButton
				};
				backButton.navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnDown = achievementsButton,
					selectOnUp = bestiaryButton
				};
			}
			CheckBestiaryAchievements();
		}

		public void ShowEncirclopedia()
		{
			BGMController.instance.SetMainMenuAudioClip(encirclepediaBgm);
			Button[] componentsInChildren = buttonParent.GetComponentsInChildren<Button>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].interactable = false;
			}
			_originalPosition = cam.transform.position;
			_originalEncirclopediaPosition = encirclepedia.transform.localPosition.y;
			overallProgressManager.PopulateFieldsDefault();
			overallProgressManager.IsActive(value: true);
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = sequence.Append(mainCanvasGroup.DOFade(0f, 0.65f)).Append(cam.transform.DOMoveX(encirclepediaPosition.position.x, 0.85f)).AppendInterval(0.085f)
				.Append(encirclepedia.transform.DOLocalMoveY(0f, 0.5f));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, (TweenCallback)(() =>
			{
				if (!ControllerInputManager.IsPointerBased)
				{
					EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
				}
				closeButtonInputMapper.enabled = true;
				_encirclepediaActive = true;
			}));
			sequence.Play();
		}

		public void HideEncirclopedia()
		{
			if (!_encirclepediaActive)
			{
				return;
			}
			BGMController.instance.SetMainMenuAudioClip(BGMController.instance.mainMenuAudioClip);
			closeButton.SetActive(value: false);
			overallProgressManager.IsActive(value: false);
			Sequence sequence = DOTween.Sequence();
			sequence.Append(encirclepedia.transform.DOLocalMoveY(_originalEncirclopediaPosition, 0.5f)).AppendInterval(0.085f).Append(cam.transform.DOMoveX(_originalPosition.x, 0.85f))
				.Append(mainCanvasGroup.DOFade(1f, 0.65f))
				.AppendCallback(() =>
				{
					achievementsParent.SetActive(value: false);
					EventSystem.current.SetSelectedGameObject(playButton);
					Button[] componentsInChildren = buttonParent.GetComponentsInChildren<Button>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].interactable = true;
					}
				});
			sequence.Play();
		}

		public void TurnOffEverything()
		{
			overallProgressContainer.SetActive(value: false);
			menu.SetActive(value: false);
			runSummaryListParent.SetActive(value: false);
			achievementsParent.SetActive(value: false);
			closeButton.SetActive(value: false);
			bestiary.SetActive(value: false);
			closeButtonInputMapper.enabled = false;
		}

		public void ShowAchievements()
		{
			TurnOffEverything();
			achievementsParent.SetActive(value: true);
			closeButton.SetActive(value: true);
		}

		public void ShowMenu()
		{
			TurnOffEverything();
			menu.SetActive(value: true);
			overallProgressContainer.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
			closeButtonInputMapper.enabled = true;
		}

		public void ShowRunSummaryList()
		{
			TurnOffEverything();
			runSummaryListParent.SetActive(value: true);
			closeButton.SetActive(value: true);
		}

		public void ShowBestiary()
		{
			TurnOffEverything();
			bestiary.SetActive(value: true);
			closeButton.SetActive(value: true);
		}

		private void CheckBestiaryAchievements()
		{
			if (CheckFullyUnlockedMonster())
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.FULLY_UNLOCK_MONSTER_IN_BESTIARY);
			}
			if (CheckCardDropObtained())
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.CARD_DROP_OBTAINED);
			}
		}

		private bool CheckCardDropObtained()
		{
			if (!AchievementData.Achievements[AchievementData.AchievementId.CARD_DROP_OBTAINED].Unlocked)
			{
				EnemyData[] enemyDataList = AssetLoading.Instance.Bestiary.enemyDataList;
				for (int i = 0; i < enemyDataList.Length; i++)
				{
					if (enemyDataList[i].CardDrop > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool CheckFullyUnlockedMonster()
		{
			if (!AchievementData.Achievements[AchievementData.AchievementId.FULLY_UNLOCK_MONSTER_IN_BESTIARY].Unlocked)
			{
				EnemyData[] enemyDataList = AssetLoading.Instance.Bestiary.enemyDataList;
				foreach (EnemyData enemyData in enemyDataList)
				{
					if (enemyData.ResearchProgress >= enemyData.ResearchPointTotal)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
