using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPitchUp : MonoBehaviour
{
	public float PitchUpAmount = 0.25f;

	private AudioSource _audioSource;

	private void OnEnable()
	{
		if (_audioSource == null)
		{
			_audioSource = GetComponent<AudioSource>();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			_audioSource.pitch += PitchUpAmount;
			_audioSource.Play();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			_audioSource.pitch -= PitchUpAmount;
			_audioSource.Play();
		}
	}
}
