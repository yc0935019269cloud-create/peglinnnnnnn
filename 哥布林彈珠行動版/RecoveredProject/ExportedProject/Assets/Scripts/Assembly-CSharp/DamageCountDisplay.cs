using System;
using System.Collections.Generic;
using System.Globalization;
using Battle;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class DamageCountDisplay : MonoBehaviour
{
	public enum TextType
	{
		DAMAGE = 0,
		HEAL = 1
	}

	private struct TextDisplay
	{
		public Vector3 position;

		public long damage;

		public int numPegsHit;

		public int critCount;

		public bool allowMulticrit;

		public TextType type;

		public StatusEffectType effect;

		public int statusEffectIntensity;

		public bool emptyAction;
	}

	[SerializeField]
	private Canvas _uiCanvas;

	[SerializeField]
	private ObjectPool _damageLabelPool;

	[SerializeField]
	private float textFullOpacityTime = 0.25f;

	[SerializeField]
	private Color critTextColor = Color.yellow;

	[SerializeField]
	private Color healTextColor = Color.green;

	[SerializeField]
	private FloatingText _infinityFloatingText;

	private static float runningZOffset;

	private static TMP_Text _previous;

	private Dictionary<int, TextDisplay> _displaysQueued = new Dictionary<int, TextDisplay>();

	private Dictionary<int, int> _frameCountdownDict = new Dictionary<int, int>();

	private const float ADDITIONAL_PEG_TEXT_UP_AMOUNT = 0.85f;

	private int _numPegsHitWhenFirstHitInfDamage = -1;

	public static string FormatDamageNumberAsString(long damage)
	{
		string text = damage.ToString();
		if (text.Length <= 6)
		{
			return text;
		}
		if (damage == long.MaxValue)
		{
			return "∞";
		}
		return damage.ToString("0.00e-0", CultureInfo.InvariantCulture);
	}

	private void OnEnable()
	{
		runningZOffset = 0f;
		if (_damageLabelPool == null)
		{
			_damageLabelPool = base.gameObject.AddComponent<ObjectPool>();
		}
		GrantStatusOnPegHits.OnPegHitStatusEffectApplied = (GrantStatusOnPegHits.PegHitEvent)Delegate.Combine(GrantStatusOnPegHits.OnPegHitStatusEffectApplied, new GrantStatusOnPegHits.PegHitEvent(QueueStatusEffectDisplay));
		BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Combine(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
	}

	private void OnDisable()
	{
		GrantStatusOnPegHits.OnPegHitStatusEffectApplied = (GrantStatusOnPegHits.PegHitEvent)Delegate.Remove(GrantStatusOnPegHits.OnPegHitStatusEffectApplied, new GrantStatusOnPegHits.PegHitEvent(QueueStatusEffectDisplay));
		BattleController.OnShotComplete = (BattleController.ShotComplete)Delegate.Remove(BattleController.OnShotComplete, new BattleController.ShotComplete(OnShotComplete));
	}

	private void Update()
	{
		if (_displaysQueued.Count == 0)
		{
			return;
		}
		int num = -1;
		foreach (int key in _displaysQueued.Keys)
		{
			int num2 = _frameCountdownDict[key];
			if (num2 - 1 <= 0)
			{
				DisplayFullText(_displaysQueued[key]);
				num = key;
				break;
			}
			_frameCountdownDict[key] = num2 - 1;
		}
		if (num != -1)
		{
			_displaysQueued.Remove(num);
			_frameCountdownDict.Remove(num);
		}
	}

	public void QueueDamageTextDisplay(Peg peg, long damage, int numPegsHit, TextType textType, bool emptyAction)
	{
		if (_displaysQueued.TryGetValue(peg.GetInstanceID(), out var value))
		{
			value.damage = damage;
			value.type = textType;
			if (value.emptyAction && !emptyAction)
			{
				value.emptyAction = false;
			}
			_displaysQueued[peg.GetInstanceID()] = value;
		}
		else
		{
			TextDisplay value2 = default;
			value2.position = peg.GetCenterOfPeg();
			value2.damage = damage;
			value2.numPegsHit = numPegsHit;
			value2.type = textType;
			value2.critCount = 0;
			value2.allowMulticrit = false;
			value2.effect = StatusEffectType.None;
			value2.statusEffectIntensity = 0;
			value2.emptyAction = emptyAction;
			_displaysQueued.Add(peg.GetInstanceID(), value2);
			_frameCountdownDict.Add(peg.GetInstanceID(), 2);
		}
	}

	public void QueueCritTextDisplay(Peg peg, int critCount, bool allowMulticrit, bool emptyAction)
	{
		if (_displaysQueued.TryGetValue(peg.GetInstanceID(), out var value))
		{
			value.critCount = critCount;
			value.allowMulticrit = allowMulticrit;
			if (value.emptyAction && !emptyAction)
			{
				value.emptyAction = false;
			}
			_displaysQueued[peg.GetInstanceID()] = value;
		}
		else
		{
			TextDisplay value2 = default;
			value2.position = peg.GetCenterOfPeg();
			value2.damage = 0L;
			value2.numPegsHit = 0;
			value2.type = TextType.DAMAGE;
			value2.critCount = critCount;
			value2.allowMulticrit = allowMulticrit;
			value2.effect = StatusEffectType.None;
			value2.statusEffectIntensity = 0;
			value2.emptyAction = emptyAction;
			_displaysQueued.Add(peg.GetInstanceID(), value2);
			_frameCountdownDict.Add(peg.GetInstanceID(), 2);
		}
	}

	public void QueueStatusEffectDisplay(Peg peg, StatusEffectType effectType, int intensity)
	{
		if (_displaysQueued.TryGetValue(peg.GetInstanceID(), out var value))
		{
			value.effect = effectType;
			_displaysQueued[peg.GetInstanceID()] = value;
			return;
		}
		TextDisplay value2 = default;
		value2.position = peg.GetCenterOfPeg();
		value2.damage = 0L;
		value2.numPegsHit = 0;
		value2.critCount = 0;
		value2.allowMulticrit = false;
		value2.type = TextType.DAMAGE;
		value2.effect = effectType;
		value2.statusEffectIntensity = intensity;
		value2.emptyAction = true;
		_displaysQueued.Add(peg.GetInstanceID(), value2);
		_frameCountdownDict.Add(peg.GetInstanceID(), 2);
	}

	private void DisplayFullText(TextDisplay display)
	{
		if (display.critCount > 0 && !display.emptyAction)
		{
			DisplayCritText(display.critCount, display.position + Vector3.up * 0.85f, display.allowMulticrit);
		}
		if (display.effect != StatusEffectType.None)
		{
			int num = ((display.critCount <= 0) ? 1 : 2);
			Vector3 vector = Vector3.up * (0.85f * (float)num);
			DisplayStatusEffect(display.effect, display.position + vector, display.statusEffectIntensity);
		}
		if (display.type == TextType.DAMAGE && !display.emptyAction)
		{
			DisplayDamage(display.damage, display.numPegsHit, display.position);
		}
		else if (display.type == TextType.HEAL && !display.emptyAction)
		{
			DisplayHeal(display.damage, display.numPegsHit, display.position);
		}
	}

	public void DisplayDamage(long damage, int numPegsHit, Vector2 position)
	{
		string textOrLocKey = FormatDamageNumberAsString(damage);
		CreateText(textOrLocKey, position, Color.white, localize: false, numPegsHit);
	}

	public void DisplayStatusEffect(StatusEffectType type, Vector2 position, int intensity)
	{
		string textOrLocKey = $"+{intensity} <sprite name=\"{type.ToString().ToUpper()}\">";
		CreateText(textOrLocKey, position, Color.white);
	}

	public void DisplayHeal(long damage, int numPegsHit, Vector2 position)
	{
		string textOrLocKey = FormatDamageNumberAsString(damage);
		CreateText(textOrLocKey, position, healTextColor, localize: false, numPegsHit);
	}

	public void DisplayCritText(int critCount, Vector2 position, bool allowMulticrit = false)
	{
		string textOrLocKey = ((!allowMulticrit || critCount <= 1) ? "Crit!" : "multi_crit");
		FloatingText floatingText = CreateText(textOrLocKey, position, critTextColor, localize: true);
		if (floatingText != null)
		{
			LocalizationParamsManager component = floatingText.GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("CRIT_COUNT", critCount.ToString());
			}
		}
	}

	public FloatingText CreateText(string textOrLocKey, Vector2 position, Color color, bool localize = false, int numPegsHit = 0)
	{
		bool flag = textOrLocKey == "∞";
		FloatingText floatingText = (flag ? _infinityFloatingText : _damageLabelPool.GetPooledObject<FloatingText>());
		if (flag)
		{
			if (_numPegsHitWhenFirstHitInfDamage == -1)
			{
				_numPegsHitWhenFirstHitInfDamage = numPegsHit;
			}
			if (floatingText.enabled)
			{
				return floatingText;
			}
			localize = false;
		}
		if (floatingText != null)
		{
			floatingText.transform.SetParent(_uiCanvas.transform);
			floatingText.transform.position = new Vector3(position.x, position.y, runningZOffset);
			runningZOffset -= 0.001f;
			if (flag)
			{
				floatingText.gameObject.transform.localScale = (1f + (float)(numPegsHit - _numPegsHitWhenFirstHitInfDamage) * 0.0001f) * Vector3.one;
			}
			if (localize)
			{
				floatingText.ActivateLocalized(textOrLocKey);
			}
			else
			{
				floatingText.Activate(textOrLocKey);
			}
			TMP_Text component = floatingText.GetComponent<TMP_Text>();
			if (floatingText.timeToFade > textFullOpacityTime)
			{
				component.color = color;
				component.DOFade(0f, floatingText.timeToFade - textFullOpacityTime).From(1f).SetDelay(textFullOpacityTime);
			}
			if (_previous == null || !_previous.isActiveAndEnabled)
			{
				runningZOffset = 0f;
			}
			_previous = component;
			floatingText.gameObject.SetActive(value: true);
			return floatingText;
		}
		return null;
	}

	private void OnShotComplete()
	{
		_numPegsHitWhenFirstHitInfDamage = -1;
	}
}
