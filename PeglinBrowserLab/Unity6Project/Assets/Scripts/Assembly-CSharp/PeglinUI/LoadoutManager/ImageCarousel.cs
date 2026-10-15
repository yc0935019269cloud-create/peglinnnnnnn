using System;
using System.Collections.Generic;
using PeglinUI.ControllerSupport;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class ImageCarousel : MonoBehaviour
	{
		[Header("Parameters")]
		public int objectsPerPage = 5;

		public bool navigableIcons;

		public bool connectElementsToPageChange;

		public int iconsPerRow = 7;

		public int currentPage;

		public bool carouselIsNeverSelectable;

		public string catalogPageLeftAction = "CatalogPageLeft";

		public string catalogPageRightAction = "CatalogPageRight";

		[Header("Scene refs")]
		public Button upArrow;

		public Button downArrow;

		public GameObject selectableParentEntity;

		public Image controllerButtonImageUp;

		public Image controllerButtonImageDown;

		public TextMeshProUGUI categoryText;

		public GameObject lastPageObject;

		private int _maxPages;

		private List<GameObject> _elements;

		private List<Selectable> _selectables = new List<Selectable>();

		private readonly List<GameObject> _currentlyVisibleElements = new List<GameObject>();

		private Player _player;

		private SelectableObjectContainer _selectableObjectContainer;

		private List<string> _categoryNames = new List<string>();

		private List<int> _categoryIndexBounds = new List<int>();

		private Dictionary<int, int> _precalculatedPageHigherBounds = new Dictionary<int, int>();

		private bool _shouldCheckLastPageObject;

		public List<GameObject> elements => _elements;

		public List<GameObject> Elements => _elements;

		private void OnEnable()
		{
			_player = ReInput.players.GetPlayer(0);
			_selectableObjectContainer = GetComponentInChildren<SelectableObjectContainer>();
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
			upArrow.onClick.AddListener(ShowPreviousPage);
			downArrow.onClick.AddListener(ShowNextPage);
			if (lastPageObject != null)
			{
				_shouldCheckLastPageObject = true;
			}
			InputTypeChanged();
		}

		private void OnDisable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
			upArrow.onClick.RemoveListener(ShowPreviousPage);
			downArrow.onClick.RemoveListener(ShowNextPage);
		}

		private void Update()
		{
			if (ControllerInputManager.IsPointerBased)
			{
				return;
			}
			bool flag = false;
			if (navigableIcons)
			{
				foreach (Selectable selectable in _selectables)
				{
					if (EventSystem.current.currentSelectedGameObject == selectable.gameObject)
					{
						flag = true;
						break;
					}
				}
			}
			if (((EventSystem.current.currentSelectedGameObject == selectableParentEntity) | flag) || carouselIsNeverSelectable)
			{
				if (controllerButtonImageUp != null)
				{
					controllerButtonImageUp.gameObject.SetActive(upArrow.gameObject.activeInHierarchy);
				}
				if (controllerButtonImageDown != null)
				{
					controllerButtonImageDown.gameObject.SetActive(downArrow.gameObject.activeInHierarchy);
				}
				if (_player.GetButtonDown(catalogPageLeftAction) && currentPage > 0)
				{
					ShowPreviousPage();
				}
				else if (_player.GetButtonDown(catalogPageRightAction) && currentPage < _maxPages)
				{
					ShowNextPage();
				}
			}
			else
			{
				if (controllerButtonImageUp != null)
				{
					controllerButtonImageUp.gameObject.SetActive(value: false);
				}
				if (controllerButtonImageDown != null)
				{
					controllerButtonImageDown.gameObject.SetActive(value: false);
				}
			}
			if (connectElementsToPageChange)
			{
				if (EventSystem.current.currentSelectedGameObject == upArrow.gameObject)
				{
					ShowPreviousPage();
					EventSystem.current.SetSelectedGameObject(_elements[objectsPerPage * (currentPage + 1) - 1]);
				}
				else if (EventSystem.current.currentSelectedGameObject == downArrow.gameObject)
				{
					ShowNextPage();
				}
			}
		}

		private void InputTypeChanged()
		{
			if (controllerButtonImageUp != null)
			{
				controllerButtonImageUp.gameObject.SetActive(!ControllerInputManager.IsPointerBased && upArrow.gameObject.activeInHierarchy && EventSystem.current.currentSelectedGameObject == selectableParentEntity);
			}
			if (controllerButtonImageDown != null)
			{
				controllerButtonImageDown.gameObject.SetActive(!ControllerInputManager.IsPointerBased && downArrow.gameObject.activeInHierarchy && EventSystem.current.currentSelectedGameObject == selectableParentEntity);
			}
		}

		public void Initialize(List<GameObject> icons, bool autoNav = false)
		{
			ClearInternalContainers();
			if (icons != null)
			{
				_elements = icons;
				_maxPages = (icons.Count - 1) / objectsPerPage + 1;
			}
			else
			{
				_elements = new List<GameObject>();
				_maxPages = 1;
			}
			upArrow.gameObject.SetActive(value: false);
			downArrow.gameObject.SetActive(_maxPages != 1);
			foreach (GameObject element in Elements)
			{
				element.SetActive(value: false);
				if (navigableIcons)
				{
					_selectables.Add(element.GetComponentInChildren<Selectable>());
				}
			}
			if (navigableIcons)
			{
				BuildNavigation(autoNav);
			}
			currentPage = 0;
			ShowPage(0);
		}

		public void ClearCategories()
		{
			ClearElements();
			if (_selectableObjectContainer == null)
			{
				_selectableObjectContainer = GetComponent<SelectableObjectContainer>();
			}
			_selectableObjectContainer.selectableObject = null;
			_selectables.Clear();
			_categoryIndexBounds.Clear();
			_categoryNames.Clear();
			_precalculatedPageHigherBounds.Clear();
			_maxPages = 0;
		}

		public void ClearElements()
		{
			if (Elements == null)
			{
				_elements = new List<GameObject>();
				return;
			}
			foreach (GameObject element in Elements)
			{
				UnityEngine.Object.Destroy(element);
			}
			Elements.Clear();
		}

		private void PrecalculatePages(int lastPage, int lastIndex, int listSize)
		{
			if (listSize <= objectsPerPage)
			{
				_precalculatedPageHigherBounds.Add(lastPage + 1, listSize + lastIndex);
			}
			else if (lastPage >= -1)
			{
				int num = listSize;
				int num2 = lastPage;
				int num3 = ((lastPage != -1) ? _precalculatedPageHigherBounds[lastPage] : 0);
				while (num > 0)
				{
					int num4 = ((num >= objectsPerPage) ? objectsPerPage : num);
					_precalculatedPageHigherBounds.Add(++num2, num3 + num4);
					num3 += num4;
					num -= objectsPerPage;
				}
			}
		}

		public void CreateCategory(List<GameObject> icons, string categoryName)
		{
			if (Elements == null)
			{
				_elements = new List<GameObject>();
			}
			int num = ((_categoryIndexBounds.Count > 0) ? _categoryIndexBounds[_categoryIndexBounds.Count - 1] : 0);
			_categoryIndexBounds.Add(icons.Count + num);
			if (_precalculatedPageHigherBounds.Count > 0)
			{
				PrecalculatePages(_precalculatedPageHigherBounds.Count - 1, num, icons.Count);
			}
			else
			{
				PrecalculatePages(-1, 0, icons.Count);
			}
			_categoryNames.Add(categoryName);
			Elements.AddRange(icons);
			_maxPages = _precalculatedPageHigherBounds.Count;
			foreach (GameObject icon in icons)
			{
				icon.SetActive(value: false);
				if (navigableIcons)
				{
					_selectables.Add(icon.GetComponent<Selectable>());
				}
			}
			currentPage = 0;
			ResolveArrowState();
			ShowPage(0);
		}

		private void ClearInternalContainers()
		{
			if (Elements != null)
			{
				ClearIcons();
			}
			else
			{
				_elements = new List<GameObject>();
			}
			_selectableObjectContainer = GetComponentInChildren<SelectableObjectContainer>();
			_selectables.Clear();
		}

		public void BuildNavigation(bool autoNav = false)
		{
			if (autoNav)
			{
				BuildNavigationAuto();
			}
			else
			{
				BuildNavigationFixed();
			}
		}

		private void BuildNavigationFixed()
		{
			for (int i = 0; i < _selectables.Count; i++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (_selectables.Count > i + 1)
				{
					navigation.selectOnRight = _selectables[i + 1];
				}
				if (i > 0)
				{
					navigation.selectOnLeft = _selectables[i - 1];
				}
				if (i - iconsPerRow >= 0 && (i - iconsPerRow) / objectsPerPage == i / objectsPerPage)
				{
					navigation.selectOnUp = _selectables[i - iconsPerRow];
				}
				if (i + iconsPerRow < _selectables.Count && (i + iconsPerRow) / objectsPerPage == i / objectsPerPage)
				{
					navigation.selectOnDown = _selectables[i + iconsPerRow];
				}
				_selectables[i].navigation = navigation;
			}
			if (connectElementsToPageChange && _selectables.Count > 0)
			{
				for (int j = objectsPerPage - 1; j < _selectables.Count - 1; j += objectsPerPage)
				{
					Navigation navigation2 = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnRight = downArrow,
						selectOnDown = downArrow,
						selectOnLeft = _selectables[j].navigation.selectOnLeft,
						selectOnUp = _selectables[j].navigation.selectOnUp
					};
					_selectables[j].navigation = navigation2;
				}
				for (int k = objectsPerPage; k < _selectables.Count; k += objectsPerPage)
				{
					Navigation navigation3 = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnLeft = upArrow,
						selectOnUp = upArrow,
						selectOnRight = _selectables[k].navigation.selectOnRight,
						selectOnDown = _selectables[k].navigation.selectOnDown
					};
					_selectables[k].navigation = navigation3;
				}
			}
		}

		public void BuildNavigationAuto()
		{
			for (int i = 0; i < _selectables.Count; i++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Automatic
				};
				_selectables[i].navigation = navigation;
			}
		}

		public void BuildNavigationPreprocessedPages()
		{
			for (int i = 0; i < _precalculatedPageHigherBounds.Count; i++)
			{
				int num = ((i != 0) ? _precalculatedPageHigherBounds[i - 1] : 0);
				int num2 = _precalculatedPageHigherBounds[i];
				for (int j = num; j < num2; j++)
				{
					int num3 = ((num2 - j < iconsPerRow) ? (num2 - j) : iconsPerRow);
					Navigation navigation = new Navigation
					{
						mode = Navigation.Mode.Explicit
					};
					if (num2 > j + 1)
					{
						navigation.selectOnRight = _selectables[j + 1];
					}
					if (j > num)
					{
						navigation.selectOnLeft = _selectables[j - 1];
					}
					if (j - num3 >= num && j - iconsPerRow >= 0)
					{
						navigation.selectOnUp = _selectables[j - iconsPerRow];
					}
					if (j + num3 < num2)
					{
						navigation.selectOnDown = _selectables[j + iconsPerRow];
					}
					_selectables[j].navigation = navigation;
				}
			}
		}

		private void ShowNextPage()
		{
			if (currentPage != _maxPages - 1)
			{
				EventSystem.current.SetSelectedGameObject(null);
				currentPage = Mathf.Clamp(++currentPage, 0, _maxPages - 1);
				ShowPage(currentPage);
				ResolveArrowState();
				SelectFirstVisibleObject();
			}
		}

		private void ShowPreviousPage()
		{
			if (currentPage != 0)
			{
				EventSystem.current.SetSelectedGameObject(null);
				currentPage = Mathf.Clamp(--currentPage, 0, _maxPages - 1);
				ShowPage(currentPage);
				ResolveArrowState();
				SelectFirstVisibleObject();
			}
		}

		public void SelectFirstVisibleObject(bool forceNonPointerBased = true)
		{
			if (!(ControllerInputManager.IsPointerBased & forceNonPointerBased))
			{
				if (_precalculatedPageHigherBounds.Count > 0 && _selectables.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject((currentPage == 0) ? _selectables[0].gameObject : _selectables[_precalculatedPageHigherBounds[currentPage - 1]].gameObject);
				}
				else if (_selectables.Count > 0)
				{
					EventSystem.current.SetSelectedGameObject(_selectables[objectsPerPage * currentPage].gameObject);
				}
			}
		}

		public void AddElement(GameObject element)
		{
			Elements.Add(element);
			if (navigableIcons)
			{
				_selectables.Add(element.GetComponent<Selectable>());
			}
			_maxPages = (Elements.Count - 1) / objectsPerPage + 1;
			ShowPage(_maxPages - 1);
			ResolveArrowState();
			if (navigableIcons)
			{
				BuildNavigation();
			}
		}

		public void RemoveElement(GameObject elem)
		{
			Elements.Remove(elem);
			if (navigableIcons)
			{
				_selectables.Remove(elem.GetComponent<Selectable>());
			}
			_maxPages = (Elements.Count - 1) / objectsPerPage + 1;
			if (currentPage >= _maxPages)
			{
				currentPage = _maxPages - 1;
			}
			ShowPage(currentPage);
			ResolveArrowState();
			if (navigableIcons)
			{
				BuildNavigation();
			}
		}

		public void RemoveAllElements()
		{
			Elements.Clear();
		}

		public GameObject GetElement(int index)
		{
			if (index >= 0 && _elements.Count > index)
			{
				return _elements[index];
			}
			Debug.LogError($"Index {index} out of bounds for ImageCarousel elements size {_elements.Count}, returning last element");
			List<GameObject> list = _elements;
			return list[list.Count - 1];
		}

		public void ShowPageForElement(int index)
		{
			ShowPage(index / objectsPerPage);
		}

		public void ShowPage(int page)
		{
			if (Elements == null)
			{
				_elements = new List<GameObject>();
			}
			foreach (GameObject element in Elements)
			{
				if (element != null)
				{
					element.SetActive(value: false);
				}
			}
			_currentlyVisibleElements.Clear();
			int num;
			int num2;
			if (_precalculatedPageHigherBounds.Count == 0)
			{
				num = Mathf.Clamp(page * objectsPerPage, 0, Elements.Count - 1);
				num2 = ((num + objectsPerPage >= Elements.Count) ? Elements.Count : (num + objectsPerPage));
			}
			else if (page == 0)
			{
				num = 0;
				num2 = _precalculatedPageHigherBounds[0];
			}
			else
			{
				num = _precalculatedPageHigherBounds[page - 1];
				num2 = _precalculatedPageHigherBounds[page];
			}
			if (num < 0)
			{
				return;
			}
			for (int i = 0; i < _categoryIndexBounds.Count; i++)
			{
				if (num < _categoryIndexBounds[i])
				{
					categoryText.text = _categoryNames[i];
					break;
				}
			}
			for (int j = num; j < num2; j++)
			{
				GameObject gameObject = Elements[j];
				gameObject.gameObject.SetActive(value: true);
				_currentlyVisibleElements.Add(gameObject);
			}
			if (_selectableObjectContainer == null)
			{
				_selectableObjectContainer = GetComponent<SelectableObjectContainer>();
			}
			if (_selectableObjectContainer != null)
			{
				_selectableObjectContainer.selectableObject = Elements[num];
			}
			currentPage = page;
			if (currentPage == _maxPages - 1 && _shouldCheckLastPageObject)
			{
				lastPageObject.SetActive(value: true);
			}
			else if (_shouldCheckLastPageObject)
			{
				lastPageObject.SetActive(value: false);
			}
			ResolveArrowState();
		}

		private void ClearIcons()
		{
			if (Elements == null)
			{
				return;
			}
			foreach (GameObject element in Elements)
			{
				UnityEngine.Object.Destroy(element);
			}
			Elements.Clear();
		}

		public void ResolveArrowState()
		{
			upArrow.gameObject.SetActive(currentPage != 0);
			downArrow.gameObject.SetActive(currentPage != _maxPages - 1);
		}
	}
}
