using System;
using Battle;
using Peglin.Achievements;
using Peglin.ClassSystem;
using Saving;
using UnityEngine;

namespace Achievements
{
	public class SlimeHitsAchievementChecker : MonoBehaviour
	{
		private int _slimeCounter;

		private void OnEnable()
		{
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegHit, new Peg.PegHitEvent(CountUp));
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(Reset));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(Reset));
			NavOnlyController.NavigationReset = (Action)Delegate.Combine(NavOnlyController.NavigationReset, new Action(Reset));
			BattleController.OnNavReset = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnNavReset, new BattleController.BattleEvent(Reset));
		}

		private void OnDisable()
		{
			Peg.OnPegHit = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegHit, new Peg.PegHitEvent(CountUp));
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(Reset));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(Reset));
			NavOnlyController.NavigationReset = (Action)Delegate.Remove(NavOnlyController.NavigationReset, new Action(Reset));
			BattleController.OnNavReset = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnNavReset, new BattleController.BattleEvent(Reset));
		}

		private void CountUp(Peg.PegType pegType, Peg peg)
		{
			if (peg.slimeType == Peg.SlimeType.None)
			{
				_slimeCounter = 0;
			}
			else
			{
				if (ShouldIgnoreSlimeType(peg.slimeType))
				{
					return;
				}
				_slimeCounter++;
				if (_slimeCounter >= 7)
				{
					Achievement achievement = AchievementData.Achievements[AchievementData.AchievementId.MULTIPLE_SLIME_BOUNCES];
					if (!achievement.Unlocked)
					{
						AchievementManager.Instance.UnlockAchievement(achievement);
						PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Spinventor);
					}
				}
			}
		}

		private bool ShouldIgnoreSlimeType(Peg.SlimeType type)
		{
			if (type == Peg.SlimeType.Web || type == Peg.SlimeType.FlameOverlay)
			{
				return true;
			}
			return false;
		}

		private void Reset()
		{
			_slimeCounter = 0;
		}
	}
}
