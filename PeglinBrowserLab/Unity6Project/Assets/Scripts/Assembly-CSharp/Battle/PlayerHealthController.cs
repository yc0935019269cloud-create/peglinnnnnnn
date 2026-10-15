using System;
using System.Collections;
using System.Globalization;
using Battle.Enemies;
using Battle.Enemies.Behaviours;
using Battle.Enemies.Polish;
using Battle.StatusEffects;
using Cruciball;
using Currency;
using DG.Tweening;
using I2.Loc;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUtils;
using Relics;
using Saving;
using TMPro;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Battle
{
	[RequireComponent(typeof(PlayerStatusEffectController))]
	public class PlayerHealthController : MonoBehaviour
	{
		public enum DamageSource
		{
			UNSPECIFIED = 0,
			ORB_SELF = 1,
			OTHER_SELF = 2,
			ENEMY_ATTACK = 3,
			RIGGED_BOMB = 4,
			PEG = 5,
			STATUS_EFFECT = 6
		}

		public delegate void PlayerHealthEvent(float healAmount);

		public delegate void PlayerHealthEventWithSource(float amount, DamageSource source);

		public delegate void HealthEvent();

		public class PlayerStatsSaveData : SaveObjectData
		{
			public static readonly string KEY = "PlayerHealthController";

			[SerializeField]
			private float _playerHealth;

			[SerializeField]
			private float _playerMaxHealth;

			[SerializeField]
			private float _reductionToPlayerMaxHealth;

			[SerializeField]
			private SerializableRandomState _damageEvasionRandomState;

			public override string Name => KEY;

			public float PlayerHealth
			{
				get
				{
					return _playerHealth;
				}
				protected set
				{
					_playerHealth = value;
				}
			}

			public float PlayerMaxHealth
			{
				get
				{
					return _playerMaxHealth;
				}
				protected set
				{
					_playerMaxHealth = value;
				}
			}

			public float ReductionToPlayerMaxHealth
			{
				get
				{
					return _reductionToPlayerMaxHealth;
				}
				protected set
				{
					_reductionToPlayerMaxHealth = value;
				}
			}

			public SerializableRandomState DamageEvasionRandomState
			{
				get
				{
					return _damageEvasionRandomState;
				}
				protected set
				{
					_damageEvasionRandomState = value;
				}
			}

			public PlayerStatsSaveData(float playerHealth, float playerMaxHealth, SerializableRandomState damageEvasionRandomState, float reductionToMaxHP)
				: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
			{
				PlayerHealth = playerHealth;
				PlayerMaxHealth = playerMaxHealth;
				_damageEvasionRandomState = damageEvasionRandomState;
				ReductionToPlayerMaxHealth = reductionToMaxHP;
			}
		}

		public static PlayerHealthEvent OnExternalHealRequested = (float healAmount) =>
		{
		};

		public static PlayerHealthEvent OnExternalPercentageHealRequested = (float healPercent) =>
		{
		};

		public static PlayerHealthEvent OnExternalMaxHealthAdjustmentRequested = (float maxHealthAdjustment) =>
		{
		};

		public static PlayerHealthEvent OnPlayerMaxHealthChanged = (float amount) =>
		{
		};

		public static PlayerHealthEvent OnPlayerHealed = (float healAmount) =>
		{
		};

		public static PlayerHealthEvent OnPlayerGainedHealth = (float healthGained) =>
		{
		};

		public static PlayerHealthEvent OnPlayerDamaged = (float amount) =>
		{
		};

		public static PlayerHealthEvent OnArmourHit = (float amount) =>
		{
		};

		public static PlayerHealthEvent OnDodge = (float amount) =>
		{
		};

		public static PlayerHealthEvent OnMiss = (float amount) =>
		{
		};

		public static PlayerHealthEvent OnPlayerSelfDamageTaken = (float amount) =>
		{
		};

		public static PlayerHealthEventWithSource OnExternalDamageRequested = (float amount, DamageSource source) =>
		{
		};

		public static HealthEvent OnHealthDepleted = () =>
		{
		};

		public static Action OnDefeat = () =>
		{
		};

		[Header("Player Stats")]
		[SerializeField]
		private FloatVariable _playerHealth;

		[SerializeField]
		private FloatVariable _maxPlayerHealth;

		[SerializeField]
		private FloatVariable _reductionToMaxPlayerHealth;

		[Header("Scene Obj Refs")]
		public GameObject GameOverObj;

		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private GameObject _damageFloatingTextPrefab;

		[SerializeField]
		private GameObject _lootTextPrefab;

		[SerializeField]
		private Vector3 _damageFloatingTextStartPos = new Vector3(0.872f, 0.17f);

		[SerializeField]
		private float _floatingTextOffset = 5f;

		[SerializeField]
		private TextMeshProUGUI _healthText;

		[SerializeField]
		private UpdateSlider _barScript;

		[SerializeField]
		private AudioSource _sfxSource;

		[SerializeField]
		private AudioClip _healSFX;

		private Camera _camera;

		private ScreenShaker _screenShaker;

		private Canvas _playerCanvas;

		private Enemy _minesBoss;

		private BattleController _battleController;

		private Vignette _vignette;

		private float _currentVignetteTarget;

		private float _nextVignetteTarget;

		[Header("ScreenShake")]
		public float screenShakeDurationMin = 0.7f;

		public float screenShakeDurationMax = 1.1f;

		public float screenShakeIntensityBase = 1f;

		public float screenShakeIntensityMax = 2.5f;

		public float screenShakeIntensityReference = 50f;

		[Header("Damage Vignette")]
		[SerializeField]
		private float _vignetteMinOffset = 0.1f;

		private float _currentVignetteMin;

		[SerializeField]
		private float _vignettePulseSpeed = 0.25f;

		[SerializeField]
		private float _maxVignetteIntensity = 0.42f;

		private float _vignetteIntensitySetting = 1f;

		[Header("Particles etc")]
		[SerializeField]
		private GameObject HealParticleAnim;

		[Tooltip("Don't show for heals less than this amount.")]
		[SerializeField]
		private float HealAnimThreshold = 2f;

		[Header("Misc Data")]
		[SerializeField]
		private PlayerSfx _playerSfx;

		[SerializeField]
		private Color _damageColor = Color.red;

		[SerializeField]
		private Color _neutralColor = Color.white;

		[SerializeField]
		private Color _healColor = Color.green;

		[SerializeField]
		private Color _maxHPLossColor;

		private float _baseEndOfBattleHealPercent = 0.2f;

		private float _armour;

		private PlayerStatusEffectController _statusEffectController;

		private Sequence _maxHPBlackColorSequence;

		private float _lastSourceOfDamage;

		private float _nextToLastSourceOfDamage;

		private float _totalDamageTakenThisBattle;

		private bool _immuneFromReload;

		private static readonly int Dead = Animator.StringToHash("dead");

		public static SerializableRandomState DamageEvasionRandomState = null;

		private int _reloadCount;

		[HideInInspector]
		public bool damageReductionSlimeUsedThisTurn;

		public float CurrentHealth => _playerHealth.Value;

		public float MaxHealth => _maxPlayerHealth.Value;

		public float playerHealthPercent => _playerHealth.Value / _maxPlayerHealth.Value;

		public float LastSourceOfDamage
		{
			get
			{
				return _lastSourceOfDamage;
			}
			set
			{
				_lastSourceOfDamage = value;
			}
		}

		public float NextToLastSourceOfDamage
		{
			get
			{
				return _nextToLastSourceOfDamage;
			}
			set
			{
				_nextToLastSourceOfDamage = value;
			}
		}

		public float totalDamageTakenThisBattle => _totalDamageTakenThisBattle;

		public float endOfBattleHealPercent
		{
			get
			{
				float num = _cruciballManager.GetModifiedPostBattleHeal(_baseEndOfBattleHealPercent);
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_END_BATTLE_HEAL))
				{
					num += (float)DeckManager.completeDeck.Count * 0.01f;
				}
				return num;
			}
		}

		public int endOfBattleHealAmount => Mathf.Min(Mathf.RoundToInt(_maxPlayerHealth.Value - _playerHealth.Value), Mathf.RoundToInt(_maxPlayerHealth.Value * endOfBattleHealPercent));

		private void OnEnable()
		{
			if (_playerCanvas == null)
			{
				_playerCanvas = GetComponentInChildren<Canvas>();
			}
			_camera = Camera.main;
			_screenShaker = _camera.GetComponent<ScreenShaker>();
			if (_vignette == null && _camera.TryGetComponent<Volume>(out var component))
			{
				component.profile.TryGet<Vignette>(out _vignette);
				_vignetteIntensitySetting = ((SettingsManager.Instance != null) ? SettingsManager.VignetteLevelToIntensity(SettingsManager.Instance.VignetteIntensity) : 1f);
			}
			if (_playerHealth.Value == 0f && _maxPlayerHealth.Value == 0f)
			{
				_playerHealth.Reset();
				_maxPlayerHealth.Reset();
				if (_reductionToMaxPlayerHealth != null)
				{
					_reductionToMaxPlayerHealth.Reset();
				}
			}
			if (_playerHealth.Value < _maxPlayerHealth.Value)
			{
				CheckForDeathAndUpdateBar();
			}
			if (_statusEffectController == null)
			{
				_statusEffectController = GetComponent<PlayerStatusEffectController>();
			}
			OnExternalDamageRequested = (PlayerHealthEventWithSource)Delegate.Combine(OnExternalDamageRequested, new PlayerHealthEventWithSource(ExternalDamage));
			OnExternalHealRequested = (PlayerHealthEvent)Delegate.Combine(OnExternalHealRequested, new PlayerHealthEvent(ExternalHeal));
			OnExternalPercentageHealRequested = (PlayerHealthEvent)Delegate.Combine(OnExternalPercentageHealRequested, new PlayerHealthEvent(ExternalPercentageHeal));
			OnExternalMaxHealthAdjustmentRequested = (PlayerHealthEvent)Delegate.Combine(OnExternalMaxHealthAdjustmentRequested, new PlayerHealthEvent(AdjustMaxHealth));
			RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(HandleRelicAdded));
			DeckManager.onDeckShuffled = (DeckManager.Shuffled)Delegate.Combine(DeckManager.onDeckShuffled, new DeckManager.Shuffled(CheckForRelicOnReload));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			BattleController.OnReloadStarted = (BattleController.ReloadStarted)Delegate.Combine(BattleController.OnReloadStarted, new BattleController.ReloadStarted(CheckForRelicOnReloadStart));
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Combine(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckForRelicOnRefreshPotion));
			BattleController.OnVictory = (Action)Delegate.Combine(BattleController.OnVictory, new Action(CheckForBallwarkAchievement));
			LootDropAnimation.OnLootDropAnimationFinished = (LootDropAnimation.LootDropAnimationFinished)Delegate.Combine(LootDropAnimation.OnLootDropAnimationFinished, new LootDropAnimation.LootDropAnimationFinished(ShowLootDropText));
			FloatVariable playerHealth = _playerHealth;
			playerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(playerHealth.onValueChanged, new FloatVariable.ValueChanged(PlayerHealthChanged));
			FloatVariable maxPlayerHealth = _maxPlayerHealth;
			maxPlayerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(maxPlayerHealth.onValueChanged, new FloatVariable.ValueChanged(MaxHealthChanged));
			Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Combine(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(Damage));
			if (_healthText != null)
			{
				_healthText.text = _playerHealth.Value + "/" + _maxPlayerHealth.Value;
			}
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckForSameTimeDeathAchievement));
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Combine(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(HandleOnCoinAddedRelics));
			_totalDamageTakenThisBattle = 0f;
			UpdateHpStats();
		}

		private void CheckForSameTimeDeathAchievement(Enemy enemy)
		{
			if ((bool)enemy.GetComponent<CheckForPlayerDeathOnDeath>())
			{
				_minesBoss = enemy;
			}
		}

		private void ShowLootDropText(int lootXp)
		{
			_sfxSource.PlayOneShot(_playerSfx.LootRewardSfx);
			DisplayLootText(string.Format("+{0} {1}", lootXp, LocalizationManager.GetTranslation("Menu/battle_research_popup")));
		}

		private void UpdateHpStats()
		{
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.maxHp = Mathf.RoundToInt(_maxPlayerHealth.Value);
				StaticGameData.CurrentRunStats.finalHp = Mathf.RoundToInt(_playerHealth.Value);
			}
		}

		private void OnDisable()
		{
			OnExternalDamageRequested = (PlayerHealthEventWithSource)Delegate.Remove(OnExternalDamageRequested, new PlayerHealthEventWithSource(ExternalDamage));
			OnExternalHealRequested = (PlayerHealthEvent)Delegate.Remove(OnExternalHealRequested, new PlayerHealthEvent(ExternalHeal));
			OnExternalPercentageHealRequested = (PlayerHealthEvent)Delegate.Remove(OnExternalPercentageHealRequested, new PlayerHealthEvent(ExternalPercentageHeal));
			OnExternalMaxHealthAdjustmentRequested = (PlayerHealthEvent)Delegate.Remove(OnExternalMaxHealthAdjustmentRequested, new PlayerHealthEvent(AdjustMaxHealth));
			RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(HandleRelicAdded));
			DeckManager.onDeckShuffled = (DeckManager.Shuffled)Delegate.Remove(DeckManager.onDeckShuffled, new DeckManager.Shuffled(CheckForRelicOnReload));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			BattleController.OnReloadStarted = (BattleController.ReloadStarted)Delegate.Remove(BattleController.OnReloadStarted, new BattleController.ReloadStarted(CheckForRelicOnReloadStart));
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckForRelicOnRefreshPotion));
			BattleController.OnVictory = (Action)Delegate.Remove(BattleController.OnVictory, new Action(CheckForBallwarkAchievement));
			LootDropAnimation.OnLootDropAnimationFinished = (LootDropAnimation.LootDropAnimationFinished)Delegate.Remove(LootDropAnimation.OnLootDropAnimationFinished, new LootDropAnimation.LootDropAnimationFinished(ShowLootDropText));
			FloatVariable playerHealth = _playerHealth;
			playerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(playerHealth.onValueChanged, new FloatVariable.ValueChanged(PlayerHealthChanged));
			FloatVariable maxPlayerHealth = _maxPlayerHealth;
			maxPlayerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(maxPlayerHealth.onValueChanged, new FloatVariable.ValueChanged(MaxHealthChanged));
			Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Remove(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(Damage));
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(CheckForSameTimeDeathAchievement));
			CurrencyManager.OnGoldAdded = (CurrencyManager.CurrencyEvent)Delegate.Remove(CurrencyManager.OnGoldAdded, new CurrencyManager.CurrencyEvent(HandleOnCoinAddedRelics));
		}

		private void Update()
		{
			if (_vignetteIntensitySetting > 0f && _vignette != null && BattleController.BattleActive)
			{
				float num = (Mathf.Sin(Time.unscaledTime * _vignettePulseSpeed) + 1f) * 0.5f;
				if (num < 0.005f && _currentVignetteTarget != _nextVignetteTarget)
				{
					_currentVignetteTarget = _nextVignetteTarget;
				}
				else if (num > 0.995f)
				{
					_currentVignetteMin = Mathf.Max(_nextVignetteTarget - _vignetteMinOffset, 0f);
				}
				_vignette.intensity.value = (_currentVignetteMin + num * (_currentVignetteTarget - _currentVignetteMin)) * _vignetteIntensitySetting;
			}
		}

		private void DisplayFloatingText(string text, Color color, Vector3 pos)
		{
			FloatingText component = UnityEngine.Object.Instantiate(_damageFloatingTextPrefab, pos, Quaternion.identity, _playerCanvas.transform).GetComponent<FloatingText>();
			component.transform.position += new Vector3(UnityEngine.Random.Range(0f - _floatingTextOffset, _floatingTextOffset), 0f - _floatingTextOffset, _floatingTextOffset);
			component.SetColor(color);
			component.Activate(text, shouldDie: true);
		}

		private void DisplayLootText(string text)
		{
			FloatingText component = UnityEngine.Object.Instantiate(_lootTextPrefab, base.transform.position + _damageFloatingTextStartPos, Quaternion.identity, _playerCanvas.transform).GetComponent<FloatingText>();
			component.transform.position += new Vector3(UnityEngine.Random.Range(0f - _floatingTextOffset, _floatingTextOffset), 0f - _floatingTextOffset, _floatingTextOffset);
			component.SetColor(Color.white);
			component.Activate(text, shouldDie: true);
		}

		private void DoScreenshake(float damage)
		{
			if (_screenShaker == null || !_screenShaker.blocked)
			{
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				float num2 = damage / screenShakeIntensityReference * num;
				float duration = Mathf.Lerp(screenShakeDurationMin, screenShakeDurationMax, num2);
				_camera.DOShakePosition(duration, Mathf.Min(screenShakeIntensityBase * num2, screenShakeIntensityMax));
			}
		}

		private void DisplayFloatingMissText(float damage = 0f)
		{
			FloatingText component = UnityEngine.Object.Instantiate(_damageFloatingTextPrefab, base.transform.position + _damageFloatingTextStartPos, Quaternion.identity, _playerCanvas.transform).GetComponent<FloatingText>();
			component.transform.position += new Vector3(UnityEngine.Random.Range(0f - _floatingTextOffset, _floatingTextOffset), 0f - _floatingTextOffset, _floatingTextOffset);
			component.SetColor(_neutralColor);
			string text = (Mathf.Approximately(damage, 0f) ? LocalizationManager.GetTranslation("miss_text") : ((int)damage).ToString(CultureInfo.InvariantCulture));
			component.Activate(text, shouldDie: true);
		}

		public void Damage(float damage, bool melee = true, Enemy enemy = null, DamageSource source = DamageSource.UNSPECIFIED, bool forceMaxHPDamage = false)
		{
			bool flag = forceMaxHPDamage;
			if (!forceMaxHPDamage && enemy != null)
			{
				flag = enemy.damagesMaxHp;
			}
			if (enemy != null && enemy.UnblockableAttacks)
			{
				DealUnblockableDamage(damage, displayFloatingDamageText: true, instakill: false, flag);
				return;
			}
			float currentHealth = CurrentHealth;
			if (damage == 0f)
			{
				TriggerDodgeEffects();
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.IMMORTAL) || (source == DamageSource.ENEMY_ATTACK && _immuneFromReload && _relicManager.AttemptUseRelic(RelicEffect.NO_DAMAGE_ON_RELOAD)))
			{
				damage = 0f;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.HALVE_INCOMING_DAMAGE))
			{
				damage = Mathf.Floor(damage / 2f);
			}
			bool flag2 = false;
			if (damage > 0f)
			{
				flag2 = CheckBallusion(source);
				if (flag2)
				{
					if (StaticGameData.CurrentRunStats != null)
					{
						StaticGameData.CurrentRunStats.totalDamageNegated += Mathf.RoundToInt(damage);
					}
					damage = 0f;
					if (enemy != null && _relicManager.AttemptUseRelic(RelicEffect.EVADE_COUNTER))
					{
						long damage2 = _statusEffectController.totalStatusIntensityGainedThisBattle + (_battleController?.enemyManager?.statusEffectIntensityAppliedToEnemiesThisBattle).GetValueOrDefault();
						enemy.Damage(damage2, screenshake: true, 1f, 1f, unblockable: false, Enemy.EnemyDamageSource.RelicLarge);
					}
				}
				if (enemy != null)
				{
					enemy.lastAttackDodged = flag2;
				}
			}
			float num = _statusEffectController.EffectStrength(StatusEffectType.Intangiball);
			if (num > 0f)
			{
				damage = Mathf.Min(damage, num);
			}
			_totalDamageTakenThisBattle += damage;
			_armour = _statusEffectController.EffectStrength(StatusEffectType.Ballwark);
			float num2 = damage;
			float num3 = 0f;
			if (_armour > 0f && source != DamageSource.STATUS_EFFECT && !flag)
			{
				if (_armour >= damage)
				{
					num3 = damage;
					num2 = 0f;
				}
				else
				{
					num2 = damage - _armour;
					num3 = _armour;
				}
			}
			if (num2 > 0f && _relicManager.AttemptUseRelic(RelicEffect.REDUCE_LOST_HEALTH))
			{
				num2--;
			}
			float damageReductionFromSlimeAmount = GetDamageReductionFromSlimeAmount();
			if (num2 > 0f && damageReductionFromSlimeAmount > 0f)
			{
				num2 -= damageReductionFromSlimeAmount;
				damageReductionSlimeUsedThisTurn = true;
			}
			num2 = Mathf.Max(0f, num2);
			if (BattleController.BattleActive && num2 > 0f && _relicManager.AttemptUseRelic(RelicEffect.PREVENT_FIRST_DAMAGE))
			{
				num2 = 0f;
			}
			if (num2 >= _playerHealth.Value && _relicManager.AttemptUseRelic(RelicEffect.PREVENT_LETHAL_DAMAGE))
			{
				num2 = 0f;
				num3 = 0f;
				if (!_relicManager.RelicEffectActive(RelicEffect.HALVE_INCOMING_DAMAGE))
				{
					_relicManager.AddRelic(_relicManager.GetRelicForEffectFromAllData(RelicEffect.HALVE_INCOMING_DAMAGE));
				}
			}
			if (num3 > 0f)
			{
				if (num3 >= _armour)
				{
					_armour = 0f;
				}
				else
				{
					_armour -= num3;
				}
				_statusEffectController.UpdateStatusEffect(new StatusEffect(StatusEffectType.Ballwark, Mathf.RoundToInt(_armour)));
				if (_armour > 0f && enemy != null && source == DamageSource.ENEMY_ATTACK && _relicManager.AttemptUseRelic(RelicEffect.BALLWARK_COUNTER))
				{
					enemy.Damage(Mathf.RoundToInt(_armour * 5f), screenshake: false);
				}
			}
			if (num3 > 0f)
			{
				DisplayFloatingMissText(num3);
			}
			if (num2 > 0f)
			{
				if (flag)
				{
					_maxPlayerHealth.Subtract((int)num2);
					_reductionToMaxPlayerHealth.Add((int)num2);
					OnPlayerMaxHealthChanged?.Invoke(0f - num2);
					if (_playerHealth.Value > _maxPlayerHealth.Value)
					{
						_playerHealth.Subtract(_playerHealth.Value - _maxPlayerHealth.Value);
					}
				}
				else
				{
					_playerHealth.Subtract((int)num2);
				}
				CheckOnDamagedEffects(num2);
				string text = ((int)num2).ToString();
				Color color = _damageColor;
				Vector3 pos = base.transform.position + _damageFloatingTextStartPos;
				if (flag)
				{
					text = "-" + (int)num2;
					color = _maxHPLossColor;
					pos = _barScript.maxHPReductionRect.transform.position;
					DoMaxHPLossBlackFlash();
				}
				DisplayFloatingText(text, color, pos);
				DoScreenshake(num2);
				ControllerVibration.DoPlayerHitVibration(num2, MaxHealth);
				OnPlayerDamaged?.Invoke(num2);
			}
			if (num3 > 0f && StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.totalDamageNegated += Mathf.RoundToInt(num3);
			}
			if (_playerHealth.Value < 0f && enemy != null && StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.defeatedBy = enemy.name;
				StaticGameData.CurrentRunStats.finalHp = 0;
				StaticGameData.CurrentRunStats.maxHp = Mathf.RoundToInt(_maxPlayerHealth.Value);
				StaticGameData.CurrentRunStats.endDate = DateTime.Now;
			}
			_nextToLastSourceOfDamage = _lastSourceOfDamage;
			_lastSourceOfDamage = num2 + num3;
			if (enemy != null)
			{
				enemy.playerHealthLostFromLastAttack = num2;
			}
			if (num3 > 0f && num2 <= 0f)
			{
				_sfxSource.PlayOneShot(_playerSfx.ArmourBlockSfx);
				OnArmourHit?.Invoke(num3);
				if (_armour == 0f && _relicManager.AttemptUseRelic(RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE))
				{
					_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance), StatusEffectSource.PLAYER);
				}
			}
			else if (num3 > 0f && num2 > 0f)
			{
				_sfxSource.PlayOneShot(_playerSfx.ArmourBreakSfx);
				if (_relicManager.AttemptUseRelic(RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE))
				{
					_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance), StatusEffectSource.PLAYER);
				}
			}
			else if (num2 > 0f && source == DamageSource.STATUS_EFFECT)
			{
				_sfxSource.PlayOneShot(_playerSfx.PlayerPoisonHitSfx);
			}
			else if (num2 > 0f)
			{
				if (flag)
				{
					_sfxSource.PlayOneShot(_playerSfx.PlayerHitMaxHPSFX);
				}
				else
				{
					_sfxSource.PlayOneShot(_playerSfx.PlayerHitSfx);
				}
			}
			else
			{
				if (flag2)
				{
					DisplayFloatingText(LocalizationManager.GetTranslation("dodge_text"), _neutralColor, base.transform.position + _damageFloatingTextStartPos);
				}
				else
				{
					DisplayFloatingText(LocalizationManager.GetTranslation("miss_text"), _neutralColor, base.transform.position + _damageFloatingTextStartPos);
				}
				_sfxSource.PlayOneShot(_playerSfx.PlayerDodgeSfx);
			}
			if (num2 > 0f)
			{
				if (StaticGameData.CurrentRunStats != null && enemy != null)
				{
					StaticGameData.CurrentRunStats.AddDamageForEnemy(enemy.enemyName, num2, melee);
				}
				CheckForRelicEffectsOnDamage(num2, source);
				if (currentHealth > 0f)
				{
					CheckForDeathAndUpdateBar();
				}
			}
			UpdateHpStats();
		}

		private void DoMaxHPLossBlackFlash()
		{
			SpriteRenderer component = GetComponent<SpriteRenderer>();
			_maxHPBlackColorSequence.Kill();
			_maxHPBlackColorSequence = DOTween.Sequence();
			_maxHPBlackColorSequence.Append(component.DOColor(Color.black, 0.05f).SetEase(Ease.OutQuad));
			_maxHPBlackColorSequence.Append(component.DOColor(Color.white, 2f).SetEase(Ease.OutQuad).SetDelay(0.3f));
		}

		private void CheckForRelicEffectsOnDamage(float lostHealth, DamageSource source)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.GAIN_BALLUSION_FROM_ENEMY_DMG))
			{
				_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, (int)lostHealth * 2), StatusEffectSource.PLAYER);
			}
			if (_statusEffectController != null && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_CREATES_DAMAGE_REDUCTION_SLIME))
			{
				_statusEffectController.QueueDamageReductionSlime(1);
			}
		}

		private bool CheckBallusion(DamageSource source)
		{
			if (source == DamageSource.ORB_SELF || source == DamageSource.OTHER_SELF || source == DamageSource.RIGGED_BOMB || source == DamageSource.PEG || source == DamageSource.STATUS_EFFECT)
			{
				return false;
			}
			float num = _statusEffectController.EffectStrength(StatusEffectType.Ballusion);
			if (num <= 0f)
			{
				return false;
			}
			bool result = false;
			float num2 = ((DamageEvasionRandomState == null) ? UnityEngine.Random.value : DamageEvasionRandomState.GetRandomValue());
			if (num2 < num * StatusEffectData.BALLUSION_EVASION_PER_STACK)
			{
				float num3 = StatusEffectData.BALLUSION_REDUCTION_PERCENTAGE;
				if (_relicManager.RelicEffectActive(RelicEffect.BALLUSION_DOUBLE_MAX) || _relicManager.RelicEffectActive(RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES))
				{
					num3 -= 0.1f;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.BALLUSION_DOUBLE_GAIN_AND_LOSS))
				{
					num3 *= 2f;
				}
				float num4 = Mathf.Clamp(1f - num3, 0f, 1f);
				int intensity = Mathf.RoundToInt(num * num4);
				_statusEffectController.UpdateStatusEffect(new StatusEffect(StatusEffectType.Ballusion, intensity));
				TriggerDodgeEffects();
				result = true;
				OnDodge?.Invoke(0f);
			}
			return result;
		}

		private void TriggerDodgeEffects()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.DODGE_PUPPET))
			{
				_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, 100), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.SPINESSE_WHEN_DODGING))
			{
				_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Finesse), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.BALLUSION_ON_DODGE))
			{
				_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, 13), StatusEffectSource.PLAYER);
			}
			if (BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION && _relicManager.AttemptUseRelic(RelicEffect.BALLUSION_DODGE_CREATE_CRIT))
			{
				_statusEffectController.CreateTemporaryCritPegs(2);
			}
		}

		private float GetDamageReductionFromSlimeAmount()
		{
			if (_battleController != null)
			{
				return _battleController.CountActiveSlimeOnBoard(Peg.SlimeType.DamageReductionSlime, Peg.BUFF_PER_DAMAGE_REDUCTION_INCREASE, Peg.DEBUFF_PER_DAMAGE_REDUCTION_INCREASE);
			}
			return 0f;
		}

		public void DealSelfDamage(float damage, bool blockable = true, DamageSource source = DamageSource.UNSPECIFIED)
		{
			if (source == DamageSource.ORB_SELF && _relicManager.AttemptUseRelic(RelicEffect.REDUCE_ORB_SELF_DAMAGE))
			{
				damage = Mathf.Floor(damage * 0.5f);
			}
			_totalDamageTakenThisBattle += damage;
			if (blockable)
			{
				float num = Mathf.Max(damage - _armour, 0f);
				Damage(damage, melee: true, null, source);
				OnPlayerSelfDamageTaken?.Invoke(num);
				if (num > 0f && _relicManager.AttemptUseRelic(RelicEffect.LOSE_HP_GAIN_BALLWARK))
				{
					_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, Mathf.RoundToInt(num * 2f)), StatusEffectSource.PLAYER);
				}
			}
			else if (!_relicManager.AttemptUseRelic(RelicEffect.IMMORTAL))
			{
				DealUnblockableDamage(damage);
				OnPlayerSelfDamageTaken?.Invoke(damage);
				if (_relicManager.AttemptUseRelic(RelicEffect.LOSE_HP_GAIN_BALLWARK))
				{
					_statusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, Mathf.RoundToInt(damage * 2f)), StatusEffectSource.PLAYER);
				}
			}
		}

		public void AdjustMaxHealth(float amount)
		{
			if (amount > 0f && _relicManager.AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
			{
				amount++;
			}
			_maxPlayerHealth.Add(amount);
			_playerHealth.Add(amount);
			_reductionToMaxPlayerHealth.Subtract(amount);
			OnPlayerMaxHealthChanged?.Invoke(amount);
			if (amount > HealAnimThreshold)
			{
				HealParticleAnim.SetActive(value: true);
			}
		}

		public void PlayMaxHPIncreaseSFX()
		{
			_sfxSource.PlayOneShot(_playerSfx.MaxHPIncreaseSfx);
		}

		private void CheckOnDamagedEffects(float damage)
		{
			if (_playerHealth.Value > 0f && _relicManager.AttemptUseRelic(RelicEffect.DAMAGE_RETURN))
			{
				Enemy.OnAllEnemiesDamaged?.Invoke(Mathf.RoundToInt(damage * RelicManager.DAMAGE_RETURN_MODIFIER));
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.DAMAGE_CREATES_COINS))
			{
				int num = Mathf.FloorToInt(damage * 0.5f);
				if (num > 0)
				{
					PegManager.AddGoldToPegsRequest?.Invoke(num);
				}
			}
			if (damage > 1f && _relicManager.AttemptUseRelic(RelicEffect.LIGHTNING_SLIME_ON_DAMAGE) && _battleController != null && _battleController.pegManager != null && BattleController.BattleActive)
			{
				_battleController.pegManager.ApplySlimeToRandomPegs(Peg.SlimeType.LightningSlime, Mathf.FloorToInt(damage * 0.5f));
			}
		}

		public void DealUnblockableDamage(float damage, bool displayFloatingDamageText = true, bool instakill = false, bool damageToMaxHp = false)
		{
			if (damage >= _playerHealth.Value && _relicManager.AttemptUseRelic(RelicEffect.PREVENT_LETHAL_DAMAGE) && !instakill)
			{
				damage = 0f;
				if (!_relicManager.RelicEffectActive(RelicEffect.HALVE_INCOMING_DAMAGE))
				{
					_relicManager.AddRelic(_relicManager.GetRelicForEffectFromAllData(RelicEffect.HALVE_INCOMING_DAMAGE));
				}
			}
			else if (_relicManager.AttemptUseRelic(RelicEffect.HALVE_INCOMING_DAMAGE) && !instakill && !damageToMaxHp)
			{
				damage = Mathf.Floor(damage / 2f);
			}
			if (BattleController.BattleActive && damage > 0f && _relicManager.AttemptUseRelic(RelicEffect.PREVENT_FIRST_DAMAGE) && !instakill)
			{
				damage = 0f;
			}
			if (damage > 0f)
			{
				_totalDamageTakenThisBattle += damage;
				if (damageToMaxHp)
				{
					_maxPlayerHealth.Subtract((int)damage);
					_reductionToMaxPlayerHealth.Add((int)damage);
					OnPlayerMaxHealthChanged?.Invoke(0f - damage);
					if (_playerHealth.Value > _maxPlayerHealth.Value)
					{
						_playerHealth.Subtract(_playerHealth.Value - _maxPlayerHealth.Value);
					}
				}
				else
				{
					_playerHealth.Subtract((int)damage);
				}
				OnPlayerDamaged(damage);
				if (!instakill)
				{
					CheckOnDamagedEffects(damage);
				}
				else
				{
					_statusEffectController.HideStatusEffectUI();
				}
				if (displayFloatingDamageText)
				{
					string text = ((int)damage).ToString();
					Color color = _damageColor;
					Vector3 pos = base.transform.position + _damageFloatingTextStartPos;
					if (damageToMaxHp)
					{
						text = "-" + (int)damage;
						color = _maxHPLossColor;
						pos = _barScript.maxHPReductionRect.transform.position;
						DoMaxHPLossBlackFlash();
						_sfxSource.PlayOneShot(_playerSfx.PlayerHitMaxHPSFX);
					}
					else
					{
						_sfxSource.PlayOneShot(_playerSfx.PlayerHitSfx);
					}
					DisplayFloatingText(text, color, pos);
					DoScreenshake(damage);
					ControllerVibration.DoPlayerHitVibration(damage, MaxHealth);
				}
				_nextToLastSourceOfDamage = _lastSourceOfDamage;
				_lastSourceOfDamage = damage;
			}
			else
			{
				DisplayFloatingMissText(damage);
				_sfxSource.PlayOneShot(_playerSfx.PlayerDodgeSfx);
			}
			CheckForDeathAndUpdateBar();
			UpdateHpStats();
		}

		public void CheckForDeathAndUpdateBar()
		{
			if (_playerHealth.Value <= 0f)
			{
				_barScript.UpdateSize(0f, MaxHealth, _reductionToMaxPlayerHealth.Value);
				if (GameOverObj != null)
				{
					GameOverObj.SetActive(value: true);
				}
				if (!StaticGameData.hasReachedBoss)
				{
					PlayerPrefs.SetInt(GameInit.BOSS_STREAK_PREF_STRING, 0);
				}
				_healthText.text = _playerHealth.Value + "/" + _maxPlayerHealth.Value;
				GetComponent<Animator>().SetBool(Dead, value: true);
				SteamStatsManager.Instance?.StateChanged(StateChange.DIED);
				UpdateHpStats();
				SaveManager.RequestDelete();
				OnHealthDepleted();
				if (_minesBoss != null && _minesBoss.CurrentHealth <= 0f)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.DIE_WHILE_KILLING_MINES_BOSS);
				}
			}
			else
			{
				if (_playerHealth.Value < _maxPlayerHealth.Value)
				{
					UpdateNextVignetteTarget();
				}
				UpdateHealthBar();
			}
		}

		public void AddArmour(float armour)
		{
			if (!(armour < 1f))
			{
				_armour = Mathf.Min(_armour + armour, StatusEffect.GetMaxIntensityForEffect(StatusEffectType.Ballwark, _relicManager));
			}
		}

		private void UpdateNextVignetteTarget()
		{
			float t = Mathf.Max(0f, 0.9f - _playerHealth.Value / _maxPlayerHealth.Value);
			_nextVignetteTarget = Mathf.Lerp(0f, _maxVignetteIntensity, t);
		}

		private void PlayerHealthChanged(float oldVal, float newVal)
		{
			_relicManager.UpdateHealthDependentRelics();
		}

		private void MaxHealthChanged(float oldVal, float newVal)
		{
			StartCoroutine(WaitAndUpdateHealthBar());
		}

		private IEnumerator WaitAndUpdateHealthBar()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			UpdateHealthBar();
		}

		private void UpdateHealthBar()
		{
			if (!(_barScript == null) || !(_healthText == null))
			{
				_barScript.UpdateSize(_playerHealth.Value, _maxPlayerHealth.Value, _reductionToMaxPlayerHealth.Value);
				_healthText.text = _playerHealth.Value + "/" + _maxPlayerHealth.Value;
			}
		}

		public void HealEndOfBattleAmount()
		{
			Heal(endOfBattleHealAmount);
		}

		public void HealToFull()
		{
			if (_maxPlayerHealth.Value > _playerHealth.Value)
			{
				Heal(_maxPlayerHealth.Value - _playerHealth.Value);
			}
		}

		public void ExternalDamage(float amount, DamageSource source)
		{
			Damage(amount, melee: false, null, source);
		}

		public void ExternalHeal(float amount)
		{
			Heal(amount);
		}

		public void ExternalPercentageHeal(float percent)
		{
			Heal(Mathf.RoundToInt(percent * MaxHealth));
		}

		public float Heal(float amount)
		{
			OnPlayerHealed?.Invoke(amount);
			float num = Mathf.Min(amount, _maxPlayerHealth.Value - _playerHealth.Value);
			if (num <= 0f || _playerHealth.Value <= 0f)
			{
				return 0f;
			}
			if (amount > HealAnimThreshold && HealParticleAnim != null)
			{
				HealParticleAnim.SetActive(value: true);
			}
			else if (amount > HealAnimThreshold && _healSFX != null)
			{
				_sfxSource.PlayOneShot(_healSFX);
			}
			if (_damageFloatingTextPrefab != null)
			{
				FloatingText component = UnityEngine.Object.Instantiate(_damageFloatingTextPrefab, base.transform.position + _damageFloatingTextStartPos, Quaternion.identity, _playerCanvas.transform).GetComponent<FloatingText>();
				component.transform.position += new Vector3(UnityEngine.Random.Range(0f - _floatingTextOffset, _floatingTextOffset), 0f - _floatingTextOffset, _floatingTextOffset);
				component.SetColor(_healColor);
				component.Activate(((int)num).ToString(), shouldDie: true);
			}
			_playerHealth.Add(num);
			UpdateNextVignetteTarget();
			UpdateHealthBar();
			UpdateHpStats();
			if (num > 0f)
			{
				OnPlayerGainedHealth?.Invoke(num);
			}
			return num;
		}

		private void HandleRelicAdded(Relic relic)
		{
			if (relic.effect == RelicEffect.MAX_HEALTH_SMALL || relic.effect == RelicEffect.MAX_HEALTH_MEDIUM || relic.effect == RelicEffect.MAX_HEALTH_LARGE)
			{
				Heal(0f);
			}
		}

		private void CheckForRelicOnReload(int deckSize)
		{
			if (_reloadCount > 0 && _relicManager.AttemptUseRelic(RelicEffect.HEAL_ON_RELOAD))
			{
				Heal(10f);
			}
			_reloadCount++;
		}

		private void CheckForRelicOnReloadStart()
		{
			if (_relicManager.RelicEffectActive(RelicEffect.NO_DAMAGE_ON_RELOAD))
			{
				_immuneFromReload = true;
			}
		}

		public void EndRelicImmunity()
		{
			_immuneFromReload = false;
		}

		private void HandlePegActivated(Peg.PegType pegType, Peg peg)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.HEAL_ON_PEG_HITS))
			{
				Heal(1f);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG))
			{
				DealSelfDamage(RelicManager.SKULL_WAND_SELF_DMG, blockable: true, DamageSource.OTHER_SELF);
			}
		}

		private void HandleOnCoinAddedRelics(int originalAmount, int amountAdded, bool silent)
		{
			_relicManager.DoCoinsProvideHealingRelicsEffect(amountAdded);
		}

		private void CheckForRelicOnRefreshPotion()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.HEAL_ON_REFRESH_POTION))
			{
				Heal(_relicManager.REFRESH_POTION_HEAL_AMOUNT);
			}
		}

		public void ShowSelfDamagePreview(float selfDamage)
		{
			_barScript.ShowSelfDamagePreview(selfDamage, MaxHealth, selfDamage >= CurrentHealth);
		}

		public void HideSelfDamagePreview()
		{
			_barScript.HideSelfDamagePreview();
		}

		private void CheckForBallwarkAchievement()
		{
			if (_armour >= 100f && !AchievementData.Achievements[AchievementData.AchievementId.SHIELD_AMOUNT_AT_END_OF_BATTLE].Unlocked && AchievementManager.AchievementsOn)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.SHIELD_AMOUNT_AT_END_OF_BATTLE]);
				PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Balladin);
			}
		}
	}
}
