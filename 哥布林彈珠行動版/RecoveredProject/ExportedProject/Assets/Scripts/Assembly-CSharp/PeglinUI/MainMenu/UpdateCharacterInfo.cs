using System;
using I2.Loc;
using Peglin.ClassSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.MainMenu
{
	public class UpdateCharacterInfo : MonoBehaviour
	{
		[Header("Scene Refs")]
		public TextMeshProUGUI classNameText;

		public TextMeshProUGUI classDescriptionText;

		public TextMeshProUGUI classRelicNameText;

		public TextMeshProUGUI classRelicDescriptonText;

		public TextMeshProUGUI classLockedDescriptionText;

		public TextMeshProUGUI classFeatTitleText;

		public TextMeshProUGUI classFeatDescriptionText;

		public Image classRelicIcon;

		public Image classSilhouette;

		public Image classNameScroll;

		public CharacterSelectController characterSelectController;

		public CharacterXpDisplayManager xpDisplayManager;

		public GameObject unlockedDetails;

		public GameObject lockedDetails;

		public GameObject startingRelicPanel;

		public GameObject lockedAchievementDetails;

		public GameObject lockedXpBar;

		private void OnEnable()
		{
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Combine(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(UpdateInfo));
			UpdateInfo(characterSelectController.currentlySelectedInstance);
		}

		private void OnDisable()
		{
			CharacterSelectController obj = characterSelectController;
			obj.OnSelectedClassChanged = (CharacterSelectController.SelectedClassChanged)Delegate.Remove(obj.OnSelectedClassChanged, new CharacterSelectController.SelectedClassChanged(UpdateInfo));
		}

		private void UpdateInfo(SelectableCharacter character)
		{
			if (character.IsUnlocked)
			{
				unlockedDetails.SetActive(value: true);
				lockedDetails.SetActive(value: false);
				lockedXpBar.SetActive(value: false);
				lockedAchievementDetails.SetActive(value: false);
				startingRelicPanel.SetActive(value: true);
				classNameText.text = LocalizationManager.GetTranslation(character.classNameLocKey);
				classDescriptionText.text = LocalizationManager.GetTranslation(character.classDescriptionLocKey);
				classRelicNameText.text = LocalizationManager.GetTranslation(character.classRelicNameLocKey);
				string translation = LocalizationManager.GetTranslation(character.classRelicDescriptionLocKey);
				LocalizationManager.ApplyLocalizationParams(ref translation);
				classRelicDescriptonText.text = translation;
				classRelicIcon.sprite = character.classRelicIcon;
				classNameScroll.sprite = character.nameScrollImage;
				character.rewardIcon.gameObject.SetActive(value: false);
				return;
			}
			unlockedDetails.SetActive(value: false);
			lockedDetails.SetActive(value: true);
			lockedXpBar.SetActive(value: true);
			lockedAchievementDetails.SetActive(value: true);
			startingRelicPanel.SetActive(value: false);
			string translation2 = LocalizationManager.GetTranslation(character.classUnlockXpMethodLocKey);
			LocalizationManager.ApplyLocalizationParams(ref translation2);
			classLockedDescriptionText.text = translation2;
			classFeatTitleText.text = LocalizationManager.GetTranslation(character.classFeatTitleLocKey);
			classFeatDescriptionText.text = LocalizationManager.GetTranslation(character.classFeatDescriptionLocKey);
			classSilhouette.sprite = character.GetSprite();
			classSilhouette.color = Color.black;
			classNameScroll.sprite = character.nameScrollImage;
			xpDisplayManager.SetupXpInfo(character.characterClass);
			character.rewardIcon.gameObject.SetActive(value: false);
			if (StaticGameData.CurrentRunStats != null)
			{
				switch (character.characterClass)
				{
				case Class.Balladin:
					character.rewardIcon.gameObject.SetActive(xpDisplayManager.BalladinStagingProgress > 0);
					break;
				case Class.Roundrel:
					character.rewardIcon.gameObject.SetActive(xpDisplayManager.RoundrelStagingProgress > 0);
					break;
				case Class.Spinventor:
					character.rewardIcon.gameObject.SetActive(xpDisplayManager.SpinventorStagingProgress > 0);
					break;
				}
			}
		}
	}
}
