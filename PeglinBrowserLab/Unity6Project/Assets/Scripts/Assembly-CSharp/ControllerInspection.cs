using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Attacks;
using Battle.Enemies;
using Battle.StatusEffects;
using Challenges;
using Peglin.PegMinigame;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUI.OrbDisplay;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerInspection : MonoBehaviour
{
	private enum InspectionState
	{
		NONE = 0,
		PEG_MINIGAME_REWARDS = 1,
		OWNED_RELICS = 2,
		UPCOMING_ORBS = 3,
		STATUS_EFFECTS = 4,
		CHALLENGES = 5
	}

	private const int FRAMES_OF_DIFFERENT_OBJECT_SELECTION_TO_RESET_AT = 2;

	private static InspectionState _currentState;

	private InspectionState _prevState;

	public GameObject _relicButtonPrompt;

	public ChallengeUI challengeUI;

	private DeckInfoManager _deckInfoManager;

	private Player _player;

	private RelicUI _relicUI;

	private TargetingManager _targetingManager;

	private EnemyManager _enemyManager;

	private PlayerStatusEffectController _playerStatusEffectController;

	private GameObject _gameController;

	private PegMinigameManager _pegMinigameManager;

	public static GameObject LastSelectedObject;

	private bool _battleEnded;

	private bool _inPegMinigame;

	private int _differentObjectSelectedForFrames;

	private List<GameObject> _statusEffectIconObjects = new List<GameObject>();

	private List<GameObject> _upcommingOrbDisplaySelectorObjects = new List<GameObject>();

	private int _numInspectionStates;

	public static bool CurrentlyInspecting => _currentState != InspectionState.NONE;

	private void Awake()
	{
		_numInspectionStates = Enum.GetNames(typeof(InspectionState)).Length;
	}

	private void Start()
	{
		_player = ReInput.players.GetPlayer(0);
		GameObject gameObject = GameObject.FindWithTag("PersistentUI");
		_relicUI = gameObject.GetComponentInChildren<RelicUI>();
		_gameController = GameObject.FindWithTag("GameController");
		if (_gameController != null)
		{
			_targetingManager = _gameController.GetComponent<TargetingManager>();
			_enemyManager = _gameController.GetComponent<EnemyManager>();
		}
		_playerStatusEffectController = UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(BattleEnded));
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OrbFired));
		AttackManager.OnAttackPerformed = (AttackManager.AttackPerformed)Delegate.Combine(AttackManager.OnAttackPerformed, new AttackManager.AttackPerformed(OnAttack));
		BattleController.OnOrbDiscarded = (Action)Delegate.Combine(BattleController.OnOrbDiscarded, new Action(OnDiscard));
		StatusEffectIconManager.OnEnemyStatusEffectsUpdated = (StatusEffectIconManager.EnemyStatusEffectsUpdated)Delegate.Combine(StatusEffectIconManager.OnEnemyStatusEffectsUpdated, new StatusEffectIconManager.EnemyStatusEffectsUpdated(OnStatusEffectsUpdated));
		PlayerStatusEffectController.OnPlayerStatusEffectsUpdated = (PlayerStatusEffectController.PlayerStatusEffectsUpdated)Delegate.Combine(PlayerStatusEffectController.OnPlayerStatusEffectsUpdated, new PlayerStatusEffectController.PlayerStatusEffectsUpdated(OnStatusEffectsUpdated));
		NavOnlyController.NavigationTriggered = (Action)Delegate.Combine(NavOnlyController.NavigationTriggered, new Action(OnPegMinigameStartNavigating));
		DeckInfoManager.onActiveOrbScaleCompleted = (DeckInfoManager.DeckInfoAnim)Delegate.Combine(DeckInfoManager.onActiveOrbScaleCompleted, new DeckInfoManager.DeckInfoAnim(UpdateInspectionPromptsVisibility));
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(BattleEnded));
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(OrbFired));
		AttackManager.OnAttackPerformed = (AttackManager.AttackPerformed)Delegate.Remove(AttackManager.OnAttackPerformed, new AttackManager.AttackPerformed(OnAttack));
		BattleController.OnOrbDiscarded = (Action)Delegate.Remove(BattleController.OnOrbDiscarded, new Action(OnDiscard));
		StatusEffectIconManager.OnEnemyStatusEffectsUpdated = (StatusEffectIconManager.EnemyStatusEffectsUpdated)Delegate.Remove(StatusEffectIconManager.OnEnemyStatusEffectsUpdated, new StatusEffectIconManager.EnemyStatusEffectsUpdated(OnStatusEffectsUpdated));
		PlayerStatusEffectController.OnPlayerStatusEffectsUpdated = (PlayerStatusEffectController.PlayerStatusEffectsUpdated)Delegate.Remove(PlayerStatusEffectController.OnPlayerStatusEffectsUpdated, new PlayerStatusEffectController.PlayerStatusEffectsUpdated(OnStatusEffectsUpdated));
		NavOnlyController.NavigationTriggered = (Action)Delegate.Remove(NavOnlyController.NavigationTriggered, new Action(OnPegMinigameStartNavigating));
		DeckInfoManager.onActiveOrbScaleCompleted = (DeckInfoManager.DeckInfoAnim)Delegate.Remove(DeckInfoManager.onActiveOrbScaleCompleted, new DeckInfoManager.DeckInfoAnim(UpdateInspectionPromptsVisibility));
	}

	private void Update()
	{
		if (ControllerInputManager.primaryInputType != InputType.Touch && _player.GetButtonDown("RelicAndOrbInfo") && !PauseMenu.Paused && !GameBlockingWindow.windowOpen)
		{
			CycleInspectionState();
		}
		if (GameBlockingWindow.windowOpen && CurrentlyInspecting)
		{
			ResetInspectionState(resetSelection: false);
		}
		if (CurrentlyInspecting && _player.GetButtonUp("Back"))
		{
			StartCoroutine(WaitAndResetInspectionState(resetSelection: true));
		}
		if (CurrentlyInspecting != CheckSelectionIsWithinControllerInspectableObjects())
		{
			_differentObjectSelectedForFrames++;
			if (_differentObjectSelectedForFrames >= 2)
			{
				ResetInspectionState(!CurrentlyInspecting);
			}
		}
		else
		{
			_differentObjectSelectedForFrames = 0;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		LastSelectedObject = null;
		_battleEnded = false;
		_inPegMinigame = false;
		if (SceneManager.GetActiveScene().name == "PegMinigame")
		{
			_inPegMinigame = true;
			_pegMinigameManager = UnityEngine.Object.FindObjectOfType<PegMinigameManager>();
		}
		ResetInspectionState(resetSelection: true);
	}

	private void BattleEnded()
	{
		_battleEnded = true;
		ResetInspectionState(resetSelection: false);
	}

	private void OrbFired(Vector2 _vector)
	{
		ResetInspectionState(resetSelection: true);
	}

	private void OnAttack(string trigger)
	{
		if (_currentState == InspectionState.UPCOMING_ORBS || _currentState == InspectionState.STATUS_EFFECTS)
		{
			ResetInspectionState(resetSelection: true);
		}
	}

	private void OnDiscard()
	{
		if (_currentState == InspectionState.UPCOMING_ORBS)
		{
			ResetInspectionState(resetSelection: true);
		}
	}

	private void OnStatusEffectsUpdated()
	{
		if (_currentState == InspectionState.STATUS_EFFECTS)
		{
			ResetInspectionState(resetSelection: true);
		}
	}

	private void OnPegMinigameStartNavigating()
	{
		if (_inPegMinigame)
		{
			ResetInspectionState(resetSelection: true);
		}
	}

	private IEnumerator WaitAndResetInspectionState(bool resetSelection)
	{
		yield return null;
		ResetInspectionState(resetSelection);
	}

	public void ResetInspectionState(bool resetSelection)
	{
		_differentObjectSelectedForFrames = 0;
		_currentState = InspectionState.NONE;
		UpdateInspectionPromptsVisibility();
		if (resetSelection)
		{
			if (LastSelectedObject != null && LastSelectedObject != base.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(LastSelectedObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
		}
	}

	private void CycleInspectionState()
	{
		if (BattleController.BattleActive && _deckInfoManager == null)
		{
			_deckInfoManager = UnityEngine.Object.FindObjectOfType<DeckInfoManager>(includeInactive: true);
		}
		if (BattleController.BattleActive && _gameController == null)
		{
			_gameController = GameObject.FindWithTag("GameController");
		}
		if (_targetingManager == null && _gameController != null)
		{
			_targetingManager = _gameController.GetComponent<TargetingManager>();
		}
		if (_enemyManager == null && _gameController != null)
		{
			_enemyManager = _gameController.GetComponent<EnemyManager>();
		}
		if (_playerStatusEffectController == null)
		{
			_playerStatusEffectController = UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>();
		}
		_currentState = (InspectionState)((int)(_currentState + 1) % Enum.GetNames(typeof(InspectionState)).Length);
		switch (_currentState)
		{
		case InspectionState.NONE:
			ResetInspectionState(resetSelection: true);
			break;
		case InspectionState.PEG_MINIGAME_REWARDS:
			if (!CanInspectPegMinigameRewards())
			{
				CycleInspectionState();
				return;
			}
			SaveLastSelectedObjectIfNeeded();
			EventSystem.current.SetSelectedGameObject(_pegMinigameManager.leftMostReward);
			break;
		case InspectionState.OWNED_RELICS:
			if (!CanInspectOwnedRelics())
			{
				CycleInspectionState();
				return;
			}
			SaveLastSelectedObjectIfNeeded();
			_relicUI.imageCarousel.SelectFirstVisibleObject(forceNonPointerBased: false);
			break;
		case InspectionState.UPCOMING_ORBS:
			if (!CanInspectUpcomingOrbs())
			{
				CycleInspectionState();
				return;
			}
			SaveLastSelectedObjectIfNeeded();
			_deckInfoManager.UpdateUINavigation();
			_upcommingOrbDisplaySelectorObjects.Clear();
			if (_deckInfoManager.currentOrb != null)
			{
				_upcommingOrbDisplaySelectorObjects.Add(_deckInfoManager.currentOrb.GetComponentInChildren<UpcomingOrbDisplay>().gameObject);
			}
			foreach (GameObject displayOrb in _deckInfoManager.displayOrbs)
			{
				_upcommingOrbDisplaySelectorObjects.Add(displayOrb.GetComponentInChildren<UpcomingOrbDisplay>().gameObject);
			}
			EventSystem.current.SetSelectedGameObject(_deckInfoManager.currentOrb.GetComponentInChildren<UpcomingOrbDisplay>().gameObject);
			break;
		case InspectionState.STATUS_EFFECTS:
			if (!CanInspectStatusEffects())
			{
				CycleInspectionState();
				return;
			}
			SaveLastSelectedObjectIfNeeded();
			SelectInitialEffectIcon();
			break;
		case InspectionState.CHALLENGES:
			if (!CanInspectChallenges())
			{
				CycleInspectionState();
				return;
			}
			SaveLastSelectedObjectIfNeeded();
			challengeUI.SetLeftmostIconSelected();
			break;
		}
		UpdateInspectionPromptsVisibility();
		_prevState = _currentState;
	}

	private void SaveLastSelectedObjectIfNeeded()
	{
		if (_prevState == InspectionState.NONE)
		{
			LastSelectedObject = EventSystem.current.currentSelectedGameObject;
		}
	}

	private void UpdateInspectionPromptsVisibility()
	{
		if (_pegMinigameManager != null)
		{
			_pegMinigameManager.rewardInspectionButtonPrompt.SetActive(value: false);
		}
		_relicButtonPrompt.SetActive(value: false);
		if (_deckInfoManager != null)
		{
			_deckInfoManager.orbInspectionButtonPrompt.SetActive(value: false);
		}
		if (_playerStatusEffectController != null && _playerStatusEffectController.statusEffectInspectionPrompt != null)
		{
			_playerStatusEffectController.statusEffectInspectionPrompt.SetActive(value: false);
		}
		challengeUI.inspectionPrompt.SetActive(value: false);
		int num = (int)_currentState;
		if (num != _numInspectionStates - 1)
		{
			num = (num + 1) % _numInspectionStates;
		}
		for (int i = 0; i < _numInspectionStates; i++)
		{
			switch (num)
			{
			case 1:
				if (CanInspectPegMinigameRewards() && _pegMinigameManager != null)
				{
					_pegMinigameManager.rewardInspectionButtonPrompt.SetActive(value: true);
					return;
				}
				break;
			case 2:
				if (CanInspectOwnedRelics())
				{
					_relicButtonPrompt.SetActive(value: true);
					return;
				}
				break;
			case 3:
				if (CanInspectUpcomingOrbs() && _deckInfoManager != null)
				{
					_deckInfoManager.orbInspectionButtonPrompt.SetActive(value: true);
					return;
				}
				break;
			case 4:
				if (CanInspectStatusEffects() && _playerStatusEffectController != null && _playerStatusEffectController.statusEffectInspectionPrompt != null)
				{
					_playerStatusEffectController.statusEffectInspectionPrompt.SetActive(value: true);
					return;
				}
				break;
			case 5:
				if (CanInspectChallenges())
				{
					challengeUI.inspectionPrompt.SetActive(value: true);
					return;
				}
				break;
			}
			num = (num + 1) % _numInspectionStates;
		}
	}

	public bool UpdateStatusEffectIconNavigation()
	{
		_statusEffectIconObjects.Clear();
		List<StatusEffectIcon> list = null;
		if (_playerStatusEffectController != null && _playerStatusEffectController.statusEffectInspectionPrompt != null)
		{
			list = new List<StatusEffectIcon>(_playerStatusEffectController.GetComponentsInChildren<StatusEffectIcon>());
			list.Sort((StatusEffectIcon x, StatusEffectIcon y) => (int)Mathf.Sign(x.gameObject.transform.position.x - y.gameObject.transform.position.x));
			foreach (StatusEffectIcon item in list)
			{
				_statusEffectIconObjects.Add(item.gameObject);
			}
		}
		if (_enemyManager == null)
		{
			if (list != null)
			{
				return list.Count > 0;
			}
			return false;
		}
		List<Enemy> list2 = new List<Enemy>();
		List<Enemy> list3 = new List<Enemy>();
		foreach (Enemy enemy2 in _enemyManager.Enemies)
		{
			if (enemy2.StatusEffects.Length != 0)
			{
				if (enemy2.IsFlying)
				{
					list2.Add(enemy2);
				}
				else
				{
					list3.Add(enemy2);
				}
			}
		}
		BarricadeEnemy[] array = UnityEngine.Object.FindObjectsOfType<BarricadeEnemy>();
		foreach (Enemy enemy in array)
		{
			if (enemy.StatusEffects.Length != 0)
			{
				if (enemy.GetComponentInParent<Enemy>().IsFlying)
				{
					list2.Add(enemy);
				}
				else
				{
					list3.Add(enemy);
				}
			}
		}
		list2.Sort((Enemy x, Enemy y) => (int)Mathf.Sign(x.gameObject.transform.position.x - y.gameObject.transform.position.x));
		list3.Sort((Enemy x, Enemy y) => (int)Mathf.Sign(x.gameObject.transform.position.x - y.gameObject.transform.position.x));
		Dictionary<Enemy, StatusEffectIcon[]> dictionary = new Dictionary<Enemy, StatusEffectIcon[]>();
		Dictionary<Enemy, StatusEffectIcon[]> dictionary2 = new Dictionary<Enemy, StatusEffectIcon[]>();
		Dictionary<Enemy, float> flyingEnemiesSlots = new Dictionary<Enemy, float>();
		Dictionary<Enemy, float> groundEnemiesSlots = new Dictionary<Enemy, float>();
		foreach (Enemy item2 in list2)
		{
			List<StatusEffectIcon> list4 = new List<StatusEffectIcon>(item2.enemyUICanvas.GetComponentsInChildren<StatusEffectIcon>());
			foreach (StatusEffectIcon item3 in list4)
			{
				_statusEffectIconObjects.Add(item3.gameObject);
			}
			list4.Sort((StatusEffectIcon x, StatusEffectIcon y) => (int)Mathf.Sign(x.gameObject.transform.position.x - y.gameObject.transform.position.x));
			dictionary.Add(item2, list4.ToArray());
			float num2 = _enemyManager.GetSlotIndexForEnemy(item2, out var isStationary);
			num2 = (isStationary ? (num2 + 0.5f) : num2);
			flyingEnemiesSlots.Add(item2, num2);
		}
		foreach (Enemy item4 in list3)
		{
			List<StatusEffectIcon> list5 = new List<StatusEffectIcon>(item4.enemyUICanvas.GetComponentsInChildren<StatusEffectIcon>());
			foreach (StatusEffectIcon item5 in list5)
			{
				_statusEffectIconObjects.Add(item5.gameObject);
			}
			list5.Sort((StatusEffectIcon x, StatusEffectIcon y) => (int)Mathf.Sign(x.gameObject.transform.position.x - y.gameObject.transform.position.x));
			dictionary2.Add(item4, list5.ToArray());
			float num3 = _enemyManager.GetSlotIndexForEnemy(item4, out var isStationary2);
			num3 = (isStationary2 ? (num3 + 0.5f) : num3);
			groundEnemiesSlots.Add(item4, num3);
		}
		int num4 = 0;
		foreach (KeyValuePair<Enemy, StatusEffectIcon[]> item6 in dictionary)
		{
			Enemy key = item6.Key;
			StatusEffectIcon[] value = item6.Value;
			for (int num5 = 0; num5 < value.Length; num5++)
			{
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (num5 == 0)
				{
					if (num4 == 0)
					{
						if (list.Count > 0)
						{
							navigation.selectOnLeft = list[list.Count - 1].selectable;
						}
					}
					else
					{
						Enemy key2 = list2[num4 - 1];
						int num6 = dictionary[key2].Length - 1;
						navigation.selectOnLeft = dictionary[key2][num6].selectable;
					}
				}
				else
				{
					navigation.selectOnLeft = value[num5 - 1].selectable;
				}
				if (num5 == value.Length - 1)
				{
					if (num4 != dictionary.Count - 1)
					{
						Enemy key3 = list2[num4 + 1];
						navigation.selectOnRight = dictionary[key3][0].selectable;
					}
				}
				else
				{
					navigation.selectOnRight = value[num5 + 1].selectable;
				}
				if (list3.Count > 0)
				{
					float mySlotIndex = flyingEnemiesSlots[key];
					List<Enemy> list6 = new List<Enemy>(list3.ToArray());
					list6.Sort((Enemy x, Enemy y) => (int)Mathf.Sign(Mathf.Abs(mySlotIndex - groundEnemiesSlots[x]) - Mathf.Abs(mySlotIndex - groundEnemiesSlots[y])));
					Enemy key4 = list6[0];
					float num7 = groundEnemiesSlots[key4];
					int num8 = dictionary2[key4].Length - 1;
					if (num7 < mySlotIndex)
					{
						navigation.selectOnDown = dictionary2[key4][num8].selectable;
					}
					else if (num7 == mySlotIndex)
					{
						if (dictionary2[key4].Length - 1 < num5)
						{
							navigation.selectOnDown = dictionary2[key4][num8].selectable;
						}
						else
						{
							navigation.selectOnDown = dictionary2[key4][num5].selectable;
						}
					}
					else
					{
						navigation.selectOnDown = dictionary2[key4][0].selectable;
					}
				}
				value[num5].selectable.navigation = navigation;
			}
			num4++;
		}
		num4 = 0;
		foreach (KeyValuePair<Enemy, StatusEffectIcon[]> item7 in dictionary2)
		{
			Enemy key5 = item7.Key;
			StatusEffectIcon[] value2 = item7.Value;
			for (int num9 = 0; num9 < value2.Length; num9++)
			{
				Navigation navigation2 = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (num9 == 0)
				{
					if (num4 == 0)
					{
						if (list.Count > 0)
						{
							navigation2.selectOnLeft = list[list.Count - 1].selectable;
						}
					}
					else
					{
						Enemy key6 = list3[num4 - 1];
						int num10 = dictionary2[key6].Length - 1;
						navigation2.selectOnLeft = dictionary2[key6][num10].selectable;
					}
				}
				else
				{
					navigation2.selectOnLeft = value2[num9 - 1].selectable;
				}
				if (num9 == value2.Length - 1)
				{
					if (num4 != dictionary2.Count - 1)
					{
						Enemy key7 = list3[num4 + 1];
						navigation2.selectOnRight = dictionary2[key7][0].selectable;
					}
				}
				else
				{
					navigation2.selectOnRight = value2[num9 + 1].selectable;
				}
				if (list2.Count > 0)
				{
					float mySlotIndex2 = groundEnemiesSlots[key5];
					List<Enemy> list7 = new List<Enemy>(list2.ToArray());
					list7.Sort((Enemy x, Enemy y) => (int)Mathf.Sign(Mathf.Abs(mySlotIndex2 - flyingEnemiesSlots[x]) - Mathf.Abs(mySlotIndex2 - flyingEnemiesSlots[y])));
					Enemy key8 = list7[0];
					float num11 = flyingEnemiesSlots[key8];
					int num12 = dictionary[key8].Length - 1;
					if (num11 < mySlotIndex2)
					{
						navigation2.selectOnUp = dictionary[key8][num12].selectable;
					}
					else if (num11 == mySlotIndex2)
					{
						if (dictionary[key8].Length - 1 < num9)
						{
							navigation2.selectOnUp = dictionary[key8][num12].selectable;
						}
						else
						{
							navigation2.selectOnUp = dictionary[key8][num9].selectable;
						}
					}
					else
					{
						navigation2.selectOnUp = dictionary[key8][0].selectable;
					}
				}
				value2[num9].selectable.navigation = navigation2;
			}
			num4++;
		}
		for (int num13 = 0; num13 < list.Count; num13++)
		{
			Navigation navigation3 = new Navigation
			{
				mode = Navigation.Mode.Explicit
			};
			if (num13 != 0)
			{
				navigation3.selectOnLeft = list[num13 - 1].selectable;
			}
			if (num13 == list.Count - 1)
			{
				if (list3.Count > 0)
				{
					navigation3.selectOnRight = dictionary2[list3[0]][0].selectable;
				}
				else if (list2.Count > 0)
				{
					navigation3.selectOnRight = dictionary[list2[0]][0].selectable;
				}
			}
			else
			{
				navigation3.selectOnRight = list[num13 + 1].selectable;
			}
			if (list2.Count > 0)
			{
				navigation3.selectOnUp = dictionary[list2[0]][0].selectable;
			}
			list[num13].selectable.navigation = navigation3;
		}
		return list3.Count + dictionary.Count + list.Count > 0;
	}

	private void SelectInitialEffectIcon()
	{
		bool flag = false;
		if (_targetingManager != null && _targetingManager.currentTarget != null && _targetingManager.currentTarget.StatusEffects.Length != 0)
		{
			StatusEffectIcon[] componentsInChildren = _targetingManager.currentTarget.GetComponentsInChildren<StatusEffectIcon>();
			EventSystem.current.SetSelectedGameObject(componentsInChildren[0].gameObject);
			flag = true;
		}
		if (!flag && _enemyManager != null)
		{
			foreach (Enemy enemy in _enemyManager.Enemies)
			{
				if (enemy.StatusEffects.Length != 0)
				{
					StatusEffectIcon[] componentsInChildren2 = enemy.GetComponentsInChildren<StatusEffectIcon>();
					EventSystem.current.SetSelectedGameObject(componentsInChildren2[0].gameObject);
					flag = true;
				}
			}
		}
		if (!flag && _playerStatusEffectController != null)
		{
			StatusEffectIcon[] componentsInChildren3 = _playerStatusEffectController.GetComponentsInChildren<StatusEffectIcon>();
			if (componentsInChildren3 != null && componentsInChildren3.Length != 0)
			{
				EventSystem.current.SetSelectedGameObject(componentsInChildren3[0].gameObject);
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.LogWarning("Status icon navigation update found status effects, but now there are non.");
			CycleInspectionState();
		}
	}

	private bool CheckSelectionIsWithinControllerInspectableObjects()
	{
		switch (_currentState)
		{
		case InspectionState.NONE:
		{
			bool flag = false;
			if (_pegMinigameManager != null && _pegMinigameManager.allRewards != null)
			{
				flag |= _pegMinigameManager.allRewards.Contains(EventSystem.current.currentSelectedGameObject);
				if (flag)
				{
					return true;
				}
			}
			flag |= _relicUI.imageCarousel.elements.Contains(EventSystem.current.currentSelectedGameObject);
			if (flag)
			{
				return true;
			}
			if (_upcommingOrbDisplaySelectorObjects != null)
			{
				flag |= _upcommingOrbDisplaySelectorObjects.Contains(EventSystem.current.currentSelectedGameObject);
				if (flag)
				{
					return true;
				}
			}
			return flag | _statusEffectIconObjects.Contains(EventSystem.current.currentSelectedGameObject);
		}
		case InspectionState.PEG_MINIGAME_REWARDS:
			return _pegMinigameManager.allRewards.Contains(EventSystem.current.currentSelectedGameObject);
		case InspectionState.OWNED_RELICS:
			return _relicUI.imageCarousel.elements.Contains(EventSystem.current.currentSelectedGameObject);
		case InspectionState.UPCOMING_ORBS:
			return _upcommingOrbDisplaySelectorObjects.Contains(EventSystem.current.currentSelectedGameObject);
		case InspectionState.STATUS_EFFECTS:
			return _statusEffectIconObjects.Contains(EventSystem.current.currentSelectedGameObject);
		case InspectionState.CHALLENGES:
		{
			List<GameObject> list = new List<GameObject>();
			ChallengeIcon[] icons = challengeUI.icons;
			foreach (ChallengeIcon challengeIcon in icons)
			{
				list.Add(challengeIcon.gameObject);
			}
			return list.Contains(EventSystem.current.currentSelectedGameObject);
		}
		default:
			return true;
		}
	}

	private bool CanInspectPegMinigameRewards()
	{
		if (_inPegMinigame)
		{
			if (!(_pegMinigameManager == null))
			{
				return !_pegMinigameManager.chooseSuccess;
			}
			return true;
		}
		return false;
	}

	private bool CanInspectOwnedRelics()
	{
		if (_relicUI != null && _relicUI.imageCarousel != null)
		{
			return _relicUI.imageCarousel.elements.Count > 0;
		}
		return false;
	}

	private bool CanInspectUpcomingOrbs()
	{
		if (BattleController.BattleActive && _deckInfoManager == null)
		{
			_deckInfoManager = UnityEngine.Object.FindObjectOfType<DeckInfoManager>(includeInactive: true);
		}
		if (_deckInfoManager != null && !_battleEnded && _deckInfoManager.currentOrb != null)
		{
			if (BattleController.BattleActive)
			{
				return !BattleController.PreBattlePause;
			}
			return false;
		}
		return false;
	}

	private bool CanInspectStatusEffects()
	{
		if (!_battleEnded && (!BattleController.BattleActive || !BattleController.PreBattlePause))
		{
			return UpdateStatusEffectIconNavigation();
		}
		return false;
	}

	private bool CanInspectChallenges()
	{
		if (challengeUI.UIContainer.activeInHierarchy)
		{
			return challengeUI.icons.Length != 0;
		}
		return false;
	}
}
