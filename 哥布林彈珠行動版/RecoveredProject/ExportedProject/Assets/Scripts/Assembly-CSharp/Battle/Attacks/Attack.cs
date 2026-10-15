using System;
using System.Collections.Generic;
using Battle.Attacks.DamageModifiers;
using Battle.Attacks.Interfaces;
using Battle.Enemies;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Challenges;
using Cruciball;
using I2.Loc;
using NaughtyAttributes;
using PeglinUI;
using Relics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battle.Attacks
{
	public class Attack : MonoBehaviour
	{
		public delegate void AttackHit(Enemy enemy, float damage);

		public delegate void AttackWhiff();

		public delegate void AttackStart(float force);

		public delegate void AttackDamageHitInfinity();

		[Serializable]
		public struct StaticDamageModifierData
		{
			public int baseDamage;

			public int critDamage;

			public float baseDamageMultiplier;

			public float critDamageMultiplier;

			public float baseDamageDivider;

			public float critDamageDivider;

			public float muscircleMultiplier;

			public float spinesseMultiplier;

			public float ballanceMultiplier;

			public StaticDamageModifierData(int BaseDamage, int CritDamage, float BaseDamageMultiplier, float CritDamageMultiplier, float BaseDamageDivider, float CritDamageDivider, float MuscircleMultiplier, float SpinesseMultiplier, float BallanceMultiplier)
			{
				baseDamage = BaseDamage;
				critDamage = CritDamage;
				baseDamageMultiplier = BaseDamageMultiplier;
				critDamageMultiplier = CritDamageMultiplier;
				baseDamageDivider = BaseDamageDivider;
				critDamageDivider = CritDamageDivider;
				muscircleMultiplier = MuscircleMultiplier;
				spinesseMultiplier = SpinesseMultiplier;
				ballanceMultiplier = BallanceMultiplier;
			}
		}

		public AttackHit OnAttackHit;

		public AttackWhiff OnAttackWhiff;

		public AttackStart OnAttackStart;

		public static AttackDamageHitInfinity OnAttackDamageHitInfinity;

		public TargetingType targetingType;

		public GameObject NextLevelPrefab;

		public GameObject PreviousLevelPrefab;

		[Header("Attack Attributes")]
		public string locNameString;

		public string[] locDescStrings;

		[ReadOnly]
		public string locName;

		[Tooltip("Readonly, this is populated from the loc text onValidate")]
		[TextArea]
		public string locDescription;

		public Tooltip.LockedDescriptionPresets lockedDescriptionPreset = Tooltip.LockedDescriptionPresets.ORB_DEFAULT;

		[Tooltip("Alternate locstring to use for 'damage' field - 'healing', for example.")]
		public string AttackLabelOverride = "";

		public string PeglinAttackAnimationTrigger = "attack";

		public int Level = -1;

		public int DamagePerPeg = 5;

		public int CritDamagePerPeg = 8;

		[Tooltip("Remove from the deck for the rest of battle after firing.")]
		public bool ExhaustOnFire;

		[Tooltip("Remove from the deck permanently after firing.")]
		public bool DestroyOnFire;

		public bool DealsMaxHPDamage;

		[HideInInspector]
		public PachinkoBall IsSummonedBy;

		protected Vector2 _playerPosition;

		protected AttackManager _attackManager;

		protected CruciballManager _cruciballManager;

		protected DeckManager _deckManager;

		protected PlayerStatusEffectController _playerStatusEffectController;

		protected PlayerHealthController _playerHealthController;

		protected RelicManager _relicManager;

		protected bool _hideOrbLevel;

		protected bool _checkForPostAttackOperations;

		private int _previousOrbRegularDamageMod;

		private int _previousOrbCritDamageMod;

		[FormerlySerializedAs("id")]
		[HideInInspector]
		public string instanceID;

		[SerializeField]
		[ReadOnly]
		private int _attackId;

		private int _critPegsOnBoardAtShotStart;

		private int _randomDmgMod;

		private int _randomCritMod;

		private bool applyUniqueBuff;

		private AttackBaseDamageModifier[] damageMods;

		private AttackDamageModifiableRules dnm;

		private bool isDNMNull = true;

		private StaticDamageModifierData _currentShotStaticDamageModifierData;

		public bool HasUpgrade
		{
			get
			{
				if (!(NextLevelPrefab != null))
				{
					return GetComponent<UnlimitedUpgrades>() != null;
				}
				return true;
			}
		}

		public string Name => LocalizationManager.GetTranslation("Orbs/" + locNameString + "_name");

		public string LockedName => LocalizationManager.GetTranslation("Orbs/" + locNameString + "_desc_locked");

		public virtual string Description
		{
			get
			{
				string text = "";
				for (int i = 0; i < locDescStrings.Length; i++)
				{
					string text2 = locDescStrings[i];
					if (text2 != null)
					{
						text = text + "<sprite name=\"BULLET\"><indent=8%>" + LocalizationManager.GetTranslation("Orbs/" + text2, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, base.gameObject) + "</indent>\n";
					}
				}
				return text;
			}
		}

		public string EnglishDescription
		{
			get
			{
				string text = "";
				string[] array = locDescStrings;
				foreach (string text2 in array)
				{
					text = text + "<sprite name=\"BULLET\"><indent=8%>" + LocalizationManager.GetTranslation("Orbs/" + text2, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, base.gameObject, "English") + "</indent>\n";
				}
				return text;
			}
		}

		public int FullLevel
		{
			get
			{
				UnlimitedUpgrades component = GetComponent<UnlimitedUpgrades>();
				if (!(component != null))
				{
					return Level;
				}
				return Level + component.timesUpgraded;
			}
		}

		public string LevelAsRomanNumeral
		{
			get
			{
				if (Level <= 0)
				{
					return "";
				}
				string text = "";
				for (int i = 0; i < Level; i++)
				{
					text += "I";
				}
				return text;
			}
		}

		public int PostBattleAddLevelCostMod
		{
			get
			{
				if (Level <= 1)
				{
					return 0;
				}
				return (Level - 1) * 10;
			}
		}

		public bool IsStone
		{
			get
			{
				if (!(locNameString == "stone"))
				{
					return locNameString == "oreb";
				}
				return true;
			}
		}

		public bool IsOreb => locNameString == "oreb";

		public bool IsBoulder => locNameString == "boulder";

		public bool IsOrbelisk => locNameString == "orbelisk";

		public bool IsHorriball => locNameString == "horriball";

		public bool IsEgg => locNameString.Contains("egg");

		public int attackId => _attackId;

		public int randomDmgMod => _randomDmgMod;

		public int randomCritMod => _randomCritMod;

		[Button(null, EButtonEnableMode.Always)]
		protected virtual void CalcLoc()
		{
			ClearBattleParameters();
			locName = Name;
			if (locDescription != null)
			{
				locDescription = Description;
			}
		}

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		public virtual void Initialize(Vector2 playerPosition, AttackManager attackManager, RelicManager relicManager, DeckManager deckManager, CruciballManager cruciballManager, PlayerHealthController healthController, PlayerStatusEffectController statusController)
		{
			damageMods = GetComponents<AttackBaseDamageModifier>();
			dnm = GetComponent<AttackDamageModifiableRules>();
			isDNMNull = dnm == null;
			_playerPosition = playerPosition;
			_attackManager = attackManager;
			_playerStatusEffectController = statusController;
			_playerHealthController = healthController;
			UpdateBoardDetails();
			SoftInit(deckManager, relicManager, cruciballManager, forUI: false);
		}

		public void InitializeInventory()
		{
			damageMods = GetComponents<AttackBaseDamageModifier>();
			dnm = GetComponent<AttackDamageModifiableRules>();
			isDNMNull = dnm == null;
		}

		public void UpdateBoardDetails()
		{
			if (_attackManager != null && _attackManager.pegManager != null)
			{
				_critPegsOnBoardAtShotStart = _attackManager.pegManager.CountSpecialPegsOnBoard(Peg.PegType.CRIT);
			}
		}

		public void SetInstanceId()
		{
			instanceID = locNameString + Level + Guid.NewGuid().ToString();
		}

		public void CloneInstanceId(Attack original)
		{
			instanceID = original.instanceID;
		}

		[Button(null, EButtonEnableMode.Always)]
		public void GenerateId()
		{
			if (_attackId != 0)
			{
				Debug.LogError("Trying to generate an id when it already exists");
			}
			else
			{
				_attackId = UnityEngine.Random.Range(0, int.MaxValue);
			}
		}

		public virtual void SoftInit(DeckManager deckManager, RelicManager relicManager, CruciballManager cruciballManager, bool forUI = true)
		{
			_cruciballManager = cruciballManager;
			_deckManager = deckManager;
			_relicManager = relicManager;
			if (_relicManager != null && _deckManager != null)
			{
				applyUniqueBuff = false;
				if (_relicManager.RelicEffectActive(RelicEffect.UNIQUE_ORBS_BUFF))
				{
					applyUniqueBuff = _deckManager.IsAttackUnique(this, "", forUI);
				}
			}
		}

		public void SoftInit(RelicManager relicManager)
		{
			_relicManager = relicManager;
			if (_relicManager != null)
			{
				applyUniqueBuff = false;
			}
		}

		public void CheckUniqueBuff(string idToIgnore)
		{
			if (_relicManager != null && _deckManager != null)
			{
				applyUniqueBuff = false;
				if (_relicManager.RelicEffectActive(RelicEffect.UNIQUE_ORBS_BUFF))
				{
					applyUniqueBuff = _deckManager.IsAttackUnique(this, idToIgnore);
				}
			}
		}

		public void ApplyUniqueBuffToUpgrades(Attack original)
		{
			if (_relicManager != null && _deckManager != null)
			{
				applyUniqueBuff = false;
				if (_relicManager.RelicEffectActive(RelicEffect.UNIQUE_ORBS_BUFF))
				{
					applyUniqueBuff = _deckManager.IsAttackUnique(original, original.instanceID);
				}
			}
		}

		public virtual void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long bonus, int critCount = 0)
		{
			Debug.LogError("Attack::Fire(): Called base class method instead of final implementation");
			OnAttackWhiff();
		}

		public virtual StaticDamageModifierData CalculateStaticDamageBuffs(bool saveResults = false)
		{
			int num = 0;
			int num2 = 0;
			float num3 = 1f;
			float num4 = 1f;
			float num5 = 1f;
			float num6 = 1f;
			float muscircleMultiplier = 1f;
			float spinesseMultiplier = 1f;
			float ballanceMultiplier = 1f;
			num += _previousOrbRegularDamageMod;
			num2 += _previousOrbCritDamageMod;
			if (_relicManager != null)
			{
				if (_relicManager.RelicEffectActive(RelicEffect.INCREASE_STRENGTH_SMALL))
				{
					num++;
					num2++;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.CONFUSION_RELIC) && _playerStatusEffectController != null && _playerStatusEffectController.HasNegativeStatusEffect())
				{
					num += 2;
					num2 += 4;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER))
				{
					num += 2;
					num2 += 4;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.NEW_MATRYORBSHKA))
				{
					num -= 2;
					num2 -= 3;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.NON_CRIT_BONUS_DMG))
				{
					num++;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.CRIT_BONUS_DMG))
				{
					num2++;
				}
				if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.NERFED_DAMAGE))
				{
					num -= 3;
					num2 -= 3;
				}
				if (IsStone)
				{
					if (_relicManager.RelicEffectActive(RelicEffect.BASIC_STONE_BONUS_DAMAGE))
					{
						num++;
						num2 += 2;
					}
					if (_cruciballManager.WeakStones())
					{
						num = num;
						num2 += -1;
					}
				}
				if (IsBoulder && _relicManager.RelicEffectActive(RelicEffect.BOULDER_BONUS_DAMAGE))
				{
					num++;
					num2 += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_BUFF))
				{
					num -= RelicManager.ALL_ORBS_BUFF_DMG_REDUCTION;
					num2 -= RelicManager.ALL_ORBS_BUFF_CRIT_REDUCTION;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.DOUBLE_STATUS_EFFECTS))
				{
					num -= 10;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.REDUCE_CRIT))
				{
					num += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.HEDGE_BETS) && _attackManager != null)
				{
					num += _critPegsOnBoardAtShotStart * 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ALL_IN_RELIC) && _attackManager != null)
				{
					int critPegsOnBoardAtShotStart = _critPegsOnBoardAtShotStart;
					num -= critPegsOnBoardAtShotStart;
					num2 += critPegsOnBoardAtShotStart;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ADJACENCY_BONUS) && BattleController.BattleActive && !IsStone)
				{
					int num7 = 0;
					foreach (GameObject item in DeckManager.completeDeck)
					{
						Attack component = item.GetComponent<Attack>();
						if (component.locNameString == locNameString && component.instanceID != instanceID)
						{
							num7++;
						}
					}
					num += num7;
					num2 += num7 * 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.RANDOMLY_ROLL_DAMAGE) && BattleController.BattleActive)
				{
					num += _randomDmgMod;
					num2 += _randomCritMod;
				}
				if (applyUniqueBuff)
				{
					num++;
					num2 += 2;
				}
			}
			if (_playerStatusEffectController != null)
			{
				StatusEffectDamageBooster[] components = GetComponents<StatusEffectDamageBooster>();
				if (components != null && components.Length != 0)
				{
					StatusEffectDamageBooster[] array = components;
					foreach (StatusEffectDamageBooster statusEffectDamageBooster in array)
					{
						switch (statusEffectDamageBooster.typeToBoost)
						{
						case StatusEffectType.Balance:
							ballanceMultiplier = statusEffectDamageBooster.multiplier;
							break;
						case StatusEffectType.Strength:
							muscircleMultiplier = statusEffectDamageBooster.multiplier;
							break;
						case StatusEffectType.Finesse:
							spinesseMultiplier = statusEffectDamageBooster.multiplier;
							break;
						}
					}
				}
			}
			if (base.gameObject != null)
			{
				if (damageMods == null)
				{
					damageMods = GetComponents<AttackBaseDamageModifier>();
				}
				if (damageMods != null && damageMods.Length != 0)
				{
					AttackBaseDamageModifier[] array2 = damageMods;
					foreach (AttackBaseDamageModifier attackBaseDamageModifier in array2)
					{
						num += attackBaseDamageModifier.GetStaticDamageMod(_deckManager, _relicManager, _playerHealthController, _playerStatusEffectController);
						num2 += attackBaseDamageModifier.GetStaticDamageMod(_deckManager, _relicManager, _playerHealthController, _playerStatusEffectController, 1);
					}
				}
			}
			if (_deckManager != null && _deckManager.battleDeck != null)
			{
				foreach (GameObject item2 in _deckManager.battleDeck)
				{
					if (item2 != null && item2.TryGetComponent<BuffOtherOrbsInDeck>(out var component2) && component2.orbLocStringNamesToBuff.Contains(locNameString))
					{
						num += component2.regularDamage;
						num2 += component2.critDamage;
					}
				}
			}
			if (_relicManager != null)
			{
				if (_relicManager.RelicEffectActive(RelicEffect.DOUBLE_DAMAGE_HURT_ON_PEG))
				{
					num3 *= 2f;
					num5 *= 2f;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_DEBUFF))
				{
					num3 *= 2f;
					num5 *= 2f;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL))
				{
					num4 *= 0.5f;
					num6 *= 0.5f;
				}
			}
			StaticDamageModifierData staticDamageModifierData = new StaticDamageModifierData(num, num2, num3, num5, num4, num6, muscircleMultiplier, spinesseMultiplier, ballanceMultiplier);
			if (saveResults)
			{
				_currentShotStaticDamageModifierData = staticDamageModifierData;
			}
			return staticDamageModifierData;
		}

		public virtual int GetModifiedDamagePerPeg(int critCount = 0, bool calculateStaticDamageModificersInPlace = true)
		{
			StaticDamageModifierData staticDamageModifierData = ((!calculateStaticDamageModificersInPlace) ? _currentShotStaticDamageModifierData : CalculateStaticDamageBuffs());
			int baseDamage = staticDamageModifierData.baseDamage;
			int critDamage = staticDamageModifierData.critDamage;
			float baseDamageMultiplier = staticDamageModifierData.baseDamageMultiplier;
			float critDamageMultiplier = staticDamageModifierData.critDamageMultiplier;
			float baseDamageDivider = staticDamageModifierData.baseDamageDivider;
			float critDamageDivider = staticDamageModifierData.critDamageDivider;
			float muscircleMultiplier = staticDamageModifierData.muscircleMultiplier;
			float spinesseMultiplier = staticDamageModifierData.spinesseMultiplier;
			float ballanceMultiplier = staticDamageModifierData.ballanceMultiplier;
			if (!isDNMNull)
			{
				if (critCount > 0 && dnm.critDamageNonMod)
				{
					return CritDamagePerPeg;
				}
				if (critCount <= 0 && dnm.baseDamageNonMod)
				{
					return DamagePerPeg;
				}
			}
			int num = 0;
			int num2 = 0;
			if (_playerStatusEffectController != null)
			{
				if (critCount <= 0)
				{
					num += Mathf.RoundToInt((float)_playerStatusEffectController.EffectStrength(StatusEffectType.Strength) * muscircleMultiplier);
				}
				else
				{
					num2 += Mathf.RoundToInt((float)_playerStatusEffectController.EffectStrength(StatusEffectType.Finesse) * spinesseMultiplier);
				}
				if (!isDNMNull)
				{
					if (dnm.critBoostAppliesToRegular)
					{
						num2 += Mathf.RoundToInt((float)_playerStatusEffectController.EffectStrength(StatusEffectType.Finesse) * spinesseMultiplier);
					}
					if (dnm.regularBoostAppliesToCrit)
					{
						num += Mathf.RoundToInt((float)_playerStatusEffectController.EffectStrength(StatusEffectType.Strength) * muscircleMultiplier);
					}
				}
				int num3 = Mathf.RoundToInt((float)_playerStatusEffectController.EffectStrength(StatusEffectType.Balance) * ballanceMultiplier);
				num += num3;
				num2 += num3;
			}
			if (base.gameObject != null && damageMods != null && damageMods.Length != 0)
			{
				AttackBaseDamageModifier[] array = damageMods;
				foreach (AttackBaseDamageModifier attackBaseDamageModifier in array)
				{
					if (critCount <= 0)
					{
						num += attackBaseDamageModifier.GetDamageMod(_deckManager, _relicManager, _playerHealthController, _playerStatusEffectController);
					}
					else
					{
						num2 += attackBaseDamageModifier.GetDamageMod(_deckManager, _relicManager, _playerHealthController, _playerStatusEffectController, 1);
					}
				}
			}
			if (!isDNMNull)
			{
				int num4 = num2;
				if ((dnm.regularBoostAppliesToCrit && num > 0) || baseDamage > 0)
				{
					num2 += Mathf.RoundToInt((float)(num + baseDamage) * dnm.regularBoostAppliesToCritMod);
				}
				if ((dnm.critBoostAppliesToRegular && num4 > 0) || critDamage > 0)
				{
					num += Mathf.RoundToInt((float)(num4 + critDamage) * dnm.critBoostAppliesToRegularMod);
				}
			}
			float num5 = DamagePerPeg + baseDamage + num;
			if (_relicManager != null)
			{
				num5 *= baseDamageMultiplier;
				if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
				{
					num5 = Mathf.Max(num5, 0f);
					num5 = Mathf.Pow(num5, 1.25f);
				}
				num5 *= baseDamageDivider;
			}
			num5 = Mathf.Clamp(Mathf.RoundToInt(num5), 0, 999);
			if (critCount > 0)
			{
				float num6 = Mathf.Max(CritDamagePerPeg + critDamage + num2, 0);
				if (_relicManager != null)
				{
					if (critCount > 1 && _relicManager.RelicEffectActive(RelicEffect.CRITS_STACK))
					{
						num6 += Mathf.Max((num6 - num5) * (float)(critCount - 1), 0f);
					}
					num6 *= critDamageMultiplier;
					if (_relicManager.RelicEffectActive(RelicEffect.DIRECT_DAMAGE))
					{
						num6 = Mathf.Max(num6, 0f);
						num6 = Mathf.Pow(num6, 1.25f);
					}
					num6 *= critDamageDivider;
				}
				return Mathf.Clamp(Mathf.RoundToInt(num6), 0, 999);
			}
			return (int)num5;
		}

		protected void CheckForPostAttackOperations()
		{
			if (_checkForPostAttackOperations)
			{
				_checkForPostAttackOperations = false;
				CallPostAttackOperations();
			}
		}

		protected void CallPostAttackOperations()
		{
			PostAttackOperation[] components = GetComponents<PostAttackOperation>();
			if (components != null && components.Length != 0)
			{
				PostAttackOperation[] array = components;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].HandleAttackFinished(_playerStatusEffectController);
				}
			}
		}

		public void CopyValuesFromDeckAttack(Attack attack)
		{
			_randomDmgMod = attack.randomDmgMod;
			_randomCritMod = attack.randomCritMod;
			_previousOrbRegularDamageMod = attack._previousOrbRegularDamageMod;
			_previousOrbCritDamageMod = attack._previousOrbCritDamageMod;
		}

		public void SetRandomRolledDamage()
		{
			int num = Mathf.Min(GetModifiedDamagePerPeg(), 2);
			_randomDmgMod = UnityEngine.Random.Range(-num, 5);
			num = Mathf.Min(GetModifiedDamagePerPeg(1), 2);
			_randomCritMod = UnityEngine.Random.Range(-num, 5);
		}

		public void ResetPreviousOrbDamageModifier()
		{
			_previousOrbRegularDamageMod = 0;
			_previousOrbCritDamageMod = 0;
		}

		public void AddPreviousOrbDamageModifier(int regularDamage, int critDamage)
		{
			_previousOrbRegularDamageMod += regularDamage;
			_previousOrbCritDamageMod += critDamage;
		}

		public float GetDamageModMultiplier(int critCount = 0)
		{
			if (critCount > 0)
			{
				return GetModifiedDamagePerPeg(critCount) / CritDamagePerPeg;
			}
			return GetModifiedDamagePerPeg(critCount) / DamagePerPeg;
		}

		public virtual void UpdateBattleParameters()
		{
		}

		public virtual void ClearBattleParameters()
		{
		}

		public virtual List<StatusEffect> GetStatusEffects(int critCount)
		{
			List<StatusEffect> list = new List<StatusEffect>();
			if (_relicManager.RelicEffectActive(RelicEffect.ATTACKS_DEAL_BLIND))
			{
				list.Add(new StatusEffect(StatusEffectType.Blind, 4));
			}
			if (_relicManager.RelicEffectActive(RelicEffect.ATTACKS_APPLY_TRANSPHERENCY))
			{
				list.Add(new StatusEffect(StatusEffectType.Transpherency, 3));
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.ATTACKS_DEAL_POISON))
			{
				list.Add(new StatusEffect(StatusEffectType.Poison));
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.ATTACKS_APPLY_EXPLOITABALL))
			{
				if (critCount > 0)
				{
					list.Add(new StatusEffect(StatusEffectType.Exploitaball, 2));
				}
				else
				{
					list.Add(new StatusEffect(StatusEffectType.Exploitaball));
				}
			}
			return list;
		}

		public string GetNameWithLevel()
		{
			string text = Name;
			UnlimitedUpgrades component = GetComponent<UnlimitedUpgrades>();
			if (!_hideOrbLevel)
			{
				if (Level > 0 && component != null)
				{
					text = text + " (" + LocalizationManager.GetTranslation("Orbs/level_short").Replace("{[ORB_LEVEL]}", (Level + component.timesUpgraded).ToString()) + ")";
				}
				else if (Level > 0)
				{
					text = text + " (" + LocalizationManager.GetTranslation("Orbs/level_short").Replace("{[ORB_LEVEL]}", Level.ToString()) + ")";
				}
			}
			return text;
		}

		public string GetNameWithCustomLevel(int level)
		{
			string text = Name;
			if (level > 0)
			{
				text = text + " (" + LocalizationManager.GetTranslation("Orbs/level_short").Replace("{[ORB_LEVEL]}", level.ToString()) + ")";
			}
			return text;
		}

		public static bool IsEquivalent(Attack A, Attack B)
		{
			if (A == null || B == null)
			{
				return false;
			}
			if (A.Name != B.Name)
			{
				return false;
			}
			if (A.Level != B.Level)
			{
				return false;
			}
			AttackDamagesSelf component = A.GetComponent<AttackDamagesSelf>();
			if (component != null)
			{
				AttackDamagesSelf component2 = B.GetComponent<AttackDamagesSelf>();
				if (component2 != null && component2.selfDamage != component.selfDamage)
				{
					return false;
				}
			}
			return true;
		}

		public virtual long GetDamage(AttackManager attackManager, int pegMultipliersTally, float dmgMult, long bonus, int critCount = 0, bool displayNegative = false)
		{
			long num = bonus;
			int modifiedDamagePerPeg = GetModifiedDamagePerPeg(critCount, calculateStaticDamageModificersInPlace: false);
			num += pegMultipliersTally * modifiedDamagePerPeg;
			float num2 = 1f;
			if (critCount > 1 && _relicManager.RelicEffectActive(RelicEffect.ANCIENT_FLEECE))
			{
				num2 = Mathf.Pow(1.3f, critCount - 1);
				num = (long)((float)num * num2);
			}
			int num3 = 0;
			if (TryGetComponent<AttackFlatDamageModifier>(out var component) && !component.addOnlyForAttack)
			{
				num3 = component.GetFlatDamageMod(_deckManager, _relicManager);
				num += num3;
			}
			num = (long)((float)num * dmgMult);
			if (num < 0 && bonus >= 0 && pegMultipliersTally >= 0 && modifiedDamagePerPeg >= 0 && num2 >= 0f && num3 >= 0 && dmgMult >= 0f)
			{
				OnAttackDamageHitInfinity?.Invoke();
				return long.MaxValue;
			}
			if (displayNegative)
			{
				return num;
			}
			if (num < 0)
			{
				return 0L;
			}
			return num;
		}

		protected float GetDamageMod(float enemyDamageMod)
		{
			if (enemyDamageMod >= 0f && enemyDamageMod < 1f && _relicManager.AttemptUseRelic(RelicEffect.NO_DAMAGE_REDUCTION))
			{
				return 1f / enemyDamageMod;
			}
			if (GetComponent<IgnoreNegativeDamageMods>() != null)
			{
				return Mathf.Max(enemyDamageMod, 1f);
			}
			return enemyDamageMod;
		}

		private void OnValidate()
		{
			Attack attack = this;
			for (int i = 0; i < 100; i++)
			{
				if (attack == null)
				{
					break;
				}
				if (attack.NextLevelPrefab == null)
				{
					break;
				}
				attack = attack.NextLevelPrefab.GetComponent<Attack>();
			}
			for (int i = 0; i < 100; i++)
			{
				if (attack == null)
				{
					break;
				}
				if (attack.PreviousLevelPrefab == null)
				{
					break;
				}
				attack = attack.PreviousLevelPrefab.GetComponent<Attack>();
			}
		}
	}
}
