using UnityEngine;

namespace Battle.Enemies
{
	public class TriggerStatusEffectsOnExtraTurn : MonoBehaviour, IExtraTurnTrigger
	{
		public void DoExtraTurnBehavior(Enemy enemy)
		{
			enemy.ResolveStatusEffects();
		}
	}
}
