using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Challenges
{
	[CreateAssetMenu(fileName = "NewChallengeSet", menuName = "Relics/ChallengeSet")]
	public class ChallengeSet : ScriptableObject
	{
		[ReorderableList]
		[SerializeField]
		private List<ChallengeData> _challenges;

		public List<ChallengeData> challenges => _challenges;

		private void OnValidate()
		{
			if (_challenges.Count() != _challenges.Distinct().Count())
			{
				Debug.LogWarning("Challenge Set " + base.name + " contains duplicate items.");
			}
		}

		[Button(null, EButtonEnableMode.Always)]
		private void ClearList()
		{
			_challenges.Clear();
		}
	}
}
