using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class GrantCoinsOnDeath : MonoBehaviour
	{
		public int coinsToAddDirectly;

		public int coinsToAddToBoard = 15;

		public int cruciballModCoinsToAddToBoard = 10;

		public int coinToDropWhenHit = 1;

		public float healthPercentageToDropExtraCoins = 0.1f;

		public GameObject animationToEnable;
	}
}
