using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class PachinkoPegSlimer : MonoBehaviour
	{
		public int slimeEveryXPegs = 5;

		public bool slimeFirstPegHit;

		public Peg.SlimeType slimeType;

		private int _currentHits;

		private bool _hasHitPeg;

		public void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("SLIME_X_PEGS_HIT", slimeEveryXPegs.ToString());
			}
		}

		public bool ShouldSlimeCurrentPeg(Peg currentPeg)
		{
			bool result = false;
			if (slimeType > currentPeg.slimeType)
			{
				_currentHits++;
				if (_currentHits >= slimeEveryXPegs)
				{
					_currentHits = 0;
					result = true;
				}
				if (slimeFirstPegHit && !_hasHitPeg)
				{
					_hasHitPeg = true;
					result = true;
				}
			}
			return result;
		}
	}
}
