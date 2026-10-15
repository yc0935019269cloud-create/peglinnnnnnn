using System;
using System.Globalization;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours.Coins
{
	public class AddCoinsToHitPegs : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		private DontPopSpecialPegsOnHit _dontPop;

		public bool addToFirstPeg;

		public int hitsUntilAdd = 10;

		private int _currentHits;

		private bool _hasHitPeg;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("HIT_COUNT", hitsUntilAdd.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager.");
			}
		}

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_dontPop = GetComponent<DontPopSpecialPegsOnHit>();
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
			if (!_pachinkoBall.IsDummy && !peg.IsSpecialPeg() && peg.pegType != Peg.PegType.BOMB && (!(_dontPop != null) || (!_dontPop.ignorePegsWithCoins && !_dontPop.dontPopPegsWithCoins) || peg.NumCoins() <= 0))
			{
				if (addToFirstPeg && !_hasHitPeg)
				{
					peg.AddCoin();
					_hasHitPeg = true;
				}
				else if (++_currentHits >= hitsUntilAdd)
				{
					peg.AddCoin();
					_currentHits = 0;
				}
			}
		}
	}
}
