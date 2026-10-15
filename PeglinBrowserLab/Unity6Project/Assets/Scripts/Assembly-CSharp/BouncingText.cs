using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class BouncingText : FloatingText
{
	[SerializeField]
	private float _maxHeight = 1f;

	[SerializeField]
	private float _maxWidth = 2f;

	[SerializeField]
	private float _minWidth = 0.5f;

	[SerializeField]
	private float _endScale = 0.4f;

	[SerializeField]
	private float _fadeDurationDelay = 0.5f;

	[SerializeField]
	private float _fadeDuration = 0.7f;

	private List<Tween> _tweens = new List<Tween>();

	protected override void Move()
	{
		float num = UnityEngine.Random.Range(0f - _maxWidth, _maxWidth);
		num = ((num >= 0f) ? Mathf.Clamp(num, _minWidth, _maxWidth) : Mathf.Clamp(num, 0f - _maxWidth, 0f - _minWidth));
		float y = UnityEngine.Random.Range(_maxHeight * 0.5f, _maxHeight);
		Vector2 vector = base.transform.position;
		Vector2 vector2 = vector + new Vector2(num / 2f, y);
		Vector2 vector3 = vector + new Vector2(num, 0f);
		TweenerCore<Vector3, Path, PathOptions> tweenerCore = base.transform.DOPath(new Vector3[3] { vector, vector2, vector3 }, timeToFade, PathType.CatmullRom, PathMode.Sidescroller2D);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(base.DieOrDeactivate));
		_tweens.Add(tweenerCore);
		_tweens.Add(base.transform.DOScale(_endScale, 2f));
		_tweens.Add(_text.DOFade(0f, _fadeDuration).SetDelay(_fadeDurationDelay));
	}

	private void OnDisable()
	{
		foreach (Tween tween in _tweens)
		{
			tween?.Kill();
		}
	}
}
