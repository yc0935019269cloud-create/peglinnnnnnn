using System;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AddFireToExitedSlot : MonoBehaviour
	{
		private void OnEnable()
		{
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Combine(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(AddFireToSlotExited));
		}

		private void OnDisable()
		{
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Remove(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(AddFireToSlotExited));
		}

		private void AddFireToSlotExited(int slotIndex, PachinkoBall pBall)
		{
			SpecialSlotController.OnAddStaticDamageToSlotIndex(slotIndex);
		}
	}
}
