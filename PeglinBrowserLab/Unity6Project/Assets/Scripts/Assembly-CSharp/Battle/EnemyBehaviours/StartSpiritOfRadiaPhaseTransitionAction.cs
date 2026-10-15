using System.Collections;
using Battle.Enemies;
using Peglin.Achievements;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class StartSpiritOfRadiaPhaseTransitionAction : AlternateEnemyAction
	{
		private SpiritOfRadiaBoss _spiritBoss;

		private void Start()
		{
			_spiritBoss = _enemy as SpiritOfRadiaBoss;
		}

		public override bool ConditionsMet()
		{
			if (SpiritOfRadiaBoss.CurrentPhase == 1)
			{
				if (_spiritBoss.crystalPegCount > 0)
				{
					return BattleController.RoundCount >= _spiritBoss.transitionOnTurnNumber;
				}
				return true;
			}
			return false;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			if (_spiritBoss.crystalPegCount <= 0)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.END_FIRST_ORBSERVER_PHASE_EARLY);
			}
			_enemy.CurrentlyActing = true;
			Debug.Log("Calling TakeAction");
			StartCoroutine(_spiritBoss.StartPhase2PreTransition());
			yield return null;
		}
	}
}
