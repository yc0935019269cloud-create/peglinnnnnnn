using System;
using System.Collections.Generic;
using Battle;
using Battle.Pachinko.BallBehaviours;
using Battle.PegBehaviour;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Relics;
using TMPro;
using UnityEngine;

public class LongPeg : Peg, IDummiablePegTypeChanger, IDummyPeg
{
	[Serializable]
	public struct LongPegColors
	{
		public Color Regular;

		public Color Bonus;

		public Color Hit;

		public Color Cleared;
	}

	[HideInInspector]
	public string id;

	public float TimeToDisappear = 0.5f;

	[Header("Long Peg Properties")]
	[SerializeField]
	private float _endBattleFadeTime = 1f;

	[SerializeField]
	protected float _bombRelicFlingForce = 450f;

	[SerializeField]
	protected GameObject _bombPrefab;

	private Sprite _resetSprite;

	private Sprite _critSprite;

	private Sprite _vineCenterSprite;

	private MeshRenderer _renderer;

	private LongPegColors _colors;

	private Material _activeMaterial;

	private Material _refreshMaterial;

	private Material _persistentRefreshMaterial;

	private Material _critMaterial;

	private Material _persistentCritMaterial;

	private Material _destroyedMaterial;

	private Material _vineMaterial;

	private Material _maxHpMaterial;

	private GameObject _resetAndCritSpriteHolder;

	private SpriteRenderer _resetOrCritSprite;

	private LongPegSlimeBehaviour _slimeBehaviour;

	private PhysicsMaterial2D _defaultPhysicsMat;

	private bool _shieldedThisHit;

	private bool _hit;

	private bool _beingHit;

	private PachinkoBall _beingHitByOrb;

	private float _timeHit;

	private bool _skipDelayedDeath;

	private int _numBouncesCleared;

	private int _maxNumBouncesCleared = 5;

	private Mesh _mesh;

	[HideInInspector]
	public PolygonCollider2D _enclosingSplinePolygonCollider;

	private Vector2 _splineNormal;

	[HideInInspector]
	public LongPegSnapData _snapData;

	private bool _isDummy;

	public bool hit => _hit;

	public void Initialize(float timeToDisappear, LongPegColors colors, Sprite resetSprite, Sprite critSprite, Vector3 normal, PolygonCollider2D polygon, LongPegSnapData longPegSnapData)
	{
		InitializeComponents();
		TimeToDisappear = timeToDisappear;
		_colors = colors;
		_resetSprite = resetSprite;
		_critSprite = critSprite;
		_splineNormal = normal;
		_enclosingSplinePolygonCollider = polygon;
		_snapData = longPegSnapData;
	}

	public void Initialize(LongPeg peg)
	{
		_enclosingSplinePolygonCollider = peg._enclosingSplinePolygonCollider;
		_splineNormal = peg._splineNormal;
		_snapData = peg._snapData;
	}

	public void SetMaterials(Material activeMaterial, Material refreshMaterial, Material critMaterial, Material destroyedMaterial, Material persistentCritMaterial, Material persistentRefreshMaterial, Material vineMaterial, Sprite vineCenterSprite, Material maxHpMaterial)
	{
		_activeMaterial = activeMaterial;
		_refreshMaterial = refreshMaterial;
		_persistentRefreshMaterial = persistentRefreshMaterial;
		_critMaterial = critMaterial;
		_persistentCritMaterial = persistentCritMaterial;
		_destroyedMaterial = destroyedMaterial;
		_maxHpMaterial = maxHpMaterial;
		_vineMaterial = vineMaterial;
		_vineCenterSprite = vineCenterSprite;
		if (_renderer != null)
		{
			_renderer.material = _activeMaterial;
		}
		if (_persistentRefreshMaterial == null)
		{
			_persistentRefreshMaterial = refreshMaterial;
			Debug.LogWarning("Persistent refresh material not set. Defaulting to regular refrsh material.");
		}
		if (_persistentCritMaterial == null)
		{
			_persistentCritMaterial = critMaterial;
			Debug.LogWarning("Persistent crit material not set. Defaulting to regular crit material.");
		}
	}

