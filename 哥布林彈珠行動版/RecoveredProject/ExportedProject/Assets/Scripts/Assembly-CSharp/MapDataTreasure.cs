using UnityEngine;

[CreateAssetMenu(fileName = "New Treasure Room", menuName = "Scenarios/TreasureRoom")]
public class MapDataTreasure : MapData
{
	public float rareChance = 0.2f;

	public GameObject[] scenarioDoodads;

	public override SceneType GetSceneType()
	{
		return SceneType.TREASURE;
	}
}
