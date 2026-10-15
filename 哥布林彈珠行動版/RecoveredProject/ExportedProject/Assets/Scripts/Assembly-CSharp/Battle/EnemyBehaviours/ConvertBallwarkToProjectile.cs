using System.Collections;
using Battle.Enemies.Attacks;
using Battle.StatusEffects;
using Cruciball;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ConvertBallwarkToProjectile : AlternateEnemyAction
	{
		[SerializeField]
		private CruciballManager _cruciballManager;

		public Projectile ballwarkProjectile;

		public int ballwarkConversionThreshold = 300;

		public float damageModifier = 0.1f;

		private PegManager _pegManager;

		private void Start()
		{
			_pegManager = Object.FindObjectOfType<BattleController>().pegManager;
		}

		public override bool ConditionsMet()
		{
			return _enemy.GetEffectIntensity(StatusEffectType.Ballwark) >= ballwarkConversionThreshold;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			int effectIntensity = _enemy.GetEffectIntensity(StatusEffectType.Ballwark);
			_pegManager.BreakAllShieldPegs();
			_enemy.RemoveEffect(StatusEffectType.Ballwark);
			return _enemy.RangedAttack(target, Mathf.Ceil((float)Mathf.RoundToInt((float)effectIntensity * damageModifier) * _cruciballManager.GetMinibossDamageModifier()), ballwarkProjectile.gameObject, "attack_alt");
		}
	}
}
