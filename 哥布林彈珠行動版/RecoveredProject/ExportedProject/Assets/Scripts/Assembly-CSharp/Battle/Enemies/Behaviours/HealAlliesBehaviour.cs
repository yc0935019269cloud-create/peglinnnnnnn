using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class HealAlliesBehaviour : MonoBehaviour
	{
		public float healAmount = 30f;

		public float damageBuffHealMod = 5f;

		public float numDamagedAlliesRequired = 2f;

		public float healInterval = 1f;

		public float turnsSinceHeal;

		public bool CheckForHeal()
		{
			if (turnsSinceHeal >= healInterval)
			{
				turnsSinceHeal = 0f;
				return true;
			}
			turnsSinceHeal++;
			return false;
		}
	}
}
