using NaughtyAttributes;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class DontPopSpecialPegsOnHit : MonoBehaviour
	{
		[EnumFlags]
		public Peg.PegType dontPopTheseTypes = Peg.PegType.CRIT;

		public bool dontPopPegsWithCoins;

		public bool ignorePegsWithCoins;

		public bool completelyIgnore;
	}
}
