using System;
using System.Collections.Generic;
using Challenges;
using DG.Tweening;
using Loading;
using Map;
using Peglin.Achievements;
using PeglinUI.ControllerSupport;
using PeglinUI.LoadoutManager;
using Relics;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.RunSummary
{
	public class RunSummary : MonoBehaviour
	{
		[Header("Scene References")]
		public ImageCarousel orbsCarousel;

		public ImageCarousel relicsCarousel;

		public Transform orbList;

		public Transform relicList;

		public GameObject continueButton;

		public GameObject continueFirst;

		public ClassUnlockAlert newClassUnlockedInfo;

		public FadeCurtain fadeCurtain;

		public RunStatisticsDetails runStatisticDetails;

		public RectTransform menuBook;

		public TextMeshProUGUI versionText;

		public FirstWinOnEachClassManager firstWinOnEachClassGroup;

		public Image coreWinImage;

		[Header("External References")]
		public DeckManager deckManager;

		public RelicManager relicManager;

		public GameObject orbCapsulePrefab;

		public GameObject relicCapsulePrefab;

		private bool _isControllerActive;

		private bool _shouldFadeOut;

		private bool _inRunSummaryDetails;

		private void OnEnable()
		{
			versionText.text = "v" + Application.version;
			SetupOrbs();
			SetupRelics();
			if (PersistentPlayerData.Instance.UnlockedChallenges != null)
			{
				string text = "";
				foreach (ChallengeEffect unlockedChallenge in PersistentPlayerData.Instance.UnlockedChallenges)
				{
					text = text + unlockedChallenge.ToString() + ", ";
				}
				Debug.Log("Unlocked Challenges: " + text);
			}
			GameObject gameObject = GameObject.FindGameObjectWithTag("PersistentUI");
			if (gameObject != null)
			{
				gameObject.GetComponent<CanvasGroup>().alpha = 0f;
			}
			if (newClassUnlockedInfo != null)
			{
				newClassUnlockedInfo.SetClass();
				newClassUnlockedInfo.gameObject.SetActive(PersistentPlayerData.Instance.QueuedClassUnlocks.Count > 0);
			}
			PersistentPlayerData.Instance.QueuedOrbUnlocks.Clear();
			PersistentPlayerData.Instance.QueuedRelicUnlocks.Clear();
			PersistentPlayerData.Instance.QueuedClassUnlocks.Clear();
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.endDate = DateTime.Now;
				runStatisticDetails.Initialize(StaticGameData.CurrentRunStats);
				if (ChallengeManager.Instance == null || !ChallengeManager.Instance.beatenMinesBossAndCanGoToCore || (ChallengeManager.Instance.chosenToGoToCore && MapController.instance != null && MapController.instance.Act == 4))
				{
					AssetLoading.Instance.AddRunStats(StaticGameData.CurrentRunStats);
				}
			}
			if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.hasWon)
			{
				deckManager.CheckForTwoEggsAchievement();
				if (deckManager.CheckForLevelOneOrbsAchievement())
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.WIN_RUN_WITH_ONLY_LEVEL_ONE_ORBS);
				}
				if (!relicManager.RelicEffectActive(RelicEffect.LIFESTEAL_PEG_HITS) && StaticGameData.CurrentRunStats.vampireDealTaken)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.REFUSE_INGOT);
				}
				if (relicManager.GetRelicCount() <= 1)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.WIN_RUN_WITH_ONLY_ONE_RELIC);
				}
			}
			PersistentPlayerData.Instance.SavePersistentData(shouldSaveFile: true);
			_shouldFadeOut = true;
		}

		private void Update()
		{
			if (!ControllerInputManager.IsPointerBased && !_isControllerActive)
			{
				if (_inRunSummaryDetails)
				{
					EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
				}
				else if (firstWinOnEachClassGroup.panelActive)
				{
					EventSystem.current.SetSelectedGameObject(firstWinOnEachClassGroup.dismissButton.gameObject);
				}
				else
				{
					EventSystem.current.SetSelectedGameObject(continueFirst.gameObject);
				}
				_isControllerActive = true;
			}
			else if (ControllerInputManager.IsPointerBased && _isControllerActive)
			{
				EventSystem.current.SetSelectedGameObject(null);
				_isControllerActive = false;
			}
		}

		private void Start()
		{
			if (_shouldFadeOut)
			{
				fadeCurtain.FadeOut();
			}
			else
			{
				fadeCurtain.SetBlack();
			}
		}

		private bool SetupRelics()
		{
			List<Relic> relicsForEffects = relicManager.GetRelicsForEffects(PersistentPlayerData.Instance.QueuedRelicUnlocks);
			if (relicsForEffects.Count > 0)
			{
				List<GameObject> list = new List<GameObject>();
				foreach (Relic item in relicsForEffects)
				{
					LoadoutIcon componentInChildren = UnityEngine.Object.Instantiate(relicCapsulePrefab, relicList).GetComponentInChildren<LoadoutIcon>();
					componentInChildren.InitializeRelic(item);
					componentInChildren.SetupTooltipEventsRelic();
					list.Add(componentInChildren.gameObject);
				}
				relicsCarousel.Initialize(list);
				return true;
			}
			relicsCarousel.gameObject.SetActive(value: false);
			return false;
		}

		private bool SetupOrbs()
		{
			List<GameObject> orbPrefabs = deckManager.GetOrbPrefabs(PersistentPlayerData.Instance.QueuedOrbUnlocks);
			if (orbPrefabs.Count > 0)
			{
				List<GameObject> list = new List<GameObject>();
				foreach (GameObject item in orbPrefabs)
				{
					LoadoutIcon componentInChildren = UnityEngine.Object.Instantiate(orbCapsulePrefab, orbList).GetComponentInChildren<LoadoutIcon>();
					componentInChildren.InitializeOrb(item, 0);
					componentInChildren.SetupTooltipEventsOrb();
					list.Add(componentInChildren.gameObject);
				}
				orbsCarousel.Initialize(list);
				return true;
			}
			orbsCarousel.gameObject.SetActive(value: false);
			return false;
		}

		public void LoadMainMenu()
		{
			PeglinSceneLoader.Instance.LoadScene(PeglinSceneLoader.Scene.MAIN_MENU);
		}

		public void MoveToRunSummaryDetails()
		{
			_inRunSummaryDetails = true;
			menuBook.DOLocalMoveX(-250f, 1.5f).SetEase(Ease.OutCubic);
			coreWinImage.DOColor(Color.black, 1.5f);
			if (_isControllerActive)
			{
				EventSystem.current.SetSelectedGameObject(continueButton);
			}
		}
	}
}
