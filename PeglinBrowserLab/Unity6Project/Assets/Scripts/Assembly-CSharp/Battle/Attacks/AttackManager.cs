using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Attacks.DamageModifiers;
using Battle.Enemies;
using PeglinUtils;
using Relics;
using UnityEngine;

namespace Battle.Attacks
{
	public class AttackManager : MonoBehaviour
	{
		public delegate void PeglinAttackAnimationRequested(string trigger);

		public delegate void AttackPerformed(string trigger);

		public delegate void AttackEnded();

		private struct AttackData
		{
			public Enemy Target;

			public int PegMultipliersTally;

			public int NumPegsHit;

			public float DmgMult;

			public long DmgBonus;

			public int CriticalHitCount;

			public AttackData(Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long dmgBonus, int criticalHitCount)
			{
				Target = target;
				PegMultipliersTally = pegMultipliersTally;
				NumPegsHit = numPegsHit;
				DmgMult = dmgMult;
				DmgBonus = dmgBonus;
				CriticalHitCount = criticalHitCount;
			}
		}

		private const float PANIC_KILL_SWITCH = 10f;

		public static PeglinAttackAnimationRequested OnPeglinAttackAnimationRequested;

		public static AttackPerformed OnAttackPerformed;

		public static AttackEnded OnAttackEnded;

		public static Action OnFireComplete = () =>
		{
		};

		private List<Enemy> _enemiesWaitingForAnimationList = new List<Enemy>();

		[SerializeField]
		private RelicManager _relicManager;

		private Attack _attack;

		public bool isHeal;

		private bool _isAttacking;

		private bool _attackAnimationFinished = true;

		private EnemyManager _enemyManager;

		private PegManager _pegManager;

		private AttackData _currentAttackData;

		private float _elapsedTime;

		public static long PreviousAttackDamage = 0L;

		public static int NumPegsHitThisBattle = 0;

		private int _cachedPegsHitThisShot;

		[SerializeField]
		private AudioSource _sfxAudioSource;

		public EnemyManager enemyManager => _enemyManager;

		public PegManager pegManager => _pegManager;

		public AudioSource SfxAudioSource => _sfxAudioSource;

		public void Initialize(Attack attack, EnemyManager em, PegManager pm)
		{
			if (attack == null)
			{
				Debug.LogError("AttackManager::Initialize(): Attack was null");
			}
			_enemyManager = em;
			_pegManager = pm;
			_attack = attack;
			isHeal = attack is HealAction;
		}

		private void OnDisable()
		{
			PeglinBattleAnimationController.OnFirePoint = (PeglinBattleAnimationController.FirePoint)Delegate.Remove(PeglinBattleAnimationController.OnFirePoint, new PeglinBattleAnimationController.FirePoint(Fire));
		}

		public void Attack(Enemy target, int pegMultipliersTally, int numPegsHitThisShot, float dmgMultiplier, long dmgBonus, DeckManager deckManager, int criticalHitCount = 0)
		{
			_cachedPegsHitThisShot = numPegsHitThisShot;
			_isAttacking = true;
			_elapsedTime = 0f;
			_attackAnimationFinished = false;
			if (_attack.TryGetComponent<AttackFlatDamageModifier>(out var component) && component.addOnlyForAttack)
			{
				dmgBonus += component.GetFlatDamageMod(deckManager, _relicManager);
			}
			EchoAttack echoAttack = _attack as EchoAttack;
			if (echoAttack != null)
			{
				dmgBonus += Mathf.FloorToInt((float)PreviousAttackDamage * echoAttack.prevAttackDmgMod);
			}
			if (_relicManager.RelicEffectActive(RelicEffect.ALL_ATTACKS_ECHO))
			{
				dmgBonus += Mathf.FloorToInt((float)PreviousAttackDamage * RelicManager.ECHO_CHAMBER_PERCENT);
			}
			_currentAttackData = new AttackData(target, pegMultipliersTally, numPegsHitThisShot, dmgMultiplier, dmgBonus, criticalHitCount);
			PeglinBattleAnimationController.OnFirePoint = (PeglinBattleAnimationController.FirePoint)Delegate.Combine(PeglinBattleAnimationController.OnFirePoint, new PeglinBattleAnimationController.FirePoint(Fire));
			OnPeglinAttackAnimationRequested?.Invoke(_attack.PeglinAttackAnimationTrigger);
			OnAttackPerformed?.Invoke(_attack.PeglinAttackAnimationTrigger);
		}

