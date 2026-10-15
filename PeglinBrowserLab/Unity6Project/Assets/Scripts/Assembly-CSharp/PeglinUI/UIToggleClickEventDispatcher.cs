using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	[RequireComponent(typeof(Toggle))]
	public class UIToggleClickEventDispatcher : MonoBehaviour
	{
		public ButtonSfx buttonSfx;

		public AudioClip sfxOverride;

		private Toggle toggle;

		private void OnEnable()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(HandleButtonClick);
		}

		private void OnDisable()
		{
			toggle.onValueChanged.RemoveListener(HandleButtonClick);
		}

		private void HandleButtonClick(bool val)
		{
			if (base.enabled)
			{
				if (sfxOverride == null)
				{
					UIButtonClickEventDispatcher.OnUIButtonClicked(buttonSfx);
				}
				else
				{
					UIButtonClickEventDispatcher.OnUIButtonCustomSfx(sfxOverride);
				}
			}
		}
	}
}
