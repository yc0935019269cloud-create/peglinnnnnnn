using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New SpawnData", menuName = "Spawn Data")]
public class SpawnData : ScriptableObject
{
	[FormerlySerializedAs("enemyPrefab")]
	public AssetReferenceGameObject enemyAssetReference;

	public AssetReferenceGameObject[] randomizedAssets;

	public int spawnSlot = -1;

	public string SpawnPointInitialAnimationTrigger = "spawn";

	public string SpawnPointFinalAnimationTrigger = "endSpawn";
}
