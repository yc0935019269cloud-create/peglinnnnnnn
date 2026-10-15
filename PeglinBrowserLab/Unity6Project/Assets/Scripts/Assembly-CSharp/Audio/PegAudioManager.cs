using System;
using System.Collections.Generic;
using Battle;
using Battle.Attacks;
using Battle.PegBehaviour;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio
{
	public class PegAudioManager : MonoBehaviour
	{
		private const float RECENTLY_PLAYED_SFX_TIMER_LENGTH = 0.1f;

		private const int DEFAULT_MAX_SIMULTANIOUS_SFX_INSTANCES = 2;

		private const int PEG_MAX_SIMULTANIOUS_SFX_INSTANCES = 15;

		[SerializeField]
		private PegSfx _pegSfx;

		[SerializeField]
		private PegSlimeSfx _pegSlimeSfx;

		[SerializeField]
		private AudioClip _wallbounceSfx;

		[SerializeField]
		private AudioClip _bombExplosionSfx;

		[SerializeField]
		private float _pegAudioPitchUpAmount = 0.1f;

		[SerializeField]
		private float _pitchMax = 2.5f;

		[SerializeField]
		private AudioSource _pegAudioSource;

		[SerializeField]
		private AudioSource _sfxAudioSource;

		private BattleController _battleController;

		private int localPegCount;

		private float recentlyPlayedSFXTimer;

		private Dictionary<AudioClip, int> recentlyPlayedSFX = new Dictionary<AudioClip, int>();

		private void OnEnable()
		{
			if (_battleController == null)
			{
				_battleController = GetComponent<BattleController>();
			}
			Peg.OnPegAudioRequest = (Peg.PegAudioRequest)Delegate.Combine(Peg.OnPegAudioRequest, new Peg.PegAudioRequest(OnPegDestroyed));
			Peg.OnSlimedPegAudioRequest = (Peg.PegSlimeAudioRequest)Delegate.Combine(Peg.OnSlimedPegAudioRequest, new Peg.PegSlimeAudioRequest(OnSlimedPegHit));
			Peg.OnBonusCoinAdded = (Action)Delegate.Combine(Peg.OnBonusCoinAdded, new Action(OnCoinAdded));
			Peg.OnCoinStolen = (Action)Delegate.Combine(Peg.OnCoinStolen, new Action(OnCoinStolen));
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Combine(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(OnCoinCollected));
			PachinkoBall.OnPachinkoWallBounceAudio = (PachinkoBall.PachinkoWallBounceAudio)Delegate.Combine(PachinkoBall.OnPachinkoWallBounceAudio, new PachinkoBall.PachinkoWallBounceAudio(PlayWallBounce));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Combine(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PlayPachinkoBallDestruction));
			BombLob.OnBombExplosionPoint = (BombLob.BombExplosionPoint)Delegate.Combine(BombLob.OnBombExplosionPoint, new BombLob.BombExplosionPoint(PlayBombExplosion));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
			BattleController.OnBallCreationComplete = (Action)Delegate.Combine(BattleController.OnBallCreationComplete, new Action(OnBallCreationComplete));
		}

		private void OnDisable()
		{
			Peg.OnPegAudioRequest = (Peg.PegAudioRequest)Delegate.Remove(Peg.OnPegAudioRequest, new Peg.PegAudioRequest(OnPegDestroyed));
			Peg.OnSlimedPegAudioRequest = (Peg.PegSlimeAudioRequest)Delegate.Remove(Peg.OnSlimedPegAudioRequest, new Peg.PegSlimeAudioRequest(OnSlimedPegHit));
			Peg.OnBonusCoinAdded = (Action)Delegate.Remove(Peg.OnBonusCoinAdded, new Action(OnCoinAdded));
			Peg.OnCoinStolen = (Action)Delegate.Remove(Peg.OnCoinStolen, new Action(OnCoinStolen));
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Remove(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(OnCoinCollected));
			PachinkoBall.OnPachinkoWallBounceAudio = (PachinkoBall.PachinkoWallBounceAudio)Delegate.Remove(PachinkoBall.OnPachinkoWallBounceAudio, new PachinkoBall.PachinkoWallBounceAudio(PlayWallBounce));
			PachinkoBall.OnPachinkoBallDestroyed = (PachinkoBall.PachinkoBallDestroyed)Delegate.Remove(PachinkoBall.OnPachinkoBallDestroyed, new PachinkoBall.PachinkoBallDestroyed(PlayPachinkoBallDestruction));
			BombLob.OnBombExplosionPoint = (BombLob.BombExplosionPoint)Delegate.Remove(BombLob.OnBombExplosionPoint, new BombLob.BombExplosionPoint(PlayBombExplosion));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
			BattleController.OnBallCreationComplete = (Action)Delegate.Remove(BattleController.OnBallCreationComplete, new Action(OnBallCreationComplete));
		}

		private void Update()
		{
			recentlyPlayedSFXTimer -= Time.deltaTime;
			if (recentlyPlayedSFXTimer <= 0f)
			{
				recentlyPlayedSFX.Clear();
				recentlyPlayedSFXTimer = 0.1f;
			}
		}

		private void PlayAudioClipWithMaxOverlap(AudioSource audioSource, AudioClip audioClip, int maxOverlap, float volume = 1f)
		{
			if (!recentlyPlayedSFX.ContainsKey(audioClip))
			{
				recentlyPlayedSFX[audioClip] = 1;
			}
			else
			{
				recentlyPlayedSFX[audioClip]++;
			}
			if (recentlyPlayedSFX[audioClip] <= maxOverlap)
			{
				audioSource.PlayOneShot(audioClip, volume);
			}
		}

		private void OnPegDestroyed(Peg.PegType pegType)
		{
			if (_battleController == null)
			{
				localPegCount++;
			}
			switch (pegType)
			{
			case Peg.PegType.REGULAR:
				if (_pegAudioSource.pitch < _pitchMax)
				{
					_pegAudioSource.pitch = CalculatePitch();
				}
				PlayAudioClipWithMaxOverlap(_pegAudioSource, _pegSfx.GetAudioClip(pegType), 15);
				break;
			case Peg.PegType.DELAYED_DEATH:
				if (_pegAudioSource.pitch < _pitchMax)
				{
					_pegAudioSource.pitch = CalculatePitch();
				}
				PlayAudioClipWithMaxOverlap(_pegAudioSource, _pegSfx.GetAudioClip(pegType), 15);
				break;
			case Peg.PegType.RESET:
				PlayAudioClipWithMaxOverlap(_sfxAudioSource, _pegSfx.GetAudioClip(pegType), 2);
				break;
			case Peg.PegType.BOUNCER:
				_sfxAudioSource.PlayOneShot(_pegSfx.GetAudioClip(pegType));
				break;
			case Peg.PegType.SPINFECTION:
				_sfxAudioSource.PlayOneShot(_pegSfx.GetAudioClip(pegType));
				break;
			case Peg.PegType.CRYSTAL:
				_pegAudioSource.pitch = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
				_sfxAudioSource.PlayOneShot(_pegSfx.GetAudioClip(pegType));
				break;
			}
		}

		private void OnSlimedPegHit(Peg.SlimeType slimeType)
		{
			if (_pegSlimeSfx != null && slimeType == Peg.SlimeType.HealSlime)
			{
				PlayAudioClipWithMaxOverlap(_sfxAudioSource, _pegSlimeSfx.GetAudioClip(slimeType), 2);
			}
		}

		private void OnCoinAdded()
		{
			_sfxAudioSource.PlayOneShot(_pegSfx.GetBonusCoinAddedClip());
		}

		private void OnCoinStolen()
		{
			_sfxAudioSource.PlayOneShot(_pegSfx.CoinStolenClip);
		}

		private void OnCoinCollected(Vector3 pos)
		{
			_sfxAudioSource.PlayOneShot(_pegSfx.GetCoinCollectedClip());
		}

		private void OnCritActivated()
		{
			_sfxAudioSource.PlayOneShot(_pegSfx.GetAudioClip(Peg.PegType.CRIT));
		}

		private void PlayWallBounce(float vol)
		{
			PlayAudioClipWithMaxOverlap(_sfxAudioSource, _wallbounceSfx, 2, vol);
		}

		private void PlayPachinkoBallDestruction(PachinkoBall pBall)
		{
			if (SceneManager.GetActiveScene().name != "MainMenu")
			{
				_sfxAudioSource.PlayOneShot(_pegSfx.BallDestructionSfx);
			}
		}

		private void PlayBombExplosion()
		{
			_sfxAudioSource.PlayOneShot(_bombExplosionSfx);
		}

		private float CalculatePitch()
		{
			if (_battleController != null)
			{
				return Mathf.Clamp(1f + _pegAudioPitchUpAmount * (float)_battleController.numPegsHit, 1f, _pitchMax);
			}
			return Mathf.Clamp(1f + _pegAudioPitchUpAmount * (float)localPegCount, 1f, _pitchMax);
		}

		private void OnBallCreationComplete()
		{
			_pegAudioSource.pitch = 1f;
		}
	}
}
