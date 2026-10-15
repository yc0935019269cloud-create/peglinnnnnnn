using System;
using System.Collections;
using Data.Audio;
using UI;
using UnityEngine;

namespace PeglinUI
{
	[RequireComponent(typeof(AudioSource))]
	public class UIAudioManager : MonoBehaviour
	{
		private const float TRANSPOSE = -4f;

		public static Action InvalidSelectionSfxRequested = () =>
		{
		};

		[SerializeField]
		private UISfx uiSfx;

		[SerializeField]
		private AudioSource _uiAudioSource;

		[SerializeField]
		private AudioSource _pitchableAudioSource;

		private bool _hoverSoundEnabled;

		private void Start()
		{
			StartCoroutine(EnableHoverSound());
		}

		private IEnumerator EnableHoverSound()
		{
			yield return null;
			_hoverSoundEnabled = true;
		}

		private void OnEnable()
		{
			if (_uiAudioSource == null)
			{
				_uiAudioSource = GetComponent<AudioSource>();
			}
			UIButtonClickEventDispatcher.OnUIButtonClicked = (UIButtonClickEventDispatcher.UIButtonClicked)Delegate.Combine(UIButtonClickEventDispatcher.OnUIButtonClicked, new UIButtonClickEventDispatcher.UIButtonClicked(PlayClickSound));
			UIButtonClickEventDispatcher.OnUIButtonCustomSfx = (UIButtonClickEventDispatcher.UIButtonCustomSFX)Delegate.Combine(UIButtonClickEventDispatcher.OnUIButtonCustomSfx, new UIButtonClickEventDispatcher.UIButtonCustomSFX(PlayCustomSound));
			ButtonHandleHover.OnUIButtonHover = (ButtonHandleHover.UIButtonHover)Delegate.Combine(ButtonHandleHover.OnUIButtonHover, new ButtonHandleHover.UIButtonHover(PlayHoverSound));
			InvalidSelectionSfxRequested = (Action)Delegate.Combine(InvalidSelectionSfxRequested, new Action(PlayInvalidSelectionSfx));
		}

		private void PlayClickSound(ButtonSfx buttonSfx)
		{
			AudioClip audioClip = uiSfx.ButtonClickSound;
			switch (buttonSfx)
			{
			case ButtonSfx.Confirm:
				audioClip = uiSfx.ConfirmButtonClickSound;
				break;
			case ButtonSfx.Back:
				audioClip = uiSfx.BackButtonClickSound;
				break;
			case ButtonSfx.Open:
				audioClip = uiSfx.WindowOpenSound;
				break;
			case ButtonSfx.Close:
				audioClip = uiSfx.WindowCloseSound;
				break;
			case ButtonSfx.AddOrb:
				audioClip = uiSfx.AddOrbSound;
				break;
			case ButtonSfx.UpgradeOrb:
				audioClip = uiSfx.UpgradeOrbSound;
				break;
			case ButtonSfx.RemoveOrb:
				audioClip = uiSfx.RemoveOrbSound;
				break;
			case ButtonSfx.AddRelic:
				audioClip = uiSfx.AddRelicSound;
				break;
			}
			if (audioClip != null)
			{
				_uiAudioSource.PlayOneShot(audioClip);
			}
		}

		private void PlayHoverSound(int noteOffset = 0)
		{
			_pitchableAudioSource.pitch = Mathf.Pow(2f, ((float)noteOffset + -4f) / 12f);
			if (_hoverSoundEnabled)
			{
				_pitchableAudioSource.PlayOneShot(uiSfx.HoverSound);
			}
		}

		private void PlayInvalidSelectionSfx()
		{
			_uiAudioSource.PlayOneShot(uiSfx.InvalidSelectionSound);
		}

		private void PlayCustomSound(AudioClip customSfx)
		{
			_uiAudioSource.PlayOneShot(customSfx);
		}

		private void OnDisable()
		{
			UIButtonClickEventDispatcher.OnUIButtonClicked = (UIButtonClickEventDispatcher.UIButtonClicked)Delegate.Remove(UIButtonClickEventDispatcher.OnUIButtonClicked, new UIButtonClickEventDispatcher.UIButtonClicked(PlayClickSound));
			UIButtonClickEventDispatcher.OnUIButtonCustomSfx = (UIButtonClickEventDispatcher.UIButtonCustomSFX)Delegate.Remove(UIButtonClickEventDispatcher.OnUIButtonCustomSfx, new UIButtonClickEventDispatcher.UIButtonCustomSFX(PlayCustomSound));
			ButtonHandleHover.OnUIButtonHover = (ButtonHandleHover.UIButtonHover)Delegate.Remove(ButtonHandleHover.OnUIButtonHover, new ButtonHandleHover.UIButtonHover(PlayHoverSound));
			InvalidSelectionSfxRequested = (Action)Delegate.Remove(InvalidSelectionSfxRequested, new Action(PlayInvalidSelectionSfx));
		}
	}
}
