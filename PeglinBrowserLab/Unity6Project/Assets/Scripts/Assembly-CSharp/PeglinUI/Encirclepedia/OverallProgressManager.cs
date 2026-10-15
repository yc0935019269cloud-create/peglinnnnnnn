using System.Collections.Generic;
using ClassSystem;
using I2.Loc;
using Peglin.ClassSystem;
using PeglinUI.ControllerSupport;
using Rewired;
using Saving;
using TMPro;
using UnityEngine;

namespace PeglinUI.Encirclepedia
{
	public class OverallProgressManager : MonoBehaviour
	{
		public ClassInfo[] classInfoList;

		public GameObject classTabPrefab;

		public Transform classTabParent;

		public TextMeshProUGUI winsText;

		public TextMeshProUGUI totalRunsText;

		public TextMeshProUGUI runsHeaderText;

		public GameObject leftTabButtonGraphic;

		public GameObject rightTabButtonGraphic;

		private List<ClassTab> _classTabs;

		private ClassTab _selectedTab;

		private int _selectedTabIndex;

		private bool _isActive;

		private Player _player;

		public void Initialize()
		{
			_classTabs = new List<ClassTab>();
			ClassInfo[] array = classInfoList;
			foreach (ClassInfo info in array)
			{
				ClassTab component = Object.Instantiate(classTabPrefab, classTabParent).GetComponent<ClassTab>();
				component.Initialize(info, this);
				_classTabs.Add(component);
			}
			_selectedTabIndex = -1;
			PopulateFieldsDefault();
		}

		public void IsActive(bool value)
		{
			_isActive = value;
			_selectedTabIndex = -1;
		}

		private void Update()
		{
			if (!_isActive)
			{
				return;
			}
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(0);
			}
			if (ControllerInputManager.IsPointerBased && leftTabButtonGraphic.gameObject.activeInHierarchy)
			{
				leftTabButtonGraphic.gameObject.SetActive(value: false);
				rightTabButtonGraphic.gameObject.SetActive(value: false);
			}
			else if (!ControllerInputManager.IsPointerBased && !leftTabButtonGraphic.gameObject.activeInHierarchy)
			{
				leftTabButtonGraphic.gameObject.SetActive(value: true);
				rightTabButtonGraphic.gameObject.SetActive(value: true);
			}
			if (_player.GetButtonDown("CatalogPageLeft"))
			{
				if (_selectedTabIndex == -1)
				{
					_selectedTabIndex = _classTabs.Count - 1;
				}
				else
				{
					_selectedTabIndex--;
				}
				if (_selectedTabIndex < 0)
				{
					SelectNoTabs();
					return;
				}
				bool flag = false;
				for (int num = _selectedTabIndex - 1; num >= 0; num--)
				{
					if (_classTabs[num].IsUnlocked())
					{
						SelectTab(_classTabs[num]);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					SelectNoTabs();
				}
			}
			else
			{
				if (!_player.GetButtonDown("CatalogPageRight"))
				{
					return;
				}
				if (_selectedTabIndex >= _classTabs.Count)
				{
					_selectedTabIndex = -1;
				}
				else
				{
					_selectedTabIndex++;
				}
				if (_selectedTabIndex < 0)
				{
					SelectNoTabs();
					return;
				}
				bool flag2 = false;
				for (int i = _selectedTabIndex; i < _classTabs.Count; i++)
				{
					if (_classTabs[i].IsUnlocked())
					{
						SelectTab(_classTabs[i]);
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					SelectNoTabs();
				}
			}
		}

		public void OnClassTabClicked(ClassTab tab)
		{
			if (tab == _selectedTab)
			{
				SelectNoTabs();
			}
			else
			{
				SelectTab(tab);
			}
		}

		private void SelectNoTabs()
		{
			foreach (ClassTab classTab in _classTabs)
			{
				classTab.AnimateToIdle();
			}
			_selectedTab = null;
			_selectedTabIndex = -1;
			PopulateFieldsDefault();
		}

		private void SelectTab(ClassTab tab)
		{
			_selectedTab = tab;
			tab.Unfold();
			PopulateFields(tab.character);
			for (int i = 0; i < _classTabs.Count; i++)
			{
				ClassTab classTab = _classTabs[i];
				if (classTab == tab)
				{
					_selectedTabIndex = i;
				}
				else
				{
					classTab.Fold();
				}
			}
		}

		public void PopulateFieldsDefault()
		{
			int totalRuns = PersistentPlayerData.Instance.PermanentStats.totalRuns;
			int totalWins = PersistentPlayerData.Instance.PermanentStats.totalWins;
			winsText.text = totalWins.ToString();
			totalRunsText.text = totalRuns.ToString();
			runsHeaderText.text = LocalizationManager.GetTranslation("Menu/Encirclepedia/all_classes_label");
		}

		private void PopulateFields(Class tabCharacter)
		{
			int num = PersistentPlayerData.Instance.PermanentStats.runsPerClass[(int)tabCharacter];
			int num2 = PersistentPlayerData.Instance.PermanentStats.winsPerClass[(int)tabCharacter];
			winsText.text = num2.ToString();
			totalRunsText.text = num.ToString();
			runsHeaderText.text = LocalizationManager.GetTranslation(classInfoList[(int)tabCharacter].classNameLocKey);
		}
	}
}
