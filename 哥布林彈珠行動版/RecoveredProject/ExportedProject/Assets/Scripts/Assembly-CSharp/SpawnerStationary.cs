using Battle.Enemies;
using Loading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnerStationary : Spawner
{
	protected override bool CanSpawn(SpawnData spawnData)
	{
		if (IsSpawning())
		{
			return false;
		}
		return _enemyManager.IsStationarySlotClear(spawnData.spawnSlot);
	}

	protected override void SpawnEnemyObject()
	{
		string key = _currentSpawnData.enemyAssetReference.RuntimeKey.ToString();
		AssetReferenceGameObject enemyAssetReference = _currentSpawnData.enemyAssetReference;
		if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key))
		{
			Debug.LogWarning($"Did not find cached prefab for enemy {enemyAssetReference}");
			GameObject value = enemyAssetReference.LoadAssetAsync<GameObject>().WaitForCompletion();
			AssetLoading.Instance.EnemyPrefabs.Add(key, value);
		}
		GameObject gameObject = AssetLoading.Instance.EnemyPrefabs[key];
		_currentEnemy = Object.Instantiate(gameObject, _enemyManager.GetSpawnPointForStationarySlot(_currentSpawnData.spawnSlot), Quaternion.identity, base.transform).GetComponent<Enemy>();
		_currentEnemy.Initialize(_statusEffectData, _enemyManager, _relicManager, gameObject.name);
		_enemyManager.AddStationaryEnemy(_currentEnemy, _currentSpawnData.spawnSlot);
		_enemyInfoManager.PopSpawn();
	}
}
