using System;
using Battle;
using UnityEngine;

namespace Peglin.Achievements
{
	public class PegCountAchievementChecker : MonoBehaviour
	{
		private static readonly int PEGS_FOR_ACHIEVEMENT = 1000;

		private int _pegCounter;

		private void OnEnable()
		{
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegHit, new Peg.PegHitEvent(CountUp));
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(Reset));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(Reset));
		}

		private void OnDisable()
		{
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegHit, new Peg.PegHitEvent(CountUp));
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(Reset));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(Reset));
		}

		private void CountUp(Peg.PegType pegType, Peg peg)
		{
			_pegCounter++;
			if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.mostPegsHitInOneTurn < _pegCounter)
			{
				StaticGameData.CurrentRunStats.mostPegsHitInOneTurn = _pegCounter;
			}
		}

		private void Reset()
		{
			if (_pegCounter >= PEGS_FOR_ACHIEVEMENT)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.Achievements[AchievementData.AchievementId.HIT_X_PEGS_IN_ONE_SHOT]);
			}
			_pegCounter = 0;
		}
	}
}
