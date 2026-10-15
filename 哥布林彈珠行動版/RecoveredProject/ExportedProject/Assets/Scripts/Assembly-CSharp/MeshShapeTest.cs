using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshShapeTest : MonoBehaviour
{
	public Texture2D Texture;

	private Mesh mesh;

	private Vector3[] vertices;

	private Vector2[] uvs;

	private int[] triangles;

	private void OnEnable()
	{
		mesh = GetComponent<MeshFilter>().mesh;
	}

	private void Start()
	{
		CreateMeshData();
		CreateMesh();
	}

	private void CreateMeshData()
	{
		vertices = new Vector3[4]
		{
			new Vector2(1f, 0f),
			new Vector2(0f, 8f),
			new Vector2(14f, 0f),
			new Vector2(15f, 8f)
		};
		uvs = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
	}

	private void CreateMesh()
	{
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
	}
}
