using UnityEngine;

namespace Battle.Enemies
{
	public class TriggerStatusEffectsOnExtraTurn : MonoBehaviour, IExtraTurnTrigger
	{
		public void DoExtraTurnBehavior(Enemy enemy)
		{
			// Disabled to prevent double decay of status effects (e.g. Spinfection) when moving/attacking in the same turn.
			// Status effects are already resolved once per round in EnemyManager.ResolveStatusEffects().
		}
	}
}
