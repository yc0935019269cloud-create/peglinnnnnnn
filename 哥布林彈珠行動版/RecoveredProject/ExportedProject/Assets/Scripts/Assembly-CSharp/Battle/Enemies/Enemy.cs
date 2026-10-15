using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Attacks;
using Battle.Enemies.Attacks;
using Battle.Enemies.Behaviours;
using Battle.Enemies.C20Upgrades;
using Battle.Enemies.Polish;
using Battle.Enemies.StatusEffects;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using Challenges;
using Cruciball;
using Currency;
using DG.Tweening;
using Loading;
using Map;
using MoreMountains.Tools;
using NaughtyAttributes;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUtils;
using PeglinUtils.Locators;
using Relics;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battle.Enemies
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AudioSource))]
	public class Enemy : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[Flags]
		public enum EnemyType
		{
			None = 0,
			Slime = 1,
			Plant = 2,
			Humanoid = 4,
			Flying = 8,
			Material = 0x10,
			Beast = 0x20,
			Miniboss = 0x40,
			Boss = 0x80,
			Eldritch = 0x100,
			Minion = 0x200
		}

		public enum EnemyDamageSource
		{
			Unspecified = 0,
			ProjectileAttack = 1,
			TargetedAttack = 2,
			AOE = 3,
			RelicChip = 4,
			StatusEffect = 5,
			OtherEnemies = 6,
			Bomb = 7,
			RelicLarge = 8
		}

		public delegate void EnemyDestroyed(Enemy enemy);

		public delegate void EnemyAttacked(float damage, bool melee, Enemy enemy, PlayerHealthController.DamageSource source = PlayerHealthController.DamageSource.ENEMY_ATTACK, bool forceMaxHPDamage = false);

		public delegate void EnemyDamaged(Enemy enemy, long damage = 0L, EnemyDamageSource damageSource = EnemyDamageSource.Unspecified);

		public delegate void EnemySpawned(Enemy enemy);

		public delegate void EnemyClicked(Enemy enemy);

		public delegate void HealEnemies(float healAmount);

		public delegate void BuffAllEnemies(int buffAmount);

		public delegate void BuffAllEnemiesMultiple(StatusEffect[] effects, StatusEffectSource source = StatusEffectSource.ENEMY);

		public delegate void DamageAllEnemies(long damageAmount, EnemyDamageSource source = EnemyDamageSource.Unspecified);

		public delegate void MouseHoverEnemy();

		public delegate void EnemyStatusEffectAction(StatusEffect statusEffect);

		public delegate void EnemyDataEvent(string locKey);

		protected delegate void MeleeAttackFinished();

		public delegate void EnemyMissEvent(float damage);

		public enum AnimationTypes
		{
			DAMAGED = 0,
			STANCE_CHANGE = 1,
			MOVEMENT = 2,
			MELEE_ATTACK = 3,
			LOOT_DROP = 4
		}

		private const float DEATH_ANIM_TIMEOUT = 3f;

		public static EnemyDestroyed OnEnemyDestroyed;

		public static EnemyAttacked OnEnemyAttack;

		public static EnemyDamaged OnEnemyDamaged;

		public static EnemySpawned OnEnemySpawned;

		public static EnemyClicked OnEnemyClicked;

		public static HealEnemies OnEnemiesHealed;

		public static BuffAllEnemies OnAllEnemiesBuffed;

		public static BuffAllEnemiesMultiple OnAllEnemiesMultiBuff;

		public static DamageAllEnemies OnAllEnemiesDamaged;

		public static MouseHoverEnemy OnMouseHoverEnterEnemy;

		public static MouseHoverEnemy OnMouseHoverExitEnemy;

		public static EnemyStatusEffectAction OnNegativeStatusEffectAdded;

		public static EnemyDataEvent OnEnemyKilled;

		public static EnemyDataEvent OnEnemyCardDropped;

		public static EnemyDataEvent OnEnemyLootDropped;

		protected static MeleeAttackFinished OnMeleeAttackFinished;

		public EnemyMissEvent OnEnemyMissed = (float damage) =>
		{
		};

		[Header("References")]
		[SerializeField]
		protected CruciballManager _cruciballManager;

		public GameObject HealthBarObj;

		public int BossHealthBarIndex;

		[HideInInspector]
		public UpdateSlider HealthBarBarSprite;

		public GameObject Projectile;

		public Vector3 ProjectileStartOffset;

		[Tooltip("Used as default option for upcoming enemy previews when no override is set.")]
		public Sprite DefaultInfoSprite;

		public Sprite UpcommingEnemySpriteOverride;

		[Header("Enemy Properties")]
		public string locKey;

		[EnumFlags]
		public EnemyType enemyTypes;

		[HideInInspector]
		public float CurrentHealth = 100f;

		protected float _maxHealth = 100f;

		public float StartingHealth = 100f;

		public float StartingHealthFirstCruciball = 150f;

		public float DamagePerMeleeAttack = 5f;

		public float DamagePerRangedAttack = 2f;

		public int AttackRange = 1;

		public bool UnblockableAttacks;

		public bool destroyAfterAttack;

		[Tooltip("How many tiles should this enemy try to move?")]
		public int defaultMovement = 1;

		[Tooltip("How far can this enemy move if it needs to squeeze past a non-moving enemy?")]
		public int MaxMovement = 2;

		[ReadOnly]
		public bool CurrentlyActing;

		public bool IsFlying;

		[Tooltip("Related to teleport enemies.")]
		public bool FreeMovement;

		[Tooltip("Takes X turns to attack.")]
		public int AttackChargeLength;

		public int currentChargeTime;

		[SerializeField]
		protected StatusEffect[] _meleeAppliedStatusEffects;

		protected StatusEffect _applyStatWhenProjectileAttacked;

		protected StatusEffect _applyStatWhenTargetedAttacked;

		[SerializeField]
		protected StatusEffectImmunity _statusEffectImmunities;

		public bool canAttack = true;

		[Tooltip("Can this unit move to try and melee attack or should it just stand and shoot?")]
		public bool IsOnlyRanged;

		[Tooltip("Should this unit move up to melee range or is it preferred or equal to stay ranged?")]
		public bool RangeOverMelee;

		[Tooltip("Override for enemies that can move up to Peglin but should use their ranged attack as a melee attack.")]
		public bool useRangedAttackForMelee;

		[Tooltip("Allow this unit to take different amounts of damage from projectile attacks")]
		public float projectileAttackDamageMod = 1f;

		[Tooltip("Allow this unit to take different amounts of damage from targeted/spell attacks")]
		public float spellAttackDamageMod = 1f;

		[Tooltip("Allow this unit to take different amounts of damage from Bomb AoE damage")]
		public float bombDamageMod = 1f;

		[Tooltip("Reduces the damage of pircing orbs that go through this enemy.")]
		public float pierceDamageReductionMod = 1f;

		[Tooltip("Enemies with this property must be targeted before anything else.")]
		public bool targetLock;

		public bool targetByDefault;

		[Tooltip("Can this enemy be manually targeted? Note that this does still allow auto-targeting as of 1.0.8")]
		public bool untargetable;

		public bool ignoreForVictoryCheck;

		public bool ignoreAutoTarget;

		public bool ignoreKillDataAndLootDrops;

		public bool damagesMaxHp;

		public bool supressHitAnimations;

		private float _prevRangedAttackDamage;

		private float _prevMeleeAttackDamage;

		[Header("ScreenShake")]
		public float screenShakeDurationMin = 0.7f;

		public float screenShakeDurationMax = 1.1f;

		public float screenShakeIntensityBase = 1f;

		public float screenShakeIntensityMax = 2.5f;

		[Tooltip("What damage base should be used as a reference to determine the screenshake intensity(300 damage is a ton against a slime but little against the demon wall, for example)")]
		public float screenShakeIntensityReference = 350f;

		[Tooltip("Some large enemies have large hit anims, and we can ignore small bits of damage so it's not flickering constantly.")]
		public float hitAnimationDamageThreshold = -1f;

		protected Animator _anim;

		private SpriteRenderer _renderer;

		[Header("UI Elements")]
		[SerializeField]
		private TextMeshProUGUI _healthText;

		[SerializeField]
		protected OneShotAnimation _vineAnimation;

		private HealAnimLocator _healAnim;

		[SerializeField]
		private Color _healTextColor = Color.green;

		private Canvas _enemyUICanvas;

		protected TargetingUI _targetingUI;

		private Vector3 _targetingUIInitialPosition;

		[Tooltip("For boss enemies, allow us to override the skull icon on the healthbar.")]
		[SerializeField]
		private Sprite _bossHealthBarOverrideIcon;

		[SerializeField]
		private Color _bossHealthBarOverrideColor = Color.white;

		[Header("Audio")]
		[SerializeField]
		protected EnemySfx _enemySfx;

		[Header("Attack Data")]
		[SerializeField]
		protected EnemyAttack _enemyAttack;

		private const float BrambleDamage = 10f;

		protected const string DAMAGED_ANIMATION_STATE = "damaged";

		protected const string REFLECT_ANIMATION_STATE = "reflect";

		protected const string DEATH_ANIMATION_STATE = "death";

		protected const string CHARGE_ANIMATION_STATE = "charge";

		protected const string CHARGE_COUNT_ANIMATION_STATE = "charge_count";

		protected const string CHARGE_TRIGGER_ANIMATION_STATE = "charge_trigger";

		protected const string LIFELINK_LASER_ASSET_KEY = "LifelinkLaser";

		public const float LIFELINK_HEAL_MULTIPLIER = 3f;

		protected bool _animationHasFinished = true;

		protected bool _meleeAttackMadeContact;

		protected Action _cbOnSpawnAnimationEnd;

		protected Action<Enemy> _cbOnDamagedAnimationEnd;

		protected FloatingText _exampleFloatingText;

		protected ObjectPool _floatingTextPool;

		protected StatusEffectIconManager _statusEffectUI;

		protected AudioSource _audioSource;

		protected EnemyManager _enemyManager;

		protected RelicManager _relicManager;

		protected Camera _camera;

		protected ScreenShaker _screenShaker;

		protected string _enemyName;

		private List<StatusEffect> _statusEffects = new List<StatusEffect>();

		private BuffAlliesBehaviour _buffAlliesBehaviour;

		private HealAlliesBehaviour _healAlliesBehaviour;

		private BuffPassingAlliesBehaviour _buffPassingAlliesBehaviour;

		private HealPassingAlliesBehaviour _healPassingAlliesBehaviour;

		private ApplyStatusEffectsToPlayerBehaviour _applyStatusEffectsToPlayerBehaviour;

		private StanceChangingReactiveBehaviour _stanceChanger;

		private Projectile _inflightProjectile;

		private int _brambleTurnCounter;

		private int _brambleAchievementTurnsNeeded = 6;

		private int _damageToUnlockAchievement = 9000;

		[HideInInspector]
		public int currentBossIconIndex;

		private int _numAttacks;

		private bool _animTriggeredThisFrame;

		private LimitDamageTaken _limitDamage;

		[HideInInspector]
		public bool lastAttackDodged;

		[HideInInspector]
		public float playerHealthLostFromLastAttack;

		public Queue<AnimationTypes> queuedAnimations = new Queue<AnimationTypes>();

		protected bool _skipActionThisTurn;

		private bool _isHoveringMouse;

		private bool _externalAnimationHasFinished = true;

		private bool _lootRolled;

		protected LineRenderer _lineRenderer;

		[HideInInspector]
		public Color damageTextColor = Color.white;

		protected int _extraAttacks;

		public float extraAttacksTime = 0.5f;

		[HideInInspector]
		public float postAttackWaitTime;

		public bool CleanupHasFinished { get; protected set; } = true;

		public Color DefaultInfoSpriteColor => GetComponent<SpriteRenderer>().color;

		public float maxHealth => _maxHealth;

		public virtual bool CanBeHealed => true;

		public StatusEffect applyStatWhenProjectileAttacked
		{
			get
			{
				return _applyStatWhenProjectileAttacked;
			}
			set
			{
				_applyStatWhenProjectileAttacked = value;
			}
		}

		public StatusEffect applyStatWhenTargetedAttacked
		{
			get
			{
				return _applyStatWhenTargetedAttacked;
			}
			set
			{
				_applyStatWhenTargetedAttacked = value;
			}
		}

		public Canvas enemyUICanvas => _enemyUICanvas;

		public EnemyManager enemyManager => _enemyManager;

		public RelicManager relicManager => _relicManager;

		protected bool screenShakeBlocked
		{
			get
			{
				if (_screenShaker != null)
				{
					return _screenShaker.blocked;
				}
				return false;
			}
		}

		public StatusEffect[] StatusEffects => _statusEffects.ToArray();

		public string enemyName => _enemyName;

		protected virtual void Awake()
		{
			_limitDamage = GetComponent<LimitDamageTaken>();
			_prevMeleeAttackDamage = DamagePerMeleeAttack;
			_prevRangedAttackDamage = DamagePerRangedAttack;
		}

		protected virtual void Start()
		{
			if (currentChargeTime > 0)
			{
				PlayAnimation("charge_trigger");
				_anim.SetBool("charge", value: true);
				currentChargeTime++;
				_anim.SetInteger("charge_count", currentChargeTime);
			}
		}

		protected virtual void OnEnable()
		{
			OnAllEnemiesBuffed = (BuffAllEnemies)Delegate.Combine(OnAllEnemiesBuffed, new BuffAllEnemies(AddDamageBuff));
			OnAllEnemiesMultiBuff = (BuffAllEnemiesMultiple)Delegate.Combine(OnAllEnemiesMultiBuff, new BuffAllEnemiesMultiple(AddMultipleBuffs));
			OnEnemiesHealed = (HealEnemies)Delegate.Combine(OnEnemiesHealed, new HealEnemies(Heal));
			OnAllEnemiesDamaged = (DamageAllEnemies)Delegate.Combine(OnAllEnemiesDamaged, new DamageAllEnemies(SimpleDamage));
			PlayerHealthController.OnPlayerGainedHealth = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerGainedHealth, new PlayerHealthController.PlayerHealthEvent(OnPlayerGainedHealth));
			PlayerHealthController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(OnPlayerSelfDamageTaken));
			_targetingUI = GetComponentInChildren<TargetingUI>(includeInactive: true);
			_targetingUIInitialPosition = _targetingUI.transform.position;
			_enemyUICanvas = GetComponentInChildren<Canvas>(includeInactive: true);
		}

		protected virtual void OnDisable()
		{
			OnAllEnemiesBuffed = (BuffAllEnemies)Delegate.Remove(OnAllEnemiesBuffed, new BuffAllEnemies(AddDamageBuff));
			OnAllEnemiesMultiBuff = (BuffAllEnemiesMultiple)Delegate.Remove(OnAllEnemiesMultiBuff, new BuffAllEnemiesMultiple(AddMultipleBuffs));
			OnEnemiesHealed = (HealEnemies)Delegate.Remove(OnEnemiesHealed, new HealEnemies(Heal));
			OnAllEnemiesDamaged = (DamageAllEnemies)Delegate.Remove(OnAllEnemiesDamaged, new DamageAllEnemies(SimpleDamage));
			PlayerHealthController.OnPlayerGainedHealth = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerGainedHealth, new PlayerHealthController.PlayerHealthEvent(OnPlayerGainedHealth));
			PlayerHealthController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(OnPlayerSelfDamageTaken));
			if (_isHoveringMouse)
			{
				_isHoveringMouse = false;
				OnMouseHoverExitEnemy?.Invoke();
			}
			if (_inflightProjectile != null)
			{
				OnEnemyAttack?.Invoke(_inflightProjectile.damage, melee: false, this);
				_enemyManager.ResolveVictoryBlockingOperation();
			}
			if (_lineRenderer != null)
			{
				UnityEngine.Object.Destroy(_lineRenderer);
			}
		}

		protected virtual void Update()
		{
			if (!_animationHasFinished && !_anim.IsInTransition(0) && _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !_animTriggeredThisFrame)
			{
				_animationHasFinished = true;
			}
			if (_animTriggeredThisFrame)
			{
				_animTriggeredThisFrame = false;
			}
		}

		public void OnPointerEnter(PointerEventData data)
		{
			if (!_isHoveringMouse)
			{
				OnMouseHoverEnterEnemy?.Invoke();
				_isHoveringMouse = true;
			}
		}

		public void OnPointerExit(PointerEventData data)
		{
			if (_isHoveringMouse)
			{
				_isHoveringMouse = false;
				OnMouseHoverExitEnemy?.Invoke();
			}
		}

		public virtual void Initialize(StatusEffectData statusEffectData, EnemyManager enemyManager, RelicManager relicManager, string enemyName)
		{
			_enemyName = enemyName;
			_anim = GetComponent<Animator>();
			_renderer = GetComponent<SpriteRenderer>();
			_exampleFloatingText = GetComponentInChildren<FloatingText>(includeInactive: true);
			_floatingTextPool = GetComponentInChildren<ObjectPool>();
			_statusEffectUI = GetComponentInChildren<StatusEffectIconManager>(includeInactive: true);
			_audioSource = GetComponent<AudioSource>();
			_enemyManager = enemyManager;
			_relicManager = relicManager;
			_camera = Camera.main;
			_screenShaker = _camera.GetComponent<ScreenShaker>();
			_healAnim = GetComponentInChildren<HealAnimLocator>(includeInactive: true);
			_buffAlliesBehaviour = GetComponent<BuffAlliesBehaviour>();
			_healAlliesBehaviour = GetComponent<HealAlliesBehaviour>();
			_buffPassingAlliesBehaviour = GetComponent<BuffPassingAlliesBehaviour>();
			_healPassingAlliesBehaviour = GetComponent<HealPassingAlliesBehaviour>();
			_applyStatusEffectsToPlayerBehaviour = GetComponent<ApplyStatusEffectsToPlayerBehaviour>();
			_stanceChanger = GetComponent<StanceChangingReactiveBehaviour>();
			if (HealthBarObj == null)
			{
				CanvasGroup[] componentsInChildren = GameObject.FindGameObjectWithTag("BossHealthBar").GetComponentsInChildren<CanvasGroup>(includeInactive: true);
				int num = ((BossHealthBarIndex > 0) ? BossHealthBarIndex : currentBossIconIndex++);
				HealthBarObj = componentsInChildren[num].gameObject;
				componentsInChildren[num].DOFade(1f, 1f);
				_healthText = HealthBarObj.GetComponentInChildren<TextMeshProUGUI>();
				if (_bossHealthBarOverrideIcon != null)
				{
					Image component = HealthBarObj.GetComponentInChildren<BossHealthBarIconLocator>(includeInactive: true).GetComponent<Image>();
					component.sprite = _bossHealthBarOverrideIcon;
					component.color = _bossHealthBarOverrideColor;
				}
				if (BossHealthBarIndex == 1)
				{
					UpcomingEnemyViewLocator upcomingEnemyViewLocator = UnityEngine.Object.FindObjectOfType<UpcomingEnemyViewLocator>();
					if (upcomingEnemyViewLocator != null)
					{
						upcomingEnemyViewLocator.gameObject.SetActive(value: false);
					}
				}
			}
			if (_statusEffectUI != null)
			{
				_statusEffectUI.EffectData = statusEffectData;
			}
			if (_cruciballManager == null)
			{
				Debug.LogError("Enemy with no Cruciball Manager assigned! " + base.name);
				_maxHealth = StartingHealth;
			}
			else
			{
				_maxHealth = (_cruciballManager.EnemyShouldUseImprovedHealth(enemyTypes.HasFlag(EnemyType.Boss), enemyTypes.HasFlag(EnemyType.Miniboss)) ? StartingHealthFirstCruciball : StartingHealth);
				if (_cruciballManager.ShouldUpgradeBosses() && TryGetComponent<BossUpgrade>(out var component2))
				{
					component2.isUpgraded = true;
					component2.Upgrade();
					_maxHealth = component2.GetUpgradedMaxHealth();
				}
			}
			if (_relicManager.RelicEffectActive(RelicEffect.RANDOM_ENEMY_HEALTH))
			{
				_maxHealth = Mathf.Ceil(UnityEngine.Random.Range(1f, _maxHealth * 2.5f));
			}
			CurrentHealth = maxHealth;
			HealthBarBarSprite = HealthBarObj.GetComponentInChildren<UpdateSlider>();
			_targetingUI.SetTargeting(status: false);
			EnemyStartWithStatusEffect component3 = GetComponent<EnemyStartWithStatusEffect>();
			if (component3 != null && component3.statuses != null)
			{
				StatusEffect[] statuses = component3.statuses;
				foreach (StatusEffect statusEffect in statuses)
				{
					ApplyStatusEffect(statusEffect, StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
				}
			}
			OnEnemySpawned?.Invoke(this);
			UpdateHealthBar();
			SetHealthAnim();
			if (_relicManager.AttemptUseRelic(RelicEffect.START_WITH_EXPLOITABALL))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Exploitaball, 3), StatusEffectSource.PLAYER);
			}
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.AddEnemyCount(_enemyName);
			}
			if (AssetLoading.Instance.EnemyDataTable.TryGetValue(locKey, out var value) && !value.Seen)
			{
				value.Seen = true;
			}
		}

		public void ApplyStatusEffect(StatusEffect statusEffect, StatusEffectSource source, bool allowKnockOnEffects = true, bool ignoreStats = false)
		{
			if (statusEffect.EffectType == StatusEffectType.None || statusEffect.Intensity <= 0)
			{
				return;
			}
			if (_statusEffectImmunities != null)
			{
				StatusEffectType[] immunities = _statusEffectImmunities.Immunities;
				foreach (StatusEffectType statusEffectType in immunities)
				{
					if (statusEffect.EffectType == statusEffectType)
					{
						return;
					}
				}
			}
			bool flag = false;
			if (StatusEffect.IsStatusEffectDebuff(statusEffect.EffectType))
			{
				flag = true;
				if (_relicManager.AttemptUseRelic(RelicEffect.INCREASE_NEGATIVE_STATUS))
				{
					statusEffect.Intensity++;
				}
			}
			if (TryGetComponent<EnemyGetsDoubleStats>(out var _))
			{
				statusEffect.Intensity = Mathf.RoundToInt((float)statusEffect.Intensity * 2f);
			}
			if (source == StatusEffectSource.PLAYER && _relicManager.AttemptUseRelic(RelicEffect.DOUBLE_STATUS_EFFECTS))
			{
				statusEffect.Intensity = Mathf.RoundToInt((float)statusEffect.Intensity * 2f);
			}
			if (StaticGameData.CurrentRunStats != null && statusEffect.EffectType == StatusEffectType.Poison && AchievementManager.AchievementsOn)
			{
				StaticGameData.CurrentRunStats.roundrelProgress += statusEffect.Intensity;
				Debug.Log($"Adding {statusEffect.Intensity} to roundrelProgress = {StaticGameData.CurrentRunStats.roundrelProgress}");
				if (!CruciballManager.IsClassUnlocked(Class.Roundrel) && PersistentPlayerData.Instance.RoundrelProgress + StaticGameData.CurrentRunStats.roundrelProgress >= PersistentPlayerData.ROUNDREL_PROGRESS_MAX && !PersistentPlayerData.Instance.QueuedClassUnlocks.Contains(Class.Roundrel))
				{
					PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Roundrel);
				}
			}
			if (flag)
			{
				OnNegativeStatusEffectAdded?.Invoke(statusEffect);
			}
			bool flag2 = false;
			foreach (StatusEffect statusEffect2 in _statusEffects)
			{
				if (statusEffect2.EffectType == statusEffect.EffectType)
				{
					flag2 = true;
					statusEffect2.Intensity = Mathf.Min(statusEffect2.Intensity + statusEffect.Intensity, StatusEffect.GetMaxIntensityForEffect(statusEffect.EffectType, relicManager));
					if (statusEffect2.EffectType == StatusEffectType.Poison)
					{
						CheckForPoisonAchievement(statusEffect2);
					}
					if (_statusEffectUI != null)
					{
						_statusEffectUI.UpdateStatusEffects(StatusEffects);
					}
					if (allowKnockOnEffects)
					{
						CheckForKnockOnEffects(statusEffect);
					}
					break;
				}
			}
			if (!flag2)
			{
				_statusEffects.Add(statusEffect);
				CheckForPoisonAchievement(statusEffect);
				if (allowKnockOnEffects)
				{
					CheckForKnockOnEffects(statusEffect);
				}
				if (_statusEffectUI != null)
				{
					_statusEffectUI.UpdateStatusEffects(StatusEffects);
				}
			}
			if (statusEffect.EffectType == StatusEffectType.Lifelink)
			{
				CreateLifelinkRenderer();
			}
			if (StaticGameData.CurrentRunStats != null && !ignoreStats && StatusEffect.IsStatusEffectDebuff(statusEffect.EffectType))
			{
				if (StaticGameData.CurrentRunStats.stacksPerStatusEffect.ContainsKey(statusEffect.EffectType))
				{
					StaticGameData.CurrentRunStats.stacksPerStatusEffect[statusEffect.EffectType] += statusEffect.Intensity;
				}
				else
				{
					StaticGameData.CurrentRunStats.stacksPerStatusEffect.Add(statusEffect.EffectType, statusEffect.Intensity);
				}
				_enemyManager.NegativeStatusEffectAddedToEnemy(statusEffect.Intensity);
			}
		}

		public void CreateLifelinkRenderer()
		{
			GameObject original = Addressables.LoadAssetAsync<GameObject>("LifelinkLaser").WaitForCompletion();
			if (_lineRenderer == null)
			{
				_lineRenderer = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity).GetComponentInChildren<LineRenderer>();
			}
			if (_lineRenderer.positionCount == 3)
			{
				SetLifelinkRendererPosition(base.transform.position.x);
			}
		}

		protected void SetLifelinkRendererPosition(float dummyParam)
		{
			_lineRenderer.SetPosition(0, new Vector3(_enemyManager.playerPosition.x, _enemyManager.playerPosition.y));
			float x = (_enemyManager.playerPosition.x + 0.5f + base.transform.position.x) / 2f;
			float num = (_enemyManager.playerPosition.y + base.transform.position.y) / 2f;
			_lineRenderer.SetPosition(1, new Vector3(x, num + 1f));
			_lineRenderer.SetPosition(2, base.transform.position + new Vector3(0f, 2f));
		}

		public void SetLifelinkVisualsStatus(bool value)
		{
			if (_lineRenderer != null)
			{
				_lineRenderer.gameObject.SetActive(value);
			}
		}

		public void IncrementAllDebuffStatusEffects(int amt = 1)
		{
			StatusEffect[] array = new StatusEffect[_statusEffects.Count];
			for (int i = 0; i < _statusEffects.Count; i++)
			{
				StatusEffect statusEffect = _statusEffects[i];
				if (statusEffect.Intensity >= 1 && StatusEffect.IsStatusEffectDebuff(statusEffect.EffectType))
				{
					array[i] = new StatusEffect(statusEffect.EffectType, amt);
				}
			}
			foreach (StatusEffect statusEffect2 in array)
			{
				if (statusEffect2 != null)
				{
					ApplyStatusEffect(statusEffect2, StatusEffectSource.PLAYER);
				}
			}
		}

		public void IncrementStatusEffectMultiplier(StatusEffectType type, int multiplier)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect.EffectType == type)
				{
					statusEffect.Intensity = Mathf.Min(statusEffect.Intensity * multiplier, StatusEffect.GetMaxIntensityForEffect(statusEffect.EffectType, relicManager));
					CheckForPoisonAchievement(statusEffect);
					_statusEffectUI?.UpdateStatusEffects(StatusEffects);
					break;
				}
			}
		}

		private void CheckForKnockOnEffects(StatusEffect statusEffect)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.BLIND_BRAMBLE_COMBO))
			{
				if (statusEffect.EffectType == StatusEffectType.Blind && !HasEffect(StatusEffectType.Thorned))
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Thorned), StatusEffectSource.PLAYER, allowKnockOnEffects: false);
				}
				else if (statusEffect.EffectType == StatusEffectType.Thorned)
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 10), StatusEffectSource.PLAYER, allowKnockOnEffects: false);
				}
			}
		}

		public bool ResolveStatusEffects()
		{
			bool result = false;
			List<StatusEffect> list = new List<StatusEffect>();
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect.EffectType == StatusEffectType.Poison)
				{
					result = true;
					float num = 1f;
					if (_relicManager.RelicEffectActive(RelicEffect.MORE_SPINFECTION_DAMAGE_PER_ACT))
					{
						num = 2f;
					}
					Damage(Mathf.RoundToInt(StatusEffectData.POISON_DAMAGE_MULTIPLIER * num * (float)statusEffect.Intensity), screenshake: false, 1f, 1f, unblockable: true, EnemyDamageSource.StatusEffect, sourceIsPlayer: false);
				}
				if (!StatusEffect.IsStatusEffectPerishable(statusEffect.EffectType))
				{
					continue;
				}
				if (!AchievementData.Achievements[AchievementData.AchievementId.BRAMBLE_ENEMY_FOR_X_TURNS].Unlocked && statusEffect.EffectType == StatusEffectType.Thorned)
				{
					CheckForBrambledAchievement(statusEffect.Intensity);
				}
				StatusEffectsDontDecrease component2;
				if (statusEffect.EffectType == StatusEffectType.Poison && _relicManager.AttemptUseRelic(RelicEffect.SPINFECTION_INC_OVER_TIME))
				{
					int num2 = 1;
					if (_relicManager.AttemptUseRelic(RelicEffect.INCREASE_NEGATIVE_STATUS))
					{
						num2++;
					}
					if (TryGetComponent<EnemyGetsDoubleStats>(out var _))
					{
						num2 *= 2;
					}
					if (_relicManager.AttemptUseRelic(RelicEffect.DOUBLE_STATUS_EFFECTS))
					{
						num2 *= 2;
					}
					statusEffect.Intensity += num2;
				}
				else if (!TryGetComponent<StatusEffectsDontDecrease>(out component2) && --statusEffect.Intensity <= 0)
				{
					list.Add(statusEffect);
				}
			}
			_statusEffectUI?.UpdateStatusEffects(StatusEffects);
			foreach (StatusEffect item in list)
			{
				_statusEffects.Remove(item);
			}
			return result;
		}

		private void CheckForBrambledAchievement(int intensity)
		{
			_brambleTurnCounter++;
			if (_brambleTurnCounter >= _brambleAchievementTurnsNeeded)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.BRAMBLE_ENEMY_FOR_X_TURNS);
			}
			if (intensity - 1 <= 0)
			{
				_brambleTurnCounter = 0;
			}
		}

		public void RemoveEffect(StatusEffectType type)
		{
			StatusEffect statusEffect = null;
			foreach (StatusEffect statusEffect2 in _statusEffects)
			{
				if (statusEffect2.EffectType == type)
				{
					statusEffect2.Intensity = 0;
					statusEffect = statusEffect2;
				}
			}
			if (statusEffect != null)
			{
				_statusEffectUI?.UpdateStatusEffects(StatusEffects);
				_statusEffects.Remove(statusEffect);
			}
		}

		public bool HasEffect(StatusEffectType type)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (type == statusEffect.EffectType && statusEffect.Intensity > 0)
				{
					return true;
				}
			}
			return false;
		}

		public StatusEffect GetEffect(StatusEffectType type)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (type == statusEffect.EffectType)
				{
					return statusEffect;
				}
			}
			return null;
		}

		public int GetEffectIntensity(StatusEffectType type)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (type == statusEffect.EffectType)
				{
					return statusEffect.Intensity;
				}
			}
			return 0;
		}

		public void SetEffectIntensity(StatusEffectType type, int intensity, StatusEffectSource source = StatusEffectSource.ENEMY)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (type == statusEffect.EffectType)
				{
					statusEffect.Intensity = intensity;
					return;
				}
			}
			ApplyStatusEffect(new StatusEffect(type, intensity), source, allowKnockOnEffects: false, ignoreStats: true);
		}

		public void ReduceEffectIntensity(StatusEffectType type, int reduction)
		{
			StatusEffect statusEffect = null;
			foreach (StatusEffect statusEffect2 in _statusEffects)
			{
				if (type == statusEffect2.EffectType)
				{
					if (reduction >= statusEffect2.Intensity)
					{
						statusEffect = statusEffect2;
					}
					statusEffect2.Intensity -= reduction;
					_statusEffectUI?.UpdateStatusEffects(StatusEffects);
					break;
				}
			}
			if (statusEffect != null)
			{
				_statusEffects.Remove(statusEffect);
			}
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			DamagesEnemyOnTrigger component = other.GetComponent<DamagesEnemyOnTrigger>();
			if (component != null)
			{
				Damage(component.Damage);
				if (CurrentHealth <= 0f && _anim.MMHasParameterOfType("ballista_hit", AnimatorControllerParameterType.Bool))
				{
					_anim.SetBool("ballista_hit", value: true);
				}
			}
			Enemy component2 = other.GetComponent<Enemy>();
			if (HasEffect(StatusEffectType.Thorned) && component2 != null)
			{
				int num = GetEffect(StatusEffectType.Thorned)?.Intensity ?? 0;
				component2.Damage(Mathf.RoundToInt((float)num * 10f), screenshake: true, 1f, 1f, unblockable: false, EnemyDamageSource.StatusEffect);
			}
			int num2 = GetEffect(StatusEffectType.Reflect)?.Intensity ?? 0;
			num2 = Mathf.Max(num2 - (GetEffect(StatusEffectType.Transpherency)?.Intensity ?? 0), 0);
			ShotBehavior component3 = other.gameObject.GetComponent<ShotBehavior>();
			if (component3 != null && num2 > 0 && !component3.isReverseAttack && component3.enemiesActuallyHitByShot.Contains(this) && CurrentHealth > 0f)
			{
				StartCoroutine(SimpleProjectileAttack(enemyManager.playerPosition, Mathf.FloorToInt(component3.shotDamage * component3.shotDamageMod * (float)num2 * 0.01f)));
				PlayAnimation("reflect");
			}
			if (CurrentHealth > 0f && applyStatWhenProjectileAttacked != null && component3 != null && component3.enemiesActuallyHitByShot.Contains(this))
			{
				ApplyStatusEffect(new StatusEffect(applyStatWhenProjectileAttacked), StatusEffectSource.ENEMY);
			}
		}

		public void DealProjectileAttackDamage(long damage = 0L, float modifier = 1f, int critCount = 0, bool dealMaxHPDamage = false)
		{
			DamageWithTypeMods(damage, modifier, critCount, EnemyDamageSource.ProjectileAttack, dealMaxHPDamage);
			if (_stanceChanger != null)
			{
				StartCoroutine(_stanceChanger.HitByProjectileAttack());
			}
		}

		public void DealTargetedAttackDamage(long damage = 0L, float modifier = 1f, int critCount = 0)
		{
			DamageWithTypeMods(damage, modifier, critCount, EnemyDamageSource.TargetedAttack);
			if (_applyStatWhenTargetedAttacked != null)
			{
				ApplyStatusEffect(new StatusEffect(applyStatWhenTargetedAttacked), StatusEffectSource.ENEMY);
			}
			if (_stanceChanger != null)
			{
				StartCoroutine(_stanceChanger.HitByTargetedAttack());
			}
		}

		public void DamageWithTypeMods(long damage = 0L, float modifier = 1f, int critCount = 0, EnemyDamageSource damageSource = EnemyDamageSource.Unspecified, bool dealMaxHPDamage = false)
		{
			long num = damage;
			if (enemyTypes.HasFlag(EnemyType.Slime) && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_BONUS_SLIME_FLAT))
			{
				num += 10;
			}
			if (enemyTypes.HasFlag(EnemyType.Plant) && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_BONUS_PLANT_FLAT))
			{
				num += 10;
			}
			if (critCount > 0)
			{
				int effectIntensity = GetEffectIntensity(StatusEffectType.Exploitaball);
				num += Mathf.CeilToInt((float)effectIntensity * ((float)num * StatusEffectData.EXPLOITABALL_BONUS_DAMAGE_PER_STACK));
			}
			if (Math.Abs((float)num - 1f) < 0.01f)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DEAL_EXACTLY_ONE_DAMAGE);
			}
			if (num >= _damageToUnlockAchievement)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DEAL_X_DAMAGE_IN_ONE_SHOT);
			}
			Damage(num, screenshake: true, 1f, modifier, unblockable: false, damageSource, sourceIsPlayer: true, dealMaxHPDamage);
		}

		public void SimpleDamage(long damage, EnemyDamageSource source = EnemyDamageSource.Unspecified)
		{
			Damage(damage, screenshake: false, 0.25f, 1f, unblockable: false, source, sourceIsPlayer: false);
		}

		public virtual void Damage(long damage = 0L, bool screenshake = true, float audioScale = 1f, float damageMod = 1f, bool unblockable = false, EnemyDamageSource damageSource = EnemyDamageSource.Unspecified, bool sourceIsPlayer = true, bool dealMaxHPDamage = false)
		{
			if (base.gameObject == null)
			{
				Debug.LogError("Attempted to damage an enemy script on a null object.");
			}
			else
			{
				if (CurrentHealth <= 0f)
				{
					return;
				}
				if (_limitDamage != null)
				{
					damage = (long)Mathf.Min(damage, _limitDamage.damageLimit);
				}
				long num = (long)((float)damage * damageMod);
				if (num < 0 && damage >= 0 && damageMod >= 0f)
				{
					num = long.MaxValue;
				}
				int effectIntensity = GetEffectIntensity(StatusEffectType.Intangiball);
				int effectIntensity2 = GetEffectIntensity(StatusEffectType.Ballwark);
				long damage2 = damage;
				if (effectIntensity > 0)
				{
					float num2 = effectIntensity;
					if (!unblockable)
					{
						num2 += (float)effectIntensity2;
					}
					damage2 = (long)Mathf.Min(num2, damage);
				}
				SpawnFloatingText(damage2, damageTextColor, damageSource, damageMod);
				if (screenshake)
				{
					DamageScreenshake(damage);
					ControllerVibration.DoEnemyHitVibration(damage, _maxHealth, screenShakeIntensityReference);
				}
				long num3 = num;
				if (!unblockable)
				{
					num3 -= effectIntensity2;
					if (effectIntensity2 > 0)
					{
						ReduceEffectIntensity(StatusEffectType.Ballwark, (int)num);
					}
				}
				audioScale *= Mathf.Clamp((float)num3 / screenShakeIntensityReference, 0.1f, 1f);
				if (num3 <= 0)
				{
					if (base.isActiveAndEnabled)
					{
						StartCoroutine(PlayDamagedAnimation(num, audioScale, damageSource));
					}
					return;
				}
				if (effectIntensity > 0)
				{
					num3 = (long)Mathf.Min(num3, effectIntensity);
				}
				long num4 = num3;
				if (dealMaxHPDamage)
				{
					_maxHealth = Mathf.Max(_maxHealth - (float)num4, 0f);
				}
				if ((damageSource == EnemyDamageSource.ProjectileAttack || damageSource == EnemyDamageSource.TargetedAttack || damageSource == EnemyDamageSource.AOE) && Math.Abs((float)num4 - CurrentHealth) < 0.01f)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DEAL_EXACT_DAMAGE_TO_ENEMY);
				}
				CurrentHealth = Mathf.Clamp((long)CurrentHealth - num4, 0f, maxHealth);
				if (sourceIsPlayer && StaticGameData.CurrentRunStats != null)
				{
					if (StaticGameData.CurrentRunStats.mostDamageDealtWithSingleAttack < num4)
					{
						if (num4 >= int.MaxValue)
						{
							StaticGameData.CurrentRunStats.mostDamageDealtWithSingleAttack = int.MaxValue;
						}
						else
						{
							StaticGameData.CurrentRunStats.mostDamageDealtWithSingleAttack = (int)num4;
						}
					}
					long num5 = StaticGameData.CurrentRunStats.totalDamageDealt + num4;
					if ((num5 < 0 && StaticGameData.CurrentRunStats.totalDamageDealt >= 0 && num4 >= 0) || num5 >= int.MaxValue)
					{
						StaticGameData.CurrentRunStats.totalDamageDealt = int.MaxValue;
					}
					else
					{
						StaticGameData.CurrentRunStats.totalDamageDealt = StaticGameData.CurrentRunStats.totalDamageDealt + (int)num4;
					}
				}
				if (TryGetComponent<GrantCoinsOnDeath>(out var component))
				{
					int num6 = component.coinToDropWhenHit * Mathf.FloorToInt(Mathf.Min(num4, maxHealth) / maxHealth / component.healthPercentageToDropExtraCoins);
					PegManager.AddGoldToPegsRequest(num6, base.transform.position);
				}
				UpdateHealthBar();
				OnEnemyDamaged?.Invoke(this, num3, damageSource);
				SetHealthAnim();
				if (base.isActiveAndEnabled)
				{
					StartCoroutine(PlayDamagedAnimation(num, audioScale, damageSource));
				}
			}
		}

		public bool WouldDamageKillEnemy(float damage = 0f, float damageMod = 1f)
		{
			return damage * damageMod > CurrentHealth;
		}

		private void AddDamageBuff(int buffAmount)
		{
			StatusEffect statusEffect = new StatusEffect(StatusEffectType.Strength, buffAmount);
			ApplyStatusEffect(statusEffect, StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
		}

		private void AddMultipleBuffs(StatusEffect[] effects, StatusEffectSource source = StatusEffectSource.ENEMY)
		{
			foreach (StatusEffect statusEffect in effects)
			{
				ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, statusEffect.Intensity), source, allowKnockOnEffects: true, ignoreStats: true);
			}
		}

		public virtual void Heal(float amount)
		{
			if (!(CurrentHealth <= 0f))
			{
				if (_healAnim != null)
				{
					_healAnim.gameObject.SetActive(value: true);
				}
				CurrentHealth += amount;
				CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
				UpdateHealthBar();
				SpawnFloatingText((long)amount, _healTextColor, EnemyDamageSource.Unspecified);
				SetHealthAnim();
			}
		}

		private void SetHealthAnim()
		{
			if (_anim != null && _anim.MMHasParameterOfType("health_percent", AnimatorControllerParameterType.Float))
			{
				_anim.SetFloat("health_percent", Mathf.Max(CurrentHealth / maxHealth, 0f));
			}
		}

		private void CheckForPoisonAchievement(StatusEffect effect)
		{
			if (effect.EffectType == StatusEffectType.Poison && effect.Intensity >= 40)
			{
				Achievement achievement = AchievementData.Achievements[AchievementData.AchievementId.POISON_AMOUNT_ON_ENEMY];
				if (!achievement.Unlocked && AchievementManager.AchievementsOn)
				{
					AchievementManager.Instance.UnlockAchievement(achievement);
					PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Roundrel);
				}
			}
		}

		public Vector3 GetCenterPosition()
		{
			return _renderer.bounds.center;
		}

		private void SpawnFloatingText(long damage, Color color, EnemyDamageSource damageSource, float damageMod = 1f)
		{
			if (_exampleFloatingText == null)
			{
				return;
			}
			bool shouldDie = false;
			FloatingText pooledObject;
			if (damageSource == EnemyDamageSource.Bomb || damageSource == EnemyDamageSource.RelicChip)
			{
				pooledObject = _floatingTextPool.GetPooledObject<FloatingText>();
				if (pooledObject == null)
				{
					return;
				}
			}
			else
			{
				pooledObject = _floatingTextPool.GetPooledObject<FloatingText>(instanceMoreIfNeeded: true);
			}
			pooledObject.transform.position = _exampleFloatingText.transform.position;
			float num = Mathf.Clamp((float)damage / 30f, 1f, 3f);
			pooledObject.transform.localScale *= num;
			string text = DamageCountDisplay.FormatDamageNumberAsString(damage);
			pooledObject.SetColor(Color.white);
			if (damageMod > 1f)
			{
				text = "<style=enemyBonusDmg>" + text + "x" + damageMod + "</style>";
			}
			else if (damageMod < 1f)
			{
				text = "<style=enemyNegDmg>" + text + "x" + damageMod + "</style>";
			}
			else
			{
				pooledObject.SetColor(color);
			}
			pooledObject.gameObject.SetActive(value: true);
			pooledObject.Activate(text, shouldDie);
		}

		protected void DamageScreenshake(long damage)
		{
			if (!screenShakeBlocked && DOTween.TweensByTarget(_camera) == null)
			{
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				float num2 = (float)damage / screenShakeIntensityReference * num;
				float duration = Mathf.Lerp(screenShakeDurationMin, screenShakeDurationMax, num2);
				Tweener tweener = _camera.DOShakePosition(duration, Mathf.Min(screenShakeIntensityBase * num2, screenShakeIntensityMax));
				tweener.onComplete = (TweenCallback)Delegate.Combine(tweener.onComplete, new TweenCallback(ScreenshakeComplete));
			}
		}

		private void ScreenshakeComplete()
		{
			List<Tween> list = DOTween.TweensByTarget(_camera);
			if (list == null || list.Count == 0)
			{
				_camera.GetComponent<ReturnCameraToDefaultPos>().ReturnToDefault();
			}
		}

		public void PreMoveTurnAction()
		{
			EnemyReflectProjectile component = GetComponent<EnemyReflectProjectile>();
			if (component != null && component.reflectIncrease > 0)
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Reflect, component.reflectIncrease), StatusEffectSource.ENEMY);
			}
			GainBallwarkBasedOnShieldPegs component2 = GetComponent<GainBallwarkBasedOnShieldPegs>();
			if (component2 != null)
			{
				component2.ApplyBallwarkBasedOnShieldPegs();
			}
			ContinuouslySummonEnemies component3 = GetComponent<ContinuouslySummonEnemies>();
			if (component3 != null)
			{
				component3.DoPreMoveTurnAction();
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.ENEMIES_GAIN_BALLWARK))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, Mathf.RoundToInt(maxHealth * 0.05f)), StatusEffectSource.ENEMY);
			}
		}

		public bool CanMove()
		{
			if (!HasEffect(StatusEffectType.Thorned))
			{
				return !HasEffect(StatusEffectType.Stunned);
			}
			return false;
		}

		public void SkipNextAction()
		{
			_skipActionThisTurn = true;
		}

		public virtual bool Move(Vector3 position, int slotIndex = -1)
		{
			if (_skipActionThisTurn)
			{
				_skipActionThisTurn = false;
				return false;
			}
			if (!HasEffect(StatusEffectType.Thorned) && !HasEffect(StatusEffectType.Stunned))
			{
				base.transform.position = position;
				return true;
			}
			return false;
		}

		protected void PlayThornedAnimation()
		{
			if (_vineAnimation != null)
			{
				CurrentlyActing = true;
				OneShotAnimation vineAnimation = _vineAnimation;
				vineAnimation.OnAnimationEnded = (OneShotAnimation.AnimationEnded)Delegate.Combine(vineAnimation.OnAnimationEnded, new OneShotAnimation.AnimationEnded(OnVinesAnimationEnded));
				_vineAnimation.gameObject.SetActive(value: true);
				_vineAnimation.Play();
			}
		}

		protected void PlayStunnedAnimation()
		{
		}

		protected virtual void OnVinesAnimationEnded()
		{
			OneShotAnimation vineAnimation = _vineAnimation;
			vineAnimation.OnAnimationEnded = (OneShotAnimation.AnimationEnded)Delegate.Remove(vineAnimation.OnAnimationEnded, new OneShotAnimation.AnimationEnded(OnVinesAnimationEnded));
			_vineAnimation.gameObject.SetActive(value: false);
			EndAction();
		}

		public void IncrementAttackCharge(int num)
		{
			currentChargeTime += num;
		}

		public virtual void Attack(int range, Vector3 targetPos)
		{
			if (HasEffect(StatusEffectType.Stunned))
			{
				PlayStunnedAnimation();
			}
			else if (_skipActionThisTurn)
			{
				_skipActionThisTurn = false;
			}
			else
			{
				if (CurrentHealth <= 0f || _enemyAttack == null)
				{
					return;
				}
				_numAttacks++;
				if (AttackChargeLength > 0 && currentChargeTime < AttackChargeLength)
				{
					PlayAnimation("charge_trigger");
					if (_anim.MMHasParameterOfType("charge", AnimatorControllerParameterType.Bool))
					{
						_anim.SetBool("charge", value: true);
					}
					currentChargeTime++;
					if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
					{
						_anim.SetInteger("charge_count", currentChargeTime);
					}
					return;
				}
				if (AttackChargeLength > 0 && currentChargeTime >= AttackChargeLength)
				{
					_anim.SetBool("charge", value: false);
					currentChargeTime = 0;
					if (_anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
					{
						_anim.SetInteger("charge_count", currentChargeTime);
					}
				}
				AlternateEnemyAction[] components = GetComponents<AlternateEnemyAction>();
				if (components != null && components.Length != 0)
				{
					List<AlternateEnemyAction> list = new List<AlternateEnemyAction>();
					AlternateEnemyAction[] array = components;
					foreach (AlternateEnemyAction alternateEnemyAction in array)
					{
						if (alternateEnemyAction.IncrementAndCheckCharge() && alternateEnemyAction.ConditionsMet())
						{
							list.Add(alternateEnemyAction);
						}
					}
					List<AlternateEnemyAction> list2 = new List<AlternateEnemyAction>(list.ToArray());
					list2.Sort((AlternateEnemyAction x, AlternateEnemyAction y) => (int)Mathf.Sign((float)y.priority + (y.isFreeAction ? 0.1f : 0f) - ((float)x.priority + (x.isFreeAction ? 0.1f : 0f))));
					if (list2.Count > 0)
					{
						StartCoroutine(DoAlterternateActions(list2, range, targetPos));
						return;
					}
				}
				StartAttackCoroutine(range, targetPos);
			}
		}

		protected virtual void StartAttackCoroutine(int range, Vector3 targetPos)
		{
			if (canAttack)
			{
				if (range == 1 && !IsOnlyRanged && !useRangedAttackForMelee)
				{
					StartCoroutine(MeleeAttack());
				}
				else if (_buffAlliesBehaviour != null || (_healAlliesBehaviour != null && _enemyManager.NumDamagedEnemies() > 0))
				{
					StartCoroutine(RangedAttack(targetPos));
				}
				else if (_applyStatusEffectsToPlayerBehaviour != null)
				{
					StartCoroutine(RangedAttack(targetPos));
				}
				else if (range <= _enemyAttack.AttackRange && !IsOnlyRanged)
				{
					StartCoroutine(RangedAttack(targetPos));
				}
				else if (IsOnlyRanged)
				{
					StartCoroutine(RangedAttack(targetPos));
				}
			}
		}

		public float GetDamageBuff()
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect.EffectType == StatusEffectType.Strength)
				{
					return statusEffect.Intensity;
				}
			}
			return 0f;
		}

		protected virtual IEnumerator MeleeAttack()
		{
			float timer = 0f;
			queuedAnimations.Enqueue(AnimationTypes.MELEE_ATTACK);
			while (timer <= 300f && (CurrentlyActing || queuedAnimations.Peek() != AnimationTypes.MELEE_ATTACK))
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 300f)
				{
					Debug.LogWarning("Timed out on our attack start wait");
				}
			}
			CurrentlyActing = true;
			if (_enemyAttack != null && !string.IsNullOrEmpty(_enemyAttack.MeleeAttackTrigger))
			{
				_anim.SetTrigger(_enemyAttack.MeleeAttackTrigger);
			}
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_meleeAttackMadeContact = false;
			timer = 0f;
			AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (!_meleeAttackMadeContact && timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			_audioSource?.PlayOneShot(_enemySfx.MeleeAttackClip);
			if (_buffAlliesBehaviour != null && _buffAlliesBehaviour.buffAfterMeleeAttack && _buffAlliesBehaviour.CheckForBuff() && _enemyManager.Enemies.Count > _buffAlliesBehaviour.numAlliesRequired)
			{
				if (_enemySfx != null && _enemySfx.SpecialClips.Length != 0)
				{
					_audioSource.PlayOneShot(_enemySfx.GetRandomEnemySpecialClip());
				}
				OnAllEnemiesBuffed?.Invoke(_buffAlliesBehaviour.buffAmount);
				if (_buffAlliesBehaviour != null && _buffAlliesBehaviour.additionalStatuses != null)
				{
					OnAllEnemiesMultiBuff?.Invoke(_buffAlliesBehaviour.additionalStatuses);
				}
			}
			if (GetComponent<MeleeAttackImmuneToBlind>() == null && MissFromBlind())
			{
				OnEnemyAttack?.Invoke(0f, melee: true, this);
			}
			else
			{
				float num = DamagePerMeleeAttack + GetDamageBuff();
				if (BattleController.isMinibossBattle)
				{
					num = Mathf.Ceil(num * _cruciballManager.GetMinibossDamageModifier());
				}
				OnEnemyAttack?.Invoke(num, melee: true, this);
				if (!lastAttackDodged && playerHealthLostFromLastAttack > 0f && TryGetComponent<HealOnHit>(out var component))
				{
					Heal(playerHealthLostFromLastAttack * component.healMult);
				}
				if (_meleeAppliedStatusEffects.Length != 0 && !lastAttackDodged)
				{
					PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(_meleeAppliedStatusEffects, StatusEffectSource.ENEMY);
				}
				if (!lastAttackDodged && _relicManager.AttemptUseRelic(RelicEffect.BLIND_WHEN_HIT))
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 15), StatusEffectSource.PLAYER);
				}
			}
			if (CurrentHealth > 0f)
			{
				_animationHasFinished = false;
				while (!_animationHasFinished)
				{
					yield return WaitObjectAllocator.waitForEndOfFrame;
				}
				if (destroyAfterAttack)
				{
					Damage((long)maxHealth, screenshake: true, 1f, 1f, unblockable: false, EnemyDamageSource.Unspecified, sourceIsPlayer: false);
				}
				else
				{
					EndAction();
				}
				OnMeleeAttackFinished?.Invoke();
				if (queuedAnimations.Count > 0)
				{
					queuedAnimations.Dequeue();
				}
				else
				{
					Debug.LogWarning("Found no animation to dequeue from MeleeAttack end");
				}
			}
		}

		public virtual IEnumerator RangedAttack(Vector3 targetPos, float damage = -1f, GameObject projectileOverride = null, string animTriggerOverride = "")
		{
			float timeOutAnimationWait = 3f;
			while (CurrentlyActing && timeOutAnimationWait > 0f)
			{
				timeOutAnimationWait -= Time.deltaTime;
				if (timeOutAnimationWait <= 0f)
				{
					Debug.LogWarning("RangedAttack: Timed out waiting for CurrentlyActing flag to stop");
				}
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			CurrentlyActing = true;
			bool shouldBuff = _buffAlliesBehaviour != null && _buffAlliesBehaviour.CheckForBuff() && _enemyManager.Enemies.Count > _buffAlliesBehaviour.numAlliesRequired;
			bool shouldHeal = _healAlliesBehaviour != null && _healAlliesBehaviour.CheckForHeal() && (float)_enemyManager.NumDamagedEnemies() > _healAlliesBehaviour.numDamagedAlliesRequired;
			bool shouldApplyStatusEffect = _applyStatusEffectsToPlayerBehaviour != null && _applyStatusEffectsToPlayerBehaviour.CheckForApplication();
			string text = animTriggerOverride;
			if (string.IsNullOrEmpty(text))
			{
				text = (shouldHeal ? "heal" : _enemyAttack.RangedAttackAnimation);
			}
			if (shouldBuff && !string.IsNullOrEmpty(_buffAlliesBehaviour.animOverride))
			{
				_anim.SetTrigger(_buffAlliesBehaviour.animOverride);
				_animTriggeredThisFrame = true;
			}
			else if (shouldApplyStatusEffect && !string.IsNullOrEmpty(_applyStatusEffectsToPlayerBehaviour.animOverride))
			{
				_anim.SetTrigger(_applyStatusEffectsToPlayerBehaviour.animOverride);
				_animTriggeredThisFrame = true;
			}
			else
			{
				_anim.SetTrigger(text);
				_animTriggeredThisFrame = true;
			}
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_animationHasFinished = false;
			float timer = 0f;
			AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (!_animationHasFinished && timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			if (shouldBuff)
			{
				if (_enemySfx != null && _enemySfx.SpecialClips.Length != 0)
				{
					_audioSource.PlayOneShot(_enemySfx.GetRandomEnemySpecialClip());
				}
				OnAllEnemiesBuffed?.Invoke(_buffAlliesBehaviour.buffAmount);
				if (_buffAlliesBehaviour != null && _buffAlliesBehaviour.additionalStatuses != null)
				{
					OnAllEnemiesMultiBuff?.Invoke(_buffAlliesBehaviour.additionalStatuses);
				}
			}
			else if (shouldHeal)
			{
				DoHeal();
			}
			else if (shouldApplyStatusEffect)
			{
				if (_enemySfx != null && _enemySfx.SpecialClips.Length != 0)
				{
					_audioSource.PlayOneShot(_enemySfx.GetRandomEnemySpecialClip());
				}
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(_applyStatusEffectsToPlayerBehaviour.statuses, StatusEffectSource.ENEMY);
			}
			else
			{
				_audioSource?.PlayOneShot(_enemySfx.RangedAttackClip);
				if (!damagesMaxHp && _extraAttacks > 0)
				{
					WaitForSeconds waitObject = new WaitForSeconds(extraAttacksTime / (float)(_extraAttacks + 1));
					for (int i = 0; i < _extraAttacks; i++)
					{
						StartCoroutine(SimpleProjectileAttack(targetPos, damage, projectileOverride, i));
						yield return waitObject;
					}
				}
				yield return SimpleProjectileAttack(targetPos, damage, projectileOverride, _extraAttacks);
			}
			EndAction();
		}

		public void DoHeal()
		{
			if (_enemySfx != null && _enemySfx.SpecialClips.Length != 0)
			{
				_audioSource.PlayOneShot(_enemySfx.GetRandomEnemySpecialClip());
			}
			OnEnemiesHealed(_healAlliesBehaviour.healAmount + GetDamageBuff() * _healAlliesBehaviour.damageBuffHealMod);
		}

		public IEnumerator SimpleProjectileAttack(Vector3 targetPos, float damage = -1f, GameObject projectileOverride = null, int extraAttackNumber = 0)
		{
			GameObject original = ((projectileOverride == null) ? Projectile : projectileOverride);
			Projectile projectile = UnityEngine.Object.Instantiate(original, base.transform.position + ProjectileStartOffset, Quaternion.identity).GetComponent<Projectile>();
			projectile.extraProjectileForAttackNumber = extraAttackNumber;
			projectile.totalExtraProjectiles = _extraAttacks;
			projectile.extraProjectileTotalSpawnTime = extraAttacksTime;
			if (damage > 0f)
			{
				projectile.damage = damage;
			}
			else
			{
				damage = DamagePerRangedAttack + GetDamageBuff();
				if (BattleController.isMinibossBattle)
				{
					damage = Mathf.Ceil(damage * _cruciballManager.GetMinibossDamageModifier());
				}
				projectile.damage = (MissFromBlind() ? 0f : damage);
			}
			if (projectile.applyEnemyColour)
			{
				projectile.GetComponent<SpriteRenderer>().color = _renderer.color;
			}
			projectile.CheckForStatusScaling(_numAttacks);
			_inflightProjectile = projectile;
			_enemyManager.CreateVictoryBlockingOperation();
			projectile.Fire(targetPos, ProjectileStartOffset);
			float timer = 0f;
			bool audioPlayed = false;
			while (!projectile.HasHitDestination)
			{
				if (timer >= projectile.sfxStartDelay && !audioPlayed && _audioSource != null)
				{
					audioPlayed = true;
					_audioSource.PlayOneShot(_enemySfx.RangedAttackClip);
				}
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			OnEnemyAttack?.Invoke(projectile.damage, melee: false, this);
			if (projectile.damage > 0f && !lastAttackDodged)
			{
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(projectile.appliedStatusEffects, StatusEffectSource.ENEMY);
				if (_relicManager.AttemptUseRelic(RelicEffect.BLIND_WHEN_HIT))
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 15), StatusEffectSource.PLAYER);
				}
			}
			if (postAttackWaitTime > 0f)
			{
				yield return new WaitForSeconds(postAttackWaitTime);
			}
			_enemyManager.ResolveVictoryBlockingOperation();
			_inflightProjectile = null;
		}

		protected IEnumerator DoAlterternateActions(List<AlternateEnemyAction> sortedAlternateActions, int range, Vector3 targetPos)
		{
			bool alternateActionsTakeUpTurn = false;
			foreach (AlternateEnemyAction action in sortedAlternateActions)
			{
				yield return StartCoroutine(action.TakeAction(targetPos));
				if (!action.isFreeAction)
				{
					alternateActionsTakeUpTurn = true;
					if (!action.forceAllowAlternateActionsAfterThis)
					{
						yield break;
					}
				}
			}
			if (!alternateActionsTakeUpTurn)
			{
				StartAttackCoroutine(range, targetPos);
			}
		}

		public bool MissFromBlind()
		{
			if (!HasEffect(StatusEffectType.Blind))
			{
				return false;
			}
			float num = ((PlayerHealthController.DamageEvasionRandomState == null) ? UnityEngine.Random.value : PlayerHealthController.DamageEvasionRandomState.GetRandomValue());
			bool num2 = num < StatusEffectData.BLIND_BASE_MISS_CHANCE + (float)GetEffect(StatusEffectType.Blind).Intensity * StatusEffectData.BLIND_ADDITIONAL_PER_STAT;
			if (num2)
			{
				EnemyMissEvent onEnemyMissed = OnEnemyMissed;
				if (onEnemyMissed == null)
				{
					return num2;
				}
				onEnemyMissed(0f);
			}
			return num2;
		}

		protected void UpdateHealthBar()
		{
			if (HealthBarBarSprite != null)
			{
				HealthBarBarSprite.UpdateSize(CurrentHealth, maxHealth, 0f, maxZero: true);
				_healthText.text = CurrentHealth + "/" + maxHealth;
			}
		}

		public virtual void ToggleTargetedUI(bool on)
		{
			_targetingUI.SetTargeting(on);
			if (_enemyUICanvas != null)
			{
				_enemyUICanvas.sortingOrder = (on ? 1 : 0);
			}
		}

		public void ShakeLockedTargetUI()
		{
			_targetingUI.transform.DOKill();
			Tweener tweener = _targetingUI.transform.DOShakePosition(0.18f, new Vector3(1f, 0f, 0f));
			tweener.onComplete = (TweenCallback)Delegate.Combine(tweener.onComplete, new TweenCallback(ResetLockedTargetUIPosition));
		}

		private void ResetLockedTargetUIPosition()
		{
			_targetingUI.transform.position = _targetingUIInitialPosition;
		}

		public void PlaySpawningAnimation()
		{
			_cbOnSpawnAnimationEnd?.Invoke();
		}

		protected virtual IEnumerator PlayDamagedAnimation(long damage = 1L, float audioScale = 1f, EnemyDamageSource damageSource = EnemyDamageSource.Unspecified)
		{
			CurrentlyActing = true;
			queuedAnimations.Enqueue(AnimationTypes.DAMAGED);
			string trigger;
			if (CurrentHealth <= 0f)
			{
				EffectsOnDeath();
				PlayAnimation("death");
				if (_audioSource != null)
				{
					_audioSource.PlayOneShot(_enemySfx.GetRandomEnemyDeadClip(), audioScale);
				}
				trigger = "death";
			}
			else
			{
				if (!supressHitAnimations && (float)damage > hitAnimationDamageThreshold)
				{
					PlayAnimation("damaged");
				}
				if (_audioSource != null && _enemySfx != null)
				{
					_audioSource.PlayOneShot(_enemySfx.GetRandomEnemyDamagedClip(), audioScale);
				}
				trigger = "damaged";
			}
			_animationHasFinished = !AnimatorUtils.HasParameter(_anim, trigger);
			yield return WaitObjectAllocator.waitForEndOfFrame;
			float timeSinceDeathStarted = 0f;
			while (!_animationHasFinished && timeSinceDeathStarted < 3f)
			{
				timeSinceDeathStarted += Time.deltaTime;
				yield return null;
			}
			if (timeSinceDeathStarted >= 3f)
			{
				Debug.LogWarning($"Enemy {base.gameObject.name} {trigger} animation timed out after {timeSinceDeathStarted}s");
			}
			while (_audioSource.isPlaying && timeSinceDeathStarted < 1.5f)
			{
				timeSinceDeathStarted += Time.deltaTime;
				yield return null;
			}
			_ = timeSinceDeathStarted;
			_ = 3f;
			if (queuedAnimations.Count > 0)
			{
				queuedAnimations.Dequeue();
			}
			else
			{
				Debug.Log("Found no animations to dequeue from Damaged Animation");
			}
			if (CurrentHealth <= 0f && !ignoreKillDataAndLootDrops)
			{
				if (AchievementManager.AchievementsOn && !_lootRolled && AssetLoading.Instance.EnemyDataTable.TryGetValue(locKey, out var value))
				{
					_lootRolled = true;
					if (value != null)
					{
						float num = UnityEngine.Random.Range(0f, 1f);
						if (num < value.CardDropRate + (float)_cruciballManager.currentCruciballLevel * 5E-05f)
						{
							bool flag = MapController.instance != null && MapController.instance.HasEnemyDroppedCardThisBattle(locKey);
							OnEnemyCardDropped?.Invoke(locKey);
							AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.CARD_DROP_OBTAINED);
							if (SettingsManager.Instance.InstantLootAndCardDrops)
							{
								LootDropAnimation.OnLootDropAnimationFinished?.Invoke(100);
								if (value.CardDrop == 0 && !flag)
								{
									CardDropAnimation.OnCardAchievementShouldPop?.Invoke(value);
								}
							}
							else
							{
								CardDropAnimation component = UnityEngine.Object.Instantiate(enemyManager.cardDropAnimation).GetComponent<CardDropAnimation>();
								component.transform.position = base.transform.position;
								component.SetupAnimation(enemyManager.playerPosition, this, value, value.CardDrop == 0 && !flag);
								queuedAnimations.Enqueue(AnimationTypes.LOOT_DROP);
								_externalAnimationHasFinished = false;
								while (!_externalAnimationHasFinished)
								{
									yield return WaitObjectAllocator.waitForEndOfFrame;
								}
								if (queuedAnimations.Count > 0)
								{
									queuedAnimations.Dequeue();
								}
								else
								{
									Debug.Log("Found no animations to dequeue from Damaged Animation");
								}
							}
						}
						else if (num < value.LootDropRate + (float)_cruciballManager.currentCruciballLevel * 0.001f)
						{
							OnEnemyLootDropped?.Invoke(locKey);
							if (SettingsManager.Instance.InstantLootAndCardDrops)
							{
								LootDropAnimation.OnLootDropAnimationFinished?.Invoke(value.PointsOnLoot);
							}
							else
							{
								LootDropAnimation lootDropObject = value.GetLootDropObject();
								lootDropObject.transform.position = base.transform.position;
								lootDropObject.DoAnimation(enemyManager.playerPosition, this, value.PointsOnLoot);
								queuedAnimations.Enqueue(AnimationTypes.LOOT_DROP);
								_externalAnimationHasFinished = false;
								while (!_externalAnimationHasFinished)
								{
									yield return WaitObjectAllocator.waitForEndOfFrame;
								}
								if (queuedAnimations.Count > 0)
								{
									queuedAnimations.Dequeue();
								}
								else
								{
									Debug.Log("Found no animations to dequeue from Damaged Animation");
								}
							}
						}
						else
						{
							OnEnemyKilled?.Invoke(locKey);
						}
					}
				}
				OnEnemyDestroyed(this);
			}
			EndAction();
			_cbOnDamagedAnimationEnd?.Invoke(this);
		}

		private void OnPlayerGainedHealth(float healAmount)
		{
			if (HasEffect(StatusEffectType.Lifelink))
			{
				Heal(Mathf.RoundToInt(healAmount * (float)GetEffectIntensity(StatusEffectType.Lifelink)));
			}
		}

		private void OnPlayerSelfDamageTaken(float amountOfSelfDamage)
		{
			if (HasEffect(StatusEffectType.Lifelink))
			{
				Damage(Mathf.RoundToInt(amountOfSelfDamage * (float)GetEffectIntensity(StatusEffectType.Lifelink)), screenshake: true, 1f, 1f, unblockable: false, EnemyDamageSource.RelicChip);
			}
		}

		public void ForceDestruction()
		{
			OnEnemyDestroyed(this);
			if (!_animationHasFinished)
			{
				EndAction();
			}
			_cbOnDamagedAnimationEnd?.Invoke(this);
		}

		protected void EffectsOnDeath()
		{
			if (TryGetComponent<BuffAlliesOnDeath>(out var component))
			{
				int num = ((component.buffAmount > 0) ? component.buffAmount : (GetEffect(StatusEffectType.Strength)?.Intensity ?? 0));
				if (num > 0)
				{
					OnAllEnemiesBuffed(num);
				}
			}
			if (TryGetComponent<CreateOrClaimPegboardShieldPegs>(out var component2))
			{
				component2.OnEnemyDeath();
			}
			if (TryGetComponent<GrantCoinsOnDeath>(out var component3))
			{
				if (component3.coinsToAddDirectly != 0)
				{
					CurrencyManager.Instance.AddGold(component3.coinsToAddDirectly);
				}
				if (component3.coinsToAddToBoard != 0)
				{
					if (_cruciballManager.IsLessGoldAvailable())
					{
						PegManager.AddGoldToPegsRequest(component3.cruciballModCoinsToAddToBoard, base.transform.position);
					}
					else
					{
						PegManager.AddGoldToPegsRequest(component3.coinsToAddToBoard, base.transform.position);
					}
				}
				if (component3.animationToEnable != null && !component3.animationToEnable.activeInHierarchy)
				{
					component3.animationToEnable.SetActive(value: true);
				}
			}
			if (TryGetComponent<LeshyBushBramballIncrementer>(out var component4))
			{
				component4.IncreaseLeshyBossBramballApplicationIntensity();
			}
			if (TryGetComponent<DamageAdjacentEnemiesOnDeath>(out var component5))
			{
				int num2 = ((component5.flatDamage > 0) ? component5.flatDamage : Mathf.RoundToInt(component5.meleeDamageModifier * (DamagePerMeleeAttack + GetDamageBuff())));
				List<Enemy> list = _enemyManager.GetSplashRangeEnemies(_enemyManager.GetSlotIndexForEnemy(this, out var _), _enemyManager.GetSlotForEnemy(this), 1, AoeAttack.AoeType.SIDE).ToList();
				if (list.Count > 0)
				{
					foreach (Enemy item in list)
					{
						if (item != null && item != this)
						{
							item.Damage(num2, screenshake: true, 1f, 1f, unblockable: false, EnemyDamageSource.OtherEnemies, sourceIsPlayer: false);
						}
					}
				}
			}
			if (TryGetComponent<ChandelierFallOnEnemyBelowOnDeath>(out var component6))
			{
				component6.DoFall();
			}
		}

		public void ExternalAnimationHasFinished()
		{
			_externalAnimationHasFinished = true;
		}

		protected virtual void EndAction(bool endTurn = true)
		{
			CurrentlyActing = false;
		}

		public void AnimationHasFinished()
		{
			_animationHasFinished = true;
		}

		public virtual void MeleeAttackMadeContact()
		{
			_meleeAttackMadeContact = true;
		}

		public void RegisterOnSpawnAnimationEnd(Action cb)
		{
			_cbOnSpawnAnimationEnd = (Action)Delegate.Combine(_cbOnSpawnAnimationEnd, cb);
		}

		public void UnregisterOnSpawnAnimationEnd(Action cb)
		{
			_cbOnSpawnAnimationEnd = (Action)Delegate.Remove(_cbOnSpawnAnimationEnd, cb);
		}

		public void RegisterOnDamagedAnimationEnd(Action<Enemy> cb)
		{
			_cbOnDamagedAnimationEnd = (Action<Enemy>)Delegate.Combine(_cbOnDamagedAnimationEnd, cb);
		}

		public void UnregisterOnDamagedAnimationEnd(Action<Enemy> cb)
		{
			_cbOnDamagedAnimationEnd = (Action<Enemy>)Delegate.Remove(_cbOnDamagedAnimationEnd, cb);
		}

		protected void PlayAnimation(string trigger)
		{
			if (_anim == null)
			{
				_animationHasFinished = true;
			}
			else if (AnimatorUtils.HasParameter(_anim, trigger))
			{
				_animationHasFinished = false;
				_anim.SetTrigger(trigger);
			}
			else
			{
				_animationHasFinished = true;
			}
		}

		public void FadeOutCanvas(float fadeDuration)
		{
			CanvasGroup componentInChildren = GetComponentInChildren<CanvasGroup>();
			if (componentInChildren != null)
			{
				componentInChildren.DOFade(0f, fadeDuration);
			}
		}

		public virtual void DoEndOfTurnCleanup()
		{
		}

		public void DestroyLifelinkRenderer()
		{
			if (_lineRenderer != null)
			{
				UnityEngine.Object.Destroy(_lineRenderer);
			}
		}

		public void SetDamageToMaxHp(bool newValue, float maxHpMeleeDamage = -1f, float maxHpRangedDamage = -1f)
		{
			damagesMaxHp = newValue;
			if (damagesMaxHp)
			{
				_prevMeleeAttackDamage = DamagePerMeleeAttack;
				_prevRangedAttackDamage = DamagePerRangedAttack;
				if (maxHpMeleeDamage != -1f)
				{
					DamagePerMeleeAttack = maxHpMeleeDamage;
				}
				else
				{
					Debug.LogWarning("Melee max HP damage not set.");
				}
				if (maxHpRangedDamage != -1f)
				{
					DamagePerRangedAttack = maxHpRangedDamage;
				}
				else
				{
					Debug.LogWarning("Ranged max HP damage not set.");
				}
			}
			else
			{
				DamagePerMeleeAttack = _prevMeleeAttackDamage;
				DamagePerRangedAttack = _prevRangedAttackDamage;
			}
		}
	}
}
