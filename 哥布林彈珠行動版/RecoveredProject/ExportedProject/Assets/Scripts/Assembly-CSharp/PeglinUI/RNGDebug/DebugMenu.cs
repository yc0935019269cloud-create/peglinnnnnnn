using System.Collections.Generic;
using Cruciball;
using Map;
using PeglinDebug;
using Relics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.RNGDebug
{
	public class DebugMenu : MonoBehaviour
	{
		public Button SkipBattleButton;

		public Toggle SkipToMapToggle;

		public Toggle PauseImmediatelyToggle;

		public TMP_Dropdown MapDropdown;

		public GameObject RootPanel;

		public GameObject StatsPanel;

		public GameObject OrbStatsPrefab;

		public Transform OrbStatsParent;

		public TextMeshProUGUI GeneralStats;

		public TMP_Dropdown StatusEffectDropdown;

		public TMP_InputField Intensity;

		public TMP_Dropdown ClassToUnlock;

		public TMP_Dropdown CruciballClassToUnlock;

		public TMP_Dropdown CruciballLevelToUnlock;

		public TMP_Dropdown SlimeTypeDropdown;

		public TMP_Dropdown AchievementDropdown;

		public TMP_Dropdown EnemyStatusEffectDrowpdown;

		public TMP_InputField EnemyIntensity;

		public TMP_InputField nodeNameInputField;

		public TMP_InputField CruciballLevelInputField;

		public CruciballManager cruciballManager;

		public OrbPool orbPool;

		public RelicSet globalRelics;

		public static bool StepThroughPrediction;

		public static bool ShowAssemballAsRewards;

		private Dictionary<string, DebugOrbStats> orbStatItems = new Dictionary<string, DebugOrbStats>();

		private MapController _mapController;

		private void OnEnable()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
