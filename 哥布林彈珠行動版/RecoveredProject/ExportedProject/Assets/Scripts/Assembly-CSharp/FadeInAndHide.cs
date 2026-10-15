using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndHide : MonoBehaviour
{
	public float fadeTime = 0.5f;

	private Image _image;

	private void Start()
	{
		_image = GetComponent<Image>();
		_image.enabled = true;
		TweenerCore<Color, Color, ColorOptions> tweenerCore = _image.DOFade(0f, fadeTime).From(1f);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(HideSelf));
	}

	private void HideSelf()
	{
		_image.enabled = false;
	}
}
