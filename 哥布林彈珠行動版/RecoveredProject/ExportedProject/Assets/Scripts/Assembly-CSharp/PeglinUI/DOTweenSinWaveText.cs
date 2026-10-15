using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace PeglinUI
{
	public class DOTweenSinWaveText : MonoBehaviour
	{
		public Vector3 sinBounds = new Vector3(0f, 1f, 0f);

		public float moveTime = 0.5f;

		public float charDelay = 0.25f;

		private Vector3 highPoint = Vector3.up;

		private DOTweenTMPAnimator _animator;

		private TMP_Text _text;

		private List<Sequence> _sequences;

		private void Awake()
		{
			_sequences = new List<Sequence>();
			highPoint = sinBounds;
			_text = GetComponentInChildren<TMP_Text>();
			_animator = new DOTweenTMPAnimator(_text);
		}

		private void OnEnable()
		{
			RestartCoroutine();
		}

		private void OnDisable()
		{
			StopAnimation();
		}

		private void StopAnimation()
		{
			if (_animator == null)
			{
				return;
			}
			StopAllCoroutines();
			foreach (Sequence sequence in _sequences)
			{
				sequence.Kill();
			}
			_sequences.Clear();
			_animator.Refresh();
		}

		public void RestartCoroutine()
		{
			if (_animator != null)
			{
				StopAnimation();
				if (base.enabled)
				{
					StartCoroutine(SetupTextSequences());
				}
			}
		}

		private IEnumerator SetupTextSequences()
		{
			for (int i = 0; i < _text.text.Length; i++)
			{
				Sequence sequence = DOTween.Sequence();
				sequence.Append(_animator.DOOffsetChar(i, highPoint, moveTime));
				sequence.SetLoops(-1, LoopType.Yoyo);
				sequence.Play();
				_sequences.Add(sequence);
				yield return new WaitForSeconds(charDelay);
			}
		}
	}
}
