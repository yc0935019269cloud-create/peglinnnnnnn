using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Rewired;

namespace PeglinUI.SettingsMenu.Options
{
	public class RewiredUIButtonTrigger : PixelCrushers.DialogueSystem.UIButtonKeyTrigger
	{
		private Player _player;

		private void OnEnable()
		{
			_player = ReInput.players.GetPlayer(0);
		}

		protected new void Update()
		{
			if (PixelCrushers.UIButtonKeyTrigger.monitorInput && base.selectable.enabled && base.selectable.interactable && base.selectable.gameObject.activeInHierarchy && (_player.GetButtonDown(buttonName) || (anyKeyOrButton && _player.GetAnyButtonDown())) && (!skipIfBeingClickedBySubmit || !IsBeingClickedBySubmit()))
			{
				Click();
			}
		}
	}
}
