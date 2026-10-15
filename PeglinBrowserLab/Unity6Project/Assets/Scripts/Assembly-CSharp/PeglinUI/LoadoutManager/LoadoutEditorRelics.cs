using System;
using System.Collections.Generic;
using I2.Loc;
using Peglin.ClassSystem;
using PeglinUI.ControllerSupport;
using Relics;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutEditorRelics : MonoBehaviour
	{
		public delegate void ButtonClicked();

		public ButtonClicked OnAcceptButtonClicked;

		public ButtonClicked OnCancelButtonClicked;

		public List<Relic> selectedStartingRelics = new List<Relic>();

		[Header("Scene refs")]
		public GameObject startingRelicsContainer;

		public GameObject relicPoolContainer;

		public ImageCarousel relicLibraryCarousel;

		public ImageCarousel startingRelicsCarousel;

		[Header("Prefabs")]
		public GameObject relicIcon;

		public GameObject relicLoadoutItem;

		public GameObject contextButtons;

		private RelicManager _relicManager;

		private Dictionary<LoadoutIcon, LoadoutIcon.RelicIconClicked> onIconClickDict = new Dictionary<LoadoutIcon, LoadoutIcon.RelicIconClicked>();

		private Dictionary<Relic, LoadoutIcon> _iconDict = new Dictionary<Relic, LoadoutIcon>();

		private SelectableObjectContainer _selectableObjectContainer;

		private RewiredEventSystem _rewiredEventSystem;

		private Player _player;

		private List<Selectable> _items = new List<Selectable>();

		private static List<RelicEffect> _classRelics = new List<RelicEffect>
		{
			RelicEffect.ADDITIONAL_PEGLIN_CHOICES,
			RelicEffect.START_WITH_BALLWARK_FOR_ORBS_IN_DECK,
			RelicEffect.RANDOM_STATUS_EFFECT_ON_HIT,
			RelicEffect.UNIQUE_ORBS_BUFF
		};

		private void OnEnable()
		{
			LoadoutEditorRelicItem.OnRelicItemIsSelected = (LoadoutEditorRelicItem.RelicItemIsSelected)Delegate.Combine(LoadoutEditorRelicItem.OnRelicItemIsSelected, new LoadoutEditorRelicItem.RelicItemIsSelected(SetItemIsSelected));
		}

		private void OnDisable()
		{
			TooltipManager.Instance.HideTooltip();
			foreach (LoadoutIcon key in onIconClickDict.Keys)
			{
				key.OnRelicIconClicked = (LoadoutIcon.RelicIconClicked)Delegate.Remove(key.OnRelicIconClicked, onIconClickDict[key]);
			}
			LoadoutEditorRelicItem.OnRelicItemIsSelected = (LoadoutEditorRelicItem.RelicItemIsSelected)Delegate.Remove(LoadoutEditorRelicItem.OnRelicItemIsSelected, new LoadoutEditorRelicItem.RelicItemIsSelected(SetItemIsSelected));
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

		public void Initialize(RelicManager relicManager, List<Relic> startingRelics, ClassLoadoutData selectedClass)
		{
			_items.Clear();
			_player = ReInput.players.GetPlayer(0);
			if (_rewiredEventSystem == null)
			{
				_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			}
			if (_selectableObjectContainer == null)
			{
				_selectableObjectContainer = startingRelicsContainer.GetComponentInChildren<SelectableObjectContainer>();
			}
			_iconDict.Clear();
			onIconClickDict.Clear();
			selectedStartingRelics.Clear();
			if (relicLibraryCarousel == null)
			{
				relicLibraryCarousel = GetComponentInChildren<ImageCarousel>();
			}
			_relicManager = relicManager;
			foreach (Transform item in relicPoolContainer.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			foreach (Transform item2 in startingRelicsContainer.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
			relicLibraryCarousel.ClearCategories();
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(_relicManager.CommonRelicPool), LocalizationManager.GetTranslation("Menu/common_relics_label"));
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(_relicManager.RareRelicPool), LocalizationManager.GetTranslation("Menu/rare_relics_label"));
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(_relicManager.BossRelicPool), LocalizationManager.GetTranslation("Menu/boss_relics_label"));
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(_relicManager.ShopRelicPool, excludeShopOnlyRelics: false), LocalizationManager.GetTranslation("Menu/shop_relics_label"));
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(_relicManager.RareScenarioRelicPool), LocalizationManager.GetTranslation("Menu/scenario_relics_label"));
			List<Relic> list = new List<Relic>();
			List<Relic> list2 = new List<Relic>();
			foreach (Relic item3 in _relicManager.UnavailableRelicPool)
			{
				if ((item3.globalRarity == RelicRarity.UNAVAILABLE && !_classRelics.Contains(item3.effect)) || selectedClass.StartingRelics.Contains(item3))
				{
					list.Add(item3);
				}
				else
				{
					list2.Add(item3);
				}
			}
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(list2), LocalizationManager.GetTranslation("Menu/other_classes_relics_label"));
			relicLibraryCarousel.CreateCategory(CreateRelicPoolIcons(list), LocalizationManager.GetTranslation("Menu/unavailable_relics_label"));
			relicLibraryCarousel.BuildNavigationPreprocessedPages();
			relicLibraryCarousel.SelectFirstVisibleObject();
			startingRelicsCarousel.Initialize(null);
			foreach (Relic startingRelic in startingRelics)
			{
				AddRelicToLoadout(startingRelic);
			}
		}

		public void SaveRelicLoadout()
		{
			OnAcceptButtonClicked?.Invoke();
		}

		public void Cancel()
		{
			OnCancelButtonClicked?.Invoke();
		}

		private List<GameObject> CreateRelicPoolIcons(List<Relic> relicPool, bool excludeShopOnlyRelics = true)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Relic item in relicPool)
			{
				if (!excludeShopOnlyRelics || !_relicManager.ShopRelicPool.Contains(item))
				{
					GameObject obj = UnityEngine.Object.Instantiate(relicIcon, relicPoolContainer.transform);
					LoadoutIcon componentInChildren = obj.GetComponentInChildren<LoadoutIcon>();
					componentInChildren.InitializeRelic(item);
					list.Add(componentInChildren.gameObject);
					LoadoutIcon.RelicIconClicked b = AddRelicToLoadout;
					componentInChildren.OnRelicIconClicked = (LoadoutIcon.RelicIconClicked)Delegate.Combine(componentInChildren.OnRelicIconClicked, b);
					onIconClickDict.Add(componentInChildren, AddRelicToLoadout);
					componentInChildren.SetupTooltipEventsRelic();
					obj.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
					_iconDict.Add(item, componentInChildren);
				}
			}
			return list;
		}

		private void AddRelicToLoadout(Relic relic)
		{
			if (!selectedStartingRelics.Contains(relic))
			{
				selectedStartingRelics.Add(relic);
				LoadoutEditorRelicItem component = UnityEngine.Object.Instantiate(relicLoadoutItem, startingRelicsContainer.transform).GetComponent<LoadoutEditorRelicItem>();
				component.Initialize(relic);
				component.OnDeleteButtonClicked = (LoadoutEditorRelicItem.DeleteButtonClicked)Delegate.Combine(component.OnDeleteButtonClicked, new LoadoutEditorRelicItem.DeleteButtonClicked(RemoveRelicFromLoadout));
				component.gameObject.GetComponentInChildren<ArrowSelection>().rewiredEventSystem = _rewiredEventSystem;
				_items.Add(component.gameObject.GetComponent<Selectable>());
				if (selectedStartingRelics.Count == 1)
				{
					_selectableObjectContainer.selectableObject = component.gameObject;
				}
				startingRelicsCarousel.AddElement(component.gameObject);
				_iconDict[relic].SetUsed(on: true);
			}
		}

		private void RemoveRelicFromLoadout(LoadoutEditorRelicItem relicItem, Relic relic)
		{
			relicItem.OnDeleteButtonClicked = (LoadoutEditorRelicItem.DeleteButtonClicked)Delegate.Remove(relicItem.OnDeleteButtonClicked, new LoadoutEditorRelicItem.DeleteButtonClicked(RemoveRelicFromLoadout));
			selectedStartingRelics.Remove(relic);
			_iconDict[relic].SetUsed(on: false);
			if (EventSystem.current.currentSelectedGameObject == relicItem.gameObject)
			{
				GameObject selectedGameObject = null;
				int num = -1;
				bool flag = false;
				foreach (Transform item in startingRelicsContainer.transform)
				{
					num++;
					if (item.gameObject == relicItem.gameObject)
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
						selectedGameObject = item.gameObject;
						break;
					}
					selectedGameObject = item.gameObject;
				}
				EventSystem.current.SetSelectedGameObject(selectedGameObject);
			}
			_items.Remove(relicItem.gameObject.GetComponent<Selectable>());
			startingRelicsCarousel.RemoveElement(relicItem.gameObject);
		}
	}
}
