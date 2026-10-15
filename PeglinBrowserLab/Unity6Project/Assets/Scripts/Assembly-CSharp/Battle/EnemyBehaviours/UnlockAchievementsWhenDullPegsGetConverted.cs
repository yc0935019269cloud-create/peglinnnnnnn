using System.Collections;
using System.Collections.Generic;
using Peglin.Achievements;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class UnlockAchievementsWhenDullPegsGetConverted : MonoBehaviour
	{
		private List<Peg> dullPegs = new List<Peg>();

		private List<Peg> pegsToClear = new List<Peg>();

		private bool _shouldCheckAchievement;

		private int _totalCount;

		private const int DULL_PEGS_TO_CLEAR = 3;

		private IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_shouldCheckAchievement = true;
			foreach (Peg allPeg in Object.FindObjectOfType<BattleController>().pegManager.allPegs)
			{
				if (allPeg.pegType == Peg.PegType.DULL)
				{
					dullPegs.Add(allPeg);
				}
			}
			if (dullPegs.Count <= 0 || AchievementData.Achievements[AchievementData.AchievementId.CONVERT_OR_DESTROY_DULL_PEGS_VS_DEMON_WALL].Unlocked)
			{
				_shouldCheckAchievement = false;
				Object.Destroy(this);
			}
			_totalCount = dullPegs.Count;
		}

		private void Update()
		{
			if (!_shouldCheckAchievement)
			{
				return;
			}
			foreach (Peg dullPeg in dullPegs)
			{
				if (dullPeg.pegType != Peg.PegType.DULL)
				{
					pegsToClear.Add(dullPeg);
				}
			}
			if (pegsToClear.Count > 0)
			{
				foreach (Peg item in pegsToClear)
				{
					dullPegs.Remove(item);
				}
				pegsToClear.Clear();
			}
			if (dullPegs.Count <= _totalCount - 3 && _shouldCheckAchievement)
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.CONVERT_OR_DESTROY_DULL_PEGS_VS_DEMON_WALL);
				_shouldCheckAchievement = false;
			}
		}
	}
}
