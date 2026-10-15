using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class CreateShieldPegsOnPegHits : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		public int hitsUntilShieldPeg = 5;

		private int _currentHits;

		protected virtual void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
		}

		public virtual bool ShouldShieldCurrentPeg(Peg peg)
		{
			if (!_pachinkoBall.IsDummy && peg.pegType == Peg.PegType.REGULAR && ++_currentHits >= hitsUntilShieldPeg)
			{
				_currentHits = 0;
				return true;
			}
			return false;
		}

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("SHIELD_PEG_COUNT", hitsUntilShieldPeg.ToString());
		}
	}
}
