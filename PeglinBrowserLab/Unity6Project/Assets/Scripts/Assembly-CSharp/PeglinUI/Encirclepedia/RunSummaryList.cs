using System;
using System.Collections.Generic;
using ClassSystem;
using DG.Tweening;
using Loading;
using PeglinUI.ControllerSupport;
using PeglinUI.RunSummary;
using Stats;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class RunSummaryList : MonoBehaviour
	{
		public GameObject runSummaryLinePrefab;

		public Transform runSummaryLineContainer;

		public RunStatisticsDetails runStatsDetails;

		public RectTransform menuBook;

		public Button moveCameraButton;

		public TextMeshProUGUI moveCameraText;

		public ControllerIconMapping rightPageMapping;

		public ControllerIconMapping leftPageMapping;

		public ButtonGraphicMapper buttonGraphicMapper;

		private Tween _tween;

		private bool _tweenPlaying;

		private readonly string MOVE_RIGHT_ARROW = ">";

		private readonly string MOVE_LEFT_ARROW = "<";

		public ClassInfo[] classInfos;

		private Button _previouslySelected;

		private bool _isOnLoadout;

		public void Awake()
		{
			if (AssetLoading.Instance.RunStats.Count == 0)
			{
				AssetLoading.Instance.LoadRunStats();
			}
			RunStats.RunStatsSaveData[] array = AssetLoading.Instance.RunStats.ToArray();
			List<Button> list = new List<Button>();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				RunStats runStats = new RunStats(array[num]);
				RunSummaryLine component = UnityEngine.Object.Instantiate(runSummaryLinePrefab, runSummaryLineContainer).GetComponent<RunSummaryLine>();
				if (classInfos.Length - 1 < (int)runStats.selectedClass)
				{
					Debug.LogError($"Index out of bounds for class {runStats.selectedClass}");
				}
				else
				{
					component.Initialize(runStats, classInfos[(int)runStats.selectedClass], runStatsDetails, this);
					list.Add(component.GetComponentInChildren<Button>());
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (list.Count > i + 1)
				{
					navigation.selectOnDown = list[i + 1];
				}
				if (i > 0)
				{
					navigation.selectOnUp = list[i - 1];
				}
				list[i].navigation = navigation;
			}
			EventSystem.current.SetSelectedGameObject(list[0].gameObject);
			moveCameraButton.onClick.AddListener(MoveCameraToLoadout);
		}

		public void MoveCameraToLoadout()
		{
			if (_tween != null && _tweenPlaying)
			{
				_tween.Kill();
			}
			_isOnLoadout = true;
			_tween = menuBook.DOLocalMoveX(-250f, 0.75f).SetEase(Ease.OutCubic);
			_tweenPlaying = true;
			Tween tween = _tween;
			tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, (TweenCallback)(() =>
			{
				moveCameraText.text = MOVE_LEFT_ARROW;
				moveCameraButton.onClick.RemoveListener(MoveCameraToLoadout);
				moveCameraButton.onClick.AddListener(MoveCameraBack);
				buttonGraphicMapper.SetIconMapping(leftPageMapping);
				moveCameraButton.GetComponent<MapButtonToInput>()._rewiredAction = "MenuTabLeft";
				EventSystem.current.SetSelectedGameObject(null);
				_tweenPlaying = false;
			}));
		}

		public void MoveCameraBack()
		{
			if (_tween != null && _tweenPlaying)
			{
				_tween.Kill();
			}
			EventSystem.current.SetSelectedGameObject(_previouslySelected?.gameObject);
			_isOnLoadout = false;
			_tween = menuBook.DOLocalMoveX(0f, 0.75f).SetEase(Ease.OutCubic);
			_tweenPlaying = true;
			Tween tween = _tween;
			tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, (TweenCallback)(() =>
			{
				moveCameraText.text = MOVE_RIGHT_ARROW;
				moveCameraButton.onClick.RemoveListener(MoveCameraBack);
				moveCameraButton.onClick.AddListener(MoveCameraToLoadout);
				buttonGraphicMapper.SetIconMapping(rightPageMapping);
				moveCameraButton.GetComponent<MapButtonToInput>()._rewiredAction = "MenuTabRight";
				_tweenPlaying = false;
			}));
		}

		private void OnDisable()
		{
			if (_tween != null && _tweenPlaying)
			{
				_tween.Kill();
			}
			if (_isOnLoadout)
			{
				_isOnLoadout = false;
				_tween = menuBook.DOLocalMoveX(0f, 0.75f).SetEase(Ease.OutCubic);
				_tweenPlaying = true;
				Tween tween = _tween;
				tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, (TweenCallback)(() =>
				{
					moveCameraText.text = MOVE_RIGHT_ARROW;
					moveCameraButton.onClick.RemoveListener(MoveCameraBack);
					moveCameraButton.onClick.AddListener(MoveCameraToLoadout);
					buttonGraphicMapper.SetIconMapping(rightPageMapping);
					moveCameraButton.GetComponent<MapButtonToInput>()._rewiredAction = "MenuTabRight";
					_tweenPlaying = false;
				}));
			}
		}

		public void SetPreviouslySelected(RunSummaryLine runSummaryLine)
		{
			_previouslySelected = runSummaryLine.GetComponentInChildren<Button>();
		}
	}
}