		public long GetCurrentDamage(int pegMultipliersTally, float dmgMult, long dmgBonus, int criticalHitCount = 0, bool displayNegative = false)
		{
			if (_attack == null)
			{
				return 0L;
			}
			return _attack.GetDamage(this, pegMultipliersTally, dmgMult, dmgBonus, criticalHitCount, displayNegative);
		}

		private void Fire()
		{
			PeglinBattleAnimationController.OnFirePoint = (PeglinBattleAnimationController.FirePoint)Delegate.Remove(PeglinBattleAnimationController.OnFirePoint, new PeglinBattleAnimationController.FirePoint(Fire));
			if (StaticGameData.CurrentRunStats != null)
			{
				if (_currentAttackData.CriticalHitCount > 0)
				{
					StaticGameData.CurrentRunStats.critShotsTaken++;
				}
				else
				{
					StaticGameData.CurrentRunStats.shotsTaken++;
				}
				StaticGameData.CurrentRunStats.AddShotForAttack(_attack);
			}
			if (_relicManager.RelicEffectActive(RelicEffect.ADD_OR_REMOVE_CRIT))
			{
				_attack.UpdateBoardDetails();
			}
			_attack.Fire(this, _currentAttackData.Target, _currentAttackData.PegMultipliersTally, _currentAttackData.NumPegsHit, _currentAttackData.DmgMult, _currentAttackData.DmgBonus, _currentAttackData.CriticalHitCount);
			PreviousAttackDamage = GetCurrentDamage(_currentAttackData.PegMultipliersTally, _currentAttackData.DmgMult, _currentAttackData.DmgBonus, _currentAttackData.CriticalHitCount);
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.AddDamageForOrb(PreviousAttackDamage, _attack);
			}
			OnFireComplete?.Invoke();
			NumPegsHitThisBattle += _cachedPegsHitThisShot;
		}

		private void Update()
		{
			if (_isAttacking)
			{
				_elapsedTime += Time.deltaTime;
				if (_elapsedTime > 10f)
				{
					Debug.LogWarning($"KILLING STUCK ATTACK. Stuck for {_elapsedTime} seconds!");
					_isAttacking = false;
				}
			}
		}

		public TargetingType GetCurrentAttackTargetingType()
		{
			if (_attack == null)
			{
				return TargetingType.SINGLE;
			}
			return _attack.targetingType;
		}

		public bool IsAttacking()
		{
			return _isAttacking;
		}

		public void AttackAnimationEnded()
		{
			_attackAnimationFinished = true;
			if (_enemiesWaitingForAnimationList.Count == 0)
			{
				_isAttacking = false;
				OnAttackEnded?.Invoke();
			}
			else
			{
				StartCoroutine(ForceWaitUntilEnemiesFinishAnimating());
			}
		}

		private IEnumerator ForceWaitUntilEnemiesFinishAnimating()
		{
			while (_enemiesWaitingForAnimationList.Count > 0)
			{
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			_isAttacking = false;
		}

		public void WaitForEnemyDamageAnimationToEnd(Enemy enemy)
		{
			enemy.RegisterOnDamagedAnimationEnd(OnEnemyDamageAnimationEnded);
			_enemiesWaitingForAnimationList.Add(enemy);
		}

		private void OnEnemyDamageAnimationEnded(Enemy enemy)
		{
			enemy.UnregisterOnDamagedAnimationEnd(OnEnemyDamageAnimationEnded);
			_enemiesWaitingForAnimationList.Remove(enemy);
			if (_enemiesWaitingForAnimationList.Count == 0 && _attackAnimationFinished)
			{
				_isAttacking = false;
				OnAttackEnded?.Invoke();
			}
		}

		public void WaitAndEndAnim()
		{
			StartCoroutine(EndAnimAfterSecond());
		}

		private IEnumerator EndAnimAfterSecond()
		{
			yield return new WaitForSeconds(1f);
			AttackAnimationEnded();
		}
	}
}
