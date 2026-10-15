using System.Collections.Generic;
using Map;
using Peglin.PegMinigame;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class MapDataPegMinigame : MapData
{
	public GameObject bouncerPrefab;

	public AssetReferenceGameObject pegLayout;

	public AssetReference pegLayoutData;

	public List<Reward> Rewards;

	public string layoutDataFilename = "";

	public override SceneType GetSceneType()
	{
		return SceneType.PEG_MINIGAME;
	}

	public abstract void PopulateRewards(SeededPegMinigameNodeData seededPegMinigameNodeData);
}
