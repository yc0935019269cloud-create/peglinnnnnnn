using System.Collections.Generic;
using Challenges;
using MoreMountains.Tools;
using PeglinUI.LoadoutManager;
using Stats;
using UnityEngine;

namespace PeglinUI.Encirclepedia
{
	public class ChallengesDisplay : MonoBehaviour
	{
		public ChallengeSet globalChallenges;

		public GameObject challengeCapsulePrefab;

		public void SetChallenges(RunStats stats)
		{
			base.transform.MMDestroyAllChildren();
			if (stats.challenges == null)
			{
				return;
			}
			List<ChallengeEffect> challenges = stats.challenges;
			for (int i = 0; i < challenges.Count; i++)
			{
				if (challenges[i] != ChallengeEffect.NONE)
				{
					LoadoutIcon componentInChildren = Object.Instantiate(challengeCapsulePrefab, base.transform).GetComponentInChildren<LoadoutIcon>();
					ChallengeData dataForChallengeFromGlobal = ChallengeManager.GetDataForChallengeFromGlobal(stats.challenges[i], globalChallenges);
					if (dataForChallengeFromGlobal == null)
					{
						Debug.LogError($"Challenge {stats.challenges[i]} does not exist in globalChallenges.");
					}
					else
					{
						componentInChildren.InitializeChallenge(dataForChallengeFromGlobal);
					}
				}
			}
		}
	}
}
