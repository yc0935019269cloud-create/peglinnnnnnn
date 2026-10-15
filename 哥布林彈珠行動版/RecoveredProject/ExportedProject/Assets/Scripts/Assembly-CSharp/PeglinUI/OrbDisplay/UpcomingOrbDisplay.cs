using System;
using Battle;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using TMPro;
using UI.OrbDisplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.OrbDisplay
{
	public class UpcomingOrbDisplay : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, ISelectHandler, IDeselectHandler
	{
		public delegate void DiscardTriggered();

		public static DiscardTriggered OnDiscardTriggered = () =>
		{
		};

		private const float HOVER_MOUSE_TIME = 0.2f;

		private bool _isActiveOrb;

		[SerializeField]
		private Vector3 _widgetOffset = new Vector3(5f, 0f);

		[SerializeField]
		private CanvasGroup _textCanvas;

		[SerializeField]
		private SpriteRenderer _levelSpecificFrame;

		[SerializeField]
		private Sprite _orbLevel2Sprite;

		[SerializeField]
		private Sprite _orbLevel3Sprite;

		[SerializeField]
		private GameObject _mainOrbLevelFrameMask;

		[SerializeField]
		private GameObject _selectionIndicator;

		private BattleController _battleController;

		private ObjectPoolerDict _orbInfoWidgetPool;

		private Canvas _orbInfoCanvas;

		private float _mouseOverTime;

		private bool _isTooltipDisplaying;

		private Animator _animator;

		private SpriteRenderer _renderer;

		private CircleCollider2D _collider;

		private Transform _aimingShotTransform;

		private bool _blockTracking;

		private Attack _attack;

		private SnowballDmgAndSize _currentOrbSnowballComponent;

		private Vector3 _defaultScale;

		private Vector3 _defaultSelectionIndicatorScale;

		private float _defaultColliderRadius;

		public GameObject mainOrbLevelFrameMask => _mainOrbLevelFrameMask;

		public Attack attack => _attack;

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
			_collider = GetComponent<CircleCollider2D>();
			_defaultScale = base.transform.localScale;
			_defaultSelectionIndicatorScale = _selectionIndicator.transform.localScale;
			_defaultColliderRadius = _collider.radius;
		}

		public void Initialize(GameObject orbPrefab, ObjectPoolerDict orbInfoWidgetPool, Canvas orbInfoCanvas, BattleController battleController)
		{
			_battleController = battleController;
			PachinkoBall component = orbPrefab.GetComponent<PachinkoBall>();
			_renderer.sprite = component.sprite;
			_renderer.color = component.color;
			Vector3 localScale = component.GetComponentInChildren<SpriteRenderer>().transform.localScale;
			_selectionIndicator.transform.localScale = _defaultSelectionIndicatorScale * (base.transform.localScale.x / localScale.x);
			base.transform.localScale = _defaultScale * localScale.x;
			_collider.radius = _defaultColliderRadius / localScale.x;
			Animator componentInChildren = orbPrefab.GetComponentInChildren<Animator>();
			if (componentInChildren != null && componentInChildren.enabled)
			{
				_animator = GetComponent<Animator>();
				_animator.runtimeAnimatorController = componentInChildren.runtimeAnimatorController;
				_animator.speed = 0f;
			}
			_attack = orbPrefab.GetComponent<Attack>();
			int num = _attack.Level;
			AssemballAttack assemballAttack = _attack as AssemballAttack;
			if (assemballAttack != null)
			{
				num = AssemballAttack.GetAssemballDisplayLevel(assemballAttack.GetNumberOfComponents());
			}
			_currentOrbSnowballComponent = null;
			if (orbPrefab.TryGetComponent<SnowballDmgAndSize>(out var component2))
			{
				_currentOrbSnowballComponent = component2;
				component2.UpdateSize();
				UpdateSnowballSize(component2.pachickoSpriteRenderer);
			}
			if (num > 1 || _attack.NextLevelPrefab != null)
			{
				switch (num)
				{
				case 2:
					_levelSpecificFrame.sprite = _orbLevel2Sprite;
					break;
				case 3:
					_levelSpecificFrame.sprite = _orbLevel3Sprite;
					break;
				}
			}
			_attack.UpdateBattleParameters();
			_orbInfoWidgetPool = orbInfoWidgetPool;
			_orbInfoCanvas = orbInfoCanvas;
		}

		public void UpdateDetails()
		{
			if (_attack != null)
			{
				_attack.UpdateBoardDetails();
				_attack.UpdateBattleParameters();
			}
		}

		private void OnEnable()
		{
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(HandleCriticalHit));
			BattleController.OnBallCreationComplete = (Action)Delegate.Combine(BattleController.OnBallCreationComplete, new Action(UpdateAimTrackingTransform));
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Combine(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(DisableCollider));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(HandleShotFired));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Combine(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(InventoryViewToggled));
			SnowballDmgAndSize.OnSnowballSizeIncreased = (SnowballDmgAndSize.SnowballSizeIncreased)Delegate.Combine(SnowballDmgAndSize.OnSnowballSizeIncreased, new SnowballDmgAndSize.SnowballSizeIncreased(UpdateSnowballSize));
		}

		private void OnDisable()
		{
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(HandleCriticalHit));
			BattleController.OnBallCreationComplete = (Action)Delegate.Remove(BattleController.OnBallCreationComplete, new Action(UpdateAimTrackingTransform));
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Remove(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(DisableCollider));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(HandleShotFired));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Remove(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(InventoryViewToggled));
			SnowballDmgAndSize.OnSnowballSizeIncreased = (SnowballDmgAndSize.SnowballSizeIncreased)Delegate.Remove(SnowballDmgAndSize.OnSnowballSizeIncreased, new SnowballDmgAndSize.SnowballSizeIncreased(UpdateSnowballSize));
		}

		private void HandleCriticalHit()
		{
			if (!_isActiveOrb || !(_attack != null))
			{
				return;
			}
			ChangeSpriteOnCrit component = _attack.GetComponent<ChangeSpriteOnCrit>();
			if (component != null)
			{
				SpriteRenderer componentInChildren = _attack.GetComponentInChildren<SpriteRenderer>();
				if (component.startsDull)
				{
					_renderer.sprite = componentInChildren.sprite;
				}
				else
				{
					_renderer.sprite = component.dullSprite;
				}
			}
		}

		private void Update()
		{
			if (_isActiveOrb && !_blockTracking && _aimingShotTransform != null)
			{
				base.transform.rotation = _aimingShotTransform.rotation;
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			_isTooltipDisplaying = true;
			TooltipManager.Instance.ShowTooltipOrb(_attack, base.transform.position, new Vector3(0f, -1f));
			_selectionIndicator.SetActive(value: true);
		}

		public void OnDeselect(BaseEventData data)
		{
			HideTooltipIfEnabled();
			_selectionIndicator.SetActive(value: false);
		}

		public void UpdatePersistentOrb(int persistLevel)
		{
			_blockTracking = false;
			if (persistLevel >= 0)
			{
				_textCanvas.gameObject.SetActive(value: true);
				_textCanvas.alpha = 1f;
				_textCanvas.GetComponentInChildren<TMP_Text>().text = "x " + (persistLevel + 2);
			}
			else
			{
				_textCanvas.alpha = 0f;
			}
		}

		public void StartTextCanvasFade(float fadeTime)
		{
			_textCanvas.alpha = 0f;
		}

		private void DisableCollider()
		{
			GetComponent<Collider2D>().enabled = false;
		}

		private void HandleShotFired(Vector2 aimVector)
		{
			DisableActiveTransform();
		}

		private void DisableActiveTransform()
		{
			_blockTracking = true;
		}

		private void OnMouseOver()
		{
			if (!_isTooltipDisplaying && !GameBlockingWindow.windowOpen && BattleController.CurrentBattleState != BattleController.BattleState.AWAITING_POST_BATTLE_CONTROLLER)
			{
				_mouseOverTime += Time.deltaTime;
				if (_mouseOverTime >= 0.2f)
				{
					_isTooltipDisplaying = true;
					TooltipManager.Instance.ShowTooltipOrb(_attack, base.transform.position, new Vector3(0f, -1f));
				}
			}
		}

		public void PreSetActive()
		{
			UpdateAimTrackingTransform();
			ChangeSpriteOnCrit component = _attack.GetComponent<ChangeSpriteOnCrit>();
			if (component != null && component.startsDull)
			{
				_renderer.sprite = _aimingShotTransform.GetComponent<SpriteRenderer>().sprite;
			}
		}

		public void SetActive()
		{
			_isActiveOrb = true;
			_renderer.sprite = _aimingShotTransform.GetComponent<SpriteRenderer>().sprite;
			UpdatePersistentOrb(BattleController.currentBallPersistLevel);
		}

		private void UpdateAimTrackingTransform()
		{
			if (_battleController != null)
			{
				_aimingShotTransform = _battleController.activePachinkoBall.transform.Find("Sprite").transform;
			}
		}

		private void OnMouseExit()
		{
			HideTooltipIfEnabled();
		}

		private void HideTooltipIfEnabled()
		{
			if (_isTooltipDisplaying)
			{
				_isTooltipDisplaying = false;
				_mouseOverTime = 0f;
				TooltipManager.Instance.HideTooltip();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_isActiveOrb)
			{
				OrbDiscardButton.OnOrbDiscardButtonClicked?.Invoke();
			}
		}

		private void InventoryViewToggled(bool open)
		{
			if (open)
			{
				HideTooltipIfEnabled();
			}
		}

		private void UpdateSnowballSize(SpriteRenderer snowballSpriteRenderer)
		{
			if (!(_currentOrbSnowballComponent == null))
			{
				_renderer.sprite = snowballSpriteRenderer.sprite;
			}
		}
	}
}
