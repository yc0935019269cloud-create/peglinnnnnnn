using System;
using System.Collections.Generic;
using Battle.Enemies;
using Relics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PegBoardFrames
{
	public class PegBoardWallsCollisionHandler : MonoBehaviour
	{
		private const float DEFAULT_ORB_MASS = 1.2f;

		private const float SIDE_WALL_SMALL_FORCE = 10f;

		public RelicManager _relicManager;

		public bool ignoreVerticalCollisionCheck;

		private float _numReverseGravityCielingBounces;

		private float _flatExtraForce;

		private float _forceMultiplier = 1f;

		private void Awake()
		{
			_flatExtraForce = 0f;
			if (SceneManager.GetActiveScene().name == "PegMinigame")
			{
				_flatExtraForce = 300f;
			}
		}

		private void OnEnable()
		{
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetBounceCount));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(SpiritOfRadiaPhase2Started));
		}

		private void OnDisable()
		{
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetBounceCount));
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(SpiritOfRadiaPhase2Started));
		}

		private void ResetBounceCount(Vector2 aimVector)
		{
			_numReverseGravityCielingBounces = 0f;
		}

		private void SpiritOfRadiaPhase2Started()
		{
			_forceMultiplier = 2f;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			OnCollisionEnterOrStay(collision);
		}

		private void OnCollisionStay2D(Collision2D collision)
		{
			OnCollisionEnterOrStay(collision);
		}

		private void OnCollisionEnterOrStay(Collision2D collision)
		{
			PachinkoBall component = collision.collider.GetComponent<PachinkoBall>();
			bool flag = false;
			List<ContactPoint2D> list = new List<ContactPoint2D>();
			collision.GetContacts(list);
			foreach (ContactPoint2D item in list)
			{
				if (ignoreVerticalCollisionCheck || (item.normal - Vector2.up).sqrMagnitude < 0.2f)
				{
					flag = true;
				}
				else if ((item.normal - Vector2.right).sqrMagnitude < 0.2f || (item.normal + Vector2.right).sqrMagnitude < 0.2f)
				{
					collision.rigidbody.AddForce(-item.normal * 10f);
				}
			}
			if (((component != null) & flag) && _relicManager.RelicEffectActive(RelicEffect.REVERSE_GRAVITY))
			{
				if (!component.IsDummy)
				{
					_relicManager.AttemptUseRelic(RelicEffect.REVERSE_GRAVITY);
				}
				Rigidbody2D component2 = component.GetComponent<Rigidbody2D>();
				component2.AddForce(Vector2.down * (845f + 12f * _numReverseGravityCielingBounces + _flatExtraForce) * (component2.mass / 1.2f) * _forceMultiplier);
				if (!component.IsDummy)
				{
					_numReverseGravityCielingBounces += 1f * (component2.mass / 1.2f);
					_numReverseGravityCielingBounces = Mathf.Min(_numReverseGravityCielingBounces, 20f);
				}
			}
		}
	}
}
