using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours.Coins
{
	public class MultiplyCoinsOnHitPegs : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		public float coinMultiplyer = 2f;

		public bool onlyAffectPegsOncePerShot = true;

		private List<Peg> affectedPegsList = new List<Peg>();

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("COIN_MULTIPLICATION_ON_HIT_PEG", coinMultiplyer.ToString());
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager.");
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
			if (!_pachinkoBall.IsDummy && !peg.IsSpecialPeg() && peg.pegType != Peg.PegType.BOMB && peg.NumCoins() != 0 && (!onlyAffectPegsOncePerShot || !affectedPegsList.Contains(peg)))
			{
				int num = Mathf.RoundToInt((float)peg.NumCoins() * coinMultiplyer) - peg.NumCoins();
				for (int i = 0; i < num; i++)
				{
					peg.AddCoin();
				}
				if (onlyAffectPegsOncePerShot)
				{
					affectedPegsList.Add(peg);
				}
			}
		}
	}
}
