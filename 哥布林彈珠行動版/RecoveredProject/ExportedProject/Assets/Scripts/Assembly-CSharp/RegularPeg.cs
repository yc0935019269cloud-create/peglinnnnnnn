using System;
using System.Collections.Generic;
using Battle;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.PegBehaviour;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MoreMountains.Feedbacks;
using Peglin.OdinSerializer.Utilities;
using Relics;
using UnityEngine;

public class RegularPeg : Peg, IDummyPeg
{
	private const int MAX_HITS_PER_SHOT = 50;

	[Header("Visuals")]
	[SerializeField]
	public Sprite _regularSprite;

	[SerializeField]
	public Sprite _dullSprite;

	[SerializeField]
	public Sprite _previouslyClearedSprite;

	[SerializeField]
	protected Sprite _bonusSprite;

	[SerializeField]
	private Sprite _resetSprite;

	[SerializeField]
	private Sprite _persistantResetSprite;

	[SerializeField]
	private Sprite _critSprite;

	[SerializeField]
	private Sprite _persistantCritSprite;

	[SerializeField]
	protected Sprite _destroyedSprite;

	[SerializeField]
	private Sprite _vinePegSprite;

	[SerializeField]
	private Sprite _crystalPegSprite;

	[SerializeField]
	private Sprite _spinfectionPegSprite;

	[SerializeField]
	private Sprite _maxHpPegSprite;

	[SerializeField]
	protected GameObject _bombPrefab;

	[SerializeField]
	private PhysicsMaterial2D _regularPegMaterial;

	[SerializeField]
	private PhysicsMaterial2D _dullPegMaterial;

	[Header("Regular Peg Properties")]
	[SerializeField]
	protected float _endBattleFadeTime = 1f;

	[Tooltip("If true, this peg will be unable to be selected as a special peg. Useful for preventing nearly-impossible refresh peg positions")]
	public bool specialPegBlocker;

	[Tooltip("If true, this peg will be unable to have coins added to it randomly.")]
	public bool coinBlocker;

	public bool canBeDull = true;

	private const float BOMB_RELIC_FLING_FORCE = 450f;

	protected const int BouncesToForcePop = 10;

	protected Vector3 _initialVector = Vector3.one;

	private SpriteRenderer _renderer;

	private Animator _animator;

	protected MMFeedbacks _feedbacks;

	private Sequence _destructionSequence;

	private RegularPegSlimeBehaviour _slimeBehaviour;

	private PhysicsMaterial2D _defaultPhysicsMat;

	private InvisibleOnAwake _invis;

	protected bool _revivedPegThisFrame;

	private bool _shieldedThisHit;

	private Dictionary<GameObject, int> _numTimesHitThisShot = new Dictionary<GameObject, int>();

	[HideInInspector]
	public string id = string.Empty;

	private bool _isDummy;

	public Sprite crystalPegSprite => _crystalPegSprite;

	public float bombRelicFlingForce => 450f;

	protected override void Awake()
	{
		base.Awake();
		_feedbacks = GetComponent<MMFeedbacks>();
		_slimeBehaviour = GetComponentInChildren<RegularPegSlimeBehaviour>(includeInactive: true);
		if (_collider != null)
		{
			_defaultPhysicsMat = _collider.sharedMaterial;
		}
		SlimeOnlyPeg slimeOnlyPeg = this as SlimeOnlyPeg;
		if (_persistantResetSprite == null && slimeOnlyPeg == null)
		{
			_persistantResetSprite = _resetSprite;
			Debug.LogWarning("Persistent reset sprite not set. Defaulting to regular reset sprite.");
		}
		if (_persistantCritSprite == null && slimeOnlyPeg == null)
		{
			_persistantCritSprite = _critSprite;
			Debug.LogWarning("Persistent crit sprite not set. Defaulting to regular crit sprite.");
		}
	}

