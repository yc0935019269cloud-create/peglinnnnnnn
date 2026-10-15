using DG.Tweening;
using UnityEngine;

namespace PeglinUI.Encirclepedia
{
	public class StampTextScreenshake : MonoBehaviour
	{
		public RectTransform Encirclopedia;

		private AudioSource _audioSource;

		private void OnEnable()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		public void DoScreenshake()
		{
			Encirclopedia.DOShakePosition(0.5f, 3f * SettingsManager.Instance.ScreenshakeInstensity);
			_audioSource.Play();
		}
	}
}
