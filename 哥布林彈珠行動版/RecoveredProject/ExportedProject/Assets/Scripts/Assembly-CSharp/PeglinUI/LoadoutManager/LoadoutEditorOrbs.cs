using System;
using System.Collections.Generic;
using I2.Loc;
using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutEditorOrbs : MonoBehaviour
	{
		public delegate void ButtonClicked();

		public ButtonClicked OnAcceptButtonClicked;

		public ButtonClicked OnCancelButtonClicked;

		public List<LoadoutOrbData> selectedStartingOrbs;

		[Header("Scene refs")]
		public GameObject orbDeckContainer;

		public GameObject orbCatalogContainer;

		public ImageCarousel startingOrbsCarousel;

		public ImageCarousel orbCatalogCarousel;

		public GameObject contextButtons;

		[Header("Prefabs")]
		public GameObject orbDeckItemPrefab;

		public GameObject orbCatalogIconPrefab;

		public GameObject orbInfoWidget;

		private Canvas _parentCanvas;

		private SelectableObjectContainer _selectableObjectContainer;

		private RewiredEventSystem _rewiredEventSystem;

		private List<Selectable> _items = new List<Selectable>();

		private Vector2 _anchorPoint = new Vector2(1f, 0f);

		private void Awake()
		{
			if (_rewiredEventSystem == null)
			{
				_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			}
		}

		private void OnEnable()
		{
			LoadoutEditorOrbItem.OnOrbItemSelected = (LoadoutEditorOrbItem.OrbItemSelected)Delegate.Combine(LoadoutEditorOrbItem.OnOrbItemSelected, new LoadoutEditorOrbItem.OrbItemSelected(SetItemIsSelected));
		}

		private void SetItemIsSelected(bool itemIsSelected)
		{
			if (itemIsSelected && !ControllerInputManager.IsPointerBased)
			{
				contextButtons.gameObject.SetActive(value: true);
			}
			else
			{
				contextButtons.gameObject.SetActive(value: false);
			}
		}

		private void OnDisable()
		{
			LoadoutEditorOrbItem.OnOrbItemSelected = (LoadoutEditorOrbItem.OrbItemSelected)Delegate.Remove(LoadoutEditorOrbItem.OnOrbItemSelected, new LoadoutEditorOrbItem.OrbItemSelected(SetItemIsSelected));
			TooltipManager.Instance.HideTooltip();
			contextButtons.gameObject.SetActive(value: false);
		}

		public void Initialize(List<LoadoutOrbData> startingOrbs, DeckManager deckManager)
		{
			CreateOrbCatalog(deckManager);
			_items.Clear();
			if (_selectableObjectContainer == null)
			{
				_selectableObjectContainer = orbDeckContainer.GetComponentInChildren<SelectableObjectContainer>();
			}
			if (orbCatalogCarousel == null)
			{
				orbCatalogCarousel = GetComponentInChildren<ImageCarousel>();
			}
			foreach (Transform item in orbDeckContainer.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			selectedStartingOrbs = startingOrbs;
			CreateStartingOrbs(startingOrbs);
			if (_items.Count > 0)
			{
				_selectableObjectContainer.selectableObject = _items[0].gameObject;
			}
			RebuildItemNavigation(_items);
		}

		private void CreateOrbCatalog(DeckManager deckManager)
		{
			orbCatalogCarousel.ClearCategories();
			List<GameObject> icons = CreateIconsFromOrbPool(deckManager.CommonOrbPool);
			List<GameObject> icons2 = CreateIconsFromOrbPool(deckManager.UncommonOrbPool);
			List<GameObject> icons3 = CreateIconsFromOrbPool(deckManager.RareOrbPool);
			List<GameObject> icons4 = CreateIconsFromOrbPool(deckManager.SpecialOrbPool);
			List<GameObject> icons5 = CreateIconsFromOrbPool(deckManager.UnavailableOrbPool);
			orbCatalogCarousel.CreateCategory(icons, LocalizationManager.GetTranslation("Menu/common_orbs_label"));
			orbCatalogCarousel.CreateCategory(icons2, LocalizationManager.GetTranslation("Menu/uncommon_orbs_label"));
			orbCatalogCarousel.CreateCategory(icons3, LocalizationManager.GetTranslation("Menu/rare_orbs_label"));
			orbCatalogCarousel.CreateCategory(icons4, LocalizationManager.GetTranslation("Menu/scenario_only_orbs_label"));
			orbCatalogCarousel.CreateCategory(icons5, LocalizationManager.GetTranslation("Menu/unavailable_orbs_label"));
			orbCatalogCarousel.BuildNavigationPreprocessedPages();
			orbCatalogCarousel.SelectFirstVisibleObject();
		}

		private List<GameObject> CreateIconsFromOrbPool(List<GameObject> orbPool)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item in orbPool)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(orbCatalogIconPrefab, orbCatalogContainer.transform);
				LoadoutIcon componentInChildren = gameObject.GetComponentInChildren<LoadoutIcon>();
				componentInChildren.InitializeOrb(item, 0);
				componentInChildren.OnOrbIconClicked = (LoadoutIcon.OrbIconClicked)Delegate.Combine(componentInChildren.OnOrbIconClicked, new LoadoutIcon.OrbIconClicked(AddToStartingOrbs));
				componentInChildren.SetupTooltipEventsOrb();
				list.Add(gameObject);
				gameObject.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
			}
			return list;
		}

		private void CreateStartingOrbs(List<LoadoutOrbData> startingOrbs)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (LoadoutOrbData startingOrb in startingOrbs)
			{
				LoadoutEditorOrbItem component = UnityEngine.Object.Instantiate(orbDeckItemPrefab, orbDeckContainer.transform).GetComponent<LoadoutEditorOrbItem>();
				if (_rewiredEventSystem == null)
				{
					_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
				}
				component.gameObject.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
				_items.Add(component.gameObject.GetComponent<Selectable>());
				component.Initialize(startingOrb);
				component.OnDeleteButtonClicked = (LoadoutEditorOrbItem.DeleteButtonClicked)Delegate.Combine(component.OnDeleteButtonClicked, new LoadoutEditorOrbItem.DeleteButtonClicked(DeleteOrbItem));
				if (startingOrb.CanBeLeveledUp)
				{
					component.OnCycleLevelClicked = (LoadoutEditorOrbItem.OrbLevelCycled)Delegate.Combine(component.OnCycleLevelClicked, new LoadoutEditorOrbItem.OrbLevelCycled(CycleLevels));
				}
				list.Add(component.gameObject);
			}
			startingOrbsCarousel.Initialize(list);
		}

		private void CycleLevels(LoadoutEditorOrbItem item)
		{
			int num = item.OrbData.Level + 1;
			bool[] levelsTaken = GetLevelsTaken(item);
			if (num > item.OrbData.MaxLevel)
			{
				for (int i = 0; i < levelsTaken.Length; i++)
				{
					if (!levelsTaken[i])
					{
						item.OrbData.Level = i + 1;
						break;
					}
				}
				return;
			}
			for (int j = num - 1; j < levelsTaken.Length; j++)
			{
				if (!levelsTaken[j])
				{
					item.OrbData.Level = j + 1;
					return;
				}
			}
			for (int k = 0; k < levelsTaken.Length; k++)
			{
				if (!levelsTaken[k])
				{
					item.OrbData.Level = k + 1;
					break;
				}
			}
		}

		private int GetMinAvailableLevel(LoadoutEditorOrbItem item)
		{
			bool[] levelsTaken = GetLevelsTaken(item);
			for (int i = 0; i < levelsTaken.Length; i++)
			{
				if (!levelsTaken[i])
				{
					return i + 1;
				}
			}
			return -1;
		}

		private bool[] GetLevelsTaken(LoadoutEditorOrbItem item)
		{
			bool[] array = new bool[item.OrbData.MaxLevel];
			foreach (LoadoutOrbData selectedStartingOrb in selectedStartingOrbs)
			{
				if (!(selectedStartingOrb.orbName != item.OrbData.orbName))
				{
					array[selectedStartingOrb.Level - 1] = true;
				}
			}
			return array;
		}

		public void SaveOrbLoadout()
		{
			OnAcceptButtonClicked?.Invoke();
		}

		public void Cancel()
		{
			OnCancelButtonClicked?.Invoke();
		}

		private void AddToStartingOrbs(GameObject prefab)
		{
			foreach (LoadoutOrbData selectedStartingOrb in selectedStartingOrbs)
			{
				if (selectedStartingOrb.orbPrefab.name == prefab.name && (selectedStartingOrb.Level == 1 || !selectedStartingOrb.CanBeLeveledUp))
				{
					if (selectedStartingOrb.Amount < 99)
					{
						selectedStartingOrb.Amount++;
					}
					return;
				}
			}
			LoadoutEditorOrbItem component = UnityEngine.Object.Instantiate(orbDeckItemPrefab, orbDeckContainer.transform).GetComponent<LoadoutEditorOrbItem>();
			LoadoutOrbData loadoutOrbData = new LoadoutOrbData(prefab, 1);
			component.Initialize(loadoutOrbData);
			component.OnDeleteButtonClicked = (LoadoutEditorOrbItem.DeleteButtonClicked)Delegate.Combine(component.OnDeleteButtonClicked, new LoadoutEditorOrbItem.DeleteButtonClicked(DeleteOrbItem));
			if (loadoutOrbData.CanBeLeveledUp)
			{
				component.OnCycleLevelClicked = (LoadoutEditorOrbItem.OrbLevelCycled)Delegate.Combine(component.OnCycleLevelClicked, new LoadoutEditorOrbItem.OrbLevelCycled(CycleLevels));
			}
			component.gameObject.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
			_items.Add(component.gameObject.GetComponent<Selectable>());
			selectedStartingOrbs.Add(loadoutOrbData);
			if (_items.Count > 0)
			{
				_selectableObjectContainer.selectableObject = _items[0].gameObject;
			}
			startingOrbsCarousel.AddElement(component.gameObject);
		}

		private void DeleteOrbItem(LoadoutEditorOrbItem item)
		{
			item.OnDeleteButtonClicked = (LoadoutEditorOrbItem.DeleteButtonClicked)Delegate.Remove(item.OnDeleteButtonClicked, new LoadoutEditorOrbItem.DeleteButtonClicked(DeleteOrbItem));
			List<LoadoutOrbData> list = new List<LoadoutOrbData>();
			foreach (LoadoutOrbData selectedStartingOrb in selectedStartingOrbs)
			{
				if (item.OrbData == selectedStartingOrb)
				{
					list.Add(selectedStartingOrb);
					if (selectedStartingOrb.CanBeLeveledUp)
					{
						item.OnCycleLevelClicked = (LoadoutEditorOrbItem.OrbLevelCycled)Delegate.Remove(item.OnCycleLevelClicked, new LoadoutEditorOrbItem.OrbLevelCycled(CycleLevels));
					}
				}
			}
			foreach (LoadoutOrbData item2 in list)
			{
				selectedStartingOrbs.Remove(item2);
			}
			if (EventSystem.current.currentSelectedGameObject == item.gameObject)
			{
				GameObject selectedGameObject = null;
				int num = -1;
				bool flag = false;
				foreach (Transform item3 in orbDeckContainer.transform)
				{
					num++;
					if (item3.gameObject == item.gameObject)
					{
						if (num == 0)
						{
							flag = true;
							continue;
						}
						break;
					}
					if (flag)
					{
						selectedGameObject = item3.gameObject;
						break;
					}
					selectedGameObject = item3.gameObject;
				}
				EventSystem.current.SetSelectedGameObject(selectedGameObject);
			}
			_items.Remove(item.gameObject.GetComponent<Selectable>());
			UnityEngine.Object.Destroy(item.gameObject);
			startingOrbsCarousel.RemoveElement(item.gameObject);
		}

		public static void RebuildItemNavigation(List<Selectable> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (items.Count > i + 1)
				{
					navigation.selectOnDown = items[i + 1];
				}
				if (i > 0)
				{
					navigation.selectOnUp = items[i - 1];
				}
				items[i].navigation = navigation;
			}
		}
	}
}
