using System;
using System.Collections;
using DG.Tweening;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class FlickeringPeg : MonoBehaviour, IDummiableLayerChanger, IDummyPeg
	{
		private Collider2D _collider;

		private Peg _peg;

		private SpriteRenderer _spriteRenderer;

		public int pegNoCollisionLayer = 17;

		public int pegCollisionLayer = 15;

		private float _alphaCollisionThreshold = 0.2f;

		public float initialDelay;

		public bool randomizeInitialDelay;

		public float minDelay = 0.4f;

		public float maxDelay = 1.2f;

		public float minDelayWhenInvisible;

		public float maxDelayWhenInvisible;

		public bool useDifferentMinAndMaxForInvisible;

		public float fadeTime = 0.8f;

		[Tooltip("Are we starting from an alpha of 1 or 0?")]
		public bool fromInvis;

		[HideInInspector]
		public string id = string.Empty;

		private bool _isDummy;

		private Sequence _fadeSeq;

		private IDummyPeg _dummyPeg;

		private void Awake()
		{
			_collider = GetComponent<Collider2D>();
			_peg = GetComponent<Peg>();
			_dummyPeg = GetComponent<IDummyPeg>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}

		private void OnDisable()
		{
			StopFlicker();
		}

		private void Start()
		{
			if (!_isDummy)
			{
				if (id.IsNullOrWhitespace())
				{
					id = Guid.NewGuid().ToString();
				}
				if (!useDifferentMinAndMaxForInvisible)
				{
					minDelayWhenInvisible = minDelay;
					maxDelayWhenInvisible = maxDelay;
				}
				StartCoroutine(DelayStartSeq());
			}
		}

		private IEnumerator DelayStartSeq()
		{
			if (initialDelay > 0f)
			{
				float seconds = initialDelay;
				if (randomizeInitialDelay)
				{
					seconds = UnityEngine.Random.Range(0f, initialDelay);
				}
				yield return new WaitForSeconds(seconds);
			}
			if (!_isDummy && _spriteRenderer != null)
			{
				Peg component = GetComponent<Peg>();
				if (component != null && component.slimeType == Peg.SlimeType.None)
				{
					StartFlickerSequence();
				}
			}
		}

		public void StartFlickerSequence()
		{
			float endValue = (fromInvis ? 1f : 0f);
			float minInclusive = (fromInvis ? minDelayWhenInvisible : minDelay);
			float maxInclusive = (fromInvis ? maxDelayWhenInvisible : maxDelay);
			float minInclusive2 = (fromInvis ? minDelay : minDelayWhenInvisible);
			float maxInclusive2 = (fromInvis ? maxDelay : maxDelayWhenInvisible);
			_fadeSeq = DOTween.Sequence();
			_fadeSeq.AppendInterval(UnityEngine.Random.Range(minInclusive, maxInclusive));
			_fadeSeq.Append(_spriteRenderer.DOFade(endValue, fadeTime).SetEase(Ease.InOutSine));
			_fadeSeq.AppendInterval(UnityEngine.Random.Range(minInclusive2, maxInclusive2));
			_fadeSeq.SetLoops(-1, LoopType.Yoyo);
			_fadeSeq.Play();
		}

		public void StopFlicker()
		{
			_fadeSeq?.Kill();
		}

		private void Update()
		{
			if (!_isDummy && !(_collider == null))
			{
				float a = _spriteRenderer.color.a;
				bool flag = _spriteRenderer.color.a >= _alphaCollisionThreshold;
				base.gameObject.layer = (flag ? pegCollisionLayer : pegNoCollisionLayer);
				_peg.UpdateOverlayAlpha(a);
			}
		}

		public void GenerateId(bool force = false)
		{
			if (id.IsNullOrWhitespace() | force)
			{
				id = Guid.NewGuid().ToString();
			}
		}

		public string GetId()
		{
			return id;
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
			if (_fadeSeq != null && _isDummy)
			{
				_fadeSeq.Kill();
			}
		}

		public void SetPegStatus(bool status)
		{
		}

		public bool GetPegStatus()
		{
			return true;
		}

		public bool IsSpecialPeg()
		{
			return false;
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public Peg.PegType GetPegType()
		{
			return Peg.PegType.NONE;
		}

		public int GetLayer()
		{
			return base.gameObject.layer;
		}

		public void SetLayer(int layer)
		{
			base.gameObject.layer = layer;
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}
	}
}