	protected override void OnEnable()
	{
		InitializeComponents();
		base.OnEnable();
		if (pegType != PegType.DESTROYED)
		{
			_hit = false;
			_beingHit = false;
			_beingHitByOrb = null;
			_numBounces = 0;
			pegType = PegType.REGULAR;
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
			BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(SoftReset));
		}
	}

	private void InitializeComponents()
	{
		_slimeBehaviour = GetComponentInChildren<LongPegSlimeBehaviour>(includeInactive: true);
		_collider = GetComponent<Collider2D>();
		if (_collider != null)
		{
			_defaultPhysicsMat = _collider.sharedMaterial;
		}
		if (_renderer == null)
		{
			_renderer = GetComponent<MeshRenderer>();
		}
		if (_mesh == null)
		{
			_mesh = GetComponent<MeshFilter>().mesh;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
		BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
		BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(SoftReset));
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		SlideOrb(collision);
		DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		PachinkoBall component = collision.collider.GetComponent<PachinkoBall>();
		if (_beingHit && component != null && _beingHitByOrb == component)
		{
			_beingHit = false;
			_beingHitByOrb = null;
		}
	}

	private void Update()
	{
		if (_beingHit)
		{
			_timeHit += Time.deltaTime;
			float num = TimeToDisappear;
			if ((relicManager.RelicEffectActive(RelicEffect.UNPOPPABLE_PEGS) || _blockPoppingFromThisHit) && _numBounces < BouncesToPop)
			{
				num += 0.5f;
			}
			if (_timeHit > num)
			{
				SetActiveStatus(active: false);
				_beingHit = false;
				_beingHitByOrb = null;
				_timeHit = 0f;
			}
		}
	}

	public void ManuallyEndHit()
	{
		_beingHit = false;
	}

	public void SetActiveStatus(bool active)
	{
		if (pegType == PegType.DESTROYED)
		{
			return;
		}
		if (_collider != null)
		{
			_collider.enabled = active;
		}
		if (_trigger != null)
		{
			_trigger.enabled = active;
		}
		if (_poppedPegTrigger != null)
		{
			_poppedPegTrigger.enabled = !active;
		}
		if (_poppedPegCollider != null)
		{
			_poppedPegCollider.enabled = !active;
		}
		if (_specialPegCollider != null)
		{
			_specialPegCollider.enabled = false;
		}
		if (_resetOrCritSprite != null && _resetOrCritSprite.enabled && !active)
		{
			_resetOrCritSprite.enabled = false;
		}
		if (active)
		{
			if (SlimeTypeIsPersistent(slimeType))
			{
				_slimeBehaviour.ApplySlime(slimeType);
			}
			else
			{
				slimeType = SlimeType.None;
			}
			SetActiveMaterial();
			_hit = false;
			return;
		}
		if (_slimeBehaviour != null)
		{
			_slimeBehaviour.StartSlimeDestruction();
		}
		if (_shielded)
		{
			BreakShieldPegImmediately();
		}
		CollectCoins();
		if (_renderer != null)
		{
			_renderer.material = _destroyedMaterial;
			_renderer.material.color = _colors.Cleared;
		}
		if (_hit)
		{
			_hit = false;
		}
		_numBouncesCleared = 0;
	}

	public override void InitPegText()
	{
		if (_pegText == null && _renderer != null)
		{
			CreateChildSpriteHolder();
			GameObject gameObject = UnityEngine.Object.Instantiate(pegTextPrefab, _resetAndCritSpriteHolder.transform, worldPositionStays: false);
			_pegText = gameObject.GetComponentInChildren<TMP_Text>();
			_pegText.enabled = false;
		}
	}

	private void SetActiveMaterial()
	{
		if (_renderer == null)
		{
			return;
		}
		switch (pegType)
		{
		case PegType.CRIT:
			_renderer.material = _critMaterial;
			if (isBonusSpecialPeg)
			{
				_renderer.material = _persistentCritMaterial;
			}
			break;
		case PegType.RESET:
			_renderer.material = _refreshMaterial;
			if (isBonusSpecialPeg)
			{
				_renderer.material = _persistentRefreshMaterial;
			}
			break;
		case PegType.VINE:
			_renderer.material = _vineMaterial;
			break;
		case PegType.MAX_HP:
			_renderer.material = _maxHpMaterial;
			break;
		default:
			_renderer.material = _activeMaterial;
			break;
		}
	}

	public override void SetIsBonusSpecialPeg(bool newIsBonusSpecialPeg)
	{
		isBonusSpecialPeg = newIsBonusSpecialPeg;
		SetActiveMaterial();
	}

	public override void PegActivated(bool playAudio = true, bool forcePop = false)
	{
		if (_hit || (TestShieldPegsOnActivated() && !relicManager.AttemptUseRelic(RelicEffect.SHIELD_PEGS_ADD_DAMAGE)))
		{
			return;
		}
		PegType pegType = base.pegType;
		_numBounces++;
		if (playAudio)
		{
			Peg.OnPegAudioRequest?.Invoke(base.pegType);
		}
		if (pegType == PegType.VINE)
		{
			bramballVine.DestroyVine();
		}
		if (slimeType == SlimeType.HealSlime && !_slimedThisHit && !IsDisabled())
		{
			float num = 1f;
			if (buffAmount > 0)
			{
				num += Mathf.Floor(buffAmount / Peg.BUFF_PER_HEAL_INCREASE);
			}
			else if (buffAmount < 0)
			{
				num += Mathf.Floor(Mathf.Abs(buffAmount) / Peg.DEBUFF_PER_HEAL_INCREASE);
			}
			PlayerHealthController.OnExternalHealRequested?.Invoke(num);
			Peg.OnSlimedPegAudioRequest?.Invoke(slimeType);
		}
		else if (slimeType == SlimeType.LightningSlime && !_slimedThisHit && !IsDisabled())
		{
			StartCoroutine(ActivateLightningSlime());
		}
		Peg.OnPegHit(base.pegType, this);
		if (overlapActivationsThisShot < 10 || !forcePop)
		{
			Peg.OnPegActivated(base.pegType, this);
		}
		if (base.pegType == PegType.RESET)
		{
			if (relicManager.AttemptUseRelic(RelicEffect.REFRESH_PEGS_SPLASH))
			{
				StartCoroutine(RefreshSplash());
			}
			if (!justDuplicated)
			{
				base.pegType = PegType.REGULAR;
				UpdateInvisibilityFromBlind();
			}
		}
		if (_renderer == null)
		{
			return;
		}
		CollectCoin();
		if (forcePop || ShouldPopPegOnHit())
		{
			_cleared = true;
			_hit = true;
			ResetShieldOverlay();
			if (!_poppedPegTrigger.enabled)
			{
				_renderer.material = _activeMaterial;
			}
			else
			{
				_renderer.material = _destroyedMaterial;
			}
			_renderer.material.color = _colors.Hit;
			base.pegType = PegType.REGULAR;
			UpdateInvisibilityFromBlind();
			SetSpriteCentered(null);
			RemoveSlime();
		}
		else if (_slimedThisHit || _shieldedThisHit)
		{
			Reset(BattleController.criticalActive);
			SetActiveStatus(active: true);
		}
		else if (!ShouldPopPegOnHit() && (pegType == PegType.CRIT || pegType == PegType.RESET))
		{
			Reset(BattleController.criticalActive);
		}
		CheckForRelicBuff();
		justDuplicated = false;
	}

	public override bool ShouldPopPegOnHit()
	{
		bool num = justDuplicated;
		justDuplicated = false;
		if (num)
		{
			return false;
		}
		if (_blockPoppingFromThisHit || _slimedThisHit || _shieldedThisHit || _shielded || _collider == null)
		{
			return false;
		}
		if (FragileFromSlime())
		{
			return true;
		}
		if ((relicManager.RelicEffectActive(RelicEffect.UNPOPPABLE_PEGS) || IndestructibleFromSlime()) && _numBounces >= BouncesToPop)
		{
			return true;
		}
		if (!IndestructibleFromSlime())
		{
			return !relicManager.RelicEffectActive(RelicEffect.UNPOPPABLE_PEGS);
		}
		return false;
	}

	public override void Reset(bool criticalHitActive = false)
	{
		_ = base.pegType;
		PegType pegType = base.pegType;
		if (pegType == PegType.DESTROYED || pegType == PegType.DULL || pegType == PegType.BOUNCER || pegType == PegType.MAX_HP)
		{
			return;
		}
		_timeHit = 0f;
		if ((base.pegType != PegType.VINE || !(bramballVine != null)) && base.pegType != PegType.SPINFECTION)
		{
			base.pegType = PegType.REGULAR;
		}
		SetIsBonusSpecialPeg(newIsBonusSpecialPeg: false);
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(isSpecial: false);
		}
		UpdateInvisibilityFromBlind();
		_specialPegCollider.enabled = false;
		SetActiveMaterial();
		if (IsDisabled())
		{
			SetActiveStatus(active: true);
		}
		if (_pegText != null)
		{
			_pegText.text = buffAmount.ToString();
			CheckPegTextEnabled();
			if (_pegText.enabled && _pegText.alpha == 0f)
			{
				SetPegTextVisible(visible: true);
			}
		}
		if (_renderer == null)
		{
			return;
		}
		if (!_hit)
		{
			Color color = _colors.Regular;
			if (criticalHitActive)
			{
				color = _colors.Bonus;
			}
			else if (_cleared)
			{
				color = _colors.Cleared;
			}
			_renderer.material.color = color;
			SetSpriteCentered(null);
		}
		else
		{
			_renderer.material.color = _colors.Hit;
			SetSpriteCentered(null);
			_hit = false;
		}
	}

	public void HardReset()
	{
		if (pegType != PegType.DESTROYED)
		{
			_hit = false;
			_numBounces = 0;
			_numBouncesCleared = 0;
			_timeHit = 0f;
			SetActiveStatus(active: true);
			ConvertPegToType(pegType);
		}
	}

	public override bool IsDelayedDeath()
	{
		if (_skipDelayedDeath)
		{
			_skipDelayedDeath = false;
			return false;
		}
		if (!ShouldPopPegOnHit())
		{
			return false;
		}
		return true;
	}

	public override GameObject ConvertPegToType(PegType type)
	{
		GameObject result = base.gameObject;
		if (base.pegType == PegType.DESTROYED)
		{
			return result;
		}
		PegType pegType = base.pegType;
		if (pegType == PegType.RESET && type == PegType.CRIT)
		{
			Debug.LogWarning("Crit potion is attempting to clobber a reset potion!");
			return result;
		}
		if (pegType == PegType.VINE || pegType == PegType.SPINFECTION)
		{
			return result;
		}
		if (_renderer == null)
		{
			return result;
		}
		if (PegCoinOverlayInstance != null)
		{
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(type == PegType.CRIT || type == PegType.RESET || type == PegType.VINE || type == PegType.SPINFECTION || type == PegType.MAX_HP || type == PegType.CRYSTAL);
		}
		base.pegType = type;
		UpdateInvisibilityFromBlind();
		SetActiveMaterial();
		switch (type)
		{
		case PegType.CRIT:
			SetPegTextVisible(visible: false);
			SetSpriteCentered(_critSprite);
			_specialPegCollider.enabled = true;
			break;
		case PegType.RESET:
			SetPegTextVisible(visible: false);
			SetSpriteCentered(_resetSprite);
			_specialPegCollider.enabled = true;
			if (_shielded)
			{
				BreakShieldPegImmediately();
			}
			break;
		case PegType.BOMB:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_bombPrefab, base.transform);
			result = gameObject;
			gameObject.transform.position = GetCenterOfPeg();
			Peg.OnPegAdded?.Invoke(gameObject, this);
			DestroyPeg(pegType);
			break;
		}
		case PegType.VINE:
			SetPegTextVisible(visible: false);
			SetSpriteCentered(_vineCenterSprite);
			break;
		case PegType.REGULAR:
			SetPegTextVisible(visible: true);
			SetSpriteCentered(null);
			if (BattleController.criticalActive)
			{
				ConvertToBonusPeg();
			}
			break;
		case PegType.MAX_HP:
			SetPegTextVisible(visible: false);
			break;
		default:
			Debug.LogWarning("Attempted to turn a peg into an unsupported peg type " + type);
			base.pegType = pegType;
			break;
		}
		if (base.pegType != PegType.DESTROYED)
		{
			UpdateInvisibilityFromBlind();
		}
		return result;
	}

	protected override void HidePeg()
	{
		base.HidePeg();
		if (slimeType != SlimeType.None && _slimeBehaviour != null)
		{
			_slimeBehaviour.StartSlimeDestruction();
		}
		UnityEngine.Object.Destroy(_collider);
		_trigger.enabled = false;
		if (_renderer != null)
		{
			_renderer.enabled = false;
		}
		if (_resetAndCritSpriteHolder != null)
		{
			_resetAndCritSpriteHolder.SetActive(value: false);
		}
	}

	private void SetPegTextVisible(bool visible)
	{
		if (!(_pegText == null))
		{
			_pegText.alpha = (visible ? 1f : 0f);
		}
	}

	public override bool IsWaitingForDeath()
	{
		return _hit;
	}

	public override void ApplySlimeToPeg(SlimeType sType)
	{
		base.ApplySlimeToPeg(sType);
		if (!SupportsPegType(PegType.SLIME_ONLY, sType) || sType == slimeType)
		{
			return;
		}
		if (relicManager.AttemptUseRelic(RelicEffect.SLIME_BUFFS_PEGS))
		{
			AddBuff(5);
		}
		if (slimeType == SlimeType.None && Peg.IsSlimeTypeBuffable(sType) && relicManager.AttemptUseRelic(RelicEffect.DEBUFF_SLIME_PEGS_HARDER))
		{
			AddBuff(-20);
		}
		_timeHit = 0f;
		_numBounces = 0;
		_numBouncesCleared = 0;
		_slimeBehaviour.gameObject.SetActive(value: true);
		slimeType = sType;
		_slimeBehaviour.ApplySlime(sType);
		if (_collider != null)
		{
			_collider.sharedMaterial = _slimeBehaviour.GetMaterialForSlimeType(sType);
			PhysicsMaterial2D materialForSlimeAndBuffs = GetMaterialForSlimeAndBuffs();
			if (materialForSlimeAndBuffs != null)
			{
				_collider.sharedMaterial = materialForSlimeAndBuffs;
			}
		}
	}

	public override void RemoveSlime(bool forceRemoval = false)
	{
		if (slimeType == SlimeType.None)
		{
			return;
		}
		_slimeBehaviour.StartSlimeDestruction();
		if (!ShouldIgnoreSlimeOnPegPop(slimeType) | forceRemoval)
		{
			slimeType = SlimeType.None;
			if (_collider != null)
			{
				_collider.sharedMaterial = _defaultPhysicsMat;
			}
		}
	}

	private void ConvertToBonusPeg()
	{
		if (!(_renderer == null) && pegType == PegType.REGULAR && !IsDisabled() && !_hit)
		{
			_renderer.material.color = _colors.Bonus;
		}
	}

	private void ConvertToRegularPeg()
	{
		if (!(_renderer == null) && pegType == PegType.REGULAR && !IsDisabled() && !_hit)
		{
			_renderer.material.color = (_cleared ? _colors.Cleared : _colors.Regular);
		}
	}

	private void SoftReset()
	{
		_timeHit = 0f;
		_hasSplashedThisShot = false;
		overlapActivationsThisShot = 0;
		if (!IsDisabled() && (pegType == PegType.REGULAR || (pegType == PegType.CRIT && !isBonusSpecialPeg && !relicManager.RelicEffectActive(RelicEffect.DUPLICATE_SPECIAL_PEGS))))
		{
			Reset();
		}
	}

	public override bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None)
	{
		if (pegType == PegType.CRYSTAL)
		{
			return false;
		}
		if ((pType == PegType.RESET || pType == PegType.CRIT) && isBonusSpecialPeg)
		{
			return false;
		}
		if ((pegType == PegType.RESET || pegType == PegType.CRIT) && (pType == PegType.RESET || pType == PegType.CRIT))
		{
			return false;
		}
		if ((pType == PegType.RESET || pType == PegType.CRIT) && _hit)
		{
			return false;
		}
		if (pegType == PegType.MAX_HP && (pType == PegType.RESET || pType == PegType.CRIT || pType == PegType.BOMB))
		{
			return false;
		}
		if (pegType == PegType.VINE || pegType == PegType.SPINFECTION)
		{
			return false;
		}
		switch (pType)
		{
		case PegType.BOMB:
			if (!_shielded && !_hit && pegType != PegType.RESET)
			{
				return pegType != PegType.CRIT;
			}
			return false;
		case PegType.CRIT:
			return pegType != PegType.RESET;
		case PegType.REGULAR:
		case PegType.RESET:
		case PegType.GOLD:
			return true;
		case PegType.VINE:
			return pegType == PegType.REGULAR;
		case PegType.SLIME_ONLY:
			if (sType > slimeType)
			{
				return sType != SlimeType.FlameOverlay;
			}
			return false;
		case PegType.CRYSTAL:
			return false;
		case PegType.MAX_HP:
			if (pegType != PegType.RESET)
			{
				return pegType != PegType.CRIT;
			}
			return false;
		default:
			return false;
		}
	}

	public override void RemoveIfCleared()
	{
		if (_renderer == null || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP)
		{
			return;
		}
		if (_cleared && NumCoins() <= 0)
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_poppedPegTrigger != null)
			{
				_poppedPegTrigger.enabled = true;
			}
			if (_poppedPegCollider != null)
			{
				_poppedPegCollider.enabled = true;
			}
			if (_specialPegCollider != null)
			{
				_specialPegCollider.enabled = false;
			}
			if (_resetOrCritSprite != null)
			{
				_resetOrCritSprite.DOFade(0f, _endBattleFadeTime - 0.01f);
			}
			if (_pegText != null && _pegText.enabled)
			{
				_pegText.DOFade(0f, _endBattleFadeTime - 0.01f);
			}
			TweenerCore<Color, Color, ColorOptions> tweenerCore = GetComponent<MeshRenderer>().material.DOFade(0f, _endBattleFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				base.gameObject.SetActive(value: false);
			}));
		}
		else if (pegType != PegType.REGULAR)
		{
			pegType = PegType.REGULAR;
			_renderer.material.color = _colors.Regular;
			UpdateInvisibilityFromBlind();
		}
	}

	public override bool IsDisabled()
	{
		if (base.gameObject.activeInHierarchy && !(_collider == null))
		{
			return !_collider.enabled;
		}
		return true;
	}

	public override bool IsPopped()
	{
		if (!(_collider == null))
		{
			return !_collider.enabled;
		}
		return true;
	}

	private void SetSpriteCentered(Sprite sprite)
	{
		if (!(_resetAndCritSpriteHolder == null) || !(sprite == null))
		{
			if (sprite == null && _resetOrCritSprite != null)
			{
				_resetOrCritSprite.enabled = false;
				return;
			}
			CreateChildSpriteHolder();
			_resetAndCritSpriteHolder.SetActive(value: true);
			_resetOrCritSprite.enabled = true;
			_resetOrCritSprite.sprite = sprite;
			_resetOrCritSprite.DOFade(1f, 0.05f);
		}
	}

	private void CreateChildSpriteHolder()
	{
		if (_resetAndCritSpriteHolder == null && _renderer != null)
		{
			_resetAndCritSpriteHolder = new GameObject("Sprites", typeof(SpriteRenderer));
			_resetOrCritSprite = _resetAndCritSpriteHolder.GetComponent<SpriteRenderer>();
			_resetOrCritSprite.sortingLayerName = "PegBoardMain";
			_resetOrCritSprite.sortingOrder = 1;
			_resetAndCritSpriteHolder.transform.parent = base.transform;
			_resetAndCritSpriteHolder.transform.position = _renderer.bounds.center;
			_resetAndCritSpriteHolder.transform.Rotate(new Vector3(0f, 0f, 1f), AlignWithBase(_resetAndCritSpriteHolder.transform.up));
		}
	}

	private float AlignWithBase(Vector2 up)
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector2 vector = mesh.vertices[0];
		Vector2 vector2 = mesh.vertices[2];
		Vector2 to = Vector2.Perpendicular(vector2 - vector);
		float num = ((vector2.y > vector.y) ? 1f : (-1f));
		float num2 = Vector2.Angle(up, to) * num;
		if (num2 > 90f)
		{
			num2 -= 180f;
		}
		else if (num2 < -90f)
		{
			num2 += 180f;
		}
		return num2;
	}

	public override void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
	{
		if (pachinko == null)
		{
			return;
		}
		if (pachinko.IsDummy && cType == CollisionType.COLLIDER)
		{
			if (relicManager.RelicEffectActive(RelicEffect.LEGACY_METEORITE))
			{
				Vector2 vector = pachinko.transform.position - base.transform.position;
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * _bombRelicFlingForce);
			}
			else if (pachinko.meteoriteEnabled && relicManager.GetRemainingCountdownForRelic(RelicEffect.BOMB_FORCE_ALWAYS) <= 1 && relicManager.RelicEffectActive(RelicEffect.BOMB_FORCE_ALWAYS))
			{
				Vector2 vector2 = pachinko.transform.position - base.transform.position;
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * _bombRelicFlingForce);
			}
		}
		else
		{
			if (pachinko.IsDummy)
			{
				return;
			}
			if (cType == CollisionType.COLLIDER && SupportsPegType(PegType.BOMB) && relicManager.AttemptUseRelic(RelicEffect.SPAWN_BOMB_ON_PEG_HITS))
			{
				StartCoroutine(WaitAndConvertToBomb());
			}
			if (cType == CollisionType.COLLIDER)
			{
				CheckAndApplySlime(pachinko);
				CheckAndApplyShield(pachinko);
			}
			else
			{
				_slimedThisHit = false;
				_shieldedThisHit = false;
			}
			SetBlockPopping(pachinko);
			if (_lightningSlimeRenderer != null)
			{
				_lightningSlimeRenderer.chain = new List<Peg>();
			}
			if ((bool)pachinko.GetComponent<PierceBehavior>() && cType == CollisionType.TRIGGER)
			{
				_numBounces = BouncesToPop;
			}
			bool flag = !_hit;
			PegType pegType = base.pegType;
			if (pachinko != null && !_beingHit && !pachinko.IsDummy)
			{
				_beingHit = true;
				_beingHitByOrb = pachinko;
				DestroyPegsOnHit component = pachinko.GetComponent<DestroyPegsOnHit>();
				bool flag2 = false;
				if (!_hit)
				{
					bool num = pachinko.GetComponent<ResetPegOnCollision>();
					if (num)
					{
						_skipDelayedDeath = true;
					}
					int numberOfMultihits = GetNumberOfMultihits(pachinko, cType, base.pegType);
					BreakPegShieldOnHit component2 = pachinko.GetComponent<BreakPegShieldOnHit>();
					if (_shielded && component2 != null)
					{
						flag2 = true;
						BreakShieldPegImmediately();
					}
					if (_shielded && (bool)component)
					{
						flag2 = true;
						BreakShieldPegImmediately();
					}
					if (cType == CollisionType.COLLIDER)
					{
						Peg.OnPegCollisionHit(base.pegType, this);
					}
					if ((!flag2 || relicManager.RelicEffectActive(RelicEffect.SHIELD_PEGS_ADD_DAMAGE)) && !_blockPoppingFromThisHit)
					{
						PegActivated();
					}
					if (num)
					{
						_beingHit = false;
						_beingHitByOrb = null;
						HardReset();
					}
					HandleMultihits(this, numberOfMultihits, pegType);
					if (cType == CollisionType.COLLIDER)
					{
						if (relicManager.AttemptUseRelic(RelicEffect.LEGACY_METEORITE))
						{
							Vector2 vector3 = pachinko.transform.position - base.transform.position;
							pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector3.normalized * _bombRelicFlingForce);
						}
						else if (pachinko.meteoriteEnabled && relicManager.AttemptUseRelic(RelicEffect.BOMB_FORCE_ALWAYS))
						{
							Vector2 vector4 = pachinko.transform.position - GetCenterOfPeg();
							pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector4.normalized * _bombRelicFlingForce);
						}
					}
				}
				else if (++_numBouncesCleared >= _maxNumBouncesCleared)
				{
					SetActiveStatus(active: false);
				}
				if (!flag2 && component != null && cType == CollisionType.COLLIDER)
				{
					DestroyPeg(pegType);
					return;
				}
				if (convertedToGold)
				{
					DestroyPeg(pegType);
					return;
				}
			}
			if (flag && cType == CollisionType.COLLIDER)
			{
				CheckForPachinkoBuff(pachinko);
			}
		}
	}

	private void CheckAndApplyShield(PachinkoBall pachinko)
	{
		CreateShieldPegsOnPegHits component = pachinko.GetComponent<CreateShieldPegsOnPegHits>();
		if (component == null)
		{
			_shieldedThisHit = false;
			return;
		}
		_shieldedThisHit = component.ShouldShieldCurrentPeg(this);
		if (_shieldedThisHit)
		{
			ApplyShielding();
		}
	}

	private void CheckAndApplySlime(PachinkoBall pachinko)
	{
		if (IsDisabled())
		{
			return;
		}
		_slimedThisHit = false;
		PachinkoPegSlimer component = pachinko.GetComponent<PachinkoPegSlimer>();
		if (component != null && !_hit && SupportsPegType(PegType.SLIME_ONLY, component.slimeType) && component.ShouldSlimeCurrentPeg(this) && !_slimedThisHit)
		{
			_slimedThisHit = true;
			ApplySlimeToPeg(component.slimeType);
		}
		if (relicManager.AttemptUseRelic(RelicEffect.APPLIES_HEALING_SLIME) && !_hit && SupportsPegType(PegType.SLIME_ONLY, SlimeType.HealSlime) && !_slimedThisHit)
		{
			_slimedThisHit = true;
			ApplySlimeToPeg(SlimeType.HealSlime);
		}
		if (slimeType < SlimeType.LightningSlime && relicManager.AttemptUseRelic(RelicEffect.RANDOM_SLIME_ON_PEGS_HIT) && !_hit && !_slimedThisHit)
		{
			SlimeType randomPrimeSlimeType = RelicManager.GetRandomPrimeSlimeType();
			if (SupportsPegType(PegType.SLIME_ONLY, randomPrimeSlimeType))
			{
				_slimedThisHit = true;
				ApplySlimeToPeg(randomPrimeSlimeType);
			}
		}
	}

	protected override void SetBlockPopping(PachinkoBall pachinko)
	{
		DontPopSpecialPegsOnHit component = pachinko.GetComponent<DontPopSpecialPegsOnHit>();
		if (component != null)
		{
			if (component.dontPopTheseTypes.HasFlag(pegType) || (!IsSpecialPeg() && component.ignorePegsWithCoins && NumCoins() > 0))
			{
				_blockPoppingFromThisHit = true;
			}
			if (component.dontPopPegsWithCoins)
			{
				_blockCoinCollectionFromThisHit = true;
			}
		}
		else
		{
			_blockPoppingFromThisHit = false;
			_blockCoinCollectionFromThisHit = false;
		}
	}

	public override float GetMagnetForce()
	{
		if (pegType != PegType.RESET && pegType != PegType.CRIT)
		{
			return 0.9f;
		}
		return 6f;
	}

	public override bool InitCoinPrefab()
	{
		if (pegCoinOverlayPrefab == null)
		{
			Debug.LogWarning("Peg Prefab has null coin overlay: " + base.gameObject.name);
			return false;
		}
		CreateChildSpriteHolder();
		if (PegCoinOverlayInstance == null)
		{
			PegCoinOverlayInstance = UnityEngine.Object.Instantiate(pegCoinOverlayPrefab, _resetAndCritSpriteHolder.transform);
		}
		else
		{
			PegCoinOverlayInstance.gameObject.SetActive(value: true);
		}
		return true;
	}

	protected override void InitShieldPrefab()
	{
		CreateChildSpriteHolder();
		if (PegShieldOverlayInstance == null)
		{
			PegShieldOverlayInstance = UnityEngine.Object.Instantiate(pegShieldOverlayPrefab, _resetAndCritSpriteHolder.transform);
		}
		else
		{
			PegShieldOverlayInstance.gameObject.SetActive(value: true);
		}
	}

	public void EndPierce()
	{
		_beingHit = false;
		_beingHitByOrb = null;
	}

	public override void ResetHitCount()
	{
		_numBounces = 0;
	}

	public void GenerateId(bool force = false)
	{
		id = Guid.NewGuid().ToString();
	}

	public string GetId()
	{
		return id;
	}

	public void SetDummyStatus(bool status)
	{
		_isDummy = status;
	}

	public void SetPegStatus(bool status)
	{
		SetActiveStatus(status);
	}

	public bool GetPegStatus()
	{
		if (pegType == PegType.DESTROYED)
		{
			return false;
		}
		return !IsDisabled();
	}

	public void SetSpecialCollider(bool value)
	{
		_specialPegCollider.enabled = value;
	}

	public PegType GetPegType()
	{
		return pegType;
	}

	public void SetPegType(PegType pType)
	{
		pegType = pType;
	}

	protected override void UpdateInvisibilityFromBlind(SpriteRenderer spriteRenderer = null)
	{
		if (!_isInvisibleFromBlind || PegTypeIngnoresInvisibilityFromBlind(pegType))
		{
			if (_renderer != null)
			{
				_renderer.enabled = true;
			}
		}
		else if (_isInvisibleFromBlind && !PegTypeIngnoresInvisibilityFromBlind(pegType) && _renderer != null)
		{
			_renderer.enabled = false;
		}
	}

	private void SlideOrb(Collision2D collision)
	{
		Vector2 point = collision.collider.transform.position;
		bool flag = _enclosingSplinePolygonCollider.OverlapPoint(point);
		if ((_snapData.slideSide.HasFlag(LongPegSnapData.LongPegSlideSide.INSIDE_OF_CURVE) & flag) || (_snapData.slideSide.HasFlag(LongPegSnapData.LongPegSlideSide.OUTSIDE_OF_CURVE) && !flag))
		{
			Vector2 velocity = collision.collider.attachedRigidbody.linearVelocity;
			Vector2 normalized = velocity.normalized;
			Vector2 vector = Vector2.Perpendicular(_splineNormal);
			Vector2 vector2 = -vector;
			Vector2 to = vector2;
			if ((normalized - vector.normalized).magnitude < (normalized - vector2.normalized).magnitude)
			{
				to = vector;
			}
			if (Vector2.Angle(normalized, to) <= _snapData.maxSnapAngle)
			{
				Vector2 velocity2 = to.normalized * velocity.magnitude;
				collision.collider.attachedRigidbody.linearVelocity = velocity2;
			}
		}
	}
}
