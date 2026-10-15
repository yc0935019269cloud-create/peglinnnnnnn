using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Enemies.Attacks;
using Battle.Enemies.Behaviours.SpiritOfRadia;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using DG.Tweening;
using MoreMountains.Tools;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.Enemies
{
	public class SpiritOfRadiaBoss : StationaryEnemy
	{
		public delegate void SpiritOfRadiaPhaseTransitionStarted();

		public const float PHASE_2_TRANSITION_LENGTH = 4f;

		public static readonly float EXTRA_PEGBOARD_HEIGHT = 6.25f;

		private const string PHASE_CHANGE_TRIGGER = "go_to_phase_2";

		public static SpiritOfRadiaPhaseTransitionStarted PreTransitionStarted = () =>
		{
		};

		public static SpiritOfRadiaPhaseTransitionStarted OnSpiritOfRadiaPhaseTransitionStarted = () =>
		{
		};

		[Header("Spirit Of Radia Specific")]
		public DeckManager _deckManager;

		public GameObject pupilOrb;

		public int transitionOnTurnNumber = 6;

		public Vector2 phase2EyePos = new Vector2(0f, 3.5f);

		[SerializeField]
		private GameObject _topCrystals;

		[SerializeField]
		private Peg eyeBouncerPeg;

		[SerializeField]
		private int _intangiballDuringPhase2 = 500;

		[SerializeField]
		private bool _dealShotDamageWhenHittingEye;

		[SerializeField]
		private int _directDamageFromHittingEye = 250;

		public Vector2[] phase2BlackHoleCoordinates;

		public Vector2[] phase2OrbStartingPositions;

		public int turnsPerExtraRegularAttack = 9;

		public float blindReductionPercentPerMaxHPAttack = 0.5f;

		public string endAttackTriggerString = "end_attack";

		private float _swordAnimationLength;

		private PredictionManager _predictionManager;

		private static int _currentPhase = 1;

		public int minCrystalPegs = 30;

		public int maxCrystalPegs = 35;

		public int intangiballAddPerCrystalPegHit = 10;

		public AudioClip phaseTransitionJingle;

		public AudioClip phase2Music;

		private BattleController _battleController;

		private PegManager _pegManager;

		private List<Peg> _crystalPegs = new List<Peg>();

		private int _currentOrbStartingPosIndex;

		private ApplyResistancesOnSpawn _applyResistancesOnSpawn;

		private int _phase2StartedOnRound;

		private MaxHPSpiritOfRadiaAttack _maxHPSpiritOfRadiaAttack;

		private SpriteMask _spriteMask;

		public static int CurrentPhase => _currentPhase;

		public int crystalPegCount => _crystalPegs.Count;

		private new void Awake()
		{
			base.Awake();
			_currentPhase = 1;
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			_pegManager = _battleController.pegManager;
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			_applyResistancesOnSpawn = GetComponent<ApplyResistancesOnSpawn>();
			_maxHPSpiritOfRadiaAttack = GetComponent<MaxHPSpiritOfRadiaAttack>();
			_spriteMask = GetComponent<SpriteMask>();
			_spriteMask.enabled = false;
			OrbserverSwordProjectile component = Projectile.GetComponent<OrbserverSwordProjectile>();
			_swordAnimationLength = component.explodeOutTime + component.minExplodeOutPostWaitTime + component.rotateToTargetTime;
		}

		private new void Start()
		{
			base.Start();
			_crystalPegs = new List<Peg>();
			foreach (Peg allPeg in _pegManager.allPegs)
			{
				if (allPeg.pegType == Peg.PegType.CRYSTAL)
				{
					_crystalPegs.Add(allPeg);
				}
			}
			StartCoroutine(WaitAndCreateCrystalPegs());
		}

		private IEnumerator WaitAndCreateCrystalPegs()
		{
			yield return null;
			_pegManager.CreateSpecialPegs(_crystalPegs, Peg.PegType.CRYSTAL, null, UnityEngine.Random.Range(minCrystalPegs, maxCrystalPegs + 1));
		}

		private new void OnEnable()
		{
			base.OnEnable();
			Peg.OnPegDestroyed = (Peg.PegDestroyed)Delegate.Combine(Peg.OnPegDestroyed, new Peg.PegDestroyed(OnPegDestroyed));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(OnPegActivated));
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
			Enemy.OnEnemyDestroyed = (EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new EnemyDestroyed(OnAnyEnemyDestroyed));
		}

		private new void OnDisable()
		{
			base.OnDisable();
			Peg.OnPegDestroyed = (Peg.PegDestroyed)Delegate.Remove(Peg.OnPegDestroyed, new Peg.PegDestroyed(OnPegDestroyed));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(OnPegActivated));
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
			Enemy.OnEnemyDestroyed = (EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new EnemyDestroyed(OnAnyEnemyDestroyed));
		}

		private void OnDestroy()
		{
			_currentPhase = 1;
		}

		private void OnPegDestroyed(Peg.PegType pegType, Peg peg)
		{
			if (_crystalPegs.Count > 0 && pegType == Peg.PegType.CRYSTAL && _crystalPegs.Contains(peg))
			{
				_crystalPegs.Remove(peg);
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				_camera.DOShakePosition(UnityEngine.Random.Range(0.1f, 0.2f), UnityEngine.Random.Range(0.1f, 0.2f) * num);
				ControllerVibration.DoVibration(0.22f, 0.15f, 0.03f, 0.03f);
				_anim.SetTrigger("damaged");
				int num2 = intangiballAddPerCrystalPegHit;
				if (GetEffectIntensity(StatusEffectType.Intangiball) == 1)
				{
					num2--;
				}
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, num2), StatusEffectSource.PLAYER);
			}
		}

		private void OnPegActivated(Peg.PegType pegType, Peg peg)
		{
			if (!(peg == eyeBouncerPeg))
			{
				return;
			}
			if (!_dealShotDamageWhenHittingEye)
			{
				Color color = damageTextColor;
				damageTextColor = Color.red;
				float num = 1f;
				if (eyeBouncerPeg.slimeType == Peg.SlimeType.DoubleDamageSlime)
				{
					num *= (float)Peg.DOUBLE_DAMAGE_SLIME_MULTIPLIER;
					if (eyeBouncerPeg.buffAmount < 0)
					{
						num *= Peg.NEGATIVE_DOUBLE_DAMAGE_SLIME_MULTIPLIER;
					}
				}
				if (eyeBouncerPeg.slimeType == Peg.SlimeType.Yolk)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.APPLY_YOLK_TO_ORBSERVER);
				}
				long damage = (long)Mathf.Max(0f, (float)_directDamageFromHittingEye + (float)eyeBouncerPeg.buffAmount * num);
				Damage(damage, screenshake: true, 1f, 1f, unblockable: true, EnemyDamageSource.RelicChip);
				damageTextColor = color;
			}
			else
			{
				StartCoroutine(WaitThenTakeShotDamage());
			}
		}

		private IEnumerator WaitThenTakeShotDamage()
		{
			yield return null;
			Color color = damageTextColor;
			damageTextColor = Color.red;
			long currentShotDamage = _battleController.GetCurrentShotDamage();
			float damageMod = 1f;
			if (_applyResistancesOnSpawn.toggledState)
			{
				damageMod = 0.5f;
			}
			Damage(currentShotDamage, screenshake: false, 1f, damageMod, unblockable: true, EnemyDamageSource.RelicChip);
			DamageScreenshake((long)((float)currentShotDamage * 0.1f));
			damageTextColor = color;
		}

		public override void Attack(int range, Vector3 targetPos)
		{
			int num = BattleController.RoundCount - _phase2StartedOnRound;
			_extraAttacks = Mathf.FloorToInt(num / turnsPerExtraRegularAttack);
			if (_extraAttacks > 0)
			{
				SetEffectIntensity(StatusEffectType.Multiattack, _extraAttacks + 1);
			}
			if (_maxHPSpiritOfRadiaAttack.chargeTurns > 0 && _anim.MMHasParameterOfType("charge_count", AnimatorControllerParameterType.Int))
			{
				_anim.SetInteger("charge_count", _maxHPSpiritOfRadiaAttack.currentChargeTurns);
			}
			base.Attack(range, targetPos);
		}

		protected override void StartAttackCoroutine(int range, Vector3 targetPos)
		{
			if (_currentPhase <= 1)
			{
				CurrentlyActing = false;
				return;
			}
			StartCoroutine(WaitForSwordAnimation());
			StartCoroutine(DoMultipleAttacks(targetPos));
		}

		private IEnumerator DoMultipleAttacks(Vector3 targetPos)
		{
			_spriteMask.enabled = true;
			yield return RangedAttack(targetPos);
			_spriteMask.enabled = false;
		}

		private IEnumerator WaitForSwordAnimation()
		{
			yield return new WaitForSeconds(_swordAnimationLength);
			_anim.SetTrigger(endAttackTriggerString);
		}

		protected override IEnumerator PlayDamagedAnimation(long damage = 1L, float audioScale = 1f, EnemyDamageSource damageSource = EnemyDamageSource.Unspecified)
		{
			bool prevSupressHitAnimations = supressHitAnimations;
			if (_maxHPSpiritOfRadiaAttack.currentChargeTurns == 1)
			{
				supressHitAnimations = true;
			}
			yield return StartCoroutine(base.PlayDamagedAnimation(damage, audioScale, damageSource));
			supressHitAnimations = prevSupressHitAnimations;
		}

		public IEnumerator StartPhase2PreTransition()
		{
			BGMController.instance.PlayOneTimeJingle(phaseTransitionJingle);
			BGMController.instance.SetNextQueuedTrack(phase2Music, isBattleMusic: true);
			yield return new WaitForSeconds(0.5f);
			PreTransitionStarted?.Invoke();
			yield return new WaitForSeconds(1.7f);
			_anim.SetTrigger("go_to_phase_2");
		}

		public IEnumerator StartPhase2Transition()
		{
			_currentPhase = 2;
			OnSpiritOfRadiaPhaseTransitionStarted?.Invoke();
			SetLifelinkVisualsStatus(value: false);
			_phase2StartedOnRound = BattleController.RoundCount;
			_pegManager.ConvertAllCrystalPegsToRegular();
			SpriteRenderer component = GetComponent<SpriteRenderer>();
			component.sortingLayerID = SortingLayer.NameToID("Default");
			component.sortingOrder = -1;
			_topCrystals.SetActive(value: false);
			_targetingUI.gameObject.SetActive(value: false);
			_statusEffectUI.gameObject.transform.position += new Vector3(11.85f, -3.8f, 0f);
			_statusEffectUI.gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
			base.transform.position = new Vector3(phase2EyePos.x, phase2EyePos.y, base.transform.position.z);
			_battleController.pachinkoBallSpawnLocation = phase2OrbStartingPositions[0];
			ApplyStatusEffect(new StatusEffect(StatusEffectType.Intangiball, _intangiballDuringPhase2), StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
			eyeBouncerPeg.gameObject.SetActive(value: true);
			_predictionManager.CopyPegToSimulation(eyeBouncerPeg.gameObject, null);
			AddBlackHoles component2 = GetComponent<AddBlackHoles>();
			component2.blackHoleCoordinates = phase2BlackHoleCoordinates;
			for (int i = 0; i < component2.blackHoleInstances.Length; i++)
			{
				if (!(component2.blackHoleInstances[i] == null))
				{
					component2.blackHoleInstances[i].transform.position = phase2BlackHoleCoordinates[i];
					_predictionManager.DestroyObjectInSimulation(component2.blackHoleInstances[i]);
					_predictionManager.AddObjectToSimulationScene(component2.blackHoleInstances[i]);
				}
			}
			yield return new WaitForSeconds(4f);
			SetLifelinkRendererPosition(0f);
			SetLifelinkVisualsStatus(value: true);
			GetComponent<Enemy>().CurrentlyActing = false;
		}

		private void HandleEnemyTurnEnd()
		{
			UpdatePachinkoPos();
		}

		private void UpdatePachinkoPos()
		{
			if (_currentPhase == 2)
			{
				_currentOrbStartingPosIndex = (_currentOrbStartingPosIndex + 1) % phase2OrbStartingPositions.Length;
				_battleController.pachinkoBallSpawnLocation = phase2OrbStartingPositions[_currentOrbStartingPosIndex];
			}
		}

		private void OnAnyEnemyDestroyed(Enemy enemy)
		{
			if (enemy == this)
			{
				_deckManager.AddOrbToDeck(pupilOrb);
			}
		}
	}
}
