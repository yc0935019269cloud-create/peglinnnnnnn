using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New PegLayout", menuName = "Peg Layout Data")]
public class PegLayout : ScriptableObject
{
	public AssetReference PegLayoutData;

	public Vector2 BallStartingPosition = new Vector2(0f, 4f);

	public float navXOverride = float.MinValue;

	public float navYOverride = float.MinValue;

	public bool forceLowFrameratePrediction;

	public string layoutDataFilename = "";
}
