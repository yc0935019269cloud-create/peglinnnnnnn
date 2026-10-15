using PeglinUI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public class UIButtonClickEventDispatcher : MonoBehaviour
	{
		public delegate void UIButtonClicked(ButtonSfx bSfx);

		public delegate void UIButtonCustomSFX(AudioClip clip);

		public static UIButtonClicked OnUIButtonClicked = (ButtonSfx sfx) =>
		{
		};

		public static UIButtonCustomSFX OnUIButtonCustomSfx = (AudioClip clip) =>
		{
		};

		public ButtonSfx buttonSfx;

		public AudioClip sfxOverride;

		private Button button;

		private void OnEnable()
		{
			button = GetComponent<Button>();
			button.onClick.AddListener(HandleButtonClick);
		}

		private void OnDisable()
		{
			button.onClick.RemoveListener(HandleButtonClick);
		}

		private void HandleButtonClick()
		{
			if (base.enabled)
			{
				if (sfxOverride == null)
				{
					OnUIButtonClicked(buttonSfx);
				}
				else
				{
					OnUIButtonCustomSfx(sfxOverride);
				}
			}
		}
	}
}
