using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class BuffAlliesBehaviour : MonoBehaviour
	{
		public int buffAmount = 1;

		public StatusEffect[] additionalStatuses;

		public bool buffAfterMeleeAttack;

		public int numAlliesRequired = 2;

		public int buffInterval = 1;

		public int turnsSinceBuff;

		public string animOverride;

		public bool CheckForBuff()
		{
			if (turnsSinceBuff >= buffInterval)
			{
				turnsSinceBuff = 0;
				return true;
			}
			turnsSinceBuff++;
			return false;
		}
	}
}
