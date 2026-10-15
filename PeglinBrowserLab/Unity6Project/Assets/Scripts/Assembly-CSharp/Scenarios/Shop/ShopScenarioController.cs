using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Scenarios.Shop
{
	public class ShopScenarioController : MonoBehaviour
	{
		public float introFadeTime = 0.6f;

		public Image curtain;

		public GameObject shopUI;

		private void Awake()
		{
			shopUI.SetActive(value: false);
			curtain.enabled = true;
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = curtain.DOFade(0f, introFadeTime).From(1f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				curtain.enabled = false;
				PauseMenu.PauseBlock = false;
			}));
		}
	}
}
