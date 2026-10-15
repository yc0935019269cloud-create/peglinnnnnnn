using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
	public float FadeTime;

	private void OnEnable()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].DOFade(1f, FadeTime).From(0f);
		}
	}

	public void FadeOut(bool disable)
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer target in componentsInChildren)
		{
			if (disable)
			{
				TweenerCore<Color, Color, ColorOptions> tweenerCore = target.DOFade(0f, FadeTime).From(1f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
				{
					base.gameObject.SetActive(value: false);
				}));
			}
			else
			{
				target.DOFade(0f, FadeTime).From(1f);
			}
		}
	}
}
