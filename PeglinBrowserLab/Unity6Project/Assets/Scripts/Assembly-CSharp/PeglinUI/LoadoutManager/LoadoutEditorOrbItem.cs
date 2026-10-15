using System;
using System.Collections;
using Battle.Attacks;
using PeglinUI.ControllerSupport;
using PeglinUI.UIUtils;
using PeglinUtils;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutEditorOrbItem : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public delegate void DeleteButtonClicked(LoadoutEditorOrbItem item);

		public delegate void OrbItemSelected(bool selection);

		public delegate void OrbLevelCycled(LoadoutEditorOrbItem item);

		public DeleteButtonClicked OnDeleteButtonClicked;

		public static OrbItemSelected OnOrbItemSelected;

		public OrbLevelCycled OnCycleLevelClicked;

		public Sprite[] _levelIcons;

		public Image iconImage;

		public Image levelImage;

		public LoadoutEditorShowTooltip tooltipDetector;

		[FormerlySerializedAs("orbNameText")]
		public TextMeshProUGUI nameText;

		public TextMeshProUGUI amountText;

		public GameObject[] contextButtons;

		private LoadoutOrbData _orbData;

		private bool _listeningForControllerInput;

		private Player _player;

		public LoadoutOrbData OrbData => _orbData;

		public void Initialize(LoadoutOrbData data)
		{
			if (_levelIcons.Length != 3)
			{
				Debug.LogError("Wrong data on the level icons array");
			}
			_orbData = data;
			_player = ReInput.players.GetPlayer(0);
			OrbUIUtils.OrbVisuals orbVisuals = OrbUIUtils.GetOrbVisuals(data.orbPrefab);
			iconImage.sprite = orbVisuals.sprite;
			iconImage.rectTransform.localScale *= orbVisuals.scale;
			iconImage.rectTransform.pivot = orbVisuals.pivot;
			iconImage.color = orbVisuals.color;
			nameText.text = data.orbPrefab.GetComponent<Attack>().Name;
			SetAmount(data.Amount);
			if (data.CanBeLeveledUp)
			{
				SetLevelSprite(data.Level);
				LoadoutOrbData orbData = _orbData;
				orbData.OnLevelChanged = (LoadoutOrbData.AmountChanged)Delegate.Combine(orbData.OnLevelChanged, new LoadoutOrbData.AmountChanged(SetLevelSprite));
			}
			else
			{
				levelImage.enabled = false;
			}
			LoadoutOrbData orbData2 = _orbData;
			orbData2.OnAmountChanged = (LoadoutOrbData.AmountChanged)Delegate.Combine(orbData2.OnAmountChanged, new LoadoutOrbData.AmountChanged(SetAmount));
			tooltipDetector.Initialize(data);
		}

		private void OnEnable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(OnInputTypeChange));
		}

		private void Update()
		{
			if (_listeningForControllerInput)
			{
				if (_player.GetButtonDown("CatalogAddOrb"))
				{
					AddOne();
				}
				else if (_player.GetButtonDown("CatalogRemoveOrb"))
				{
					DeleteOne();
				}
				else if (_player.GetButtonDown("CatalogDeleteOrb"))
				{
					StartCoroutine(DeleteOrbNextFrame());
				}
				else if (_player.GetButtonDown("CatalogCycleOrbLevel"))
				{
					CycleLevelClicked();
				}
				else if (_player.GetButtonDown("MapButton"))
				{
					tooltipDetector.ShowTooltip();
				}
			}
		}

		private void SetAmount(int amt)
		{
			amountText.text = $"x{amt}";
		}

		public void AddOne()
		{
			_orbData.Amount = Mathf.Clamp(_orbData.Amount + 1, 0, 99);
		}

		public void DeleteOne()
		{
			_orbData.Amount = Mathf.Clamp(_orbData.Amount - 1, 0, 99);
		}

		public void CycleLevelClicked()
		{
			OnCycleLevelClicked?.Invoke(this);
		}

		public void SetLevelSprite(int level)
		{
			if (level - 1 < 0 || level - 1 >= _levelIcons.Length)
			{
				if (level != -1)
				{
					Debug.LogError("Wrong data on the level icons array");
				}
			}
			else
			{
				levelImage.sprite = _levelIcons[level - 1];
			}
		}

		private void OnDisable()
		{
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(OnInputTypeChange));
		}

		private void OnDestroy()
		{
			LoadoutOrbData orbData = _orbData;
			orbData.OnAmountChanged = (LoadoutOrbData.AmountChanged)Delegate.Remove(orbData.OnAmountChanged, new LoadoutOrbData.AmountChanged(SetAmount));
		}

		public void DeleteOrb()
		{
			OnDeleteButtonClicked?.Invoke(this);
		}

		private IEnumerator DeleteOrbNextFrame()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			OnDeleteButtonClicked?.Invoke(this);
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!ControllerInputManager.IsPointerBased && contextButtons != null)
			{
				_listeningForControllerInput = true;
			}
			OnOrbItemSelected?.Invoke(selection: true);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (_listeningForControllerInput)
			{
				TooltipManager.Instance.HideTooltip();
			}
			_listeningForControllerInput = false;
			if (!ControllerInputManager.IsPointerBased && contextButtons != null)
			{
				GameObject[] array = contextButtons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
			OnOrbItemSelected?.Invoke(selection: false);
		}

		private void OnInputTypeChange()
		{
			if (ControllerInputManager.IsPointerBased && contextButtons != null)
			{
				GameObject[] array = contextButtons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
		}
	}
}
