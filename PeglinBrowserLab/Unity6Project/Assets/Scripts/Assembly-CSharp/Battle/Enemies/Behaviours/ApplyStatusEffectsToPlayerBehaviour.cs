using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class ApplyStatusEffectsToPlayerBehaviour : MonoBehaviour
	{
		public StatusEffect[] statuses;

		public int applicationInterval = 1;

		public int turnsSinceApplication = 1;

		public string animOverride;

		public bool CheckForApplication()
		{
			if (turnsSinceApplication >= applicationInterval)
			{
				turnsSinceApplication = 0;
				return true;
			}
			turnsSinceApplication++;
			return false;
		}

		public void IncreaseStatusEffectApplicationIntencity(int amount)
		{
			for (int i = 0; i < statuses.Length; i++)
			{
				statuses[i].Intensity += amount;
			}
		}
	}
}
