using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class ContinuousFlash : MonoBehaviour
	{
		private Image _img;

		private Tween _tween;

		private void Awake()
		{
			_img = GetComponent<Image>();
		}

		public void StartFlash()
		{
			StopFlash();
			_tween = _img.DOFade(0.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
		}

		public void StopFlash()
		{
			if (_tween != null)
			{
				_tween.Kill();
				_tween = null;
			}
			if (_img != null)
			{
				_img.color = Color.white;
			}
		}
	}
}
