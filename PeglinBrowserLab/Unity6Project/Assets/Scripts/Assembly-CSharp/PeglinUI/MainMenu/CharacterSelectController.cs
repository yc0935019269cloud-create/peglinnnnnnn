using System;
using System.Collections;
using System.Collections.Generic;
using Peglin.ClassSystem;
using PeglinUI.LoadoutManager;
using PeglinUtils;
using Saving;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu
{
	public class CharacterSelectController : MonoBehaviour
	{
		public delegate void SelectedClassChanged(SelectableCharacter newClass);

		public static Action OnOrbDropRequestFailed = () =>
		{
		};

		public SelectedClassChanged OnSelectedClassChanged = delegate
		{
		};

		public Button leftArrow;

		public Button rightArrow;

		private bool _classesLoaded;

		public GameObject[] characterSelectPrefabs;

		private List<SelectableCharacter> _characterSelectInstances = new List<SelectableCharacter>();

		private List<SelectableCharacter> _availableCharacters = new List<SelectableCharacter>();

		public float initialSpacing = 3f;

		public float loadoutShowcasePositionX = 9.5f;

		public Class currentlySelectedClass;

		[HideInInspector]
		public bool isInLoadoutShowcase;

		private int _currentClassIndex;

		private bool _playClicked;

		public static int NumClassesUnlocked = 1;

		public SelectableCharacter currentlySelectedInstance { get; protected set; }

		public bool currentlySelectedClassUnlocked => currentlySelectedInstance.IsUnlocked;

		private void OnEnable()
		{
			MainMenuRandomOrbDrop.OnNextDropPosChosen = (MainMenuRandomOrbDrop.PachinkoBallDropEvent)Delegate.Combine(MainMenuRandomOrbDrop.OnNextDropPosChosen, new MainMenuRandomOrbDrop.PachinkoBallDropEvent(ChooseCharacterToMove));
			MainMenuRandomOrbDrop.OnPachinkoBallDropRequest = (MainMenuRandomOrbDrop.PachinkoBallDropEvent)Delegate.Combine(MainMenuRandomOrbDrop.OnPachinkoBallDropRequest, new MainMenuRandomOrbDrop.PachinkoBallDropEvent(TellCharacterToDropOrb));
			PersistentPlayerData.OnAchievementsLoaded = (PersistentPlayerData.PersistentPlayerDataEvent)Delegate.Combine(PersistentPlayerData.OnAchievementsLoaded, new PersistentPlayerData.PersistentPlayerDataEvent(EnableUnlockedCharacters));
			PeglinUI.LoadoutManager.LoadoutManager.OnPlayClicked = (PeglinUI.LoadoutManager.LoadoutManager.PlayClickedEvent)Delegate.Combine(PeglinUI.LoadoutManager.LoadoutManager.OnPlayClicked, new PeglinUI.LoadoutManager.LoadoutManager.PlayClickedEvent(DisableCharacterSelection));
		}

		private void DisableCharacterSelection()
		{
			foreach (SelectableCharacter characterSelectInstance in _characterSelectInstances)
			{
				characterSelectInstance.enabled = false;
			}
			_playClicked = true;
		}

		private void OnDisable()
		{
			MainMenuRandomOrbDrop.OnNextDropPosChosen = (MainMenuRandomOrbDrop.PachinkoBallDropEvent)Delegate.Remove(MainMenuRandomOrbDrop.OnNextDropPosChosen, new MainMenuRandomOrbDrop.PachinkoBallDropEvent(ChooseCharacterToMove));
			MainMenuRandomOrbDrop.OnPachinkoBallDropRequest = (MainMenuRandomOrbDrop.PachinkoBallDropEvent)Delegate.Remove(MainMenuRandomOrbDrop.OnPachinkoBallDropRequest, new MainMenuRandomOrbDrop.PachinkoBallDropEvent(TellCharacterToDropOrb));
			PersistentPlayerData.OnAchievementsLoaded = (PersistentPlayerData.PersistentPlayerDataEvent)Delegate.Remove(PersistentPlayerData.OnAchievementsLoaded, new PersistentPlayerData.PersistentPlayerDataEvent(EnableUnlockedCharacters));
			PeglinUI.LoadoutManager.LoadoutManager.OnPlayClicked = (PeglinUI.LoadoutManager.LoadoutManager.PlayClickedEvent)Delegate.Remove(PeglinUI.LoadoutManager.LoadoutManager.OnPlayClicked, new PeglinUI.LoadoutManager.LoadoutManager.PlayClickedEvent(DisableCharacterSelection));
		}

		private IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (PersistentPlayerData.Instance.AchievementsLoaded && !_classesLoaded)
			{
				EnableUnlockedCharacters();
			}
		}

		public void ScrollClassRight()
		{
			if (!_playClicked)
			{
				int count = _availableCharacters.Count;
				if (_currentClassIndex + 1 >= count)
				{
					_currentClassIndex = 0;
				}
				else
				{
					_currentClassIndex++;
				}
				ChangeClass(_currentClassIndex);
			}
		}

		public void ScrollClassLeft()
		{
			if (!_playClicked)
			{
				int count = _availableCharacters.Count;
				if (_currentClassIndex - 1 < 0)
				{
					_currentClassIndex = count - 1;
				}
				else
				{
					_currentClassIndex--;
				}
				ChangeClass(_currentClassIndex);
			}
		}

		private void ChangeClass(int index)
		{
			if (!isInLoadoutShowcase && !_playClicked)
			{
				currentlySelectedInstance.DeselectCharacter();
				currentlySelectedClass = _availableCharacters[index].characterClass;
				currentlySelectedInstance = _availableCharacters[index];
				currentlySelectedInstance.SelectCharacter();
				OnSelectedClassChanged?.Invoke(currentlySelectedInstance);
			}
		}

		private void ChangeClass(SelectableCharacter character)
		{
			if (_playClicked)
			{
				return;
			}
			for (int i = 0; i < _availableCharacters.Count; i++)
			{
				if (_availableCharacters[i].characterClass == character.characterClass)
				{
					ChangeClass(i);
					return;
				}
			}
			Debug.LogError($"Asked for class not in our available class list {character.characterClass}");
		}

		private void ChooseCharacterToMove(Vector2 pos)
		{
			SelectableCharacter selectableCharacter = FindClosestPeglin(pos, idle: true);
			if (selectableCharacter != null)
			{
				selectableCharacter.MoveToPoint(pos);
			}
		}

		private SelectableCharacter FindClosestPeglin(Vector2 pos, bool idle = false)
		{
			float num = float.MaxValue;
			SelectableCharacter result = null;
			foreach (SelectableCharacter characterSelectInstance in _characterSelectInstances)
			{
				if (!characterSelectInstance.droppingOrb && characterSelectInstance.IsUnlocked)
				{
					float magnitude = (characterSelectInstance.transform.position - (Vector3)pos).magnitude;
					if (magnitude < num)
					{
						num = magnitude;
						result = characterSelectInstance;
					}
				}
			}
			return result;
		}

		private void TellCharacterToDropOrb(Vector2 pos)
		{
			SelectableCharacter selectableCharacter = FindClosestPeglin(pos, idle: true);
			if (selectableCharacter != null)
			{
				selectableCharacter.MoveAndDropOrb(pos);
			}
			else
			{
				OnOrbDropRequestFailed?.Invoke();
			}
		}

		private void EnableUnlockedCharacters()
		{
			if (!_classesLoaded)
			{
				_classesLoaded = true;
				int num = 0;
				GameObject[] array = characterSelectPrefabs;
				for (int i = 0; i < array.Length; i++)
				{
					GameObject obj = UnityEngine.Object.Instantiate(array[i].gameObject, base.transform);
					obj.transform.position += Vector3.right * initialSpacing * num++;
					SelectableCharacter component = obj.GetComponent<SelectableCharacter>();
					SpriteRenderer componentInChildren = obj.GetComponentInChildren<SpriteRenderer>();
					component.OnClassMouseSelected = (SelectableCharacter.ClassMouseSelectionAction)Delegate.Combine(component.OnClassMouseSelected, new SelectableCharacter.ClassMouseSelectionAction(ChangeClass));
					componentInChildren.sortingOrder = num;
					component.loadoutShowcasePositionX = loadoutShowcasePositionX;
					_characterSelectInstances.Add(component);
					_availableCharacters.Add(component);
				}
				NumClassesUnlocked = num;
				currentlySelectedClass = _characterSelectInstances[0].characterClass;
				currentlySelectedInstance = _characterSelectInstances[0];
				currentlySelectedInstance.gameObject.SetActive(value: true);
				OnSelectedClassChanged?.Invoke(currentlySelectedInstance);
			}
		}
	}
}
