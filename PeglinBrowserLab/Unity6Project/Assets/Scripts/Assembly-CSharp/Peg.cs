using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.Pachinko.Obstacles;
using Battle.PegBehaviour;
using Battle.StatusEffects;
using Cruciball;
using Peglin.Achievements;
using Peglin.ClassSystem;
using Relics;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public abstract class Peg : MonoBehaviour
{
	public enum PegType
	{
		NONE = 0,
		REGULAR = 1,
		CRIT = 2,
		BOMB = 4,
		RESET = 8,
		DELAYED_DEATH = 0x10,
		DESTROYED = 0x20,
		SLIME_ONLY = 0x40,
		DULL = 0x80,
		BOUNCER = 0x100,
		GOLD = 0x200,
		VINE = 0x400,
		SPINFECTION = 0x800,
		MAX_HP = 0x1000,
		CRYSTAL = 0x2000
	}

	public enum SlimeType
	{
		None = 0,
		Slow = 1,
		Web = 3,
		Yolk = 5,
		LightningSlime = 7,
		BouncySlime = 10,
		DoubleDamageSlime = 12,
		PoisonSlime = 13,
		DamageReductionSlime = 14,
		HealSlime = 15,
		FlameOverlay = 99
	}

	public enum CollisionType
	{
		COLLIDER = 0,
		TRIGGER = 1
	}

	public delegate void PegHitEvent(PegType pegType, Peg peg);

	public delegate void PegAdded(GameObject newPeg, IDummyPeg parent = null);

	public delegate void PegDestroyed(PegType pegType, Peg peg);

	public delegate void PegTypeConverted(PegType pegType, Vector3 pos, bool isBombRigged = false);

	public delegate void PegAudioRequest(PegType pegType);

	public delegate void PegSlimeAudioRequest(SlimeType slimeType);

	public delegate void PegMultihit(PegType pegType, Peg peg, int level);

	public static Action OnBonusCoinAdded = () =>
	{
	};

	public static Action OnCoinStolen = () =>
	{
	};

	private const int BUFF_PER_EXTRA_LIGHTNING_SLIME_ZAP = 10;

	private const int DEBUFF_PER_EXTRA_LIGHTNING_SLIME_ZAP = 3;

	private const int MAX_EXTRA_LIGHTNING_SLIME_ZAPS = 5;

	private const float DEFAULT_BOUNCY_SLIME_BOUNCINESS = 1.05f;

	private const float MAX_BOUNCINESS = 1.5f;

	private const int BUFF_PER_BOUNCINESS_INCREASE = 10;

	private const int DEBUFF_PER_BOUNCINESS_INCREASE = 3;

	private const float EXTRA_BOUNCINESS_INCREASE = 0.05f;

	public static readonly int DOUBLE_DAMAGE_SLIME_MULTIPLIER = 2;

	public static readonly float NEGATIVE_DOUBLE_DAMAGE_SLIME_MULTIPLIER = -2f;

	public static int BUFF_PER_DAMAGE_REDUCTION_INCREASE = 10;

	public static int DEBUFF_PER_DAMAGE_REDUCTION_INCREASE = 3;

	public static int BUFF_PER_HEAL_INCREASE = 10;

	public static int DEBUFF_PER_HEAL_INCREASE = 3;

	public static int POISON_SLIME_DAMAGE = 10;

	public const int MAX_OVERLAP_ACTIVATIONS_PER_SHOT = 10;

	public PegType pegType = PegType.REGULAR;

	public SlimeType slimeType;

	public RelicManager relicManager;

	public int defaultDamageMultiplier = 1;

	public GameObject pegTextPrefab;

	protected bool _cleared;

	protected TMP_Text _pegText;

	public int buffAmount;

	[FormerlySerializedAs("_shieldPegOverlayPrefab")]
	[SerializeField]
	protected PegShieldOverlay pegShieldOverlayPrefab;

	protected PegShieldOverlay PegShieldOverlayInstance;

	protected bool _shielded;

	public int numCoinOverride;

	protected bool _slimedThisHit;

	private bool _lightningSlimeChainGoing;

	[SerializeField]
	protected PegCoinOverlay pegCoinOverlayPrefab;

	protected PegCoinOverlay PegCoinOverlayInstance;

	[SerializeField]
	protected Collider2D _collider;

	[SerializeField]
	protected Collider2D _trigger;

	[SerializeField]
	protected Collider2D _poppedPegTrigger;

	[SerializeField]
	protected Collider2D _poppedPegCollider;

	[SerializeField]
	protected Collider2D _specialPegCollider;

	protected Vector3 _position = Vector3.negativeInfinity;

	[HideInInspector]
	public int BouncesToPop = 3;

	protected int _numBounces;

	protected bool _blockPoppingFromThisHit;

	protected bool _blockCoinCollectionFromThisHit;

	protected bool _isInvisibleFromBlind;

	[HideInInspector]
	public bool isBonusSpecialPeg;

	[HideInInspector]
	public bool justDuplicated;

	[HideInInspector]
	public bool convertedToGold;

	[HideInInspector]
	public PegBoardBramballVine bramballVine;

	protected bool _hasSplashedThisShot;

	public int overlapActivationsThisShot;

	[SerializeField]
	private GameObject _lightningSlimeLineRendererPrefab;

	protected LightningSlimeRenderer _lightningSlimeRenderer;

	public GameObject AddCoinParticles;

	public static PegHitEvent OnPegHit = delegate
	{
	};

	public static PegHitEvent OnPegActivated = delegate
	{
	};

	public static PegHitEvent OnPegCollisionHit = delegate
	{
	};

	public static PegAdded OnPegAdded = delegate
	{
	};

	public static PegDestroyed OnPegDestroyed = delegate
	{
	};

	public static PegTypeConverted OnPegTypeConverted = (PegType type, Vector3 pos, bool isBombRigged) =>
	{
	};

	public static PegAudioRequest OnPegAudioRequest = (PegType type) =>
	{
	};

	public static PegSlimeAudioRequest OnSlimedPegAudioRequest = (SlimeType slimeType) =>
	{
	};

	public static PegMultihit OnPegMultihit = (PegType pegType, Peg peg, int level) =>
	{
	};

	public int damageMultiplier
	{
		get
		{
			int num = defaultDamageMultiplier;
			if (slimeType == SlimeType.DoubleDamageSlime)
			{
				num *= DOUBLE_DAMAGE_SLIME_MULTIPLIER;
			}
			return num;
		}
	}

	public float buffDamageMultiplier
	{
		get
		{
			float num = 1f;
			if (slimeType == SlimeType.DoubleDamageSlime)
			{
				num *= (float)DOUBLE_DAMAGE_SLIME_MULTIPLIER;
				if (buffAmount < 0)
				{
					num *= NEGATIVE_DOUBLE_DAMAGE_SLIME_MULTIPLIER;
				}
			}
			return num;
		}
	}

	public bool Cleared => _cleared;

	public TMP_Text pegText => _pegText;

	public bool shielded => _shielded;

	public bool lightningSlimeChainGoing => _lightningSlimeChainGoing;

	public bool isInvisibleFromBlind => _isInvisibleFromBlind;

	public static bool PegTypeIndestructible(PegType pegType)
	{
		if (pegType != PegType.DULL && pegType != PegType.BOUNCER)
		{
			return pegType == PegType.DESTROYED;
		}
		return true;
	}

	protected bool PegTypeIngnoresInvisibilityFromBlind(PegType pegType)
	{
		if (pegType != PegType.CRIT && pegType != PegType.RESET && pegType != PegType.VINE && pegType != PegType.MAX_HP && pegType != PegType.SPINFECTION && pegType != PegType.BOMB)
		{
			return pegType == PegType.CRYSTAL;
		}
		return true;
	}

	public static bool IsSlimeTypePlayerApplied(SlimeType type)
	{
		switch (type)
		{
		case SlimeType.Yolk:
		case SlimeType.LightningSlime:
		case SlimeType.BouncySlime:
		case SlimeType.DoubleDamageSlime:
		case SlimeType.PoisonSlime:
		case SlimeType.DamageReductionSlime:
		case SlimeType.HealSlime:
			return true;
		default:
			return false;
		}
	}

	public static bool IsSlimeTypeBuffable(SlimeType type)
	{
		switch (type)
		{
		case SlimeType.LightningSlime:
		case SlimeType.BouncySlime:
		case SlimeType.DoubleDamageSlime:
		case SlimeType.PoisonSlime:
		case SlimeType.DamageReductionSlime:
		case SlimeType.HealSlime:
			return true;
		default:
			return false;
		}
	}

	protected virtual void Awake()
	{
		PegShieldOverlayInstance = GetComponentInChildren<PegShieldOverlay>();
		if (PegShieldOverlayInstance != null)
		{
			ApplyShielding(claimed: false, startupShield: true);
		}
	}

	protected virtual void Start()
	{
		if (numCoinOverride > 0)
		{
			AddCoinsForceBouncer(numCoinOverride);
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		ForceDisableLightningSlimeChain();
	}

	public abstract void PegActivated(bool playAudio = true, bool forcePop = false);

	public abstract bool IsDelayedDeath();

	public abstract GameObject ConvertPegToType(PegType type);

	public virtual bool ShouldPopPegOnHit()
	{
		return false;
	}

	public virtual void ApplySlimeToPeg(SlimeType sType)
	{
		if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.isRunActive && IsSlimeTypePlayerApplied(sType))
		{
			if (StaticGameData.CurrentRunStats.slimePegsPerSlimeType.ContainsKey(sType))
			{
				StaticGameData.CurrentRunStats.slimePegsPerSlimeType[sType]++;
			}
			else
			{
				StaticGameData.CurrentRunStats.slimePegsPerSlimeType.Add(sType, 1);
			}
		}
		if (StaticGameData.CurrentRunStats != null && AchievementManager.AchievementsOn && IsSlimeTypePlayerApplied(sType) && SceneManager.GetActiveScene().name != "MainMenu")
		{
			StaticGameData.CurrentRunStats.spinventorProgress++;
			if (!CruciballManager.IsClassUnlocked(Class.Spinventor) && PersistentPlayerData.Instance.SpinventorProgress + StaticGameData.CurrentRunStats.spinventorProgress >= PersistentPlayerData.SPINVENTOR_PROGRESS_MAX && !PersistentPlayerData.Instance.QueuedClassUnlocks.Contains(Class.Spinventor))
			{
				PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Spinventor);
			}
		}
	}

	public virtual void RemoveSlime(bool forceRemoval = false)
	{
	}

	public bool SlimeTypeIsPersistent(SlimeType sType)
	{
		if (sType != SlimeType.BouncySlime && sType != SlimeType.HealSlime && sType != SlimeType.DoubleDamageSlime && sType != SlimeType.LightningSlime && sType != SlimeType.PoisonSlime)
		{
			return slimeType == SlimeType.DamageReductionSlime;
		}
		return true;
	}

	public bool ShouldIgnoreSlimeOnPegPop(SlimeType sType)
	{
		if (sType != SlimeType.HealSlime && sType != SlimeType.LightningSlime)
		{
			return sType == SlimeType.DoubleDamageSlime;
		}
		return true;
	}

	public bool FragileFromSlime()
	{
		return slimeType == SlimeType.Web;
	}

	public bool IndestructibleFromSlime()
	{
		if (slimeType != SlimeType.BouncySlime && slimeType != SlimeType.HealSlime && slimeType != SlimeType.LightningSlime && slimeType != SlimeType.PoisonSlime && slimeType != SlimeType.DoubleDamageSlime)
		{
			return slimeType == SlimeType.DamageReductionSlime;
		}
		return true;
	}

	public abstract void ResetHitCount();

	public abstract void Reset(bool criticalHitActive = false);

	public abstract bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None);

	public abstract void RemoveIfCleared();

	public abstract bool IsDisabled();

	public virtual bool IsPopped()
	{
		return IsDisabled();
	}

	public abstract void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER);

	protected int GetNumberOfMultihits(PachinkoBall pachinko, CollisionType cType, PegType pegType, bool updateNumBounces = false)
	{
		Multihit component = pachinko.gameObject.GetComponent<Multihit>();
		MultihitOnPegType component2 = pachinko.gameObject.GetComponent<MultihitOnPegType>();
		MorbidPachinkoBehaviour component3 = pachinko.gameObject.GetComponent<MorbidPachinkoBehaviour>();
		bool flag = component != null && component.multihitLevel > 0;
		bool flag2 = component2 != null && component2.multihitPegType == pegType;
		bool flag3 = component3 != null && component3.morbidLevel > 1 && cType == CollisionType.TRIGGER;
		int num = 0;
		if (flag | flag2 | flag3)
		{
			if (flag)
			{
				num += component.multihitLevel;
			}
			if (flag2)
			{
				num += component2.multihitLevel;
			}
			if (flag3)
			{
				num += component3.morbidLevel - 1;
			}
			if (updateNumBounces)
			{
				_numBounces += num;
			}
			if (cType != CollisionType.TRIGGER)
			{
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					if (TestShieldPegsOnActivated() && !relicManager.AttemptUseRelic(RelicEffect.SHIELD_PEGS_ADD_DAMAGE))
					{
						num2++;
					}
				}
				num -= num2;
			}
		}
		return num + ((pachinko.IsOrbSmallerThanDefault() && relicManager != null && relicManager.AttemptUseRelic(RelicEffect.SMALL_ORBS_MULTIHIT)) ? 1 : 0);
	}

	protected void HandleMultihits(Peg peg, int multihitLevel, PegType multihitPegType)
	{
		if (multihitLevel > 0)
		{
			OnPegMultihit(multihitPegType, peg, multihitLevel);
		}
	}

	public void AddBounce(int bounceCount)
	{
		_numBounces += bounceCount;
	}

	public abstract float GetMagnetForce();

	public virtual bool ShouldEndPierce()
	{
		return true;
	}

	public Vector3 GetCenterOfPeg()
	{
		if (_collider != null && _collider.isActiveAndEnabled)
		{
			_position = _collider.bounds.center;
		}
		else if (_trigger != null && _trigger.isActiveAndEnabled)
		{
			_position = _trigger.bounds.center;
		}
		else if (_poppedPegCollider != null && _poppedPegCollider.isActiveAndEnabled)
		{
			_position = _poppedPegCollider.bounds.center;
		}
		else if (_specialPegCollider != null && _specialPegCollider.isActiveAndEnabled)
		{
			_position = _specialPegCollider.bounds.center;
		}
		else if (_position.x < -500000f)
		{
			_position = base.transform.position;
		}
		return _position;
	}

	public virtual void InitPegText()
	{
		if (_pegText == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(pegTextPrefab, base.transform, worldPositionStays: false);
			_pegText = gameObject.GetComponentInChildren<TMP_Text>();
			_pegText.enabled = false;
		}
	}

	public void AddBuff(int amount)
	{
		UpdateBuff(amount);
	}

	protected void CheckForPachinkoBuff(PachinkoBall pachinko = null)
	{
		int num = 0;
		if (pachinko != null && BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION && BattleController.BattleActive)
		{
			PachinkoBuff component = pachinko.gameObject.GetComponent<PachinkoBuff>();
			if (component != null)
			{
				num += component.buffAmount;
			}
			if (relicManager.RelicEffectActive(RelicEffect.BUFF_FIRST_PEG_HIT) && pachinko.CheckForApplyingFirstPegHitBuff() && relicManager.AttemptUseRelic(RelicEffect.BUFF_FIRST_PEG_HIT))
			{
				num += RelicManager.FIRST_PEG_HIT_BUFF_AMOUNT;
			}
			BuffFirstPegHit component2 = pachinko.gameObject.GetComponent<BuffFirstPegHit>();
			if (component2 != null && !component2.hasBuffed)
			{
				num += component2.buffAmmount;
				component2.hasBuffed = true;
			}
		}
		UpdateBuff(num);
	}

	public void CheckForRelicBuff()
	{
		int num = 0;
		if (relicManager.AttemptUseRelic(RelicEffect.ALL_ORBS_BUFF))
		{
			num += RelicManager.ALL_ORBS_BUFF_AMOUNT;
		}
		if (relicManager.AttemptUseRelic(RelicEffect.ALL_ORBS_DEBUFF))
		{
			num += RelicManager.ALL_ORBS_DEBUFF_AMOUNT;
		}
		UpdateBuff(num);
	}

	protected PhysicsMaterial2D GetMaterialForSlimeAndBuffs()
	{
		if (slimeType == SlimeType.BouncySlime)
		{
			PhysicsMaterial2D physicsMaterial2D = new PhysicsMaterial2D();
			physicsMaterial2D.friction = 0f;
			physicsMaterial2D.bounciness = 1.05f;
			if (buffAmount >= 0)
			{
				physicsMaterial2D.bounciness += (float)(buffAmount / 10) * 0.05f;
			}
			else
			{
				physicsMaterial2D.bounciness += (float)(-buffAmount / 3) * 0.05f;
			}
			physicsMaterial2D.bounciness = Mathf.Clamp(physicsMaterial2D.bounciness, 1.05f, 1.5f);
			return physicsMaterial2D;
		}
		return null;
	}

	protected IEnumerator RefreshSplash()
	{
		yield return null;
		if (!relicManager.AttemptUseRelic(RelicEffect.REFRESH_PEGS_SPLASH) || _hasSplashedThisShot)
		{
			yield break;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(GetCenterOfPeg(), 1.5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<Peg>(out var component))
			{
				PegGridObscurer componentInParent = component.GetComponentInParent<PegGridObscurer>();
				if (component != this && !component.IsDisabled() && (componentInParent == null || componentInParent.isRevealed))
				{
					component.PegActivated(playAudio: false);
				}
			}
		}
		_hasSplashedThisShot = true;
	}

	private void UpdateBuff(int buffMod)
	{
		InitPegText();
		buffAmount += buffMod;
		buffAmount = Mathf.Clamp(buffAmount, -999, 999);
		if (_pegText != null)
		{
			_pegText.text = buffAmount.ToString();
			CheckPegTextEnabled();
			if (slimeType != SlimeType.None || this is SlimeOnlyPeg)
			{
				CheckPegSlimeShineEnabled();
			}
		}
		PhysicsMaterial2D materialForSlimeAndBuffs = GetMaterialForSlimeAndBuffs();
		if (materialForSlimeAndBuffs != null && _collider != null)
		{
			_collider.sharedMaterial = materialForSlimeAndBuffs;
		}
	}

	protected virtual void CheckPegSlimeShineEnabled()
	{
	}

	protected virtual void CheckPegTextEnabled()
	{
		TMP_Text tMP_Text = _pegText;
		int num;
		if (buffAmount != 0)
		{
			PegType pegType = this.pegType;
			if (pegType != PegType.DESTROYED && pegType != PegType.CRIT && pegType != PegType.RESET && pegType != PegType.BOMB)
			{
				num = ((NumCoins() <= 0) ? 1 : 0);
				goto IL_0035;
			}
		}
		num = 0;
		goto IL_0035;
		IL_0035:
		tMP_Text.enabled = (byte)num != 0;
	}

	public void StartDelayedBombConversion(bool makeRigged = false)
	{
		StartCoroutine(WaitAndConvertToBomb(makeRigged));
	}

	protected IEnumerator WaitAndConvertToBomb(bool makeRigged = false)
	{
		yield return null;
		GameObject gameObject = ConvertPegToType(PegType.BOMB);
		if (makeRigged)
		{
			gameObject.GetComponent<Bomb>()?.ConvertToRigged();
		}
		PegManager.ExternalBombCreatedNotification?.Invoke();
	}

	public virtual bool IsWaitingForDeath()
	{
		return false;
	}

	public void DestroyPeg(PegType pegTypeBeforeDestruction)
	{
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.EndBattleFadeOut();
		}
		pegType = PegType.DESTROYED;
		_collider.enabled = false;
		_poppedPegCollider.enabled = false;
		_poppedPegTrigger.enabled = false;
		_specialPegCollider.enabled = false;
		HidePeg();
		_lightningSlimeChainGoing = false;
		OnPegDestroyed?.Invoke(pegTypeBeforeDestruction, this);
	}

	protected virtual void HidePeg()
	{
		ResetShieldOverlay();
	}

	public bool IsSpecialPeg()
	{
		if (pegType != PegType.RESET)
		{
			return pegType == PegType.CRIT;
		}
		return true;
	}

	public virtual void SetIsBonusSpecialPeg(bool newIsBonusSpecialPeg)
	{
		isBonusSpecialPeg = newIsBonusSpecialPeg;
	}

	protected virtual void SetBlockPopping(PachinkoBall pachinko)
	{
		DontPopSpecialPegsOnHit component = pachinko.GetComponent<DontPopSpecialPegsOnHit>();
		if (component != null)
		{
			if (component.dontPopTheseTypes.HasFlag(pegType) || (!IsSpecialPeg() && (component.dontPopPegsWithCoins || component.ignorePegsWithCoins) && NumCoins() > 0))
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

	public void UpdateOverlayAlpha(float alpha)
	{
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.UpdateAlpha(alpha);
		}
		if ((bool)PegShieldOverlayInstance)
		{
			PegShieldOverlayInstance.UpdateAlpha(alpha);
		}
	}

	public void SetInvisibleFromBlind(bool value)
	{
		if (value != _isInvisibleFromBlind)
		{
			_isInvisibleFromBlind = value;
			UpdateInvisibilityFromBlind();
		}
	}

	protected virtual void UpdateInvisibilityFromBlind(SpriteRenderer spriteRenderer = null)
	{
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (!_isInvisibleFromBlind || PegTypeIngnoresInvisibilityFromBlind(pegType))
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.enabled = true;
			}
		}
		else if (_isInvisibleFromBlind && !PegTypeIngnoresInvisibilityFromBlind(pegType) && spriteRenderer != null)
		{
			spriteRenderer.enabled = false;
		}
	}

	public virtual bool CanAddCoin()
	{
		if (pegType != PegType.BOMB && pegType != PegType.BOUNCER && pegType != PegType.CRYSTAL && pegCoinOverlayPrefab != null)
		{
			if (!(PegCoinOverlayInstance == null))
			{
				return PegCoinOverlayInstance.NumCoins < 9;
			}
			return true;
		}
		return false;
	}

	public virtual bool AddCoin(bool doParticleEffects = true)
	{
		if (InitCoinPrefab() && CanAddCoin())
		{
			PegCoinOverlayInstance.AddCoins();
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(pegType == PegType.CRIT || pegType == PegType.RESET || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP || pegType == PegType.CRYSTAL);
			if (doParticleEffects)
			{
				UnityEngine.Object.Instantiate(AddCoinParticles, GetCenterOfPeg(), Quaternion.identity);
				OnBonusCoinAdded?.Invoke();
			}
			return true;
		}
		return false;
	}

	public virtual bool RemoveCoinsWithoutCollecting(int num = -1)
	{
		if (InitCoinPrefab() && NumCoins() > 0)
		{
			PegCoinOverlayInstance.RemoveCoinsWithoutCollecting(num);
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(pegType == PegType.CRIT || pegType == PegType.RESET || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP || pegType == PegType.CRYSTAL);
			OnCoinStolen?.Invoke();
			return true;
		}
		return false;
	}

	private void AddCoinsForceBouncer(int numCoins)
	{
		if (InitCoinPrefab())
		{
			PegCoinOverlayInstance.AddCoins(numCoins);
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(pegType == PegType.CRIT || pegType == PegType.RESET || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP || pegType == PegType.CRYSTAL);
		}
	}

	public virtual bool CollectExtraCoins(int amount = 1)
	{
		if (pegType == PegType.BOMB || pegType == PegType.BOUNCER)
		{
			return false;
		}
		if (InitCoinPrefab())
		{
			PegCoinOverlayInstance.AddCoins(amount);
			PegCoinOverlayInstance.CollectCoins(amount);
			PegCoinOverlayInstance.ToggleGuardingSpecialPeg(pegType == PegType.CRIT || pegType == PegType.RESET || pegType == PegType.VINE || pegType == PegType.SPINFECTION || pegType == PegType.MAX_HP || pegType == PegType.CRYSTAL);
			return true;
		}
		return false;
	}

	public int NumCoins()
	{
		if (PegCoinOverlayInstance != null)
		{
			return PegCoinOverlayInstance.NumCoins;
		}
		return 0;
	}

	public virtual bool InitCoinPrefab()
	{
		if (pegCoinOverlayPrefab == null)
		{
			return false;
		}
		if (PegCoinOverlayInstance == null)
		{
			PegCoinOverlayInstance = UnityEngine.Object.Instantiate(pegCoinOverlayPrefab, base.transform);
		}
		else
		{
			PegCoinOverlayInstance.gameObject.SetActive(value: true);
		}
		return true;
	}

	protected void CollectCoin()
	{
		if ((bool)PegCoinOverlayInstance && !_blockCoinCollectionFromThisHit)
		{
			PegCoinOverlayInstance.CollectCoin();
		}
	}

	protected void CollectCoins()
	{
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.CollectCoins();
		}
	}

	protected virtual void ResetShieldOverlay()
	{
		if (PegShieldOverlayInstance != null)
		{
			PegShieldOverlayInstance.hitCount = 0;
			PegShieldOverlayInstance.gameObject.SetActive(value: false);
			_shielded = false;
		}
	}

	public bool SupportsShielding()
	{
		if (pegType != PegType.REGULAR || IsDisabled())
		{
			return false;
		}
		if (_shielded)
		{
			return false;
		}
		return true;
	}

	public void ApplyShielding(bool claimed = false, bool startupShield = false)
	{
		if (!_shielded)
		{
			_shielded = true;
			InitShieldPrefab();
			PegShieldOverlayInstance.hitCount = 0;
			PegShieldOverlayInstance.isClaimed = claimed;
			if (!startupShield && relicManager.AttemptUseRelic(RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_CREATED))
			{
				StatusEffect[] statusEffects = new StatusEffect[1]
				{
					new StatusEffect(StatusEffectType.Ballwark, 4)
				};
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(statusEffects, StatusEffectSource.PLAYER);
			}
		}
	}

	protected virtual void InitShieldPrefab()
	{
		if (PegShieldOverlayInstance == null)
		{
			PegShieldOverlayInstance = UnityEngine.Object.Instantiate(pegShieldOverlayPrefab, base.transform);
		}
		else
		{
			PegShieldOverlayInstance.gameObject.SetActive(value: true);
		}
	}

	protected bool TestShieldPegsOnActivated()
	{
		if (_shielded)
		{
			_shielded = PegShieldOverlayInstance.TestShieldedOnActivated();
		}
		return _shielded;
	}

	public void BreakShieldPegImmediately()
	{
		_shielded = false;
		if (PegShieldOverlayInstance != null)
		{
			PegShieldOverlayInstance.BreakShieldPegImmediately();
		}
	}

	private void CreateLightningSlimeRenderer()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_lightningSlimeLineRendererPrefab, Vector3.zero, Quaternion.identity);
		gameObject.transform.parent = base.transform;
		_lightningSlimeRenderer = gameObject.GetComponent<LightningSlimeRenderer>();
	}

	protected int GetNumLightningSlimeZaps()
	{
		int num = 1;
		if (buffAmount > 0)
		{
			num += Mathf.Min(buffAmount / 10, 5);
		}
		else if (buffAmount < 0)
		{
			num += Mathf.Min(Mathf.Abs(buffAmount) / 3, 5);
		}
		return num;
	}

	protected IEnumerator ActivateLightningSlime()
	{
		_lightningSlimeChainGoing = true;
		if (_lightningSlimeRenderer == null)
		{
			CreateLightningSlimeRenderer();
		}
		int numZaps = GetNumLightningSlimeZaps();
		float zapDistance = 2f;
		float zapTime = 0.1f;
		Peg peg = this;
		if (peg != null)
		{
			if (peg is LongPeg && (peg as LongPeg).hit)
			{
				yield break;
			}
			if (!_lightningSlimeRenderer.chain.Contains(this))
			{
				_lightningSlimeRenderer.chain.Add(this);
			}
			Peg previouslyHitPeg = peg;
			List<Vector3> lightningPositions = new List<Vector3> { GetCenterOfPeg() };
			for (int i = 0; i < numZaps; i++)
			{
				Collider2D[] array = Physics2D.OverlapCircleAll(previouslyHitPeg.GetCenterOfPeg(), zapDistance);
				float num = float.PositiveInfinity;
				float num2 = float.PositiveInfinity;
				Peg peg2 = null;
				Collider2D[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					if (!array2[j].TryGetComponent<Peg>(out var component) || component == this || _lightningSlimeRenderer.chain.Contains(component) || (component is LongPeg && (component as LongPeg).hit))
					{
						continue;
					}
					PegGridObscurer componentInParent = component.GetComponentInParent<PegGridObscurer>();
					if ((!(componentInParent != null) || componentInParent.isRevealed) && !component.IsDisabled() && (!component.IsDelayedDeath() || !component.IsWaitingForDeath()))
					{
						float num3 = Vector2.Distance(component.GetCenterOfPeg(), base.transform.position);
						if (component.slimeType == SlimeType.LightningSlime && num3 < num2)
						{
							peg2 = component;
							num2 = num3;
						}
						else if (num2 == float.PositiveInfinity && num3 < num)
						{
							peg2 = component;
							num = num3;
						}
					}
				}
				if (!(peg2 != null))
				{
					break;
				}
				_lightningSlimeRenderer.chain.Add(peg2);
				StartCoroutine(LightningSlimeActivatePegAfterTime(peg2, zapTime, _lightningSlimeRenderer.chain));
				lightningPositions.Add(peg2.GetCenterOfPeg());
				_lightningSlimeRenderer.lineRenderer.enabled = true;
				_lightningSlimeRenderer.lineRenderer.positionCount = lightningPositions.Count;
				_lightningSlimeRenderer.lineRenderer.SetPositions(lightningPositions.ToArray());
				previouslyHitPeg = peg2;
				yield return new WaitForSeconds(zapTime);
			}
		}
		StartCoroutine(DisableLightningSlimeLineRenderer(zapTime));
	}

	private IEnumerator LightningSlimeActivatePegAfterTime(Peg hitPeg, float time, List<Peg> chainRefrence)
	{
		yield return new WaitForSeconds(time);
		if (hitPeg.slimeType == SlimeType.LightningSlime)
		{
			if (hitPeg._lightningSlimeRenderer == null)
			{
				hitPeg.CreateLightningSlimeRenderer();
			}
			hitPeg._lightningSlimeRenderer.chain = chainRefrence;
		}
		hitPeg.SetupPegToBeActivatedByLightningSlime();
		hitPeg.PegActivated();
	}

	public void SetupPegToBeActivatedByLightningSlime()
	{
		_slimedThisHit = false;
	}

	private IEnumerator DisableLightningSlimeLineRenderer(float time)
	{
		yield return new WaitForSeconds(time);
		_lightningSlimeRenderer.lineRenderer.enabled = false;
		_lightningSlimeRenderer.lineRenderer.positionCount = 0;
		yield return new WaitForSeconds(0.05f);
		_lightningSlimeChainGoing = false;
	}

	private void ForceDisableLightningSlimeChain()
	{
		_lightningSlimeChainGoing = false;
		if (_lightningSlimeRenderer != null)
		{
			if (_lightningSlimeRenderer.lineRenderer != null)
			{
				_lightningSlimeRenderer.lineRenderer.enabled = false;
				_lightningSlimeRenderer.lineRenderer.positionCount = 0;
			}
			_lightningSlimeRenderer.chain.Clear();
		}
	}

	public static string GetSlimeLocKey(SlimeType type)
	{
		return type switch
		{
			SlimeType.Yolk => "Statuses/yolk_slime_name", 
			SlimeType.BouncySlime => "Statuses/bouncy_slime_name", 
			SlimeType.HealSlime => "Statuses/heal_slime_name", 
			SlimeType.LightningSlime => "Statuses/lightning_slime_name", 
			SlimeType.PoisonSlime => "Statuses/poison_slime_name", 
			SlimeType.DamageReductionSlime => "Statuses/damage_reduction_slime_name", 
			SlimeType.DoubleDamageSlime => "Statuses/double_damage_slime", 
			_ => "", 
		};
	}
}
