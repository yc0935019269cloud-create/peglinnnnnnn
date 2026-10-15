using System;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class RefreshBoardOnPegHits : MonoBehaviour
	{
		private BattleController _battleController;

		private PachinkoBall _pachinkoBall;

		public bool refreshesOnFirstHit;

		public int hitsUntilRefresh = 7;

		private int _currentHits;

		public int refreshLimit = 10;

		private int _refreshCount;

		private bool _hasHitFirstPeg;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue("REFRESH_PEG_COUNT", hitsUntilRefresh.ToString());
			component.SetParameterValue("REFRESH_LIMIT", refreshLimit.ToString());
		}

		private void Awake()
		{
			_battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
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
				if (refreshesOnFirstHit && !_hasHitFirstPeg)
				{
					ResetBoard();
					_hasHitFirstPeg = true;
				}
				else if (++_currentHits >= hitsUntilRefresh && _refreshCount < refreshLimit)
				{
					ResetBoard();
					_currentHits = 0;
					_refreshCount++;
				}
			}
		}

		private void ResetBoard()
		{
			if (_battleController != null)
			{
				_battleController.ResetField();
			}
		}
	}
}
