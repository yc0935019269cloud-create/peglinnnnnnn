using UnityEngine;

namespace Battle.Enemies
{
	[RequireComponent(typeof(Enemy))]
	public class SpawnEnemyOnDeath : MonoBehaviour
	{
		public GameObject enemyPrefabToSpawn;

		public bool passStatusEffects = true;
	}
}
