using UnityEngine;

namespace PeglinUtils
{
	public class PlayAudioClipOnEnable : MonoBehaviour
	{
		public AudioClip audioClip;

		private void OnEnable()
		{
			GetComponentInParent<AudioSource>().PlayOneShot(audioClip);
		}
	}
}
