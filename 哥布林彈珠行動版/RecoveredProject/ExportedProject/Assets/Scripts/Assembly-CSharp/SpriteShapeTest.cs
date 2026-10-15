using UnityEngine;

public class SpriteShapeTest : MonoBehaviour
{
	public Texture2D Texture;

	private SpriteRenderer sr;

	private Vector2[] vertices;

	private ushort[] triangles;

	private void OnEnable()
	{
		if (sr == null)
		{
			sr = GetComponent<SpriteRenderer>();
		}
	}

	private void Start()
	{
		CreateMeshData();
		CreateSprite();
	}

	private void CreateMeshData()
	{
		vertices = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 13f),
			new Vector2(16f, 0f),
			new Vector2(11f, 16f)
		};
		triangles = new ushort[6] { 0, 1, 2, 1, 2, 3 };
	}

	private void CreateSprite()
	{
		sr.sprite = Sprite.Create(Texture, new Rect(0f, 0f, Texture.width, Texture.height), Vector2.zero, 16f);
		sr.sprite.OverrideGeometry(vertices, triangles);
	}

	private void CreateMesh()
	{
	}
}