	protected override void Start()
	{
		base.Start();
		_invis = GetComponent<InvisibleOnAwake>();
		if (slimeType != SlimeType.None)
		{
			_slimeBehaviour.gameObject.SetActive(value: true);
			SpriteRenderer component = _slimeBehaviour.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				component.color = _slimeBehaviour.GetColorForSlimeType(slimeType);
			}
			if (_collider != null)
			{
				_collider.sharedMaterial = _slimeBehaviour.GetMaterialForSlimeType(slimeType);
				PhysicsMaterial2D materialForSlimeAndBuffs = GetMaterialForSlimeAndBuffs();
				if (materialForSlimeAndBuffs != null)
				{
					_collider.sharedMaterial = materialForSlimeAndBuffs;
				}
			}
		}
		_initialVector = base.transform.localScale;
	}

	protected override void OnEnable()
	{
		CheckComponentRefs();
		base.OnEnable();
		if (_renderer != null && _renderer.sprite != _destroyedSprite && pegType != PegType.RESET && pegType != PegType.CRIT && pegType != PegType.DULL && pegType != PegType.CRYSTAL)
		{
			if (BattleController.criticalActive)
			{
				_renderer.sprite = _bonusSprite;
			}
			else
			{
				_renderer.sprite = (_cleared ? _previouslyClearedSprite : _regularSprite);
			}
		}
		BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
		BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
		BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(SoftReset));
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetNumTimesHitThisShot));
	}

	private void CheckComponentRefs()
	{
		if (_renderer == null)
		{
			_renderer = GetComponent<SpriteRenderer>();
		}
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
		BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
		BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(SoftReset));
		PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetNumTimesHitThisShot));
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
		if (_revivedPegThisFrame)
		{
			_revivedPegThisFrame = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (_revivedPegThisFrame)
		{
			_revivedPegThisFrame = false;
		}
		else
		{
			DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
		}
	}

	public override void PegActivated(bool playAudio = true, bool forcePop = false)
	{
		if (!base.gameObject.activeInHierarchy || (TestShieldPegsOnActivated() && !relicManager.AttemptUseRelic(RelicEffect.SHIELD_PEGS_ADD_DAMAGE)))
		{
			return;
		}
		_cleared = true;
		_numBounces++;
		PegType pegType = base.pegType;
		if (base.pegType == PegType.RESET)
		{
			if (!justDuplicated)
			{
				Reset(BattleController.criticalActive);
			}
			if (relicManager.AttemptUseRelic(RelicEffect.REFRESH_PEGS_SPLASH))
			{
				StartCoroutine(RefreshSplash());
			}
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
		Peg.OnPegHit(pegType, this);
		if (overlapActivationsThisShot < 10 || !forcePop)
		{
			Peg.OnPegActivated(pegType, this);
		}
		if (playAudio)
		{
			Peg.OnPegAudioRequest?.Invoke(pegType);
		}
		if (_feedbacks != null)
		{
			_feedbacks.PlayFeedbacks();
		}
		if (!Peg.PegTypeIndestructible(base.pegType) && (forcePop || ShouldPopPegOnHit()))
		{
			PopPeg();
		}
		else
		{
			if (!_slimedThisHit)
			{
				_ = _shieldedThisHit;
			}
			CollectCoin();
		}
		CheckForRelicBuff();
		justDuplicated = false;
		if (pegType == PegType.CRYSTAL)
		{
			DestroyPeg(pegType);
		}
	}

	protected void SwitchToRegularColliders()
	{
		if (_collider != null)
		{
			_collider.enabled = true;
			_trigger.enabled = true;
			_poppedPegTrigger.enabled = false;
			_poppedPegCollider.enabled = false;
			_specialPegCollider.enabled = false;
		}
	}

	protected void DisableRegularColliders()
	{
		if (!(base.gameObject == null) && !(this == null))
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_trigger != null)
			{
				_trigger.enabled = false;
			}
			if (_specialPegCollider != null)
			{
				_specialPegCollider.enabled = false;
			}
			if (_poppedPegCollider != null)
			{
				_poppedPegCollider.enabled = true;
			}
		}
	}

	protected void EnablePoppedColliders()
	{
		if (!(base.gameObject == null))
		{
			if (_poppedPegTrigger != null)
			{
				_poppedPegTrigger.enabled = true;
			}
			if (_poppedPegCollider != null)
			{
				_poppedPegCollider.enabled = true;
			}
		}
	}

	private void PopPeg()
	{
		if (Peg.PegTypeIndestructible(pegType))
		{
			Debug.LogWarning("Something is causing us to try and pop an indestructible peg");
			return;
		}
		DisableRegularColliders();
		ResetShieldOverlay();
		CollectCoins();
		pegType = PegType.REGULAR;
		UpdateInvisibilityFromBlind();
		PlayDestructionAnimation();
		RemoveSlime();
	}

	public override void RemoveSlime(bool forceRemoval = false)
	{
		if (slimeType != SlimeType.None)
		{
			_slimeBehaviour.StartSlimeDestruction();
			if (!ShouldIgnoreSlimeOnPegPop(slimeType) | forceRemoval)
			{
				slimeType = SlimeType.None;
				_collider.sharedMaterial = _defaultPhysicsMat;
			}
		}
	}

	public override bool ShouldPopPegOnHit()
	{
		bool num = justDuplicated;
		justDuplicated = false;
		if (num)
		{
			return false;
		}
		if (_slimedThisHit || _shieldedThisHit || Peg.PegTypeIndestructible(pegType) || !_collider.enabled || _shielded)
		{
			return false;
		}
		if (FragileFromSlime() || _numBounces >= BouncesToPop - 1)
		{
			return true;
		}
		if (_blockPoppingFromThisHit || IndestructibleFromSlime())
		{
			return false;
		}
		return !relicManager.RelicEffectActive(RelicEffect.UNPOPPABLE_PEGS);
	}

	private void PlayDestructionAnimation()
	{
		_destructionSequence = DOTween.Sequence();
		float num = Mathf.Sign(base.transform.localScale.x);
		float x = 1.5f * num;
		float x2 = 0.3f * num;
		_destructionSequence.Append(base.transform.DOScale(new Vector3(x, 1.5f), 0.1f)).Append(base.transform.DOScale(new Vector3(x2, 0.3f), 0.3f)).OnComplete(OnDestroyAnimationEnded);
	}

	private void SoftReset()
	{
		if ((!IsDisabled() && pegType == PegType.REGULAR) || (pegType == PegType.CRIT && !isBonusSpecialPeg && !relicManager.RelicEffectActive(RelicEffect.DUPLICATE_SPECIAL_PEGS)))
		{
			Reset();
		}
		else
		{
			ResetHitCount();
		}
		_hasSplashedThisShot = false;
		overlapActivationsThisShot = 0;
	}

	public override void Reset(bool criticalHitActive = false)
	{
		if (this == null)
		{
			Debug.LogWarning("Trying to reset a regular peg that is null.");
			return;
		}
		_ = pegType;
		if (Peg.PegTypeIndestructible(pegType))
		{
			return;
		}
		if ((pegType != PegType.VINE || !(bramballVine != null)) && pegType != PegType.SPINFECTION)
		{
			pegType = PegType.REGULAR;
		}
		SetIsBonusSpecialPeg(newIsBonusSpecialPeg: false);
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(isSpecial: false);
		}
		UpdateInvisibilityFromBlind();
		FlickeringPeg component = GetComponent<FlickeringPeg>();
		if (SlimeTypeIsPersistent(slimeType) && !_slimeBehaviour.gameObject.activeSelf)
		{
			_slimeBehaviour.gameObject.SetActive(value: true);
			_slimeBehaviour.ApplySlime(slimeType, buffAmount);
		}
		else if (IsDisabled() && slimeType != SlimeType.None && !SlimeTypeIsPersistent(slimeType) && component != null)
		{
			component.StartFlickerSequence();
		}
		SwitchToRegularColliders();
		_numBounces = 0;
		if (_pegText != null)
		{
			_pegText.text = buffAmount.ToString();
			CheckPegTextEnabled();
		}
		if (!(_renderer == null))
		{
			ResetRendererStatus();
			if (criticalHitActive)
			{
				_renderer.sprite = _bonusSprite;
			}
			else
			{
				_renderer.sprite = (_cleared ? _previouslyClearedSprite : _regularSprite);
			}
		}
	}

	public override GameObject ConvertPegToType(PegType type)
	{
		GameObject result = base.gameObject;
		if (_renderer == null)
		{
			return result;
		}
		PegType pegType = base.pegType;
		if (pegType == PegType.RESET && type == PegType.CRIT)
		{
			Debug.LogWarning("Crit potion is attempting to clobber a reset potion!");
			return result;
		}
		switch (pegType)
		{
		case PegType.VINE:
			return result;
		case PegType.SPINFECTION:
			if (type != PegType.REGULAR && type != PegType.DULL)
			{
				return result;
			}
			break;
		}
		base.pegType = type;
		if (_pegText != null)
		{
			_pegText.enabled = false;
		}
		PegGridObscurer componentInParent = GetComponentInParent<PegGridObscurer>();
		if (componentInParent != null && (base.pegType == PegType.CRIT || base.pegType == PegType.RESET) && componentInParent != null)
		{
			componentInParent.PegTypeChanged(base.pegType);
		}
		if (PegCoinOverlayInstance != null)
		{
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(type == PegType.CRIT || type == PegType.RESET || type == PegType.VINE || type == PegType.SPINFECTION || type == PegType.MAX_HP || type == PegType.CRYSTAL);
		}
		UpdateInvisibilityFromBlind();
		switch (type)
		{
		case PegType.CRIT:
			_renderer.sprite = _critSprite;
			if (isBonusSpecialPeg)
			{
				_renderer.sprite = _persistantCritSprite;
			}
			_specialPegCollider.enabled = true;
			break;
		case PegType.RESET:
			_renderer.sprite = _resetSprite;
			if (isBonusSpecialPeg)
			{
				_renderer.sprite = _persistantResetSprite;
			}
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
			Peg.OnPegAdded?.Invoke(gameObject, this);
			if (componentInParent != null)
			{
				componentInParent.assignedPeg = gameObject.GetComponent<Peg>();
				componentInParent.PegTypeChanged(PegType.BOMB);
			}
			DestroyPeg(pegType);
			break;
		}
		case PegType.VINE:
			_renderer.sprite = _vinePegSprite;
			break;
		case PegType.CRYSTAL:
			_renderer.sprite = _crystalPegSprite;
			break;
		case PegType.SPINFECTION:
			base.pegType = PegType.REGULAR;
			Reset();
			base.pegType = PegType.SPINFECTION;
			_renderer.sprite = _spinfectionPegSprite;
			if (PegCoinOverlayInstance != null)
			{
				PegCoinOverlayInstance.ToggleGuardingSpecialPeg(type == PegType.CRIT || type == PegType.RESET || type == PegType.VINE || type == PegType.SPINFECTION || type == PegType.MAX_HP || type == PegType.CRYSTAL);
			}
			break;
		case PegType.DULL:
			base.pegType = PegType.REGULAR;
			Reset();
			base.pegType = PegType.DULL;
			_renderer.sprite = _dullSprite;
			_collider.sharedMaterial = _dullPegMaterial;
			break;
		case PegType.MAX_HP:
			base.pegType = PegType.MAX_HP;
			_renderer.sprite = _maxHpPegSprite;
			_collider.sharedMaterial = _regularPegMaterial;
			break;
		case PegType.REGULAR:
			if (pegType == PegType.DULL || pegType == PegType.MAX_HP || pegType == PegType.SPINFECTION)
			{
				if (BattleController.criticalActive)
				{
					_renderer.sprite = _bonusSprite;
				}
				else
				{
					_renderer.sprite = _regularSprite;
				}
				_collider.sharedMaterial = _regularPegMaterial;
			}
			break;
		default:
			Debug.LogWarning("Attempted to turn a peg into an unsupported peg type " + type);
			base.pegType = pegType;
			break;
		}
		CheckRendererAlpha();
		if (base.pegType != PegType.DESTROYED)
		{
			UpdateInvisibilityFromBlind();
		}
		return result;
	}

	private void ConvertPegToTypeDummy(PegType pType)
	{
		switch (pType)
		{
		case PegType.CRIT:
		case PegType.RESET:
			_specialPegCollider.enabled = true;
			pegType = pType;
			break;
		case PegType.REGULAR:
			_specialPegCollider.enabled = false;
			_collider.sharedMaterial = _regularPegMaterial;
			pegType = pType;
			break;
		case PegType.DULL:
			_specialPegCollider.enabled = false;
			_collider.sharedMaterial = _dullPegMaterial;
			pegType = pType;
			break;
		}
	}

	public override void SetIsBonusSpecialPeg(bool newIsBonusSpecialPeg)
	{
		isBonusSpecialPeg = newIsBonusSpecialPeg;
		switch (pegType)
		{
		case PegType.CRIT:
			_renderer.sprite = _critSprite;
			if (isBonusSpecialPeg)
			{
				_renderer.sprite = _persistantCritSprite;
			}
			break;
		case PegType.RESET:
			_renderer.sprite = _resetSprite;
			if (isBonusSpecialPeg)
			{
				_renderer.sprite = _persistantResetSprite;
			}
			break;
		}
	}

	private void CheckRendererAlpha()
	{
		if (!(GetComponent<FlickeringPeg>() != null))
		{
			Color color = _renderer.color;
			if (pegType == PegType.CRIT || pegType == PegType.RESET)
			{
				color.a = ((_invis == null) ? 1f : 0.75f);
			}
			else if (_invis != null)
			{
				color.a = 0f;
			}
			_renderer.color = color;
		}
	}

	public override void ApplySlimeToPeg(SlimeType sType)
	{
		base.ApplySlimeToPeg(sType);
		if (SupportsPegType(PegType.SLIME_ONLY, sType) && sType != slimeType)
		{
			if (sType == SlimeType.FlameOverlay)
			{
				BreakShieldPegImmediately();
			}
			else if (relicManager.AttemptUseRelic(RelicEffect.SLIME_BUFFS_PEGS))
			{
				AddBuff(5);
			}
			if (slimeType == SlimeType.None && Peg.IsSlimeTypeBuffable(sType) && relicManager.AttemptUseRelic(RelicEffect.DEBUFF_SLIME_PEGS_HARDER))
			{
				AddBuff(-20);
			}
			_numBounces = 0;
			_slimeBehaviour.gameObject.SetActive(value: true);
			slimeType = sType;
			_slimeBehaviour.ApplySlime(sType, buffAmount);
			if (_collider != null)
			{
				_collider.sharedMaterial = _slimeBehaviour.GetMaterialForSlimeType(sType);
			}
			FlickeringPeg component = GetComponent<FlickeringPeg>();
			if (component != null)
			{
				component.StopFlicker();
				ResetRendererStatus();
			}
		}
	}

	public virtual void ConvertToBonusPeg()
	{
		if (_renderer != null && pegType == PegType.REGULAR && !IsDisabled())
		{
			_renderer.sprite = _bonusSprite;
		}
	}

	public virtual void ConvertToRegularPeg()
	{
		if (_renderer != null && pegType == PegType.REGULAR && !IsDisabled())
		{
			_renderer.sprite = (_cleared ? _previouslyClearedSprite : _regularSprite);
		}
	}

	public override void RemoveIfCleared()
	{
		if (pegType == PegType.DESTROYED || _renderer == null || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP)
		{
			return;
		}
		if (_cleared && !IndestructibleFromSlime() && NumCoins() <= 0)
		{
			_collider.enabled = false;
			_specialPegCollider.enabled = false;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _renderer.DOFade(0f, _endBattleFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(Disable));
			if (_pegText != null && _pegText.enabled)
			{
				_pegText.DOFade(0f, _endBattleFadeTime - 0.01f);
			}
			if ((bool)PegCoinOverlayInstance)
			{
				PegCoinOverlayInstance.EndBattleFadeOut(_endBattleFadeTime);
			}
		}
		else if (pegType != PegType.REGULAR && pegType != PegType.DULL && pegType != PegType.BOUNCER)
		{
			pegType = PegType.REGULAR;
			_renderer.sprite = _regularSprite;
			UpdateInvisibilityFromBlind();
		}
	}

	private void ResetRendererStatus()
	{
		if (_destructionSequence != null && _destructionSequence.active)
		{
			_destructionSequence.Kill();
		}
		if (_renderer != null && (GetComponent<FlickeringPeg>() == null || slimeType != SlimeType.None))
		{
			float a = ((_invis != null && pegType != PegType.CRIT && pegType != PegType.RESET) ? 0f : 1f);
			_renderer.color = new Color(1f, 1f, 1f, a);
		}
		base.transform.localScale = _initialVector;
	}

	private void OnDestroyAnimationEnded()
	{
		if (pegType == PegType.DESTROYED)
		{
			base.transform.localScale = Vector3.one;
			return;
		}
		EnablePoppedColliders();
		if (!(_renderer == null))
		{
			ResetRendererStatus();
			_renderer.sprite = _destroyedSprite;
		}
	}

	protected override void HidePeg()
	{
		base.HidePeg();
		if (slimeType != SlimeType.None && _slimeBehaviour != null)
		{
			_slimeBehaviour.StartSlimeDestruction();
		}
		_collider.enabled = false;
		_specialPegCollider.enabled = false;
		if (_renderer != null)
		{
			_renderer.enabled = false;
		}
		if (_pegText != null && _pegText.enabled)
		{
			_pegText.enabled = false;
		}
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public override bool IsDelayedDeath()
	{
		return false;
	}

	public override bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None)
	{
		if (pegType == PegType.BOUNCER)
		{
			return false;
		}
		if (pegType == PegType.CRYSTAL)
		{
			return false;
		}
		if (pType == PegType.DULL && !canBeDull)
		{
			return false;
		}
		if (pegType == PegType.DULL)
		{
			switch (pType)
			{
			case PegType.SLIME_ONLY:
				return sType > slimeType;
			case PegType.BOMB:
			case PegType.SPINFECTION:
				return true;
			default:
				return false;
			}
		}
		if (pegType == PegType.MAX_HP && (pType == PegType.RESET || pType == PegType.CRIT || pType == PegType.BOMB))
		{
			return false;
		}
		if (specialPegBlocker && (pType == PegType.RESET || pType == PegType.CRIT))
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
		if ((pegType == PegType.RESET || pegType == PegType.CRIT || pegType == PegType.DESTROYED) && pType == PegType.DULL)
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
			if (!_shielded && pegType != PegType.RESET)
			{
				return pegType != PegType.CRIT;
			}
			return false;
		case PegType.CRIT:
			return pegType != PegType.RESET;
		case PegType.REGULAR:
		case PegType.RESET:
		case PegType.DULL:
		case PegType.GOLD:
			return true;
		case PegType.VINE:
		case PegType.SPINFECTION:
			if (pegType != PegType.REGULAR)
			{
				return pegType == PegType.DULL;
			}
			return true;
		case PegType.SLIME_ONLY:
			return sType > slimeType;
		case PegType.CRYSTAL:
			if (pegType == PegType.REGULAR)
			{
				return NumCoins() <= 0;
			}
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

	public override bool IsDisabled()
	{
		if (base.gameObject.activeSelf && !(_collider == null))
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
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * 450f);
			}
			else if (pachinko.meteoriteEnabled && relicManager.GetRemainingCountdownForRelic(RelicEffect.BOMB_FORCE_ALWAYS) <= 1 && relicManager.RelicEffectActive(RelicEffect.BOMB_FORCE_ALWAYS))
			{
				Vector2 vector2 = pachinko.transform.position - base.transform.position;
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * 450f);
			}
		}
		else
		{
			if (pachinko.IsDummy)
			{
				return;
			}
			if (cType == CollisionType.COLLIDER)
			{
				if (!_numTimesHitThisShot.ContainsKey(pachinko.gameObject))
				{
					_numTimesHitThisShot[pachinko.gameObject] = 1;
				}
				else if (_numTimesHitThisShot[pachinko.gameObject]++ > 50)
				{
					pachinko.StartDestroy();
					Debug.LogWarning($"{pachinko} destroyed after hitting {this} over {50} times.");
				}
			}
			if (base.pegType == PegType.DULL && pachinko.lastHitPeg == base.gameObject)
			{
				return;
			}
			PegType pegType = base.pegType;
			if (slimeType == SlimeType.FlameOverlay && !_slimedThisHit)
			{
				PlayerHealthController.OnExternalDamageRequested?.Invoke(2f, PlayerHealthController.DamageSource.PEG);
				RemoveSlime();
			}
			DontPopSpecialPegsOnHit component = pachinko.GetComponent<DontPopSpecialPegsOnHit>();
			if (component != null && component.completelyIgnore && (component.dontPopTheseTypes.HasFlag(base.pegType) || (!IsSpecialPeg() && component.ignorePegsWithCoins && NumCoins() > 0)))
			{
				if (++_numBounces >= 9)
				{
					PopPeg();
				}
				return;
			}
			if (cType == CollisionType.COLLIDER && _poppedPegCollider.enabled)
			{
				_revivedPegThisFrame = true;
			}
			if (cType == CollisionType.COLLIDER && SupportsPegType(PegType.BOMB) && relicManager.AttemptUseRelic(RelicEffect.SPAWN_BOMB_ON_PEG_HITS))
			{
				StartCoroutine(WaitAndConvertToBomb());
				return;
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
			if (_lightningSlimeRenderer != null)
			{
				_lightningSlimeRenderer.chain = new List<Peg>();
			}
			SetBlockPopping(pachinko);
			int numberOfMultihits = GetNumberOfMultihits(pachinko, cType, base.pegType, updateNumBounces: true);
			if ((bool)pachinko.GetComponent<PierceBehavior>() && cType == CollisionType.TRIGGER)
			{
				_numBounces = BouncesToPop;
			}
			bool flag = false;
			BreakPegShieldOnHit component2 = pachinko.GetComponent<BreakPegShieldOnHit>();
			if (_shielded && component2 != null)
			{
				flag = true;
				BreakShieldPegImmediately();
			}
			DestroyPegsOnHit component3 = pachinko.GetComponent<DestroyPegsOnHit>();
			if (_shielded && (bool)component3)
			{
				flag = true;
				BreakShieldPegImmediately();
			}
			if (cType == CollisionType.COLLIDER)
			{
				Peg.OnPegCollisionHit(base.pegType, this);
			}
			if (!flag || relicManager.RelicEffectActive(RelicEffect.SHIELD_PEGS_ADD_DAMAGE))
			{
				PegActivated();
			}
			if (!pachinko.IsFiring() && (!(pachinko.GetComponent<EggPachinko>() != null) || pachinko.CurrentState != PachinkoBall.FireballState.AWAITING_RESULTS))
			{
				return;
			}
			HandleMultihits(this, numberOfMultihits, pegType);
			if (cType == CollisionType.COLLIDER)
			{
				if (relicManager.AttemptUseRelic(RelicEffect.LEGACY_METEORITE))
				{
					Vector2 vector3 = pachinko.transform.position - base.transform.position;
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector3.normalized * 450f);
				}
				else if (pachinko.meteoriteEnabled && relicManager.AttemptUseRelic(RelicEffect.BOMB_FORCE_ALWAYS))
				{
					Vector2 vector4 = pachinko.transform.position - base.transform.position;
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector4.normalized * 450f);
				}
			}
			if (!flag && component3 != null && cType == CollisionType.COLLIDER)
			{
				DestroyPeg(pegType);
			}
			else if (convertedToGold)
			{
				DestroyPeg(pegType);
			}
			else if (!(_renderer == null) && cType == CollisionType.COLLIDER)
			{
				CheckForPachinkoBuff(pachinko);
			}
		}
	}

	private void CheckAndApplySlime(PachinkoBall pachinko)
	{
		_slimedThisHit = false;
		if (IsDisabled())
		{
			return;
		}
		PachinkoPegSlimer component = pachinko.GetComponent<PachinkoPegSlimer>();
		if (component != null && SupportsPegType(PegType.SLIME_ONLY, component.slimeType) && component.ShouldSlimeCurrentPeg(this))
		{
			_slimedThisHit = true;
			ApplySlimeToPeg(component.slimeType);
		}
		if (SupportsPegType(PegType.SLIME_ONLY, SlimeType.HealSlime) && relicManager.AttemptUseRelic(RelicEffect.APPLIES_HEALING_SLIME) && !_slimedThisHit)
		{
			_slimedThisHit = true;
			ApplySlimeToPeg(SlimeType.HealSlime);
		}
		if (slimeType < SlimeType.LightningSlime && relicManager.AttemptUseRelic(RelicEffect.RANDOM_SLIME_ON_PEGS_HIT) && !_slimedThisHit)
		{
			SlimeType randomPrimeSlimeType = RelicManager.GetRandomPrimeSlimeType();
			if (SupportsPegType(PegType.SLIME_ONLY, randomPrimeSlimeType))
			{
				_slimedThisHit = true;
				ApplySlimeToPeg(randomPrimeSlimeType);
			}
		}
	}

	private void CheckAndApplyShield(PachinkoBall pachinko)
	{
		CreateShieldPegsOnPegHits component = pachinko.GetComponent<CreateShieldPegsOnPegHits>();
		_shieldedThisHit = component != null && component.ShouldShieldCurrentPeg(this);
		if (_shieldedThisHit)
		{
			ApplyShielding();
		}
	}

	public override float GetMagnetForce()
	{
		if (pegType != PegType.RESET && pegType != PegType.CRIT)
		{
			return 1.1f;
		}
		return 6f;
	}

	public override void ResetHitCount()
	{
		_numBounces = 0;
	}

	protected override void CheckPegSlimeShineEnabled()
	{
		_slimeBehaviour.SetSlimeShine(buffAmount);
	}

	private void ResetNumTimesHitThisShot(Vector2 aimVector)
	{
		_numTimesHitThisShot.Clear();
	}

	public void GenerateId(bool force = false)
	{
		if (id.IsNullOrWhitespace() | force)
		{
			id = Guid.NewGuid().ToString();
		}
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
		if (!status)
		{
			DisableRegularColliders();
		}
		else
		{
			SwitchToRegularColliders();
		}
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

	public void SetPegType(PegType pegType)
	{
		ConvertPegToTypeDummy(pegType);
	}

	protected override void UpdateInvisibilityFromBlind(SpriteRenderer spriteRenderer = null)
	{
		base.UpdateInvisibilityFromBlind(_renderer);
	}
}
