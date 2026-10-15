using System;
using Challenges;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Loading;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	[RequireComponent(typeof(Image))]
	public class FadeCurtain : MonoBehaviour
	{
		private Image _fadeImg;

		private void OnEnable()
		{
			if (_fadeImg == null)
			{
				_fadeImg = GetComponent<Image>();
			}
			_fadeImg.color = new Color(0f, 0f, 0f, 0f);
		}

		public void SetBlack()
		{
			_fadeImg.color = new Color(0f, 0f, 0f, 1f);
		}

		public void FadeIn(float alpha)
		{
			_fadeImg.enabled = true;
			_fadeImg.DOFade(alpha, 0.5f).From(0f);
		}

		public void FadeInAndLoadRunSummary()
		{
			if (!ChallengeManager.Instance.IsAct4Unlocked() || StaticGameData.CurrentRunStats.hasWonCore)
			{
				SaveManager.RequestDelete();
			}
			StaticGameData.dataToLoad = null;
			FadeInAndLoadScene(PeglinSceneLoader.Scene.RUN_SUMMARY);
		}

		public void FadeInAndLoadScene(PeglinSceneLoader.Scene sceneName)
		{
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeImg.DOFade(1f, 0.5f).From(0f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				PeglinSceneLoader.Instance.LoadScene(sceneName);
			}));
		}

		public void FadeInThenOut(float alpha, float length = 0.5f, bool useUnscaledTime = false)
		{
			_fadeImg.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeImg.DOFade(alpha, length).From(0f).SetUpdate(useUnscaledTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				FadeOutAndDisableObj(length);
			}));
		}

		public void FadeOut()
		{
			_fadeImg.DOFade(0f, 0.5f).From(1f);
		}

		public void FadeOutAndDisableObj(float length = 0.5f, bool useUnscaledTime = false)
		{
			_fadeImg.DOKill();
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fadeImg.DOFade(0f, length).SetUpdate(useUnscaledTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableImage));
		}

		private void DisableImage()
		{
			_fadeImg.enabled = false;
		}
	}
}
