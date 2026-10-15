using UnityEngine;

namespace Peglin
{
	public abstract class PegMinigameChooser : MonoBehaviour
	{
		public abstract void HandleSlotTriggerActivated(int index);

		public abstract int GetNumberOfSlots();
	}
}
