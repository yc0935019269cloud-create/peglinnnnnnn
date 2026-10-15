using UnityEngine;

namespace Data.Audio
{
	[CreateAssetMenu(menuName = "Audio/UI Sfx")]
	public class UISfx : ScriptableObject
	{
		public AudioClip ButtonClickSound;

		public AudioClip HoverSound;

		public AudioClip ConfirmButtonClickSound;

		public AudioClip BackButtonClickSound;

		public AudioClip InvalidSelectionSound;

		public AudioClip WindowOpenSound;

		public AudioClip WindowCloseSound;

		public AudioClip AddOrbSound;

		public AudioClip UpgradeOrbSound;

		public AudioClip RemoveOrbSound;

		public AudioClip AddRelicSound;
	}
}
