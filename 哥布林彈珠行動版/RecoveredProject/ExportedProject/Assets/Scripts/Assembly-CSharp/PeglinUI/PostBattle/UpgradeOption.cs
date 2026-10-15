using System.Collections;
using Battle.Attacks;
using DG.Tweening;
using PeglinUI.ControllerSupport;
using PeglinUI.OrbDisplay;
using PeglinUtils;
using Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	[RequireComponent(typeof(Button))]
	public class UpgradeOption : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public delegate void UpgradeOptionClicked(UpgradeType type, GameObject clickedButton, GameObject chosen = null);

		public enum UpgradeType
		{
			HEALTH_HEAL = 0,
			RANDOM_ORB = 1,
			INSPECT_ORB_FOR_UPGRADE = 2,
			UPGRADE_CONFIRMED = 3,
			INSPECT_NEW_ORB = 4,
			NEW_ORB_CONFIRMED = 5,
			ACCEPT_RELIC = 6,
			SKIP = 7,
			REMOVE_ORB_CONFIRMED = 8,
			DUPLICATE_ORB_CONFIRMED = 9,
			INSPECT_RELIC = 10,
			MAX_HP_INC = 11,
			INSPECT_CHALLENGE = 12,
			ADD_CHALLENGE = 13
		}

		private const float HOVER_MOUSE_TIME = 0.3f;

		public static UpgradeOptionClicked OnUpgradeOptionClicked;

		[SerializeField]
		private Vector3 _widgetOffset = new Vector3(1f, 1f);

		[SerializeField]
		private GameObject _infoWidgetPrefab;

		public UpgradeType upgradeType;

		[SerializeField]
		private Image orbDisplayImage;

		[SerializeField]
		private Animator orbDisplayAnimator;

		private bool _canCreateAnimator;

		[SerializeField]
		private Image _orbBackgroundImage;

		[SerializeField]
		private Sprite _orbLevel2Sprite;

		[SerializeField]
		private Sprite _orbLevel3Sprite;

		[SerializeField]
		private Sprite _orbPurchasedSprite;

		public GameObject newRewardIndicator;

		private GameObject _specifiedOrb;

		private Attack _attack;

		private Button _button;

		private Canvas _tooltipCanvas;

		private bool _isTooltipDisplaying;

		private bool _isMouseOver;

		private bool _followMouse;

		private float _mouseOverTime;

		private ObjectPoolerDict _orbInfoWidgetPool;

		private GameObject _infoWidgetGameObject;

		public GameObject SpecifiedOrb
		{
			get
			{
				return _specifiedOrb;
			}
			set
			{
				_specifiedOrb = value;
				_attack = _specifiedOrb.GetComponent<Attack>();
				Animator componentInChildren = _specifiedOrb.GetComponentInChildren<Animator>();
				if (componentInChildren != null && componentInChildren.runtimeAnimatorController != null)
				{
					if (orbDisplayAnimator == null && _canCreateAnimator)
					{
						orbDisplayAnimator = base.gameObject.AddComponent<Animator>();
					}
					if (orbDisplayAnimator != null)
					{
						orbDisplayAnimator.enabled = true;
						orbDisplayAnimator.runtimeAnimatorController = componentInChildren.runtimeAnimatorController;
					}
				}
				else if (orbDisplayAnimator != null)
				{
					Object.Destroy(orbDisplayAnimator);
					orbDisplayAnimator = null;
					_canCreateAnimator = true;
				}
				if (orbDisplayImage != null)
				{
					PachinkoBall component = _specifiedOrb.GetComponent<PachinkoBall>();
					orbDisplayImage.sprite = component.sprite;
					orbDisplayImage.color = component.color;
					StartCoroutine(ResetSpriteAfterRemovingAnimator(component.sprite));
					float x = _specifiedOrb.GetComponentInChildren<SpriteRenderer>().transform.localScale.x;
					orbDisplayImage.transform.localScale = new Vector3(x, x, 1f);
				}
				int num = _attack.Level;
				AssemballAttack assemballAttack = _attack as AssemballAttack;
				if (assemballAttack != null)
				{
					num = AssemballAttack.GetAssemballDisplayLevel(assemballAttack.GetNumberOfComponents());
				}
				if (num > 1 || _attack.NextLevelPrefab != null)
				{
					if (num == 2 && _orbLevel2Sprite != null)
					{
						_orbBackgroundImage.sprite = _orbLevel2Sprite;
					}
					else if (num == 3 && _orbLevel3Sprite != null)
					{
						_orbBackgroundImage.sprite = _orbLevel3Sprite;
					}
				}
				if (newRewardIndicator != null)
				{
					if (!PersistentPlayerData.Instance.UnlockedOrbs.Contains(_attack.locNameString) && !PersistentPlayerData.Instance.QueuedOrbUnlocks.Contains(_attack.locNameString) && PersistentPlayerData.Instance.AnyCruciballUnlockedForClass(StaticGameData.chosenClass))
					{
						newRewardIndicator.SetActive(value: true);
					}
					else
					{
						newRewardIndicator.SetActive(value: false);
					}
				}
			}
		}

		public Attack attack => _attack;

		private void OnEnable()
		{
			if (_orbInfoWidgetPool == null)
			{
				GameObject gameObject = new GameObject();
				_orbInfoWidgetPool = gameObject.AddComponent<ObjectPoolerDict>();
			}
		}

		private void OnDisable()
		{
			_isMouseOver = false;
			if (_isTooltipDisplaying)
			{
				HideTooltip();
			}
			if (_orbInfoWidgetPool != null)
			{
				Object.Destroy(_orbInfoWidgetPool.gameObject);
				_orbInfoWidgetPool = null;
			}
		}

		private void Start()
		{
			_button = GetComponent<Button>();
			_button.onClick.AddListener(() =>
			{
				OnClick();
			});
		}

		public void OnClick()
		{
			if (!PauseMenu.Paused && !ControllerInspection.CurrentlyInspecting)
			{
				OnUpgradeOptionClicked?.Invoke(upgradeType, base.gameObject, SpecifiedOrb);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (_attack != null && !_isMouseOver && _button != null && _button.interactable && ControllerInputManager.primaryInputType != InputType.Touch)
			{
				_isMouseOver = true;
				_followMouse = false;
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (_attack != null)
			{
				_isMouseOver = false;
				HideTooltip();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (ControllerInputManager.IsPointerBased)
			{
				_isMouseOver = true;
				_followMouse = true;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_mouseOverTime = 0f;
			_isMouseOver = false;
			_followMouse = false;
			HideTooltip();
		}

		private void Update()
		{
			if (!_isTooltipDisplaying && _isMouseOver && upgradeType != UpgradeType.NEW_ORB_CONFIRMED && upgradeType != UpgradeType.UPGRADE_CONFIRMED && _button != null && _button.interactable)
			{
				_mouseOverTime += Time.deltaTime;
				if (_mouseOverTime >= 0.3f)
				{
					_isTooltipDisplaying = true;
					ShowTooltipNew(_followMouse);
				}
			}
		}

		public void SetOptionInactive()
		{
			orbDisplayImage.DOFade(0f, 0.5f);
			RectTransform component = orbDisplayImage.GetComponent<RectTransform>();
			component.DOLocalMoveY(component.anchoredPosition.y + 20f, 0.5f);
			_orbBackgroundImage.GetComponent<Image>().sprite = _orbPurchasedSprite;
			if (newRewardIndicator != null)
			{
				newRewardIndicator.SetActive(value: false);
			}
			if (_button != null)
			{
				_button.interactable = false;
			}
			if (_isTooltipDisplaying)
			{
				HideTooltip();
			}
		}

		private IEnumerator ResetSpriteAfterRemovingAnimator(Sprite sprite)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			orbDisplayImage.sprite = sprite;
		}

		private void ShowTooltip()
		{
			_isTooltipDisplaying = true;
			if (_tooltipCanvas == null)
			{
				_tooltipCanvas = GameObject.FindWithTag("TooltipCanvas")?.GetComponent<Canvas>();
			}
			_infoWidgetGameObject = _orbInfoWidgetPool.InstantiateObject(_infoWidgetPrefab);
			_infoWidgetGameObject.SetActive(value: true);
			_infoWidgetGameObject.transform.SetParent(_tooltipCanvas.transform);
			_infoWidgetGameObject.transform.localScale = Vector3.one;
			OrbInfoWidget component = _infoWidgetGameObject.GetComponent<OrbInfoWidget>();
			_attack.ClearBattleParameters();
			component.Initialize(_attack);
			component.MoveToPosition(base.transform.position, _widgetOffset);
			_mouseOverTime = 0f;
		}

		private void ShowTooltipNew(bool followMouse)
		{
			if (!(_attack == null))
			{
				_attack.InitializeInventory();
				TooltipManager.Instance.ShowTooltipOrb(_attack, base.transform.position, _widgetOffset);
			}
		}

		private void HideTooltip()
		{
			if (_isTooltipDisplaying)
			{
				_isTooltipDisplaying = false;
				_mouseOverTime = 0f;
				if (TooltipManager.Instance != null)
				{
					TooltipManager.Instance.HideTooltip();
				}
			}
		}
	}
}
