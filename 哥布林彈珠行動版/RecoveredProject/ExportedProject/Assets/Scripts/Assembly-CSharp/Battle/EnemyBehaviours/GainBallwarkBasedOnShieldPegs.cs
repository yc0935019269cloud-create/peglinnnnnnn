using Battle.Enemies;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class GainBallwarkBasedOnShieldPegs : MonoBehaviour
	{
		private Enemy _enemy;

		private PegManager _pegManager;

		public int pegToBallwarkMod = 5;

		private void Start()
		{
			_enemy = GetComponent<Enemy>();
			_pegManager = Object.FindObjectOfType<BattleController>().pegManager;
		}

		public void ApplyBallwarkBasedOnShieldPegs()
		{
			if (_enemy == null)
			{
				_enemy = GetComponent<Enemy>();
			}
			_enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, _pegManager.CountShieldPegsOnBoard().Item1 * pegToBallwarkMod), StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
		}
	}
}
