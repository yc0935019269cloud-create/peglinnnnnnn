using System.Collections.Generic;
using Battle.PegBehaviour;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

public class BezierCurveManager : NonDummyPegboardBehaviour
{
	[SerializeField]
	private int nSegments = 25;

	[SerializeField]
	private float objectLengthMax = 0.7f;

	[SerializeField]
	private float objectLengthMin = 0.3f;

	private SpriteShapeController spriteShapeController;

	private BezierMeshGenerator meshGenerator;

	private List<Vector3> objectPositions = new List<Vector3>();

	private List<Vector3> normalPositions = new List<Vector3>();

	private PolygonCollider2D _polygon;

	public List<Vector3> ObjectPositions => objectPositions;

	public List<Vector3> NormalPositions => normalPositions;

	public PolygonCollider2D polygon => _polygon;

	public void GenerateLongPegs()
	{
		if (SetupPegGeneration())
		{
			meshGenerator.GenerateMeshArray(objectPositions.ToArray(), normalPositions.ToArray(), _polygon);
		}
	}

	public bool SetupPegGeneration()
	{
		if (isDummy)
		{
			return false;
		}
		if (spriteShapeController == null)
		{
			spriteShapeController = GetComponent<SpriteShapeController>();
		}
		if (spriteShapeController == null)
		{
			return false;
		}
		if (meshGenerator == null)
		{
			meshGenerator = GetComponent<BezierMeshGenerator>();
		}
		Spline spline = spriteShapeController.spline;
		SampleSpline(spline, objectPositions, normalPositions);
		GeneratePolygon();
		Object.Destroy(spriteShapeController);
		Object.Destroy(GetComponent<SpriteShapeRenderer>());
		return true;
	}

	public void SampleSpline(Spline spline, List<Vector3> objectPosList, List<Vector3> normalPosList)
	{
		for (int i = 0; i < spline.GetPointCount() - 1; i++)
		{
			Vector3 position = spline.GetPosition(i);
			Vector3 p = spline.GetRightTangent(i) + position;
			Vector3 position2 = spline.GetPosition(i + 1);
			Vector3 p2 = spline.GetLeftTangent(i + 1) + position2;
			Bezier bezier = new Bezier(position, p, p2, position2, nSegments);
			bezier.PlaceObjectsAlongCurve(objectLengthMax, objectLengthMin, centered: false);
			objectPosList.AddRange(bezier.ObjectPositions);
			normalPosList.AddRange(bezier.Normals);
			if (i == spline.GetPointCount() - 2)
			{
				objectPosList.Add(bezier.GetPoint(1f));
				normalPosList.Add(bezier.GetNormal(1f));
			}
		}
	}

	private void GeneratePolygon()
	{
		_polygon = base.gameObject.AddComponent<PolygonCollider2D>();
		_polygon.isTrigger = true;
		_polygon.transform.parent = base.gameObject.transform;
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < objectPositions.Count; i++)
		{
			list.Add(new Vector2(objectPositions[i].x, objectPositions[i].y));
		}
		_polygon.SetPath(0, list.ToArray());
	}
}
