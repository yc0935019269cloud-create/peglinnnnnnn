using UnityEngine;

[CreateAssetMenu(fileName = "Background Data")]
public class BackgroundData : ScriptableObject
{
	public GameObject Background;

	public int lengthOfRooms;

	public GameObject BouncerPrefab;
}
