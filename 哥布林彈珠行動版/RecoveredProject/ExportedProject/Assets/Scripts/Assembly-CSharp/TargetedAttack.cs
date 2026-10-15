using System;
using System.Collections;
using Battle;
using Battle.Attacks;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using Cruciball;
using Relics;
using UnityEngine;

public class TargetedAttack : Attack
{
	[SerializeField]
	protected GameObject _thunderPrefab;

	[SerializeField]
	protected GameObject _criticalThunderPrefab;

	[Tooltip("Should we aim at the bottom of the enemy, instead of its center?")]
	public bool verticalAttack;

	[Tooltip("Does this attack hit all enemies in a column?")]
	public bool targetColumn;

	public bool targetColumnOnCrit;

	public Vector3 offset = Vector3.zero;

	protected long _hitDamage;

	protected Enemy _target;

	protected SpellBehavior _spell;

	protected SpellBehavior _critSpell;

	private bool _lastAttackCrit;

	protected int _critCount;

	[HideInInspector]
	public bool modifiedToAttackAll;

	public GameObject SpellPrefab => _thunderPrefab;

	public GameObject CritPrefab => _criticalThunderPrefab;

	public override void Initialize(Vector2 playerPosition, AttackManager attackManager, RelicManager relicManager, DeckManager deckManager, CruciballManager cruciballManager, PlayerHealthController healthController, PlayerStatusEffectController playerStatusEffectController)
	{
		base.Initialize(playerPosition, attackManager, relicManager, deckManager, cruciballManager, healthController, playerStatusEffectController);
	}

