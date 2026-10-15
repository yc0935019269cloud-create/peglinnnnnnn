using System;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Challenges;
using Coffee.UIEffects;
using PeglinUI.ControllerSupport;
using PeglinUI.UIUtils;
using Relics;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		public delegate void OrbIconClicked(GameObject prefab);

		public delegate void RelicIconClicked(Relic r);

		public delegate void ChallengeIconClicked(ChallengeData c);

		public OrbIconClicked OnOrbIconClicked;

		public RelicIconClicked OnRelicIconClicked;

		public ChallengeIconClicked OnChallengeIconClicked;

		public TextMeshProUGUI text;

		public Button button;

		public Image image;

		public Image dropShadow;

		public Image backgroundImage;

		public Color lockedColor = Color.black;

		public bool isUnlocked;

		public bool isSelectable = true;

		private bool _selected;

		private bool _positionCalculated;

		private Vector3 _imageStartPos;

		private Vector3 _imageOffsetPos;

		private RectTransform _iconRect;

		private Attack _attack;

		private Relic _relic;

		private ChallengeData _challenge;

		private Vector2 _anchorPoint = new Vector2(1f, -1f);

		private UIEffect _imageFillEffect;

		private void OnEnable()
		{
			_iconRect = GetComponent<RectTransform>();
		}

		public void InitializeOrb(GameObject orbPrefab, int amount)
		{
			_attack = orbPrefab.GetComponent<Attack>();
			OrbUIUtils.OrbVisuals orbVisuals = OrbUIUtils.GetOrbVisuals(orbPrefab);
			isUnlocked = PersistentPlayerData.Instance.UnlockedOrbs.Contains(orbPrefab.GetComponent<Attack>().locNameString);
			_imageFillEffect = image.GetComponent<UIEffect>();
			image.sprite = orbVisuals.sprite;
			image.rectTransform.localScale *= orbVisuals.scale;
			image.rectTransform.pivot = orbVisuals.pivot;
			image.color = (isUnlocked ? orbVisuals.color : lockedColor);
			if (_imageFillEffect != null)
			{
				_imageFillEffect.colorFactor = (isUnlocked ? 0f : 1f);
			}
			if (dropShadow != null)
			{
				dropShadow.sprite = orbVisuals.sprite;
				dropShadow.rectTransform.localScale *= orbVisuals.scale;
				dropShadow.rectTransform.pivot = orbVisuals.pivot;
			}
			if (button != null && isUnlocked)
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					OnOrbIconClicked?.Invoke(orbPrefab);
				});
			}
			SetAmount(amount);
			base.gameObject.name = "OrbCapsule-" + orbPrefab.name;
		}

		public void InitializeAssemball(GameObject orbPrefab, AssemballType types, int amount)
		{
			InitializeOrb(orbPrefab, amount);
			image.sprite = orbPrefab.GetComponent<AssemballParent>().BuildSprite(types);
		}

		public void InitializeUnlimitedUpgrades(GameObject orbPrefab, int level)
		{
			InitializeOrb(orbPrefab, 1);
			if (text != null)
			{
				text.text = $"Lv{level}";
				text.alignment = TextAlignmentOptions.Center;
				if (level >= 100)
				{
					text.enableAutoSizing = true;
					text.fontSizeMin = 5f;
					text.fontSizeMax = 20f;
				}
			}
		}

		public void CalculatePositions()
		{
			_imageStartPos = image.rectTransform.localPosition;
			_imageOffsetPos = _imageStartPos + new Vector3(-0.5f, 0.5f);
		}

		public void InitializeRelic(Relic r)
		{
			if (r == null)
			{
				return;
			}
			_relic = r;
			isUnlocked = PersistentPlayerData.Instance.UnlockedRelics.Contains(r.effect);
			image.sprite = r.sprite;
			if (dropShadow != null)
			{
				dropShadow.sprite = r.sprite;
			}
			image.color = ((isUnlocked || r.effect == RelicEffect.NONE) ? Color.white : lockedColor);
			_imageFillEffect = image.GetComponent<UIEffect>();
			if (_imageFillEffect != null)
			{
				_imageFillEffect.colorFactor = (isUnlocked ? 0f : 1f);
			}
			if (button != null && isUnlocked)
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					OnRelicIconClicked?.Invoke(r);
				});
			}
			SetAmount(0);
		}

		public void InitializeChallenge(ChallengeData challenge)
		{
			if (challenge == null)
			{
				return;
			}
			_challenge = challenge;
			isUnlocked = PersistentPlayerData.Instance.UnlockedChallenges.Contains(challenge.effect);
			image.sprite = challenge.sprite;
			if (dropShadow != null)
			{
				dropShadow.sprite = challenge.sprite;
			}
			image.color = ((isUnlocked || challenge.effect == ChallengeEffect.NONE) ? Color.white : lockedColor);
			_imageFillEffect = image.GetComponent<UIEffect>();
			if (_imageFillEffect != null)
			{
				_imageFillEffect.colorFactor = (isUnlocked ? 0f : 1f);
			}
			if (button != null && isUnlocked)
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					OnChallengeIconClicked?.Invoke(challenge);
				});
			}
			SetAmount(0);
		}

		public void SetUsed(bool on)
		{
			image.color = (on ? lockedColor : Color.white);
			if (_imageFillEffect != null)
			{
				_imageFillEffect.colorFactor = (on ? 0.5f : 0f);
			}
		}

		private void SetAmount(int amt)
		{
			if (this.text != null)
			{
				TextMeshProUGUI textMeshProUGUI = this.text;
				object obj;
				if (amt <= 0)
				{
					obj = "";
				}
				else
				{
					string text = (this.text.text = $"x{amt}");
					obj = text;
				}
				textMeshProUGUI.text = (string)obj;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (isUnlocked)
			{
				if (!_positionCalculated)
				{
					_positionCalculated = true;
					CalculatePositions();
				}
				if (!_selected && dropShadow != null && isSelectable)
				{
					image.rectTransform.localPosition = _imageOffsetPos;
				}
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (isUnlocked)
			{
				if (!_positionCalculated)
				{
					_positionCalculated = true;
					CalculatePositions();
				}
				if (!_selected && dropShadow != null)
				{
					image.rectTransform.localPosition = _imageStartPos;
				}
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!isUnlocked)
			{
				return;
			}
			if (!_positionCalculated)
			{
				_positionCalculated = true;
				CalculatePositions();
			}
			if (!ControllerInputManager.IsPointerBased)
			{
				_selected = true;
				if (dropShadow != null)
				{
					image.rectTransform.localPosition = _imageOffsetPos;
				}
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (!isUnlocked)
			{
				return;
			}
			if (!_positionCalculated)
			{
				_positionCalculated = true;
				CalculatePositions();
			}
			if (!ControllerInputManager.IsPointerBased)
			{
				_selected = false;
				if (dropShadow != null)
				{
					image.rectTransform.localPosition = _imageStartPos;
				}
			}
		}

		public void SetupTooltipEventsOrb()
		{
			MouseOverDetectorPointerHandler component = GetComponent<MouseOverDetectorPointerHandler>();
			component.OnPointerEntered = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerEntered, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				ShowTooltipOrb(followMouse: true);
			}));
			component.OnPointerExited = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerExited, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				HideTooltip();
			}));
			SelectionEventListener component2 = GetComponent<SelectionEventListener>();
			component2.OnSelectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnSelectionEvent, new SelectionEventListener.SelectionEvent(ShowTooltipControllerOrb));
			component2.OnDeselectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnDeselectionEvent, new SelectionEventListener.SelectionEvent(HideTooltip));
		}

		public void SetupTooltipEventsRelic()
		{
			MouseOverDetectorPointerHandler component = GetComponent<MouseOverDetectorPointerHandler>();
			component.OnPointerEntered = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerEntered, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				ShowTooltipRelic();
			}));
			component.OnPointerExited = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerExited, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				HideTooltip();
			}));
			SelectionEventListener component2 = GetComponent<SelectionEventListener>();
			component2.OnSelectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnSelectionEvent, new SelectionEventListener.SelectionEvent(ShowTooltipControllerRelic));
			component2.OnDeselectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnDeselectionEvent, new SelectionEventListener.SelectionEvent(HideTooltip));
		}

		public void SetupTooltipEventsChallenge()
		{
			MouseOverDetectorPointerHandler component = GetComponent<MouseOverDetectorPointerHandler>();
			component.OnPointerEntered = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerEntered, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				ShowTooltipChallenge();
			}));
			component.OnPointerExited = (MouseOverDetector.MouseEvent)Delegate.Combine(component.OnPointerExited, (MouseOverDetector.MouseEvent)((GameObject o) =>
			{
				HideTooltip();
			}));
			SelectionEventListener component2 = GetComponent<SelectionEventListener>();
			component2.OnSelectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnSelectionEvent, new SelectionEventListener.SelectionEvent(ShowTooltipControllerRelic));
			component2.OnDeselectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(component2.OnDeselectionEvent, new SelectionEventListener.SelectionEvent(HideTooltip));
		}

		private void ShowTooltipOrb(bool followMouse)
		{
			_attack.UpdateBattleParameters();
			if (isUnlocked)
			{
				TooltipManager.Instance.ShowTooltipOrb(_attack, base.transform.position, _anchorPoint, followMouse);
			}
			else
			{
				TooltipManager.Instance.ShowTooltipOrbLocked(_attack, base.transform.position, _anchorPoint, followMouse);
			}
		}

		private void ShowTooltipControllerOrb()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				ShowTooltipOrb(followMouse: false);
			}
		}

		private void ShowTooltipRelic()
		{
			if (isUnlocked)
			{
				TooltipManager.Instance.ShowTooltipRelic(_relic, base.transform.position, _anchorPoint, followMousePos: true);
			}
			else
			{
				TooltipManager.Instance.ShowTooltipRelicLocked(_relic, base.transform.position, _anchorPoint, followMousePos: true);
			}
		}

		private void ShowTooltipControllerRelic()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				if (isUnlocked)
				{
					TooltipManager.Instance.ShowTooltipRelic(_relic, base.transform.position, _anchorPoint);
				}
				else
				{
					TooltipManager.Instance.ShowTooltipRelicLocked(_relic, base.transform.position, _anchorPoint);
				}
			}
		}

		private void ShowTooltipChallenge()
		{
			if (isUnlocked)
			{
				TooltipManager.Instance.ShowTooltipChallenge(_challenge.effect, base.transform.position, _anchorPoint, followMousePos: true);
			}
			else
			{
				TooltipManager.Instance.ShowTooltipChallengeLocked(_challenge.effect, base.transform.position, _anchorPoint, followMousePos: true);
			}
		}

		private void ShowTooltipControllerChallenge()
		{
			if (!ControllerInputManager.IsPointerBased)
			{
				if (isUnlocked)
				{
					TooltipManager.Instance.ShowTooltipChallenge(_challenge.effect, base.transform.position, _anchorPoint);
				}
				else
				{
					TooltipManager.Instance.ShowTooltipChallengeLocked(_challenge.effect, base.transform.position, _anchorPoint);
				}
			}
		}

		private void HideTooltip()
		{
			TooltipManager.Instance.HideTooltip();
		}
	}
}
