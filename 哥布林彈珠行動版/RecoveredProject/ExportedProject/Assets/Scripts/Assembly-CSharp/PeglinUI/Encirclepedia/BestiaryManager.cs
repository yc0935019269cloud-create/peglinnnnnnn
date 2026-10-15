using System.Collections.Generic;
using Battle.Enemies;
using Loading;
using PeglinUI.ControllerSupport;
using PeglinUI.LoadoutManager;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class BestiaryManager : MonoBehaviour
	{
		public class BestiaryHistorySaveData : SaveObjectData
		{
			public static string KEY = "BestiaryHistory";

			public EnemyData.BestiarySaveData bestiaryHistory;

			public override string Name => KEY;

			public BestiaryHistorySaveData(EnemyData.BestiarySaveData history)
				: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
			{
				bestiaryHistory = history;
			}
		}

		[Header("Scene Refs")]
		public GameObject leftPageContainer;

		public GameObject rightPageContainer;

		public BestiaryEnemyDetail viewBestiaryEntryOverlay;

		public ImageCarousel bestiaryCarousel;

		public GameObject closeButton;

		[Header("External references")]
		public GameObject bestiaryIconPrefab;

		private bool _initialized;

		private BestiaryHistorySaveData _bestiaryHistory;

		private Dictionary<string, EnemyData.EnemySaveData> _enemyHistoryTable = new Dictionary<string, EnemyData.EnemySaveData>();

		private GameObject _previouslySelectedEnemy;

		private int _currentEnemyIndex;

		private int _lowestVisibleEnemyIndex;

		private int _highestVisibleEnemyIndex;

		private void OnEnable()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (_initialized)
			{
				return;
			}
			LoadBestiaryHistory();
			List<GameObject> list = new List<GameObject>();
			_lowestVisibleEnemyIndex = AssetLoading.Instance.Bestiary.enemyDataList.Length - 1;
			for (int i = 0; i < AssetLoading.Instance.Bestiary.enemyDataList.Length; i++)
			{
				GameObject gameObject = ((i / (bestiaryCarousel.objectsPerPage / 2) % 2 == 0) ? leftPageContainer : rightPageContainer);
				BestiaryIcon component = Object.Instantiate(bestiaryIconPrefab, gameObject.transform).GetComponent<BestiaryIcon>();
				EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[i];
				if (enemyData.Seen && i < _lowestVisibleEnemyIndex)
				{
					_lowestVisibleEnemyIndex = i;
				}
				if (enemyData.Seen && i > _highestVisibleEnemyIndex)
				{
					_highestVisibleEnemyIndex = i;
				}
				component.Initialize(enemyData, this, i);
				list.Add(component.gameObject);
			}
			if (list.Count <= 0)
			{
				Debug.LogError("Bestiary list is somehow 0");
				return;
			}
			bestiaryCarousel.Initialize(list, autoNav: true);
			_initialized = true;
			if (!ControllerInputManager.IsPointerBased)
			{
				EventSystem.current.SetSelectedGameObject(list[0].GetComponentInChildren<Button>().gameObject);
			}
		}

		private void LoadBestiaryHistory()
		{
			_bestiaryHistory = (BestiaryHistorySaveData)DataSerializer.Load<SaveObjectData>(BestiaryHistorySaveData.KEY, DataSerializer.SaveType.BASE);
			_enemyHistoryTable.Clear();
			if (_bestiaryHistory == null)
			{
				CreateBlankBestiaryHistory();
			}
			else if (_bestiaryHistory.bestiaryHistory.EnemyData.Length < AssetLoading.Instance.Bestiary.enemyDataList.Length)
			{
				List<EnemyData.EnemySaveData> list = new List<EnemyData.EnemySaveData>(_bestiaryHistory.bestiaryHistory.EnemyData);
				EnemyData[] enemyDataList = AssetLoading.Instance.Bestiary.enemyDataList;
				foreach (EnemyData enemyData in enemyDataList)
				{
					bool flag = false;
					foreach (EnemyData.EnemySaveData item in list)
					{
						if (item.LocKey == enemyData.LocKey)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list.Add(new EnemyData.EnemySaveData(enemyData));
					}
				}
				_bestiaryHistory.bestiaryHistory.EnemyData = list.ToArray();
			}
			EnemyData.EnemySaveData[] enemyData2 = _bestiaryHistory.bestiaryHistory.EnemyData;
			foreach (EnemyData.EnemySaveData enemySaveData in enemyData2)
			{
				_enemyHistoryTable.Add(enemySaveData.LocKey, enemySaveData);
			}
			ClobberHistoryWithMemoryValues();
		}

		private void CreateBlankBestiaryHistory()
		{
			EnemyData.EnemySaveData[] array = new EnemyData.EnemySaveData[AssetLoading.Instance.Bestiary.enemyDataList.Length];
			for (int i = 0; i < AssetLoading.Instance.Bestiary.enemyDataList.Length; i++)
			{
				EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[i];
				array[i] = new EnemyData.EnemySaveData(enemyData.LocKey);
			}
			EnemyData.BestiarySaveData history = new EnemyData.BestiarySaveData(array);
			_bestiaryHistory = new BestiaryHistorySaveData(history);
		}

		private void ClobberHistoryWithMemoryValues()
		{
			Dictionary<string, EnemyData.EnemySaveData> dictionary = new Dictionary<string, EnemyData.EnemySaveData>();
			List<EnemyData.EnemySaveData> list = new List<EnemyData.EnemySaveData>();
			EnemyData[] enemyDataList = AssetLoading.Instance.Bestiary.enemyDataList;
			foreach (EnemyData enemyData in enemyDataList)
			{
				if (_enemyHistoryTable.TryGetValue(enemyData.LocKey, out var value))
				{
					dictionary.Add(enemyData.LocKey, value);
					list.Add(value);
				}
				else
				{
					EnemyData.EnemySaveData enemySaveData = new EnemyData.EnemySaveData(enemyData);
					dictionary.Add(enemyData.LocKey, enemySaveData);
					list.Add(enemySaveData);
				}
			}
			_enemyHistoryTable = dictionary;
			_bestiaryHistory = new BestiaryHistorySaveData(new EnemyData.BestiarySaveData(list.ToArray()));
		}

		private void SaveBestiaryHistory()
		{
			EnemyData.EnemySaveData[] enemyData = _bestiaryHistory.bestiaryHistory.EnemyData;
			foreach (EnemyData.EnemySaveData enemySaveData in enemyData)
			{
				if (_enemyHistoryTable.TryGetValue(enemySaveData.LocKey, out var value))
				{
					enemySaveData.Update(value);
				}
			}
			_bestiaryHistory.Save();
			SaveManager.SaveBase();
		}

		public void ViewEnemy(EnemyData enemyData, GameObject selectedGameObject, int index)
		{
			_currentEnemyIndex = index;
			bestiaryCarousel.downArrow.gameObject.SetActive(value: false);
			bestiaryCarousel.upArrow.gameObject.SetActive(value: false);
			leftPageContainer.SetActive(value: false);
			rightPageContainer.SetActive(value: false);
			_enemyHistoryTable.TryGetValue(enemyData.LocKey, out var value);
			viewBestiaryEntryOverlay.Initialize(enemyData, value, this);
			viewBestiaryEntryOverlay.gameObject.SetActive(value: true);
			viewBestiaryEntryOverlay.SetupArrows(index > _lowestVisibleEnemyIndex, index < _highestVisibleEnemyIndex);
			_previouslySelectedEnemy = selectedGameObject;
			closeButton.SetActive(value: false);
		}

		private void ChangeCurrentlyViewedEnemy(int index)
		{
			EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[index];
			_enemyHistoryTable.TryGetValue(enemyData.LocKey, out var value);
			viewBestiaryEntryOverlay.ChangeActiveEnemy(enemyData, value);
			viewBestiaryEntryOverlay.SetupArrows(index > _lowestVisibleEnemyIndex, index < _highestVisibleEnemyIndex);
			_previouslySelectedEnemy = bestiaryCarousel.GetElement(index);
		}

		public void ViewPreviousEnemy()
		{
			if (_currentEnemyIndex != _lowestVisibleEnemyIndex)
			{
				_currentEnemyIndex = Mathf.Clamp(_currentEnemyIndex - 1, _lowestVisibleEnemyIndex, _highestVisibleEnemyIndex);
				EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[_currentEnemyIndex];
				while (!enemyData.Seen)
				{
					_currentEnemyIndex = Mathf.Clamp(_currentEnemyIndex - 1, 0, _highestVisibleEnemyIndex);
					enemyData = AssetLoading.Instance.Bestiary.enemyDataList[_currentEnemyIndex];
				}
				ChangeCurrentlyViewedEnemy(_currentEnemyIndex);
			}
		}

		public void ViewNextEnemy()
		{
			if (_currentEnemyIndex != _highestVisibleEnemyIndex)
			{
				_currentEnemyIndex = Mathf.Clamp(_currentEnemyIndex + 1, _lowestVisibleEnemyIndex, _highestVisibleEnemyIndex);
				EnemyData enemyData = AssetLoading.Instance.Bestiary.enemyDataList[_currentEnemyIndex];
				while (!enemyData.Seen)
				{
					_currentEnemyIndex = Mathf.Clamp(_currentEnemyIndex + 1, _lowestVisibleEnemyIndex, _highestVisibleEnemyIndex);
					enemyData = AssetLoading.Instance.Bestiary.enemyDataList[_currentEnemyIndex];
				}
				ChangeCurrentlyViewedEnemy(_currentEnemyIndex);
			}
		}

		public void CloseOverlay()
		{
			bestiaryCarousel.ResolveArrowState();
			leftPageContainer.SetActive(value: true);
			rightPageContainer.SetActive(value: true);
			viewBestiaryEntryOverlay.gameObject.SetActive(value: false);
			closeButton.SetActive(value: true);
			bestiaryCarousel.ShowPageForElement(_currentEnemyIndex);
			if (_previouslySelectedEnemy != null && !ControllerInputManager.IsPointerBased)
			{
				EventSystem.current.SetSelectedGameObject(_previouslySelectedEnemy);
			}
			UpdateNewIcon();
		}

		private void UpdateNewIcon()
		{
			foreach (GameObject element in bestiaryCarousel.Elements)
			{
				element.GetComponent<BestiaryIcon>().UpdateNewIcon();
			}
		}

		private void OnDisable()
		{
			SaveBestiaryHistory();
		}
	}
}
