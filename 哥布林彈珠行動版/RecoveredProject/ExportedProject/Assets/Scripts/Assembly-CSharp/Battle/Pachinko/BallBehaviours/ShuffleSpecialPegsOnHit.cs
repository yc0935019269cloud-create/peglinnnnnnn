using System;
using System.Globalization;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ShuffleSpecialPegsOnHit : MonoBehaviour
	{
		private BattleController _battleController;

		private PachinkoBall _pachinkoBall;

		public int hitsUntilRefresh = 5;

		public int refreshPerShotLimit = 10;

		private int _currentHits;

		private int _numShuffles;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue(LocParamKeys.SHUFFLE_HITS, hitsUntilRefresh.ToString(CultureInfo.InvariantCulture));
			component.SetParameterValue("SHUFFLE_LIMIT", refreshPerShotLimit.ToString(CultureInfo.InvariantCulture));
		}

		private void Awake()
		{
			_battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
			_pachinkoBall = GetComponent<PachinkoBall>();
		}

		private void OnEnable()
		{
			if (_pachinkoBall != null)
			{
				PachinkoBall pachinkoBall = _pachinkoBall;
				pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			}
		}

		private void OnDisable()
		{
			if (_pachinkoBall != null)
			{
				PachinkoBall pachinkoBall = _pachinkoBall;
				pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			}
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (!(peg == null) && _numShuffles < refreshPerShotLimit && _battleController != null && ++_currentHits >= hitsUntilRefresh && peg.pegType == Peg.PegType.REGULAR)
			{
				_battleController.pegManager.ShuffleSpecialPegs(forceRefresh: true);
				_currentHits = 0;
				_numShuffles++;
			}
		}
	}
}
