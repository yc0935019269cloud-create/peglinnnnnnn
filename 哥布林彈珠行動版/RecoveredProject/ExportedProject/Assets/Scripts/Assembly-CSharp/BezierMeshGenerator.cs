using Battle.PegBehaviour;
using Relics;
using UnityEngine;

public class BezierMeshGenerator : MonoBehaviour
{
	[SerializeField]
	public LongPegSnapData longPegSnapData;

	[Header("Prefabs")]
	[SerializeField]
	private LongPeg longPegPrefab;

	[SerializeField]
	private GameObject pegTextPrefab;

	[Header("Long Peg Materials")]
	[SerializeField]
	private Material activeMaterial;

	[SerializeField]
	private Material refreshMaterial;

	[SerializeField]
	private Material persistentRefreshMaterial;

	[SerializeField]
	private Material critMaterial;

	[SerializeField]
	private Material persistentCritMaterial;

	[SerializeField]
	private Material destroyedMaterial;

	[SerializeField]
	private Material vineMaterial;

	[SerializeField]
	private Material maxHpMaterial;

	[SerializeField]
	private Sprite vineCenterSprite;

	[SerializeField]
	private PhysicsMaterial2D physicsMaterial;

	[SerializeField]
	public float timeToDisappear = 0.5f;

	[SerializeField]
	private RelicManager relicManager;

	[SerializeField]
	private LongPeg.LongPegColors colors = new LongPeg.LongPegColors
	{
		Regular = Color.white,
		Bonus = new Color(1f, 0.4588f, 0.4588f),
		Hit = Color.red,
		Cleared = Color.white
	};

	[SerializeField]
	private int damageMultiplier = 1;

	[SerializeField]
	private Sprite resetSprite;

	[SerializeField]
	private Sprite critSprite;

	private const int PEG_COLLIDER_LAYER = 15;

	private const int TRIGGER_LAYER = 14;

	private const int POPPED_TRIGGER_LAYER = 19;

	public void GenerateMeshArray(Vector3[] objectPositions, Vector3[] normalPositions, PolygonCollider2D polygon, Transform parent = null)
	{
		Transform parent2 = ((parent == null) ? base.transform : parent);
		Vector3 position = base.transform.position;
		for (int i = 0; i < objectPositions.Length - 1; i++)
		{
			Vector2 vector = (normalPositions[i] + normalPositions[i + 1]) / 2f;
			GenerateMesh(position, objectPositions[i] + normalPositions[i] * -1f, objectPositions[i] + normalPositions[i], objectPositions[i + 1] + normalPositions[i + 1] * -1f, objectPositions[i + 1] + normalPositions[i + 1], i, vector, parent2, polygon);
		}
	}

	public void GenerateMesh(Vector3 curvePos, Vector3 sw, Vector3 nw, Vector3 se, Vector3 ne, int index, Vector3 normal, Transform parent, PolygonCollider2D polygon)
	{
		LongPeg longPeg = Object.Instantiate(longPegPrefab, parent);
		longPeg.tag = base.gameObject.tag;
		MeshRenderer component = longPeg.GetComponent<MeshRenderer>();
		Mesh mesh = longPeg.GetComponent<MeshFilter>().mesh;
		mesh.vertices = new Vector3[4] { sw, nw, se, ne };
		mesh.triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		component.material = activeMaterial;
		Vector2[] points = new Vector2[4] { sw, se, ne, nw };
		SetPolygonColliderPoints(longPeg.gameObject.GetComponent<PolygonCollider2D>(), points);
		LongPegSlimeBehaviour componentInChildren = longPeg.GetComponentInChildren<LongPegSlimeBehaviour>(includeInactive: true);
		if (componentInChildren != null)
		{
			Mesh mesh2 = componentInChildren.GetComponent<MeshFilter>().mesh;
			Vector3 vector = (sw + se + ne + nw) / 4f;
			mesh2.vertices = new Vector3[4]
			{
				vector + (sw - vector) * componentInChildren.scaleMod,
				vector + (nw - vector) * componentInChildren.scaleMod,
				vector + (se - vector) * componentInChildren.scaleMod,
				vector + (ne - vector) * componentInChildren.scaleMod
			};
			mesh2.triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
			mesh2.uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 0f),
				new Vector2(1f, 1f)
			};
		}
		longPeg.SetMaterials(activeMaterial, refreshMaterial, critMaterial, destroyedMaterial, persistentCritMaterial, persistentRefreshMaterial, vineMaterial, vineCenterSprite, maxHpMaterial);
		longPeg.relicManager = relicManager;
		longPeg.defaultDamageMultiplier = damageMultiplier;
		longPeg.gameObject.layer = 15;
		longPeg.pegTextPrefab = pegTextPrefab;
		PolygonCollider2D[] componentsInChildren = longPeg.GetComponentsInChildren<PolygonCollider2D>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			SetPolygonColliderPoints(componentsInChildren[i], points);
		}
		longPeg.Initialize(timeToDisappear, colors, resetSprite, critSprite, normal, polygon, longPegSnapData);
	}

	private PolygonCollider2D SetPolygonColliderPoints(PolygonCollider2D polygonCollider, Vector2[] points)
	{
		polygonCollider.pathCount = 4;
		polygonCollider.points = points;
		polygonCollider.sharedMaterial = physicsMaterial;
		return polygonCollider;
	}
}
