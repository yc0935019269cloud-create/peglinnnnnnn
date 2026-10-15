using System.Collections.Generic;
using Battle.Attacks;
using Cruciball;
using MoreMountains.Tools;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using Relics;
using Rewired.Integration.UnityUI;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGridPopulator : MonoBehaviour
{
	public delegate void InventoryViewInitializedWithNoContents();

	public static InventoryViewInitializedWithNoContents OnInventoryViewInitializedWithNoContents = () =>
	{
	};

	public CruciballManager cruciballManager;

	public DeckManager deckManager;

	public RelicManager relicManager;

	public GameObject upgradeOptionPrefab;

	public int numPerRow = 5;

	private GameObject _firstButton;

	private RewiredEventSystem _rewiredEventSystem;

	private ControllerMenuHookup _controllerMenuHookup;

	public bool onlyShowUpgrades;

	public bool isRemoving;

	public bool showPebballs = true;

	public GameObject firstObject => _firstButton;

	private void OnEnable()
	{
		UpdateGrid();
	}

	public void UpdateGrid()
	{
		base.transform.MMDestroyAllChildren();
		_firstButton = null;
		Canvas.ForceUpdateCanvases();
		int num = -8;
		int num2 = -8;
		int num3 = 10;
		bool flag = true;
		if (DeckManager.completeDeck == null)
		{
			return;
		}
		List<Button> list = new List<Button>();
		foreach (GameObject item in DeckManager.completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (!(component == null) && (component.HasUpgrade || !onlyShowUpgrades) && (!isRemoving || !(item.GetComponent<CannotBeRemoved>() != null)) && (showPebballs || !component.IsStone))
			{
				component.SoftInit(deckManager, relicManager, cruciballManager);
				GameObject gameObject = Object.Instantiate(upgradeOptionPrefab);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localScale = Vector3.one;
				UpgradeOption component2 = gameObject.GetComponent<UpgradeOption>();
				component2.SpecifiedOrb = item;
				component2.upgradeType = UpgradeOption.UpgradeType.INSPECT_ORB_FOR_UPGRADE;
				gameObject.GetComponent<ButtonHandleHover>().noteOffset = num2;
				if (flag && num2 + 2 > num3)
				{
					flag = false;
					num2 -= 2;
				}
				else if (!flag && num2 - 2 < num)
				{
					flag = true;
					num2 += 2;
				}
				else
				{
					num2 += (flag ? 2 : (-2));
				}
				if (_firstButton == null)
				{
					_firstButton = gameObject;
				}
				list.Add(component2.GetComponent<Button>());
			}
		}
		if (_firstButton == null)
		{
			OnInventoryViewInitializedWithNoContents?.Invoke();
		}
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		_controllerMenuHookup = _rewiredEventSystem.GetComponent<ControllerMenuHookup>();
		SetActiveButton();
		SetupNavigation(list, numPerRow);
		isRemoving = false;
		showPebballs = true;
	}

	public static void SetupNavigation(List<Button> buttons, int iconsPerRow)
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			Navigation navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit
			};
			if (buttons.Count > i + 1)
			{
				navigation.selectOnRight = buttons[i + 1];
			}
			if (i > 0)
			{
				navigation.selectOnLeft = buttons[i - 1];
			}
			if (buttons.Count > iconsPerRow && i < buttons.Count - iconsPerRow)
			{
				int index = Mathf.Clamp(i + iconsPerRow, i, buttons.Count - 1);
				navigation.selectOnDown = buttons[index];
			}
			if (i >= iconsPerRow)
			{
				navigation.selectOnUp = buttons[i - iconsPerRow];
			}
			buttons[i].navigation = navigation;
		}
	}

	private void SetActiveButton()
	{
		if (_firstButton != null)
		{
			_controllerMenuHookup.LastSelected = _firstButton;
			_rewiredEventSystem.GetComponent<ControllerMenuHookup>().LastSelected = _firstButton;
			if (!ControllerInputManager.IsPointerBased)
			{
				_rewiredEventSystem.SetSelectedGameObject(_firstButton);
			}
		}
	}
}
