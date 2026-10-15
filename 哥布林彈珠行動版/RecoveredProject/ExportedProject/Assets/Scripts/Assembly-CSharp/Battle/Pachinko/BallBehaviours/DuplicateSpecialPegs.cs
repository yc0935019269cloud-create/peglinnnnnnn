using System;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class DuplicateSpecialPegs : MonoBehaviour
	{
		public delegate void DuplicatorbHitSpecialPeg(Peg peg, int numberOfDuplicatesToAdd);

		public static DuplicatorbHitSpecialPeg OnDuplicatorbHitSpecialPeg = (Peg peg, int numberOfDuplicatesToAdd) =>
		{
		};

		private PachinkoBall _pachinkoBall;

		[Range(1f, 10f)]
		public int numberOfDuplicatesToAdd = 1;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("NUM_DUPLICATES_TO_ADD", numberOfDuplicatesToAdd.ToString());
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

		public bool CanDuplicatePeg(Peg peg)
		{
			if (peg.pegType == Peg.PegType.CRIT || peg.pegType == Peg.PegType.RESET)
			{
				return !peg.isBonusSpecialPeg;
			}
			return false;
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (!_pachinkoBall.IsDummy && CanDuplicatePeg(peg))
			{
				OnDuplicatorbHitSpecialPeg?.Invoke(peg, numberOfDuplicatesToAdd);
			}
		}
	}
}
