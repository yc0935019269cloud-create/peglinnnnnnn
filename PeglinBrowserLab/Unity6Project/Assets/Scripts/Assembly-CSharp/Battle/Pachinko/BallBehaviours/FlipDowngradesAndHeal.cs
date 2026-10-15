using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using PeglinUtils;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class FlipDowngradesAndHeal : MonoBehaviour
	{
		public delegate void SpinDoctorbFlippedPeg(Peg peg, bool isFlippingFromNegativeToPositive);

		public static SpinDoctorbFlippedPeg OnSpinDoctorbFlippedPeg = (Peg peg, bool isFlippingFromNegativeToPositive) =>
		{
		};

		private PachinkoBall _pachinkoBall;

		private PlayerHealthController _playerHealthController;

		public AudioClip flipSFX;

		private AudioSource _audioSource;

		public int hitsNeeded = 5;

		public int flipMult = 1;

		public int healOnFlip = 1;

		public bool onlyAffectPegsOncePerShot = true;

		private int _currentBuffedPegsHit;

		private ObjectPool _effectPool;

		private List<Peg> affectedPegsList = new List<Peg>();

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
			_audioSource.clip = flipSFX;
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

		public virtual void PegHit(Peg peg, PachinkoBall pBall)
		{
			StartCoroutine(WaitAndDoubleBuffs(peg));
		}

		private IEnumerator WaitAndDoubleBuffs(Peg peg)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (!_pachinkoBall.IsDummy && peg.buffAmount != 0 && (!onlyAffectPegsOncePerShot || !affectedPegsList.Contains(peg)) && ++_currentBuffedPegsHit >= hitsNeeded)
			{
				_currentBuffedPegsHit = 0;
				bool isFlippingFromNegativeToPositive = peg.buffAmount < 0;
				peg.AddBuff(-peg.buffAmount + peg.buffAmount * -flipMult);
				if (_playerHealthController == null)
				{
					_playerHealthController = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealthController>();
				}
				if (_playerHealthController != null)
				{
					_playerHealthController.Heal(healOnFlip);
				}
				if (flipSFX != null)
				{
					_audioSource.pitch = 1f + UnityEngine.Random.Range(-0.03f, 0.03f);
					_audioSource.Play();
				}
				if (onlyAffectPegsOncePerShot)
				{
					affectedPegsList.Add(peg);
				}
				OnSpinDoctorbFlippedPeg?.Invoke(peg, isFlippingFromNegativeToPositive);
			}
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("NUM_HITS_NEEDED", hitsNeeded.ToString(CultureInfo.InvariantCulture));
				component.SetParameterValue("FLIP_MULT", flipMult.ToString(CultureInfo.InvariantCulture));
				component.SetParameterValue("FLIP_HEAL_AMOUNT", healOnFlip.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
