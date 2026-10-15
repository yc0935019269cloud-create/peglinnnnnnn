using System;
using Cruciball;
using DG.Tweening;
using I2.Loc;
using NaughtyAttributes;
using Peglin.ClassSystem;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu.Cruciball
{
	public class CruciballLevelSelector : MonoBehaviour
	{
		[Header("References")]
		public CruciballManager cruciballManager;

		public CharacterSelectController characterSelectController;

		public Button cruciballToggleButton;

		private TMP_Text _cruciballToggleText;

		private RectTransform _cruciballToggleButtonRect;

		public TMP_Text currentLevelDescText;

		public Button lowerLevelButton;

		public Button raiseLevelButton;

		public GameObject cruciballSliceContainer;

		public Image[] cruciballCompletedSlices;

		private CanvasGroup _wheelCanvas;

		public CruciballParticleToggle cruciballParticleToggle;

		public CruciballNumberDisplayManager CruciballNumberDisplay;

		public Image cruciballCompleteOverlay;

		public Button customLoadoutButton;

		public Button playButton;

		[ReorderableList]
		public string[] cruciballDescriptionKeys;

		[Header("Params")]
		public Color cruciballActiveTextColor = Color.white;

		public Color cruciballInactiveTextColor = Color.gray;

		private float _cruciballRotateDuration = 0.45f;

		public float fadeTime = 0.35f;

		public float cruciballButtonMoveTime = 0.35f;

		public float slicePositionOffset = -2f;

		private float _sliceWidth = 5f;

		private const float CRUCIBALL_BUTTON_SHOWN_HEIGHT = -5f;

		private float _cruciballButtonHiddenHeight;

		private int currentClassCompletedCruciballLevel = -1;

		private Tween currentWheelTween;

		private bool _demoMode;

		private Class _selectedClass;

		private void Awake()
		{
			_wheelCanvas = GetComponentInParent<CanvasGroup>();
			_cruciballToggleButtonRect = cruciballToggleButton.GetComponent<RectTransform>();
			_cruciballButtonHiddenHeight = cruciballToggleButton.transform.localPosition.y;
			_cruciballToggleText = cruciballToggleButton.GetComponentInChildren<TMP_Text>();
			_cruciballToggleText.color = cruciballInactiveTextColor;
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Combine(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(ClassChanged));
		}

		private void OnEnable()
		{
			if (ReleaseDemoManager.ReleaseDemoActive != _demoMode)
			{
				Init();
			}
			UpdateCruciballSelection();
		}

		private void OnDestroy()
		{
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Remove(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(ClassChanged));
		}

		private void Start()
		{
			Init();
		}

		private void Init()
		{
			_sliceWidth = 18f;
			ClassChanged(characterSelectController.currentlySelectedClass, characterSelectController.currentlySelectedClassUnlocked);
		}

		private void ClassChanged(SelectableCharacter character)
		{
			ClassChanged(character.characterClass, character.IsUnlocked);
		}

		private void ClassChanged(Class newClass, bool unlocked)
		{
			_selectedClass = newClass;
			if (ReleaseDemoManager.ReleaseDemoActive || (IAPManager.PlatformRequiresIAPUnlock && !IAPManager.FullGamePurchased))
			{
				currentClassCompletedCruciballLevel = -1;
			}
			else
			{
				currentClassCompletedCruciballLevel = PersistentPlayerData.Instance.CruciballLevels[newClass];
			}
			_demoMode = ReleaseDemoManager.ReleaseDemoActive;
			if (!unlocked)
			{
				HideCruciballSelect();
				cruciballToggleButton.enabled = false;
				customLoadoutButton.enabled = false;
				customLoadoutButton.gameObject.SetActive(value: false);
				_cruciballToggleButtonRect.DOAnchorPosY(_cruciballButtonHiddenHeight, cruciballButtonMoveTime);
				playButton.gameObject.SetActive(value: false);
			}
			else if (currentClassCompletedCruciballLevel < 0)
			{
				HideCruciballSelect();
				cruciballManager.SetRunCruciballLevelAndClass(0, _selectedClass);
				cruciballToggleButton.enabled = false;
				_cruciballToggleButtonRect.DOAnchorPosY(_cruciballButtonHiddenHeight, cruciballButtonMoveTime);
				customLoadoutButton.enabled = false;
				customLoadoutButton.gameObject.SetActive(value: false);
				playButton.gameObject.SetActive(value: true);
			}
			else if (currentClassCompletedCruciballLevel >= 0 || PersistentPlayerData.Instance.CruciballActive)
			{
				customLoadoutButton.enabled = true;
				customLoadoutButton.gameObject.SetActive(value: true);
				int selectedCruciballLevel = PersistentPlayerData.Instance.GetSelectedCruciballLevel(_selectedClass);
				if (selectedCruciballLevel > 0)
				{
					PersistentPlayerData.Instance.CruciballActive = true;
					cruciballManager.SetRunCruciballLevelAndClass(selectedCruciballLevel, _selectedClass);
					ShowCruciballSelect();
				}
				else
				{
					PersistentPlayerData.Instance.CruciballActive = false;
					HideCruciballSelect();
					cruciballManager.SetRunCruciballLevelAndClass(0, _selectedClass);
				}
				cruciballToggleButton.enabled = true;
				_cruciballToggleButtonRect.DOAnchorPosY(-5f, cruciballButtonMoveTime);
				playButton.gameObject.SetActive(value: true);
			}
		}

		public void ToggleCruciballActive()
		{
			if (PersistentPlayerData.Instance.CruciballActive)
			{
				PersistentPlayerData.Instance.CruciballActive = false;
				HideCruciballSelect();
			}
			else if (currentClassCompletedCruciballLevel >= 0)
			{
				ShowCruciballSelect();
			}
		}

		private void HideCruciballSelect()
		{
			cruciballManager.currentCruciballLevel = 0;
			_wheelCanvas.DOFade(0f, fadeTime);
			_cruciballToggleText.color = cruciballInactiveTextColor;
			cruciballParticleToggle.DisableSystem();
		}

		private void ShowCruciballSelect()
		{
			if (cruciballManager.currentCruciballLevel == 0)
			{
				cruciballManager.currentCruciballLevel = Mathf.Max(PersistentPlayerData.Instance.GetSelectedCruciballLevel(_selectedClass), 1);
				PersistentPlayerData.Instance.SetLastSelectedCruciball(_selectedClass, cruciballManager.currentCruciballLevel);
			}
			PersistentPlayerData.Instance.CruciballActive = true;
			_wheelCanvas.DOFade(1f, fadeTime);
			_cruciballToggleText.color = cruciballActiveTextColor;
			for (int i = 0; i < cruciballCompletedSlices.Length; i++)
			{
				bool flag = i < currentClassCompletedCruciballLevel;
				bool flag2 = i >= 20;
				Color endValue = Color.white;
				endValue.a = ((flag | flag2) ? 1f : 0f);
				if (i == currentClassCompletedCruciballLevel)
				{
					endValue = Color.yellow;
					endValue.a = 0.3f;
				}
				cruciballCompletedSlices[i].DOColor(endValue, fadeTime);
			}
			if (currentClassCompletedCruciballLevel <= 0)
			{
				lowerLevelButton.gameObject.SetActive(value: false);
				raiseLevelButton.gameObject.SetActive(value: false);
			}
			else
			{
				lowerLevelButton.gameObject.SetActive(value: true);
				raiseLevelButton.gameObject.SetActive(value: true);
			}
			cruciballParticleToggle.EnableSystem();
			UpdateCruciballSelection();
		}

		public void LowerCruciballLevel()
		{
			int selectedCruciballLevel = PersistentPlayerData.Instance.GetSelectedCruciballLevel(_selectedClass);
			int num = selectedCruciballLevel - 1;
			if (num <= 0)
			{
				num = Mathf.Min(currentClassCompletedCruciballLevel + 1, 20);
			}
			PersistentPlayerData.Instance.SetLastSelectedCruciball(_selectedClass, num);
			cruciballManager.currentCruciballLevel = num;
			if (selectedCruciballLevel != num)
			{
				UpdateCruciballSelection();
			}
		}

		public void RaiseCruciballLevel()
		{
			int selectedCruciballLevel = PersistentPlayerData.Instance.GetSelectedCruciballLevel(_selectedClass);
			int num = selectedCruciballLevel + 1;
			int num2 = Mathf.Min(currentClassCompletedCruciballLevel + 1, 20);
			if (num > num2)
			{
				num = 1;
			}
			PersistentPlayerData.Instance.SetLastSelectedCruciball(_selectedClass, num);
			cruciballManager.currentCruciballLevel = num;
			if (selectedCruciballLevel != num)
			{
				UpdateCruciballSelection();
			}
		}

		private void UpdateCruciballSelection()
		{
			int selectedCruciballLevel = PersistentPlayerData.Instance.GetSelectedCruciballLevel(_selectedClass);
			if (selectedCruciballLevel > 0)
			{
				currentLevelDescText.text = selectedCruciballLevel + ". " + LocalizationManager.GetTranslation("Cruciball/" + cruciballDescriptionKeys[selectedCruciballLevel - 1], FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true);
				CruciballNumberDisplay.UpdateDisplay(selectedCruciballLevel);
				float num = slicePositionOffset + _sliceWidth * 0.5f + (float)(selectedCruciballLevel - 1) * _sliceWidth;
				if (currentWheelTween != null)
				{
					currentWheelTween.Kill();
				}
				currentWheelTween = base.transform.DORotate(Vector3.forward * num, _cruciballRotateDuration);
			}
		}

		public bool GetCruciballActive()
		{
			return PersistentPlayerData.Instance.CruciballActive;
		}
	}
}
