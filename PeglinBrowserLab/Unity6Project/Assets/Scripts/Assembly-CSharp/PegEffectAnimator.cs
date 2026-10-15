using System;
using System.Collections;
using Battle;
using DG.Tweening;
using UnityEngine;

public class PegEffectAnimator : MonoBehaviour
{
	public delegate void EffectAnimationFinished();

	public EffectAnimationFinished OnFlyToOrFromPegAnimationFinished = () =>
	{
	};

	public EffectAnimationFinished OnExplodeAnimationFinished = () =>
	{
	};

	[Tooltip("Time between reajusting the flight path of the effect if the peg it's targeting has moved.")]
	private float _flightTimer;

	private Tween _flightTween;

	private Peg _pegToConvertWhenFinished;

	private bool _havePegToConvert;

	private float _effectLength;

	private Vector3 _startPosition;

	private bool _midFlight;

	public void DoExplodeOutFromPoint(Vector2 startPosition, float explodeOutDistance, float preDelay, float effectLength, float postDelay)
	{
		StartCoroutine(AnimateExplode(startPosition, explodeOutDistance, preDelay, effectLength, postDelay));
	}

	private IEnumerator AnimateExplode(Vector2 startPosition, float explodeOutDistance, float preDelay, float effectLength, float postDelay)
	{
		float f = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		Vector2 endPosition = startPosition + new Vector2(Mathf.Cos(f), Mathf.Sin(f)).normalized * explodeOutDistance;
		base.transform.position = startPosition;
		yield return new WaitForSeconds(preDelay);
		base.transform.DOMove(endPosition, effectLength).SetEase(Ease.OutQuart);
		yield return new WaitForSeconds(effectLength);
		yield return new WaitForSeconds(postDelay);
		OnExplodeAnimationFinished?.Invoke();
	}

	public void DoFlyToOrFromPegEffect(Vector2 startPosition, Vector2 endPosition, float preDelay, float effectLength, float postDelay, Peg pegToConvertWhenFinished = null, Peg.PegType pegType = Peg.PegType.NONE)
	{
		StartCoroutine(AnimateFlyToOrFromPeg(startPosition, endPosition, preDelay, effectLength, postDelay, pegToConvertWhenFinished, pegType));
	}

	private IEnumerator AnimateFlyToOrFromPeg(Vector2 startPosition, Vector2 endPosition, float preDelay, float effectLength, float postDelay, Peg pegToConvertWhenFinished = null, Peg.PegType pegType = Peg.PegType.NONE)
	{
		_effectLength = effectLength;
		_startPosition = startPosition;
		_pegToConvertWhenFinished = pegToConvertWhenFinished;
		_havePegToConvert = _pegToConvertWhenFinished != null;
		base.transform.position = startPosition;
		yield return new WaitForSeconds(preDelay);
		if (_havePegToConvert)
		{
			_flightTimer = 0f;
			_midFlight = true;
		}
		else
		{
			_flightTween = base.transform.DOMove(endPosition, effectLength).SetEase(Ease.OutSine);
		}
		yield return new WaitForSeconds(effectLength);
		_midFlight = false;
		yield return new WaitForSeconds(postDelay);
		if (pegToConvertWhenFinished != null && pegToConvertWhenFinished.pegType != pegType)
		{
			if (pegType == Peg.PegType.SPINFECTION)
			{
				pegToConvertWhenFinished.Reset(BattleController.criticalActive);
			}
			pegToConvertWhenFinished.ConvertPegToType(pegType);
			if (pegType == Peg.PegType.REGULAR)
			{
				pegToConvertWhenFinished.Reset(BattleController.criticalActive);
			}
		}
		OnFlyToOrFromPegAnimationFinished?.Invoke();
	}

	private void Update()
	{
		if (_midFlight && _havePegToConvert && _flightTween == null)
		{
			_flightTimer += Time.deltaTime;
			float t = Mathf.Sin(_flightTimer / _effectLength * MathF.PI / 2f);
			base.transform.position = Vector3.Lerp(_startPosition, _pegToConvertWhenFinished.GetCenterOfPeg(), t);
		}
	}
}
