using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Map.Doodads
{
	public class HoveringPlatform : MonoBehaviour
	{
		public float occolationSpeed = 2f;

		public float occolationSpeedRandomOffset = 0.5f;

		public float amplitude = 0.1f;

		public float amplitudeRandomOffset = 0.01f;

		public float radius = 0.2f;

		public float steppedOnMaxBounceDistance = 1f;

		public float bounceDownLength = 0.15f;

		public float bounceUpLength = 0.25f;

		private GameObject mapGoblin;

		private float _homeYPosition;

		private float _t;

		private bool _bouncing;

		private bool _hasBounced;

		private void Awake()
		{
			_homeYPosition = base.transform.position.y;
			occolationSpeed += UnityEngine.Random.Range(0f - occolationSpeedRandomOffset, occolationSpeedRandomOffset);
			_t = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			SetPositionBasedOnOccolation(_t);
		}

		private void OnEnable()
		{
			_hasBounced = false;
		}

		private void Update()
		{
			if (!_bouncing)
			{
				if (mapGoblin == null)
				{
					mapGoblin = GameObject.FindGameObjectWithTag("Player");
				}
				if (!_hasBounced && Mathf.Abs(mapGoblin.transform.position.y - 0.5f - (_homeYPosition + radius)) < 0.05f)
				{
					DoStepedOnBounce();
					return;
				}
				_t += Time.deltaTime * occolationSpeed;
				_t %= MathF.PI * 2f;
				SetPositionBasedOnOccolation(_t);
			}
		}

		private void SetPositionBasedOnOccolation(float new_t)
		{
			base.transform.position = new Vector3(base.transform.position.x, _homeYPosition - amplitude / 2f + Mathf.Sin(_t) * amplitude, base.transform.position.z);
		}

		[Button(null, EButtonEnableMode.Always)]
		private void DoStepedOnBounce()
		{
			_hasBounced = true;
			_bouncing = true;
			Sequence sequence = DOTween.Sequence();
			sequence.Append(base.transform.DOMoveY(base.transform.position.y - steppedOnMaxBounceDistance, bounceDownLength).SetEase(Ease.OutCubic));
			sequence.Append(base.transform.DOMoveY(_homeYPosition, bounceUpLength).SetEase(Ease.InOutSine).OnComplete(() =>
			{
				OnBounceComplete();
			}));
			sequence.Play();
		}

		private void OnBounceComplete()
		{
			_t = 0.5f;
			_bouncing = false;
		}
	}
}
