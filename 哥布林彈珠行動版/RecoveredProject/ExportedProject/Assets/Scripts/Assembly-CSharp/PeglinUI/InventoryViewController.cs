using System;
using System.Collections;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Challenges;
using Cruciball;
using I2.Loc;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using PeglinUtils;
using PixelCrushers.DialogueSystem;
using RNG.Scenarios;
using Relics;
using Rewired.Integration.UnityUI;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PeglinUI
{
	public class InventoryViewController : MonoBehaviour
	{
		public delegate void InventoryViewToggle(bool open);

		public delegate void ExternalForceRemoveOrbRequest();

		private enum InventoryViewState
		{
			Viewing = 0,
			Upgrading = 1,
			Removing = 2,
			Duplicating = 3,
			RemovingShop = 4,
			RemovingViaChallenge = 5,
			RemovingMulitpleOrbs = 6
		}

		private const string REMOVE_MULTIPLE_ORBS_LOC_KEY = "Menu/multiple_orb_removals_left";

		private const string REMOVE_SINGLE_ORB_LOC_KEY = "Menu/Store/remove_orb_label";

		private const string REMOVE_MULTIPLE_ORBS_PARAM_NAME = "ORB_REMOVALS_LEFT";

		public static InventoryViewToggle onInventoryViewToggled = (bool open) =>
		{
		};

		public static ExternalForceRemoveOrbRequest OnExternalForceRemoveOrbRequest = () =>
		{
		};

		public DeckManager deckManager;

		public CruciballManager cruciballManager;

		public RelicManager relicManager;

		[Header("Internal References")]
		[SerializeField]
		private GameObject _mainContainer;

		public InventoryGridPopulator inventoryGridPopulator;

		public UpgradeConfirmationOrbInspect upgradeConfirmationPanelInspect;

		public UpgradeConfirmationOrbUpgrade upgradeConfirmationOrbUpgrade;

		public Button backButton;

		public GameObject closeButton;

		public TextMeshProUGUI removeMultipleOrbsText;

		public LocalizationParamsManager removeMultipleOrbsLocParamManager;

		public Localize removeMultipleOrbsLoc;

		public GameObject removeTextContainer;

		[Header("External References")]
		public GameObject playfieldMouseDetector;

		private bool _wasPlayfieldDetectorActive = true;

		private bool _skippable = true;

		public static bool isOpen = false;

		public static bool wasOpenThisFrame = false;

		private RewiredEventSystem _rewiredEventSystem;

		private GameObject _previouslySelected;

		private int _orbRemovalQueueLength;

		private InventoryViewState _orbRemovalQueueShowState;

		private bool _orbRemovalQueueSkipable = true;

		private bool _orbRemovalQueueCloseable = true;

		private bool _orbRemovalQueueShowPebballs = true;

		private bool _continueDialogueAfterChallengeRemoval;

		private string _continueDialogueAfterChallengeRemovalOrb = "";

		private string _continueDialogueAfterChallengeRemovalUpgrade = "";

		private InventoryViewState _state;

		private void Awake()
		{
			_state = InventoryViewState.Viewing;
		}

		private void OnEnable()
		{
			DialogueSystemScenario.DialogueCurtainRemoved = (Action)Delegate.Combine(DialogueSystemScenario.DialogueCurtainRemoved, new Action(SetCurtainInactive));
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			OnExternalForceRemoveOrbRequest = (ExternalForceRemoveOrbRequest)Delegate.Combine(OnExternalForceRemoveOrbRequest, new ExternalForceRemoveOrbRequest(QueueForceShowInventoryForOrbRemovalChallenge));
			InventoryGridPopulator.OnInventoryViewInitializedWithNoContents = (InventoryGridPopulator.InventoryViewInitializedWithNoContents)Delegate.Combine(InventoryGridPopulator.OnInventoryViewInitializedWithNoContents, new InventoryGridPopulator.InventoryViewInitializedWithNoContents(CloseIfRemovingMultipleOrbs));
			upgradeConfirmationPanelInspect.Init(deckManager, relicManager, cruciballManager);
			ToggleCloseButton(showButton: true);
		}

		private void OnDisable()
		{
			isOpen = false;
			wasOpenThisFrame = false;
			DialogueSystemScenario.DialogueCurtainRemoved = (Action)Delegate.Remove(DialogueSystemScenario.DialogueCurtainRemoved, new Action(SetCurtainInactive));
			OnExternalForceRemoveOrbRequest = (ExternalForceRemoveOrbRequest)Delegate.Remove(OnExternalForceRemoveOrbRequest, new ExternalForceRemoveOrbRequest(QueueForceShowInventoryForOrbRemovalChallenge));
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			InventoryGridPopulator.OnInventoryViewInitializedWithNoContents = (InventoryGridPopulator.InventoryViewInitializedWithNoContents)Delegate.Remove(InventoryGridPopulator.OnInventoryViewInitializedWithNoContents, new InventoryGridPopulator.InventoryViewInitializedWithNoContents(CloseIfRemovingMultipleOrbs));
		}

		private void HandleUpgradeOptionClicked(UpgradeOption.UpgradeType type, GameObject clickedButton = null, GameObject chosen = null)
		{
			if (isOpen && type == UpgradeOption.UpgradeType.INSPECT_ORB_FOR_UPGRADE)
			{
				GameObject setActiveOnReturn = ((_rewiredEventSystem != null) ? _rewiredEventSystem.currentSelectedGameObject : inventoryGridPopulator.firstObject);
				switch (_state)
				{
				case InventoryViewState.Viewing:
					upgradeConfirmationPanelInspect.gameObject.SetActive(value: true);
					upgradeConfirmationPanelInspect.PopulateForInspectOrb(chosen);
					upgradeConfirmationPanelInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = setActiveOnReturn;
					break;
				case InventoryViewState.Upgrading:
					upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: true);
					upgradeConfirmationOrbUpgrade.Populate(chosen, 0);
					upgradeConfirmationOrbUpgrade.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = setActiveOnReturn;
					UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(ConfirmationButtonClicked));
					break;
				case InventoryViewState.Removing:
				case InventoryViewState.RemovingShop:
				case InventoryViewState.RemovingViaChallenge:
				case InventoryViewState.RemovingMulitpleOrbs:
					upgradeConfirmationPanelInspect.gameObject.SetActive(value: true);
					upgradeConfirmationPanelInspect.PopulateForRemoveOrb(chosen, 0);
					upgradeConfirmationPanelInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = setActiveOnReturn;
					UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(ConfirmationButtonClicked));
					break;
				case InventoryViewState.Duplicating:
					upgradeConfirmationPanelInspect.gameObject.SetActive(value: true);
					upgradeConfirmationPanelInspect.PopulateForDuplicateOrb(chosen, 0);
					upgradeConfirmationPanelInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = setActiveOnReturn;
					UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(ConfirmationButtonClicked));
					break;
				}
			}
		}

		private void ConfirmationButtonClicked(UpgradeOption.UpgradeType type, GameObject clickedButton, GameObject chosen)
		{
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(ConfirmationButtonClicked));
			_skippable = true;
			ToggleBackButton(showButton: true);
			switch (type)
			{
			case UpgradeOption.UpgradeType.UPGRADE_CONFIRMED:
			{
				upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: false);
				GameObject gameObject = deckManager.UpgradeSpecificOrb(chosen);
				UnlimitedUpgrades component = chosen.GetComponent<UnlimitedUpgrades>();
				if (component != null)
				{
					Attack component2 = chosen.GetComponent<Attack>();
					string nameWithCustomLevel = component2.GetNameWithCustomLevel(component2.Level + component.timesUpgraded - 1);
					OrbSelected(nameWithCustomLevel, gameObject.GetComponent<Attack>()?.GetNameWithLevel());
				}
				else
				{
					OrbSelected(chosen.GetComponent<Attack>()?.GetNameWithLevel(), gameObject.GetComponent<Attack>()?.GetNameWithLevel());
				}
				UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.UpgradeOrb);
				break;
			}
			case UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED:
				upgradeConfirmationPanelInspect.gameObject.SetActive(value: false);
				deckManager.RemoveSpecifiedOrbFromDeck(chosen);
				OrbSelected(chosen.GetComponent<Attack>()?.GetNameWithLevel());
				UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.RemoveOrb);
				break;
			case UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED:
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.REMOVE_ORB_AFTER_ADDING))
				{
					_continueDialogueAfterChallengeRemoval = true;
					_continueDialogueAfterChallengeRemovalOrb = chosen.GetComponent<Attack>()?.GetNameWithLevel();
					_continueDialogueAfterChallengeRemovalUpgrade = "";
				}
				upgradeConfirmationPanelInspect.gameObject.SetActive(value: false);
				deckManager.AddOrbToDeck(chosen);
				OrbSelected(chosen.GetComponent<Attack>()?.GetNameWithLevel());
				UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddOrb);
				break;
			}
		}

		public void DeckButtonPressed()
		{
			if (!PauseMenu.Paused && !PauseMenu.PauseBlock && _state != InventoryViewState.RemovingShop && _state != InventoryViewState.RemovingMulitpleOrbs)
			{
				if (isOpen)
				{
					HideInventory();
				}
				else if (!GameBlockingWindow.windowOpen)
				{
					inventoryGridPopulator.onlyShowUpgrades = false;
					ShowInventory();
				}
			}
		}

		public void ShowAllInventory()
		{
			inventoryGridPopulator.onlyShowUpgrades = false;
			ShowInventory();
		}

		public void ShowInventoryUpgradeOnly()
		{
			inventoryGridPopulator.onlyShowUpgrades = true;
			ShowInventory();
		}

		public void ShowInventory()
		{
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			_previouslySelected = _rewiredEventSystem.currentSelectedGameObject;
			_rewiredEventSystem.SetSelectedGameObject(null);
			if (playfieldMouseDetector != null)
			{
				_wasPlayfieldDetectorActive = playfieldMouseDetector.activeSelf;
				playfieldMouseDetector.SetActive(value: false);
			}
			isOpen = true;
			wasOpenThisFrame = true;
			onInventoryViewToggled(open: true);
			_mainContainer.SetActive(value: true);
			upgradeConfirmationPanelInspect.gameObject.SetActive(value: false);
			upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: false);
			ToggleCloseButton(showButton: true);
			UpdateMultiOrbRemovalText();
		}

		public void HideInventory()
		{
			if (!PauseMenu.Paused && !upgradeConfirmationPanelInspect.gameObject.activeInHierarchy && !upgradeConfirmationOrbUpgrade.gameObject.activeInHierarchy && _skippable)
			{
				if (_state != InventoryViewState.Viewing && _state != InventoryViewState.RemovingShop && _state != InventoryViewState.RemovingMulitpleOrbs)
				{
					DialogueLua.SetVariable("chosenOrb", "nil");
					DialogueManager.UpdateResponses();
					Sequencer.Message("MenuClosed");
				}
				if (_state == InventoryViewState.RemovingMulitpleOrbs && _orbRemovalQueueSkipable && _orbRemovalQueueCloseable)
				{
					_orbRemovalQueueLength = 0;
				}
				_state = InventoryViewState.Viewing;
				Close();
			}
		}

		public void OrbSelected(string chosenOrb, string upgradedOrb = "")
		{
			if (_state != InventoryViewState.Viewing && _state != InventoryViewState.RemovingShop && _state != InventoryViewState.RemovingViaChallenge && _state != InventoryViewState.RemovingMulitpleOrbs)
			{
				DialogueLua.SetVariable("chosenOrb", chosenOrb);
				DialogueLua.SetVariable("upgradedOrb", upgradedOrb);
				DialogueManager.UpdateResponses();
				Sequencer.Message("MenuClosed");
			}
			if (_state == InventoryViewState.RemovingShop)
			{
				RemoveOrbButton removeOrbButton = UnityEngine.Object.FindObjectOfType<RemoveOrbButton>();
				if (removeOrbButton == null)
				{
					Debug.LogError("Cannot find RemoveOrbButton!");
				}
				else
				{
					removeOrbButton.OrbRemoved();
				}
			}
			_state = InventoryViewState.Viewing;
			Close();
		}

		private void Close()
		{
			isOpen = false;
			if (_mainContainer != null)
			{
				_mainContainer.SetActive(value: false);
			}
			upgradeConfirmationOrbUpgrade.gameObject.SetActive(value: false);
			upgradeConfirmationPanelInspect.gameObject.SetActive(value: false);
			if (_orbRemovalQueueLength > 0)
			{
				_orbRemovalQueueLength--;
				StartNextOrbRemoval();
				return;
			}
			if (SceneManager.GetActiveScene().name == "TextScenario" && _continueDialogueAfterChallengeRemoval)
			{
				_continueDialogueAfterChallengeRemoval = false;
				DialogueLua.SetVariable("chosenOrb", _continueDialogueAfterChallengeRemovalOrb);
				DialogueLua.SetVariable("upgradedOrb", _continueDialogueAfterChallengeRemovalUpgrade);
				DialogueManager.UpdateResponses();
				Sequencer.Message("MenuClosed");
				_continueDialogueAfterChallengeRemovalOrb = "";
				_continueDialogueAfterChallengeRemovalUpgrade = "";
			}
			StartCoroutine(MarkClosedAtEndOfFrame());
			onInventoryViewToggled(open: false);
			if (playfieldMouseDetector != null)
			{
				playfieldMouseDetector.SetActive(_wasPlayfieldDetectorActive);
			}
			if (_previouslySelected != null)
			{
				_rewiredEventSystem.SetSelectedGameObject(_previouslySelected);
			}
		}

		private IEnumerator MarkClosedAtEndOfFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			wasOpenThisFrame = false;
		}

		public void ShowInventoryForOrbRemoval(bool skippable = true, bool showPebballs = true)
		{
			_state = InventoryViewState.Removing;
			inventoryGridPopulator.onlyShowUpgrades = false;
			inventoryGridPopulator.isRemoving = true;
			inventoryGridPopulator.showPebballs = showPebballs;
			_skippable = skippable;
			ShowInventory();
			ToggleBackButton(skippable);
		}

		public void ShowInventoryForMultipleOrbRemovals(int amount, bool skippable = true, bool showPebballs = true)
		{
			_orbRemovalQueueLength = amount - 1;
			_state = InventoryViewState.RemovingMulitpleOrbs;
			inventoryGridPopulator.onlyShowUpgrades = false;
			inventoryGridPopulator.isRemoving = true;
			inventoryGridPopulator.showPebballs = showPebballs;
			_skippable = skippable;
			_orbRemovalQueueShowState = InventoryViewState.RemovingMulitpleOrbs;
			_orbRemovalQueueSkipable = skippable;
			_orbRemovalQueueCloseable = true;
			_orbRemovalQueueShowPebballs = showPebballs;
			ShowInventory();
			ToggleBackButton(skippable);
		}

		public void ShowInventoryForOrbRemovalShop()
		{
			_state = InventoryViewState.RemovingShop;
			inventoryGridPopulator.onlyShowUpgrades = false;
			inventoryGridPopulator.isRemoving = true;
			ShowInventory();
			ToggleBackButton(showButton: true);
		}

		public void QueueForceShowInventoryForOrbRemovalChallenge()
		{
			_state = InventoryViewState.RemovingViaChallenge;
			inventoryGridPopulator.onlyShowUpgrades = false;
			inventoryGridPopulator.isRemoving = true;
			inventoryGridPopulator.showPebballs = false;
			_skippable = false;
			_orbRemovalQueueShowState = InventoryViewState.RemovingViaChallenge;
			_orbRemovalQueueSkipable = false;
			_orbRemovalQueueCloseable = false;
			_orbRemovalQueueShowPebballs = false;
			if (isOpen)
			{
				UpdateMultiOrbRemovalText();
				inventoryGridPopulator.UpdateGrid();
				_orbRemovalQueueLength++;
			}
			else
			{
				ShowInventory();
			}
			ToggleBackButton(showButton: false);
			ToggleCloseButton(showButton: false);
		}

		private void StartNextOrbRemoval()
		{
			_state = _orbRemovalQueueShowState;
			inventoryGridPopulator.onlyShowUpgrades = false;
			inventoryGridPopulator.isRemoving = true;
			inventoryGridPopulator.showPebballs = _orbRemovalQueueShowPebballs;
			_skippable = _orbRemovalQueueSkipable;
			ShowInventory();
			ToggleBackButton(_orbRemovalQueueSkipable);
			ToggleCloseButton(_orbRemovalQueueCloseable);
		}

		private void CloseIfRemovingMultipleOrbs()
		{
			if (_state == InventoryViewState.RemovingViaChallenge || _state == InventoryViewState.RemovingMulitpleOrbs)
			{
				_skippable = true;
				ToggleBackButton(showButton: true);
				Close();
			}
		}

		private void UpdateMultiOrbRemovalText()
		{
			if (_state != InventoryViewState.RemovingMulitpleOrbs && _state != InventoryViewState.RemovingViaChallenge)
			{
				removeTextContainer.SetActive(value: false);
				return;
			}
			if (_orbRemovalQueueLength <= 0)
			{
				removeMultipleOrbsLoc.Term = "Menu/Store/remove_orb_label";
			}
			else
			{
				removeMultipleOrbsLocParamManager.SetParameterValue("ORB_REMOVALS_LEFT", (_orbRemovalQueueLength + 1).ToString());
				removeMultipleOrbsLoc.Term = "Menu/multiple_orb_removals_left";
			}
			removeTextContainer.gameObject.SetActive(value: true);
		}

		public void ShowInventoryForOrbUpgrade(bool skippable = true)
		{
			_state = InventoryViewState.Upgrading;
			inventoryGridPopulator.onlyShowUpgrades = true;
			_skippable = skippable;
			ShowInventory();
			ToggleBackButton(skippable);
		}

		public void ShowInventoryForOrbDuplication(bool skippable = true)
		{
			_state = InventoryViewState.Duplicating;
			inventoryGridPopulator.onlyShowUpgrades = false;
			ShowInventory();
			ToggleBackButton(skippable);
		}

		public void ToggleBackButton(bool showButton)
		{
			if (backButton != null)
			{
				backButton.gameObject.SetActive(showButton);
			}
		}

		private void ToggleCloseButton(bool showButton)
		{
			if (closeButton != null)
			{
				closeButton.SetActive(showButton);
			}
		}

		private void SetCurtainInactive()
		{
			_wasPlayfieldDetectorActive = true;
		}
	}
}
