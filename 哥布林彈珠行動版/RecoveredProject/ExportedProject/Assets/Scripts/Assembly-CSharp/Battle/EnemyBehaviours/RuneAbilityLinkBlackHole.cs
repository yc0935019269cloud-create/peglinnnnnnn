using System;
using Battle.Enemies;
using Battle.Pachinko.Obstacles;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class RuneAbilityLinkBlackHole : MonoBehaviour
	{
		public Vector2 blackHolePosition;

		public GameObject blackHolePrefab;

		private Enemy _thisEnemy;

		private PredictionManager _predictionManager;

		private PegboardBlackHole _blackHoleInstance;

		private void OnEnable()
		{
			_thisEnemy = GetComponent<Enemy>();
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			_blackHoleInstance = UnityEngine.Object.Instantiate(blackHolePrefab, blackHolePosition, Quaternion.identity).GetComponent<PegboardBlackHole>();
			_blackHoleInstance.gravityRange = 3f;
			_blackHoleInstance.gravityStrength = 4f;
			_predictionManager.AddObjectToSimulationScene(_blackHoleInstance.gameObject);
		}

		private void OnEnemyDestroyed(Enemy enemy)
		{
			if (enemy == _thisEnemy && _blackHoleInstance != null)
			{
				_predictionManager.DestroyObjectInSimulation(_blackHoleInstance.gameObject);
				UnityEngine.Object.Destroy(_blackHoleInstance.gameObject);
			}
		}
	}
}
