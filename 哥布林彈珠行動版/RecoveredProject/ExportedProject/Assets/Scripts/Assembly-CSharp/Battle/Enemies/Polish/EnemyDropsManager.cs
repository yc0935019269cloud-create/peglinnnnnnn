using Loading;
using UnityEngine;

namespace Battle.Enemies.Polish
{
	public class EnemyDropsManager : MonoBehaviour
	{
		public Canvas canvas;

		public GameObject xpPopupPrefab;

		private ObjectPool _xpPopupObjectPool;

		private void OnEnable()
		{
			if (_xpPopupObjectPool == null)
			{
				_xpPopupObjectPool = base.gameObject.AddComponent<ObjectPool>();
				_xpPopupObjectPool.CreatePool(8, xpPopupPrefab);
			}
		}

		private void OnDisable()
		{
		}

		private void HandleEnemyDrops(Enemy enemy)
		{
			if (AssetLoading.Instance.EnemyDataTable.TryGetValue(enemy.locKey, out var value) && value != null)
			{
				float num = Random.Range(0f, 1f);
				FloatingText pooledObject = _xpPopupObjectPool.GetPooledObject<FloatingText>();
				pooledObject.transform.position = enemy.transform.position;
				if (num < value.CardDropRate)
				{
					pooledObject.Activate("+1 XP");
				}
				else if (num < value.LootDropRate)
				{
					pooledObject.Activate("+10 XP");
				}
				else
				{
					pooledObject.Activate("+100 XP");
				}
			}
		}
	}
}
