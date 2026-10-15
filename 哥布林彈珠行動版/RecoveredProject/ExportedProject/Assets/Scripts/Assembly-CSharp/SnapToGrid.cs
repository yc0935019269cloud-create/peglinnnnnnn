using UnityEngine;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour
{
	public enum SnapType
	{
		GRID = 0
	}

	public bool ShouldSnap;

	private SnapType _snapType;

	[SerializeField]
	private float gridWidth = 0.5f;

	private void Start()
	{
		if (Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (ShouldSnap)
		{
			Snap();
		}
	}

	private void Snap()
	{
		if (_snapType == SnapType.GRID)
		{
			base.transform.position = new Vector3((float)Mathf.RoundToInt(base.transform.position.x / gridWidth) * gridWidth, (float)Mathf.RoundToInt(base.transform.position.y / gridWidth) * gridWidth);
		}
	}
}