	public override void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long dmgBonus, int critCount = 0)
	{
		_critCount = critCount;
		_target = target;
		_hitDamage = GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount);
		if (_relicManager.AttemptUseRelic(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK))
		{
			long num = _hitDamage % 2;
			_hitDamage = _hitDamage / 2 + num;
		}
		SpellBehavior component;
		if (critCount > 0 && _hitDamage > 0)
		{
			component = UnityEngine.Object.Instantiate(_criticalThunderPrefab).GetComponent<SpellBehavior>();
			component.Initialize(attackManager.SfxAudioSource);
			component.target = target;
			component.gameObject.SetActive(value: false);
		}
		else
		{
			component = UnityEngine.Object.Instantiate(_thunderPrefab).GetComponent<SpellBehavior>();
			component.Initialize(attackManager.SfxAudioSource);
			component.target = target;
			component.gameObject.SetActive(value: false);
		}
		SpellBehavior spellBehavior = null;
		if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK))
		{
			if (critCount > 0 && _hitDamage > 0)
			{
				spellBehavior = UnityEngine.Object.Instantiate(_criticalThunderPrefab).GetComponent<SpellBehavior>();
				spellBehavior.Initialize(attackManager.SfxAudioSource);
				spellBehavior.gameObject.SetActive(value: false);
			}
			else
			{
				spellBehavior = UnityEngine.Object.Instantiate(_thunderPrefab).GetComponent<SpellBehavior>();
				spellBehavior.Initialize(attackManager.SfxAudioSource);
				spellBehavior.gameObject.SetActive(value: false);
			}
		}
		_checkForPostAttackOperations = true;
		_lastAttackCrit = critCount > 0;
		SpawnSpell(component, target, critCount);
		if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REVERSE_PROJECTILE_ATTACK))
		{
			if (IsSummonedBy == null)
			{
				StartCoroutine(WaitAndSpawnSpellOnLastEnemy(spellBehavior, critCount));
			}
			else
			{
				IsSummonedBy.StartCoroutine(WaitAndSpawnSpellOnLastEnemy(spellBehavior, critCount));
			}
		}
	}

	private IEnumerator WaitAndSpawnSpellOnLastEnemy(SpellBehavior spell, int critCount)
	{
		yield return new WaitForSeconds(0.5f);
		SpawnSpell(spell, spell.target = _attackManager.enemyManager.GetFarthestEnemyFromPlayer(), critCount, isExtraAttack: true);
	}

	protected virtual void SpawnSpell(SpellBehavior spell, Enemy target, int critCount, bool isExtraAttack = false)
	{
		if ((target == null || !target.gameObject.activeSelf) && !isExtraAttack)
		{
			TargetingManager targetingManager = UnityEngine.Object.FindObjectOfType<TargetingManager>();
			if (targetingManager == null)
			{
				_attackManager.AttackAnimationEnded();
				return;
			}
			targetingManager.AutoSelect();
			if (targetingManager.currentTarget == null)
			{
				_attackManager.AttackAnimationEnded();
				return;
			}
			target = targetingManager.currentTarget;
		}
		else if ((target == null || !target.gameObject.activeSelf) & isExtraAttack)
		{
			target = _attackManager.enemyManager.GetFarthestEnemyFromPlayer();
			if (target == null || !target.gameObject.activeSelf)
			{
				return;
			}
		}
		if (targetColumn || (targetColumnOnCrit && critCount > 0))
		{
			TargetingUI componentInChildren = target.GetComponentInChildren<TargetingUI>(includeInactive: true);
			if (componentInChildren != null)
			{
				Vector2 vector = componentInChildren.transform.position;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 5f, LayerMask.GetMask("BattlefieldFloor"));
				raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 5f, LayerMask.GetMask("Obstacles"));
				vector.y = ((raycastHit2D.collider == null) ? target.transform.position.y : raycastHit2D.point.y);
				spell.gameObject.transform.position = vector;
			}
			else
			{
				Debug.LogWarning("Targeting UI null for enemy: " + target.name);
			}
		}
		else if (verticalAttack)
		{
			TargetingUI componentInChildren2 = target.GetComponentInChildren<TargetingUI>(includeInactive: true);
			if (componentInChildren2 != null)
			{
				Vector2 vector2 = componentInChildren2.transform.position;
				vector2.y = target.transform.position.y;
				spell.gameObject.transform.position = vector2;
			}
			else
			{
				Debug.LogWarning("Targeting UI null for enemy: " + target.name);
			}
		}
		else
		{
			spell.gameObject.transform.position = target.GetComponentInChildren<TargetingUI>(includeInactive: true).transform.position;
		}
		spell.gameObject.transform.position += offset;
		if (isExtraAttack && target == _target)
		{
			spell.gameObject.transform.position += new Vector3(0.4f, 0f, 0f);
		}
		spell.gameObject.SetActive(value: true);
		spell.OnSpellHit = (SpellBehavior.SpellHit)Delegate.Combine(spell.OnSpellHit, new SpellBehavior.SpellHit(HandleSpellHit));
		spell.OnSpellEnded = (SpellBehavior.SpellEnded)Delegate.Combine(spell.OnSpellEnded, new SpellBehavior.SpellEnded(HandleSpellEnded));
	}

	private void HandleSpellHit(Enemy spellTarget)
	{
		if (spellTarget == null || !spellTarget.gameObject.activeSelf)
		{
			return;
		}
		int slotIndexForEnemy = _attackManager.enemyManager.GetSlotIndexForEnemy(spellTarget, out var _);
		EnemyManager.SlotType slotForEnemy = _attackManager.enemyManager.GetSlotForEnemy(spellTarget);
		bool flag = _relicManager.AttemptUseRelic(RelicEffect.TARGETED_ATTACKS_HIT_ALL);
		if (flag || modifiedToAttackAll || _relicManager.AttemptUseRelic(RelicEffect.SPLASH_EFFECT_ON_TARGETED_ATTACKS))
		{
			int range = 1;
			AoeAttack.AoeType aoeType = AoeAttack.AoeType.SIDE;
			if (flag || modifiedToAttackAll)
			{
				aoeType = AoeAttack.AoeType.ALL;
			}
			Enemy[] array = (AttackTargetsVertically() ? _attackManager.enemyManager.GetColumnSplashRangeEnemies(slotIndexForEnemy, slotForEnemy, range, aoeType) : _attackManager.enemyManager.GetSplashRangeEnemies(slotIndexForEnemy, slotForEnemy, range, aoeType));
			Enemy[] array2 = array;
			foreach (Enemy enemy in array2)
			{
				DealHitDamageToEnemy(enemy, _critCount);
			}
			return;
		}
		if (AttackTargetsVertically())
		{
			Enemy verticalSplashEnemy = _attackManager.enemyManager.GetVerticalSplashEnemy(slotIndexForEnemy, slotForEnemy);
			if (verticalSplashEnemy != null)
			{
				DealHitDamageToEnemy(verticalSplashEnemy, _critCount);
			}
		}
		DealHitDamageToEnemy(spellTarget, _critCount);
	}

	private void OnDisable()
	{
		CheckForPostAttackOperations();
	}

	public bool AttackTargetsVertically()
	{
		if (!targetColumn)
		{
			if (targetColumnOnCrit)
			{
				return _lastAttackCrit;
			}
			return false;
		}
		return true;
	}

	private void DealHitDamageToEnemy(Enemy enemy, int critCount)
	{
		enemy.DealTargetedAttackDamage(_hitDamage, GetDamageMod(enemy.spellAttackDamageMod), critCount);
		IAffectEnemyOnHit[] components = GetComponents<IAffectEnemyOnHit>();
		if (components != null && components.Length != 0)
		{
			IAffectEnemyOnHit[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AffectEnemy(enemy, _relicManager, _critCount);
			}
		}
		foreach (StatusEffect statusEffect in GetStatusEffects(_critCount))
		{
			if (statusEffect.EffectType != StatusEffectType.None)
			{
				enemy.ApplyStatusEffect(statusEffect, StatusEffectSource.PLAYER);
			}
		}
		_attackManager.WaitForEnemyDamageAnimationToEnd(enemy);
	}

	private void HandleSpellEnded()
	{
		CheckForPostAttackOperations();
		_attackManager.AttackAnimationEnded();
	}
}
