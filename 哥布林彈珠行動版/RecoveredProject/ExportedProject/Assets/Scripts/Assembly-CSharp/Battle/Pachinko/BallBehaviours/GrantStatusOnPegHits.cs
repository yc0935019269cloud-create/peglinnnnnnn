using System;
using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class GrantStatusOnPegHits : MonoBehaviour
	{
		public delegate void PegHitEvent(Peg peg, StatusEffectType effectType, int intensity);

		public static PegHitEvent OnPegHitStatusEffectApplied = delegate
		{
		};

		public RelicManager relicManager;

		private PachinkoBall _pachinkoBall;

		private PlayerStatusEffectController _playerStatusEffectController;

		public StatusEffect statusToAdd;

		public string pegHitsParamString = "";

		public string intensityParamString = "";

		public int hitsUntilAddStatus = 5;

		private int _currentHits;

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_playerStatusEffectController = UnityEngine.Object.FindObjectOfType<PlayerStatusEffectController>();
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

		public virtual void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (_pachinkoBall.IsDummy || peg.pegType != Peg.PegType.REGULAR || ++_currentHits < hitsUntilAddStatus)
			{
				return;
			}
			_currentHits = 0;
			if (_playerStatusEffectController != null)
			{
				int num = (relicManager.AttemptUseRelic(RelicEffect.INCREASE_POSITIVE_STATUS) ? (statusToAdd.Intensity + 1) : statusToAdd.Intensity);
				if (relicManager.RelicEffectActive(RelicEffect.DOUBLE_STATUS_EFFECTS))
				{
					num = Mathf.RoundToInt((float)num * 2f);
				}
				OnPegHitStatusEffectApplied?.Invoke(peg, statusToAdd.EffectType, num);
				_playerStatusEffectController.ApplyStatusEffect(statusToAdd, StatusEffectSource.PLAYER);
			}
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				if (!string.IsNullOrEmpty(pegHitsParamString))
				{
					component.SetParameterValue(pegHitsParamString, hitsUntilAddStatus.ToString());
				}
				if (!string.IsNullOrEmpty(intensityParamString) && statusToAdd != null)
				{
					component.SetParameterValue(intensityParamString, statusToAdd.Intensity.ToString());
				}
			}
		}
	}
}
