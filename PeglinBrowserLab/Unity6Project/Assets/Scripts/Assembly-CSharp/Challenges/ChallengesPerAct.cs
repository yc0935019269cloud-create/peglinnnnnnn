using System.Collections.Generic;
using UnityEngine;

namespace Challenges
{
	[CreateAssetMenu(fileName = "NewChallengesPerAct", menuName = "Relics/ChallengePerAct")]
	public class ChallengesPerAct : ScriptableObject
	{
		[SerializeField]
		private ChallengeData _forestChallenge;

		[SerializeField]
		private ChallengeData _castleChallenge;

		[SerializeField]
		private ChallengeData _minesChallenge;

		[SerializeField]
		private ChallengeData _coreChallenge;

		public ChallengeData forestChallenge => _forestChallenge;

		public ChallengeData castleChallenge => _castleChallenge;

		public ChallengeData minesChallenge => _minesChallenge;

		public ChallengeData coreChallenge => _coreChallenge;

		public ChallengeData[] allDebugChallenges
		{
			get
			{
				List<ChallengeData> list = new List<ChallengeData>();
				if (_forestChallenge != null && _forestChallenge.effect != ChallengeEffect.NONE)
				{
					list.Add(_forestChallenge);
				}
				if (_castleChallenge != null && _castleChallenge.effect != ChallengeEffect.NONE)
				{
					list.Add(_castleChallenge);
				}
				if (_minesChallenge != null && _minesChallenge.effect != ChallengeEffect.NONE)
				{
					list.Add(_minesChallenge);
				}
				if (_coreChallenge != null && _coreChallenge.effect != ChallengeEffect.NONE)
				{
					list.Add(_coreChallenge);
				}
				return list.ToArray();
			}
		}

		public ChallengeEffect[] challengesEffectsPerAct
		{
			get
			{
				List<ChallengeEffect> list = new List<ChallengeEffect>();
				if (_forestChallenge == null)
				{
					list.Add(ChallengeEffect.NONE);
				}
				else
				{
					list.Add(_forestChallenge.effect);
				}
				if (_castleChallenge == null)
				{
					list.Add(ChallengeEffect.NONE);
				}
				else
				{
					list.Add(_castleChallenge.effect);
				}
				if (_minesChallenge == null)
				{
					list.Add(ChallengeEffect.NONE);
				}
				else
				{
					list.Add(_minesChallenge.effect);
				}
				if (_coreChallenge == null)
				{
					list.Add(ChallengeEffect.NONE);
				}
				else
				{
					list.Add(_coreChallenge.effect);
				}
				return list.ToArray();
			}
		}
	}
}
