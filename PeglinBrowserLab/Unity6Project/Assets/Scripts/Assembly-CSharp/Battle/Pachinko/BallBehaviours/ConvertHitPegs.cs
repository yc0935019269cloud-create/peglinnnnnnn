using System;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ConvertHitPegs : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		[Header("Conversion Params")]
		public Peg.PegType convertTo = Peg.PegType.BOMB;

		public bool makeRiggedBomb;

		public bool convertsFirstPeg;

		public int hitsUntilConversion = 8;

		private int _currentHits;

		private bool _hasHitPeg;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("CONVERT_COUNT", hitsUntilConversion.ToString());
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager   ");
			}
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
			if (!_pachinkoBall.IsDummy && peg.SupportsPegType(convertTo))
			{
				if (convertsFirstPeg && !_hasHitPeg)
				{
					DoConversion(peg);
					_hasHitPeg = true;
				}
				else if (++_currentHits >= hitsUntilConversion)
				{
					DoConversion(peg);
					_currentHits = 0;
				}
			}
		}

		private void DoConversion(Peg peg)
		{
			if (convertTo == Peg.PegType.BOMB)
			{
				peg.StartDelayedBombConversion(makeRiggedBomb);
				Peg.OnPegTypeConverted?.Invoke(convertTo, peg.GetCenterOfPeg(), makeRiggedBomb);
			}
			else if (convertTo == Peg.PegType.CRIT || convertTo == Peg.PegType.RESET)
			{
				peg.ConvertPegToType(convertTo);
				peg.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
				Peg.OnPegTypeConverted?.Invoke(convertTo, peg.GetCenterOfPeg());
			}
			else
			{
				peg.ConvertPegToType(convertTo);
				Peg.OnPegTypeConverted?.Invoke(convertTo, peg.GetCenterOfPeg());
			}
		}
	}
}
