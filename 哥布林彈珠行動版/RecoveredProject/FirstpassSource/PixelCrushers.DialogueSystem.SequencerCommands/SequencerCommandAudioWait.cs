using System;
using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
	[AddComponentMenu("")]
	public class SequencerCommandAudioWait : SequencerCommand
	{
		protected float stopTime;

		protected AudioSource audioSource;

		protected int nextClipIndex = 2;

		protected AudioClip currentClip;

		protected AudioClip originalClip;

		protected bool restoreOriginalClip;

		public IEnumerator Start()
		{
			string audioClipName = GetParameter(0);
			Transform subject = GetSubject(1);
			nextClipIndex = 2;
			if (DialogueDebug.logInfo)
			{
				Debug.Log(string.Format("{0}: Sequencer: AudioWait({1})", new object[2]
				{
					"Dialogue System",
					GetParameters()
				}));
			}
			audioSource = GetAudioSource(subject);
			if (audioSource == null)
			{
				if (DialogueDebug.logWarnings)
				{
					Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: can't find or add AudioSource to {1}.", new object[2] { "Dialogue System", subject.name }));
				}
				Stop();
			}
			else
			{
				originalClip = audioSource.clip;
				stopTime = DialogueTime.time + 1f;
				yield return null;
				originalClip = audioSource.clip;
				TryAudioClip(audioClipName);
			}
		}

		protected virtual AudioSource GetAudioSource(Transform subject)
		{
			return SequencerTools.GetAudioSource(subject);
		}

		private void TryAudioClip(string audioClipName)
		{
			try
			{
				if (string.IsNullOrEmpty(audioClipName))
				{
					if (DialogueDebug.logWarnings)
					{
						Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Audio clip name is blank.", new object[1] { "Dialogue System" }));
					}
					stopTime = 0f;
					return;
				}
				DialogueManager.LoadAsset(audioClipName, typeof(AudioClip), delegate(UnityEngine.Object asset)
				{
					AudioClip audioClip = asset as AudioClip;
					if (audioClip == null)
					{
						if (DialogueDebug.logWarnings)
						{
							Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Clip '{1}' wasn't found.", new object[2] { "Dialogue System", audioClipName }));
						}
						stopTime = 0f;
					}
					else
					{
						if (IsAudioMuted())
						{
							if (DialogueDebug.logInfo)
							{
								Debug.Log(string.Format("{0}: Sequencer: AudioWait(): waiting but not playing '{1}'; audio is muted.", new object[2] { "Dialogue System", audioClipName }));
							}
						}
						else
						{
							if (DialogueDebug.logInfo)
							{
								Debug.Log(string.Format("{0}: Sequencer: AudioWait(): playing '{1}'.", new object[2] { "Dialogue System", audioClipName }));
							}
							currentClip = audioClip;
							audioSource.clip = audioClip;
							audioSource.Play();
						}
						stopTime = DialogueTime.time + audioClip.length;
					}
				});
			}
			catch (Exception)
			{
				stopTime = 0f;
			}
		}

		public void Update()
		{
			if (DialogueTime.time >= stopTime)
			{
				DialogueManager.UnloadAsset(currentClip);
				if (nextClipIndex < base.parameters.Length)
				{
					TryAudioClip(GetParameter(nextClipIndex));
					nextClipIndex++;
				}
				else
				{
					currentClip = null;
					Stop();
				}
			}
		}

		public void OnDialogueSystemPause()
		{
			if (!(audioSource == null))
			{
				audioSource.Pause();
			}
		}

		public void OnDialogueSystemUnpause()
		{
			if (!(audioSource == null))
			{
				audioSource.Play();
			}
		}

		public void OnDestroy()
		{
			if (audioSource != null)
			{
				DialogueManager.UnloadAsset(currentClip);
				if (audioSource.isPlaying && audioSource.clip == currentClip)
				{
					audioSource.Stop();
				}
				if (restoreOriginalClip)
				{
					audioSource.clip = originalClip;
				}
			}
		}
	}
}
