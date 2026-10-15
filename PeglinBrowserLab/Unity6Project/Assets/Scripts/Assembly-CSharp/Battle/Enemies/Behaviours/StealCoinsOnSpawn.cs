using System.Collections;
using Challenges;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class StealCoinsOnSpawn : MonoBehaviour
	{
		public float waitTimeBeforeStealing = 0.3f;

		public int coinsToStealOnSpawn = 2;

		[Tooltip("When defeated, this enemy will return all of the coins stolen x this amount:")]
		public float coinReturnMultiplier = 2f;

		private Enemy _enemy;

		private GrantCoinsOnDeath _grantCoinsOnDeath;

		private void Start()
		{
			_enemy = GetComponent<Enemy>();
			_grantCoinsOnDeath = GetComponent<GrantCoinsOnDeath>();
			StartCoroutine(WaitAndStealCoins());
		}

		private IEnumerator WaitAndStealCoins()
		{
			yield return new WaitForSeconds(waitTimeBeforeStealing);
			int num = _enemy.enemyManager.GetComponent<BattleController>().pegManager.RemoveGoldFromPegsWithoutCollecting(coinsToStealOnSpawn, _enemy.GetCenterPosition());
			ChallengeManager.Instance.painbowCoinsStolen += num;
			_grantCoinsOnDeath.coinsToAddToBoard = Mathf.RoundToInt((float)ChallengeManager.Instance.painbowCoinsStolen * coinReturnMultiplier);
			_grantCoinsOnDeath.cruciballModCoinsToAddToBoard = Mathf.RoundToInt((float)ChallengeManager.Instance.painbowCoinsStolen * coinReturnMultiplier);
		}
	}
}
