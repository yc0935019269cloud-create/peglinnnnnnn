using AssetIcons;
using UnityEngine;

namespace Challenges
{
	[CreateAssetMenu(fileName = "NewChallenge", menuName = "Relics/Challenge")]
	public class ChallengeData : ScriptableObject
	{
		public string locKey;

		[AssetIcon("100%", "100%", "0", "0", 64, IconAnchor.Center, IconAspect.Fit, "true", "#ffffff", 0, FontStyle.Normal, IconAnchor.Center, IconProjection.Perspective, -1, null)]
		public Sprite sprite;

		public ChallengeEffect effect;

		public string nameKey => "Challenges/" + locKey + "_name";

		public string descKey => "Challenges/" + locKey + "_desc";
	}
}
