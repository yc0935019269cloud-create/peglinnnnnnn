using System;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ActivateCritOnPegHits : MonoBehaviour
	{
		public delegate void CritOnPegHitActivated();

		public static CritOnPegHitActivated OnCritOnPegHitActivated = () =>
		{
		};

		private PachinkoBall _pachinkoBall;

		public bool critOnFirstHit;

		public int hitsUntilCrit = 7;

		private int _currentHits;

		public int critLimit = 10;

		private int _critCount;

		private bool _hasHitFirstPeg;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue("CRIT_PEG_COUNT", hitsUntilCrit.ToString());
			component.SetParameterValue("CRIT_LIMIT", critLimit.ToString());
		}

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
		}

		private void OnEnable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void OnDisable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (!_pachinkoBall.IsDummy && peg.pegType == Peg.PegType.REGULAR)
			{
				if (critOnFirstHit && !_hasHitFirstPeg)
				{
					OnCritOnPegHitActivated();
					_hasHitFirstPeg = true;
				}
				else if (++_currentHits >= hitsUntilCrit && _critCount < critLimit)
				{
					OnCritOnPegHitActivated();
					_currentHits = 0;
					_critCount++;
				}
			}
		}
	}
}
