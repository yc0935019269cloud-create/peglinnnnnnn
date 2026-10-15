using Battle.Enemies;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class DelayShotByEnemyCount : MonoBehaviour
	{
		private static int _currentDelay;

		private void Awake()
		{
			_currentDelay = 0;
		}

		private void Start()
		{
			GetComponent<Enemy>()?.IncrementAttackCharge(_currentDelay++);
		}
	}
}
