using System.Collections;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies.StatusEffects
{
	public class AddPlayerStatusOnEnemyStart : MonoBehaviour
	{
		public StatusEffect[] statusEffectsToAdd;

		public IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(statusEffectsToAdd, StatusEffectSource.ENEMY);
		}
	}
}
