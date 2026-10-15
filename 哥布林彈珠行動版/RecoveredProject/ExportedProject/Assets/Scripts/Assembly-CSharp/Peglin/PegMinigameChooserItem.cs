using UnityEngine;

namespace Peglin
{
	public class PegMinigameChooserItem : PegMinigameChooser
	{
		[Header("Parameters")]
		public int NumberOfSlots = 2;

		public override int GetNumberOfSlots()
		{
			return NumberOfSlots;
		}

		public override void HandleSlotTriggerActivated(int index)
		{
		}
	}
}
