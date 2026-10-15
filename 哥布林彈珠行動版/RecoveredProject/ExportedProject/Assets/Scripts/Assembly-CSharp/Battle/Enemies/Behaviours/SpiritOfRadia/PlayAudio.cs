using UnityEngine;

namespace Battle.Enemies.Behaviours.SpiritOfRadia
{
	public class PlayAudio : MonoBehaviour
	{
		public AudioSource audioSource;

		public void Play()
		{
			if (!(audioSource == null))
			{
				audioSource.Play();
			}
		}
	}
}
