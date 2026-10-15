using System;
using Battle;
using Battle.Attacks;
using Battle.StatusEffects;
using Challenges;
using I2.Loc;
using PeglinUI;
using Relics;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class TooltipManager : MonoBehaviour
{
	private static TooltipManager _instance = null;

	public static readonly Vector2 TOOLTIP_DEFAULT_OFFSET = new Vector2(1f, -0.5f);

	private const float HOVER_MOUSE_TIME = 0.3f;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject _tooltipPrefab;

	[SerializeField]
	private GameObject _tooltipRelicPrefab;

	[SerializeField]
	private GameObject _tooltipStatusEffectsPrefab;

	private LocalizationParamsManager _relicDescParams;

	private Tooltip _tooltipOrb;

	private Tooltip _tooltipRelic;

	private Tooltip _tooltipStatusEffects;

	private Canvas _canvas;

	private RectTransform _canvasRect;

	private bool _mouseControl;

	private Vector3 _offset;

	private Tooltip _currentTooltip;

	private bool _tooltipShowing;

	private float _tooltipTimer;

	public static TooltipManager Instance => _instance;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		_tooltipOrb = UnityEngine.Object.Instantiate(_tooltipPrefab, base.transform).GetComponent<Tooltip>();
		_tooltipRelic = UnityEngine.Object.Instantiate(_tooltipRelicPrefab, base.transform).GetComponent<Tooltip>();
		_tooltipStatusEffects = UnityEngine.Object.Instantiate(_tooltipStatusEffectsPrefab, base.transform).GetComponent<Tooltip>();
		_relicDescParams = _tooltipRelic.GetComponentInChildren<LocalizationParamsManager>(includeInactive: true);
		_currentTooltip = _tooltipOrb;
		HideTooltip();
		_canvas = GetComponent<Canvas>();
		_canvas.worldCamera = Camera.main;
		_canvasRect = _canvas.GetComponent<RectTransform>();
		AttackManager.OnFireComplete = (Action)Delegate.Combine(AttackManager.OnFireComplete, new Action(UpdateRelicParamsOnAttack));
		DeckManager.onDeckSizeChanged = (Action)Delegate.Combine(DeckManager.onDeckSizeChanged, new Action(UpdateDeckSizeParam));
	}

	private void OnDestroy()
	{
		AttackManager.OnFireComplete = (Action)Delegate.Remove(AttackManager.OnFireComplete, new Action(UpdateRelicParamsOnAttack));
		DeckManager.onDeckSizeChanged = (Action)Delegate.Remove(DeckManager.onDeckSizeChanged, new Action(UpdateDeckSizeParam));
	}

	private void Update()
	{
		if (_mouseControl && _tooltipTimer < 0.3f)
		{
			_tooltipTimer += Time.deltaTime;
		}
		else if (_mouseControl && _tooltipTimer >= 0.3f && !_tooltipShowing)
		{
			_currentTooltip.gameObject.SetActive(value: true);
			_tooltipShowing = true;
		}
	}

	public void ShowTooltipStatusEffect(StatusEffectType effect, Vector3 position, Vector3 offset, bool isOnPlayer, bool followMousePos = false)
	{
		_tooltipStatusEffects.Initialize(effect, isOnPlayer);
		SetupTooltip(_tooltipStatusEffects, position, offset, followMousePos);
	}

	public void ShowTooltipRelic(Relic relic, Vector3 position, Vector3 offset, bool followMousePos = false)
	{
		_tooltipRelic.Initialize(relic);
		SetupTooltip(_tooltipRelic, position, offset, followMousePos);
	}

	public void ShowTooltipRelicLocked(Relic relic, Vector3 position, Vector3 offset, bool followMousePos = false)
	{
		_tooltipRelic.InitializeLocked(relic);
		SetupTooltip(_tooltipRelic, position, offset, followMousePos);
	}

	public void ShowTooltipOrb(Attack attack, Vector3 position, Vector3 offset, bool followMousePos = false)
	{
		_tooltipOrb.Initialize(attack);
		SetupTooltip(_tooltipOrb, position, offset, followMousePos);
	}

	public void ShowTooltipOrbLocked(Attack attack, Vector3 position, Vector3 offset, bool followMousePos = false)
	{
		_tooltipOrb.InitializeLocked(attack);
		SetupTooltip(_tooltipOrb, position, offset, followMousePos);
	}

	public void ShowTooltipChallenge(ChallengeEffect challenge, Vector3 position, Vector2 offset, bool followMousePos = false)
	{
		_tooltipRelic.Initialize(challenge);
		SetupTooltip(_tooltipRelic, position, offset, followMousePos);
	}

	public void ShowTooltipChallengeLocked(ChallengeEffect challenge, Vector3 position, Vector3 offset, bool followMousePos = false)
	{
		_tooltipRelic.InitializeLocked(challenge);
		SetupTooltip(_tooltipRelic, position, offset, followMousePos);
	}

	public void HideTooltip()
	{
		_mouseControl = false;
		if (_tooltipOrb != null)
		{
			_tooltipOrb.gameObject.SetActive(value: false);
		}
		if (_tooltipRelic != null)
		{
			_tooltipRelic.gameObject.SetActive(value: false);
		}
		if (_tooltipStatusEffects != null)
		{
			_tooltipStatusEffects.gameObject.SetActive(value: false);
		}
		_tooltipShowing = false;
		_tooltipTimer = 0f;
	}

	private void SetupTooltip(Tooltip tooltip, Vector3 position, Vector3 offset, bool mouseControl)
	{
		_mouseControl = mouseControl;
		_currentTooltip = tooltip;
		_offset = offset;
		if (!mouseControl)
		{
			tooltip.gameObject.SetActive(value: true);
		}
		MoveToPosition(tooltip, new Vector3(position.x, position.y), offset);
	}

	private void UpdateRelicParamsOnAttack()
	{
		if (_relicDescParams != null)
		{
			if (BattleController.BattleActive && AttackManager.PreviousAttackDamage > 0)
			{
				_relicDescParams.SetParameterValue("ECHO_CURRENT_DMG", "(+" + Mathf.FloorToInt(Mathf.FloorToInt((float)AttackManager.PreviousAttackDamage * RelicManager.ECHO_CHAMBER_PERCENT)) + ")");
			}
			else
			{
				_relicDescParams.SetParameterValue("ECHO_CURRENT_DMG", "");
			}
		}
	}

	private void UpdateDeckSizeParam()
	{
		if (_relicDescParams != null)
		{
			_relicDescParams.SetParameterValue("DECK_SIZE_HEAL_TOTAL", RelicManager.GetDeckSizeHealValue().ToString());
		}
	}

	private void MoveToPosition(Tooltip tooltip, Vector3 targetPos, Vector3 offset)
	{
		if (_canvas.worldCamera == null)
		{
			_canvas.worldCamera = Camera.main;
		}
		tooltip.transform.position = new Vector2(targetPos.x, targetPos.y) + new Vector2(offset.x, offset.y);
		if (_canvasRect.rect.height - Mathf.Abs(tooltip.position.y) - tooltip.height < 2f)
		{
			float y = Mathf.Max(Mathf.Min(tooltip.position.y + tooltip.height - offset.y, -10f), -258f);
			tooltip.position = new Vector3(tooltip.position.x, y);
		}
		if (_canvasRect.rect.width - Mathf.Abs(tooltip.position.x) - tooltip.width < 0f)
		{
			float x = Mathf.Max(tooltip.position.x - tooltip.width - offset.x * 2f, 4f);
			tooltip.position = new Vector3(x, tooltip.position.y);
		}
	}
}
