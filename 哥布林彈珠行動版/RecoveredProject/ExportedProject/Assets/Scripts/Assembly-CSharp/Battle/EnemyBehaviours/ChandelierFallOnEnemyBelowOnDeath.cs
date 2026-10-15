using System.Collections.Generic;
using Battle.Enemies;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class ChandelierFallOnEnemyBelowOnDeath : MonoBehaviour
	{
		public int flatDamage = 100;

		public GameObject fallingChandelierObject;

		public float spawnObjectWaitTime = 0.2f;

		public float spawnYOffset = -0.1f;

		private Enemy[] _enemiesBelow;

		public void DoFall()
		{
			Enemy component = GetComponent<Enemy>();
			EnemyManager enemyManager = component.enemyManager;
			List<Enemy> list = new List<Enemy>();
			int slotIndexForEnemy = enemyManager.GetSlotIndexForEnemy(component, out var _);
			Enemy groundEnemyAtSlot = enemyManager.GetGroundEnemyAtSlot(slotIndexForEnemy);
			if (groundEnemyAtSlot != null)
			{
				list.Add(groundEnemyAtSlot);
			}
			if (slotIndexForEnemy - 1 >= 0)
			{
				Enemy stationaryEnemyAtSlot = enemyManager.GetStationaryEnemyAtSlot(slotIndexForEnemy - 1);
				if (stationaryEnemyAtSlot != null)
				{
					list.Add(stationaryEnemyAtSlot);
				}
			}
			Enemy stationaryEnemyAtSlot2 = enemyManager.GetStationaryEnemyAtSlot(slotIndexForEnemy);
			if (stationaryEnemyAtSlot2 != null)
			{
				list.Add(stationaryEnemyAtSlot2);
			}
			_enemiesBelow = list.ToArray();
			Object.Instantiate(fallingChandelierObject, component.GetCenterPosition() + new Vector3(0f, spawnYOffset, 0f), Quaternion.identity).GetComponent<FallingChandelier>().SetOnLandAction(OnFallingChandelierHitGround);
		}

		private void OnFallingChandelierHitGround()
		{
			if (_enemiesBelow != null)
			{
				Enemy[] enemiesBelow = _enemiesBelow;
				for (int i = 0; i < enemiesBelow.Length; i++)
				{
					enemiesBelow[i].Damage(flatDamage);
				}
			}
			GetComponent<Enemy>().AnimationHasFinished();
		}
	}
}
