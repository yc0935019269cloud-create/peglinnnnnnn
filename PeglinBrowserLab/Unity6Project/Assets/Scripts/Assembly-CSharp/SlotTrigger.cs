using System;
using System.Collections;
using Battle;
using Battle.Enemies;
using Battle.Pachinko.BallBehaviours;
using DG.Tweening;
using Map;
using TMPro;
using UnityEngine;

public class SlotTrigger : MonoBehaviour
{
	public delegate void SlotTriggerActivated(int index, PachinkoBall pBall);

	private const string FLAME_END_TRIGGER = "EndFire";

	public static SlotTriggerActivated onSlotTriggerActivated = delegate
	{
	};

	public SpriteRenderer slotGradient;

	public GameObject damageFlames;

	private Animator[] damageFlamesAnimators;

	public RuntimeAnimatorController orangeFlames;

	public RuntimeAnimatorController purpleFlames;

	public Color goodColor = Color.yellow;

	public Color badColor = Color.red;

	private Color _originalColor = Color.white;

	public int index = -1;

	private bool _damageOnEnter;

	private float _multiplier = 1f;

	private TMP_Text _slotText;

	private bool _isPortal;

	private int _portalUsageCount;

	private const int _portalUsageLimit = 3;

	private bool _useHigherBattleYPos;

	public bool damageOnEnter => _damageOnEnter;

	public float multiplier
	{
		get
		{
			return _multiplier;
		}
		set
		{
			if (value != -2f)
			{
				if (value == -1f)
				{
					_multiplier = 0.5f;
				}
				else
				{
					_multiplier = value;
				}
			}
			else
			{
				_multiplier = 1f;
			}
			if (_slotText != null)
			{
				_slotText.text = $"x{_multiplier}";
			}
		}
	}

	private void Awake()
	{
		_slotText = GetComponentInChildren<TMP_Text>(includeInactive: true);
		_originalColor = _slotText.color;
		if (MapController.instance != null && MapController.instance.Act == 4)
		{
			SetSlotColorPurple();
		}
		else
		{
			SetSlotColorOrange();
		}
	}

	private void OnEnable()
	{
		BattleController.OnVictory = (Action)Delegate.Combine(BattleController.OnVictory, new Action(DisablePortal));
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(OnSpiritOfRadiaPhaseTrasitionStarted));
	}

	private void OnDisable()
	{
		BattleController.OnVictory = (Action)Delegate.Remove(BattleController.OnVictory, new Action(DisablePortal));
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(OnSpiritOfRadiaPhaseTrasitionStarted));
	}

	private void DisablePortal()
	{
		TogglePortal(on: false, Color.white);
	}

	public void TogglePortal(bool on, Color portalColor)
	{
		slotGradient.color = portalColor;
		slotGradient.gameObject.SetActive(on);
		_isPortal = on;
		_portalUsageCount = 0;
	}

	public void SetSlotFlame(bool on)
	{
		if (_damageOnEnter == on)
		{
			return;
		}
		_damageOnEnter = on;
		if (on)
		{
			damageFlames.SetActive(value: true);
			return;
		}
		if (damageFlamesAnimators == null)
		{
			damageFlames.SetActive(value: false);
			return;
		}
		Animator[] array = damageFlamesAnimators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTrigger("EndFire");
		}
		StartCoroutine(WaitAndDisableFire());
	}

	public void SetSlotColorOrange()
	{
		if (!(damageFlames == null))
		{
			if (damageFlamesAnimators == null)
			{
				damageFlamesAnimators = damageFlames.GetComponentsInChildren<Animator>();
			}
			Animator[] array = damageFlamesAnimators;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].runtimeAnimatorController = orangeFlames;
			}
		}
	}

	public void SetSlotColorPurple()
	{
		if (!(damageFlames == null))
		{
			if (damageFlamesAnimators == null)
			{
				damageFlamesAnimators = damageFlames.GetComponentsInChildren<Animator>();
			}
			Animator[] array = damageFlamesAnimators;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].runtimeAnimatorController = purpleFlames;
			}
		}
	}

	private IEnumerator WaitAndDisableFire()
	{
		yield return new WaitForSeconds(0.275f);
		damageFlames.SetActive(_damageOnEnter);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PachinkoBall component = collision.GetComponent<PachinkoBall>();
		if (!(component != null))
		{
			return;
		}
		TeleportToTop component2 = collision.GetComponent<TeleportToTop>();
		if ((BattleController.AwaitingShotCompletion() && _isPortal && _portalUsageCount++ < 3) || (component2 != null && component2.CanTeleport()))
		{
			Vector3 position = collision.transform.position;
			position.y = StaticGameData.battleYBounds[1];
			if (_useHigherBattleYPos)
			{
				position.y = StaticGameData.act4BossPhase2TallerYBounds[1];
			}
			component.TeleportTo(position);
			if (_portalUsageCount >= 3)
			{
				slotGradient.DOFade(0f, 0.65f);
			}
			return;
		}
		onSlotTriggerActivated(index, component);
		if (BattleController.AwaitingShotCompletion() && _slotText != null && _slotText.isActiveAndEnabled && Math.Abs(_multiplier - 1f) > 0.05f)
		{
			Color endValue = ((_multiplier > 1f) ? goodColor : badColor);
			float interval = ((_multiplier > 1f) ? 0.25f : 0.45f);
			Sequence sequence = DOTween.Sequence();
			sequence.Append(_slotText.DOColor(endValue, 0.1f));
			sequence.AppendInterval(interval);
			sequence.Append(_slotText.DOColor(_originalColor, 0.1f));
			sequence.Play();
		}
	}

	private void OnSpiritOfRadiaPhaseTrasitionStarted()
	{
		_useHigherBattleYPos = true;
	}
}
