using Peglin.ClassSystem;
using UnityEngine;

namespace PeglinDebug
{
	public class DebugPopulateOrbs : MonoBehaviour
	{
		public DeckManager deckManager;

		public Class playerClass;

		public ClassLoadoutData data;

		private void Awake()
		{
			deckManager.SetupClassOrbPools(playerClass);
		}
	}
}
