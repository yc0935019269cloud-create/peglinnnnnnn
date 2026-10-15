using System.Linq;
using Battle;
using Battle.StatusEffects;
using Data;
using Map;
using Relics;
using UnityEngine;

public class SpawnerFactory : MonoBehaviour
{
	public static Spawner CreateSpawner(WaveGroup waveGroup, EnemyManager enemyManager, StatusEffectData statusEffectData, RelicManager relicManager, EnemyInfoManager enemyInfoManager)
	{
		bool flag = waveGroup.spawnPoint != null && waveGroup.spawnPoint.GetComponent<Spawner>().forAct == MapController.instance.Act;
		GameObject gameObject = ((waveGroup.spawnPoint == null || !flag) ? new GameObject() : Object.Instantiate(waveGroup.spawnPoint));
		Spawner spawner = gameObject.GetComponent<Spawner>();
		switch (waveGroup.waveType)
		{
		case WaveGroup.WaveType.GROUND:
			gameObject.name = "Ground Spawner";
			if (spawner == null)
			{
				spawner = gameObject.AddComponent<Spawner>();
			}
			spawner.Initialize(waveGroup.waveData.ToArray(), enemyManager, statusEffectData, relicManager, enemyInfoManager);
			break;
		case WaveGroup.WaveType.STATIONARY:
			gameObject.name = "Stationary Spawner";
			if (spawner == null)
			{
				spawner = gameObject.AddComponent<SpawnerStationary>();
			}
			spawner.Initialize(waveGroup.waveData.ToArray(), enemyManager, statusEffectData, relicManager, enemyInfoManager);
			break;
		case WaveGroup.WaveType.FLYING:
			gameObject.name = "Flying Spawner";
			if (spawner == null)
			{
				spawner = gameObject.AddComponent<Spawner>();
			}
			spawner.Initialize(waveGroup.waveData.ToArray(), enemyManager, statusEffectData, relicManager, enemyInfoManager);
			break;
		default:
			gameObject.name = "Ground Spawner";
			if (spawner == null)
			{
				spawner = gameObject.AddComponent<Spawner>();
			}
			spawner.Initialize(waveGroup.waveData.ToArray(), enemyManager, statusEffectData, relicManager, enemyInfoManager);
			break;
		}
		return spawner;
	}
}
