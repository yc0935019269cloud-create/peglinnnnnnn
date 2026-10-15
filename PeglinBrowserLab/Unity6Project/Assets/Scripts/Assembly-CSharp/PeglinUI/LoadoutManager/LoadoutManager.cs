using System;
using System.Collections.Generic;
using Battle.Attacks;
using Cruciball;
using DG.Tweening;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUI.MainMenu;
using Relics;
using Rewired.Integration.UnityUI;
using Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutManager : MonoBehaviour
	{
		public delegate void PlayClickedEvent();

		public float canvasFadeTime = 0.6f;

		public int objectsPerLine = 5;

		public static PlayClickedEvent OnPlayClicked;

		[FormerlySerializedAs("loadoutOrbImagePrefab")]
		[Header("Prefab refs")]
		public GameObject loadoutImagePrefab;

		[Header("Scene Refs")]
		public Camera cam;

		public CharacterSelectController characterSelectController;

		public CanvasGroup characterSelectCanvasGroup;

		public CanvasGroup loadoutBaseDisplayCanvasGroup;

		public HorizontalLayoutGroup orbImagesContainer;

		public HorizontalLayoutGroup relicImagesContainer;

		[FormerlySerializedAs("imageCarousel")]
		public ImageCarousel orbImagesCarousel;

		public ImageCarousel relicImagesCarousel;

		public GameObject loadoutBasePanel;

		public LoadoutEditorOrbs orbLoadoutEditorPanel;

		public LoadoutEditorRelics relicLoadoutEditorPanel;

		public GameObject customLoadoutWarning;

		public GameObject editOrbsButton;

		public CruciballParticleToggle CruciballParticleSystem;

		public SeedInputPopup seedInputPopup;

		public SeedDisplay seedDisplay;

		[Header("ScriptableObject Refs")]
		[SerializeField]
		private OrbPool _availableOrbs;

		[SerializeField]
		private OrbPool _specialOrbs;

		[SerializeField]
		private RelicSet _globalRelics;

		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private DeckManager _deckManager;

		[SerializeField]
		private CruciballManager _cruciballManager;

		[Header("Class Loadouts")]
		public ClassLoadoutPairs[] ClassLoadouts;

		private Class selectedClass;

		[HideInInspector]
		public List<GameObject> StartingOrbs;

		[HideInInspector]
		public List<Relic> StartingRelics;

		private List<GameObject> _originalStartingOrbs = new List<GameObject>();

		private List<Relic> _originalStartingRelics = new List<Relic>();

		private RewiredEventSystem _rewiredEventSystem;

		private List<LoadoutOrbData> _startingOrbData = new List<LoadoutOrbData>();

		private bool _hasCustomLoadout;

		public ClassLoadoutData SelectedClassLoadout { get; protected set; }

		public bool hasCustomLoadout
		{
			get
			{
				return _hasCustomLoadout;
			}
			set
			{
				_hasCustomLoadout = value;
			}
		}

		private void Awake()
		{
			_relicManager.ResetRelicPools();
			ClassChange(Class.Peglin);
			characterSelectController.isInLoadoutShowcase = false;
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Combine(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(ClassChange));
			_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			LoadoutEditorOrbs loadoutEditorOrbs = orbLoadoutEditorPanel;
			loadoutEditorOrbs.OnAcceptButtonClicked = (LoadoutEditorOrbs.ButtonClicked)Delegate.Combine(loadoutEditorOrbs.OnAcceptButtonClicked, new LoadoutEditorOrbs.ButtonClicked(SaveStartingOrbsAndSwitchToBaseLoadout));
			LoadoutEditorOrbs loadoutEditorOrbs2 = orbLoadoutEditorPanel;
			loadoutEditorOrbs2.OnCancelButtonClicked = (LoadoutEditorOrbs.ButtonClicked)Delegate.Combine(loadoutEditorOrbs2.OnCancelButtonClicked, new LoadoutEditorOrbs.ButtonClicked(ReturnToBaseLoadoutFromOrbEditor));
			LoadoutEditorRelics loadoutEditorRelics = relicLoadoutEditorPanel;
			loadoutEditorRelics.OnAcceptButtonClicked = (LoadoutEditorRelics.ButtonClicked)Delegate.Combine(loadoutEditorRelics.OnAcceptButtonClicked, new LoadoutEditorRelics.ButtonClicked(SaveRelicsAndSwitchToBaseLoadout));
			LoadoutEditorRelics loadoutEditorRelics2 = relicLoadoutEditorPanel;
			loadoutEditorRelics2.OnCancelButtonClicked = (LoadoutEditorRelics.ButtonClicked)Delegate.Combine(loadoutEditorRelics2.OnCancelButtonClicked, new LoadoutEditorRelics.ButtonClicked(ReturnToBaseLoadoutFromRelicEditor));
			StaticGameData.classLoadouts = ClassLoadouts;
		}

		private void OnDestroy()
		{
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Remove(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(ClassChange));
			LoadoutEditorOrbs loadoutEditorOrbs = orbLoadoutEditorPanel;
			loadoutEditorOrbs.OnAcceptButtonClicked = (LoadoutEditorOrbs.ButtonClicked)Delegate.Remove(loadoutEditorOrbs.OnAcceptButtonClicked, new LoadoutEditorOrbs.ButtonClicked(SaveStartingOrbsAndSwitchToBaseLoadout));
			LoadoutEditorOrbs loadoutEditorOrbs2 = orbLoadoutEditorPanel;
			loadoutEditorOrbs2.OnCancelButtonClicked = (LoadoutEditorOrbs.ButtonClicked)Delegate.Remove(loadoutEditorOrbs2.OnCancelButtonClicked, new LoadoutEditorOrbs.ButtonClicked(ReturnToBaseLoadoutFromOrbEditor));
			LoadoutEditorRelics loadoutEditorRelics = relicLoadoutEditorPanel;
			loadoutEditorRelics.OnAcceptButtonClicked = (LoadoutEditorRelics.ButtonClicked)Delegate.Remove(loadoutEditorRelics.OnAcceptButtonClicked, new LoadoutEditorRelics.ButtonClicked(SaveRelicsAndSwitchToBaseLoadout));
			LoadoutEditorRelics loadoutEditorRelics2 = relicLoadoutEditorPanel;
			loadoutEditorRelics2.OnCancelButtonClicked = (LoadoutEditorRelics.ButtonClicked)Delegate.Remove(loadoutEditorRelics2.OnCancelButtonClicked, new LoadoutEditorRelics.ButtonClicked(ReturnToBaseLoadoutFromRelicEditor));
		}

		private void OnEnable()
		{
			_originalStartingOrbs.Clear();
			_originalStartingOrbs.AddRange(StartingOrbs);
			_originalStartingRelics.Clear();
			_originalStartingRelics.AddRange(StartingRelics);
			EventSystem.current.SetSelectedGameObject(editOrbsButton);
			StaticGameData.currentSeed = string.Empty;
			StaticGameData.seedSet = false;
		}

		public void SetupDataForNewGame()
		{
			AchievementManager.AchievementsOn = !_hasCustomLoadout && !seedInputPopup.hasCustomSeed;
			AddStartingOrbs();
			AddStartingRelics();
			PersistentPlayerData.Instance.ClobberBestiaryDataWithMemoryValues();
			OnPlayClicked?.Invoke();
		}

		private void ClassChange(SelectableCharacter character)
		{
			ClassChange(character.characterClass);
		}

		private void ClassChange(Class cl)
		{
			ClassLoadoutPairs[] classLoadouts = ClassLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				ClassLoadoutPairs classLoadoutPairs = classLoadouts[i];
				if (classLoadoutPairs.Class != cl)
				{
					continue;
				}
				selectedClass = cl;
				SelectedClassLoadout = classLoadoutPairs.Loadout;
				if (!hasCustomLoadout)
				{
					if (StartingOrbs == null)
					{
						StartingOrbs = new List<GameObject>();
					}
					StartingOrbs.Clear();
					StartingOrbs.AddRange(SelectedClassLoadout.StartingOrbs);
					if (StartingRelics == null)
					{
						StartingRelics = new List<Relic>();
					}
					_originalStartingOrbs.Clear();
					_originalStartingOrbs.AddRange(SelectedClassLoadout.StartingOrbs);
					StartingRelics.Clear();
					StartingRelics.AddRange(SelectedClassLoadout.StartingRelics);
					_originalStartingRelics.Clear();
					_originalStartingRelics.AddRange(SelectedClassLoadout.StartingRelics);
				}
				SetupLoadout();
			}
		}

		private void PopulateOrbUI(List<GameObject> orbsToPopulate)
		{
			_startingOrbData.Clear();
			foreach (Transform item in orbImagesContainer.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			Dictionary<string, LoadoutOrbData> dictionary = new Dictionary<string, LoadoutOrbData>();
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item2 in orbsToPopulate)
			{
				if (dictionary.ContainsKey(item2.name))
				{
					dictionary[item2.name].Amount++;
					continue;
				}
				LoadoutOrbData value = new LoadoutOrbData(item2, 1);
				dictionary.Add(item2.name, value);
			}
			foreach (LoadoutOrbData value2 in dictionary.Values)
			{
				LoadoutIcon component = UnityEngine.Object.Instantiate(loadoutImagePrefab, orbImagesContainer.transform).GetComponent<LoadoutIcon>();
				component.InitializeOrb(value2.orbPrefab, value2.Amount);
				list.Add(component.gameObject);
				_startingOrbData.Add(value2);
			}
			orbImagesCarousel.Initialize(list);
		}

		private void PopulateRelicUI(List<Relic> startingRelics)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Relic startingRelic in startingRelics)
			{
				LoadoutIcon component = UnityEngine.Object.Instantiate(loadoutImagePrefab, relicImagesContainer.transform).GetComponent<LoadoutIcon>();
				component.InitializeRelic(startingRelic);
				list.Add(component.gameObject);
			}
			relicImagesCarousel.Initialize(list);
		}

		private void SetupLoadout()
		{
			_relicManager.PopulateRelicPools(selectedClass);
			_deckManager.SetupClassOrbPools(selectedClass);
			AddStartingRelics();
		}

		public void PopulateRelicPools()
		{
			foreach (Relic relic in _globalRelics.relics)
			{
				switch (GetFinalRarityForRelic(relic))
				{
				case RelicRarity.COMMON:
					_relicManager.CommonRelicPool.Add(relic);
					break;
				case RelicRarity.RARE:
					_relicManager.RareRelicPool.Add(relic);
					break;
				case RelicRarity.BOSS:
					_relicManager.BossRelicPool.Add(relic);
					break;
				case RelicRarity.NONE:
					_relicManager.RareScenarioRelicPool.Add(relic);
					break;
				case RelicRarity.UNAVAILABLE:
					_relicManager.UnavailableRelicPool.Add(relic);
					break;
				}
			}
		}

		public void DefaultLoadout()
		{
			StartingOrbs.Clear();
			StartingOrbs.AddRange(SelectedClassLoadout.StartingOrbs);
			StartingRelics.Clear();
			StartingRelics.AddRange(SelectedClassLoadout.StartingRelics);
			PopulateOrbUI(StartingOrbs);
			PopulateRelicUI(StartingRelics);
			StaticGameData.currentSeed = string.Empty;
			seedDisplay.gameObject.SetActive(value: false);
			_hasCustomLoadout = false;
			seedInputPopup.hasCustomSeed = false;
		}

		public void Cancel()
		{
			StartingOrbs.Clear();
			StartingOrbs.AddRange(_originalStartingOrbs);
			StartingRelics.Clear();
			StartingRelics.AddRange(_originalStartingRelics);
			StaticGameData.currentSeed = string.Empty;
			seedInputPopup.hasCustomSeed = false;
			_hasCustomLoadout = false;
			ReturnFromLoadoutSelection();
		}

		public void MoveToLoadoutSelection()
		{
			CruciballParticleSystem.DisableSystem();
			PopulateOrbUI(StartingOrbs);
			PopulateRelicUI(StartingRelics);
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = sequence.Append(characterSelectCanvasGroup.DOFade(0f, canvasFadeTime));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(SwitchToLoadoutCanvas));
			sequence.Play();
			if (characterSelectController.currentlySelectedInstance != null)
			{
				characterSelectController.isInLoadoutShowcase = true;
			}
		}

		public void ReturnFromLoadoutSelection()
		{
			Sequence sequence = DOTween.Sequence();
			Sequence sequence2 = sequence.Append(loadoutBaseDisplayCanvasGroup.DOFade(0f, canvasFadeTime));
			sequence2.onComplete = (TweenCallback)Delegate.Combine(sequence2.onComplete, new TweenCallback(SwitchToCharacterSelectCanvas));
			sequence.Play();
			if (characterSelectController.currentlySelectedInstance != null)
			{
				characterSelectController.isInLoadoutShowcase = false;
			}
			if (_hasCustomLoadout || seedInputPopup.hasCustomSeed)
			{
				customLoadoutWarning.SetActive(value: true);
			}
			else
			{
				customLoadoutWarning.SetActive(value: false);
			}
		}

		public void MoveToOrbEditor()
		{
			loadoutBasePanel.SetActive(value: false);
			orbLoadoutEditorPanel.Initialize(_startingOrbData, _deckManager);
			orbLoadoutEditorPanel.gameObject.SetActive(value: true);
		}

		public void MoveToSeedEditor()
		{
			loadoutBasePanel.SetActive(value: false);
			seedInputPopup.gameObject.SetActive(value: true);
		}

		private void ReturnToBaseLoadoutFromOrbEditor()
		{
			orbLoadoutEditorPanel.gameObject.SetActive(value: false);
			PopulateOrbUI(StartingOrbs);
			PopulateRelicUI(StartingRelics);
			loadoutBasePanel.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(editOrbsButton);
		}

		public void MoveToRelicEditor()
		{
			loadoutBasePanel.SetActive(value: false);
			relicLoadoutEditorPanel.Initialize(_relicManager, StartingRelics, SelectedClassLoadout);
			relicLoadoutEditorPanel.gameObject.SetActive(value: true);
		}

		private void ReturnToBaseLoadoutFromRelicEditor()
		{
			relicLoadoutEditorPanel.gameObject.SetActive(value: false);
			PopulateOrbUI(StartingOrbs);
			PopulateRelicUI(StartingRelics);
			loadoutBasePanel.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(editOrbsButton);
		}

		private void SwitchToLoadoutCanvas()
		{
			characterSelectCanvasGroup.gameObject.SetActive(value: false);
			loadoutBaseDisplayCanvasGroup.gameObject.SetActive(value: true);
			loadoutBaseDisplayCanvasGroup.DOFade(1f, canvasFadeTime).From(0f);
		}

		private void SwitchToCharacterSelectCanvas()
		{
			characterSelectCanvasGroup.gameObject.SetActive(value: true);
			characterSelectCanvasGroup.DOFade(1f, canvasFadeTime).From(0f);
			loadoutBaseDisplayCanvasGroup.gameObject.SetActive(value: false);
			if (PersistentPlayerData.Instance.CruciballActive)
			{
				CruciballParticleSystem.EnableSystem();
			}
		}

		private void AddStartingRelics()
		{
			StaticGameData.StartingRelics = StartingRelics.ToArray();
		}

		private void AddStartingOrbs()
		{
			if (hasCustomLoadout)
			{
				StaticGameData.StartingOrbs = StartingOrbs.ToArray();
			}
			else
			{
				StaticGameData.StartingOrbs = SelectedClassLoadout.StartingOrbs.ToArray();
			}
		}

		private RelicRarity GetFinalRarityForRelic(Relic r)
		{
			if (SelectedClassLoadout.CommonRelicOverrides != null && SelectedClassLoadout.CommonRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.COMMON;
			}
			if (SelectedClassLoadout.RareRelicOverrides != null && SelectedClassLoadout.RareRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.RARE;
			}
			if (SelectedClassLoadout.BossRelicOverrides != null && SelectedClassLoadout.BossRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.BOSS;
			}
			if (SelectedClassLoadout.RareScenarioRelicOverrides != null && SelectedClassLoadout.RareScenarioRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.NONE;
			}
			if (SelectedClassLoadout.UnavailableRelicOverrides != null && SelectedClassLoadout.UnavailableRelicOverrides.relics.Contains(r))
			{
				return RelicRarity.UNAVAILABLE;
			}
			return r.globalRarity;
		}

		public void AddClassSpecificRelicsToPools(ClassLoadoutData loadoutData)
		{
			if (loadoutData.CommonRelicOverrides != null)
			{
				AddRelicOverridesToPool(SelectedClassLoadout.CommonRelicOverrides.relics, _relicManager.CommonRelicPool);
			}
			if (loadoutData.RareRelicOverrides != null)
			{
				AddRelicOverridesToPool(SelectedClassLoadout.RareRelicOverrides.relics, _relicManager.RareRelicPool);
			}
			if (loadoutData.BossRelicOverrides != null)
			{
				AddRelicOverridesToPool(SelectedClassLoadout.BossRelicOverrides.relics, _relicManager.BossRelicPool);
			}
			if (loadoutData.RareScenarioRelicOverrides != null)
			{
				AddRelicOverridesToPool(SelectedClassLoadout.RareScenarioRelicOverrides.relics, _relicManager.RareScenarioRelicPool);
			}
		}

		private static void AddRelicOverridesToPool(List<Relic> overrideList, List<Relic> pool)
		{
			foreach (Relic @override in overrideList)
			{
				if (!pool.Contains(@override))
				{
					pool.Add(@override);
				}
			}
		}

		private void SaveStartingOrbsAndSwitchToBaseLoadout()
		{
			hasCustomLoadout = true;
			StartingOrbs.Clear();
			foreach (LoadoutOrbData selectedStartingOrb in orbLoadoutEditorPanel.selectedStartingOrbs)
			{
				for (int i = 0; i < selectedStartingOrb.Amount; i++)
				{
					Attack component = selectedStartingOrb.orbPrefab.GetComponent<Attack>();
					GameObject gameObject = selectedStartingOrb.orbPrefab;
					if (component.Level > selectedStartingOrb.Level)
					{
						while (component.Level > selectedStartingOrb.Level)
						{
							gameObject = component.PreviousLevelPrefab;
							component = gameObject.GetComponent<Attack>();
						}
					}
					else
					{
						while (component.Level < selectedStartingOrb.Level)
						{
							gameObject = component.NextLevelPrefab;
							component = gameObject.GetComponent<Attack>();
						}
					}
					StartingOrbs.Add(gameObject);
				}
			}
			ReturnToBaseLoadoutFromOrbEditor();
		}

		private void SaveRelicsAndSwitchToBaseLoadout()
		{
			hasCustomLoadout = true;
			StartingRelics.Clear();
			StartingRelics.AddRange(relicLoadoutEditorPanel.selectedStartingRelics);
			ReturnToBaseLoadoutFromRelicEditor();
		}
	}
}
