using Cruciball;
using Map;
using UnityEngine;

public class ChangePotentialMapDataBasedOnCruciball : MonoBehaviour
{
	[SerializeField]
	private CruciballManager _cruciballManager;

	public int cruciallLevel = 10;

	public MapData newMapData;

	public MapDataType newMapDataType;

	[HideInInspector]
	public SeededMapNodeData seededMapNodeData;

	public void SetSeededData(SeededMapNodeData newSeededMapNodeData)
	{
		seededMapNodeData = newSeededMapNodeData;
	}
}
