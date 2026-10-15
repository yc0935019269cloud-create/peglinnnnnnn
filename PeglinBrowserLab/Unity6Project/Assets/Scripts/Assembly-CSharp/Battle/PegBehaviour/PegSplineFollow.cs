using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

namespace Battle.PegBehaviour
{
	[RequireComponent(typeof(SpriteShapeRenderer))]
	[RequireComponent(typeof(SpriteShapeController))]
	public class PegSplineFollow : NonDummyPegboardBehaviour
	{
		private const float REGULAR_PEG_RADIUS = 0.25f;

		private const float DISTANCE_DELTA_FOR_PRECALULATED_POSITIONS = 0.03f;

		public Peg[] pegPrefabs;

		[Tooltip("Series of floats corresponding to the odds of spawning a particular peg.Size should equal pegPrefabs and values should ascend up to 1.0")]
		public float[] pegTypeSpawnRatio;

		[Space(10f)]
		[Range(0.0001f, 3f)]
		public float pegDencity = 1f;

		public float pegMoveSpeed = 1f;

		[Space(10f)]
		[Header("Spline Parameters")]
		public int numSamplesPerBezier = 25;

		private SpriteShapeController spriteShapeController;

		private Bezier[] beziersInSpline;

		private float totalSplineLength;

		private int numPegs;

		public List<Peg> _pegs;

		private List<float> _pegSplineDistances;

		private BattleController _battleController;

		private PredictionManager _predictionManager;

		private List<float> _precalculatedSplineDistances = new List<float>();

		private List<Vector3> _precalculatedSplinePositions = new List<Vector3>();

		private void OnDrawGizmos()
		{
			Init();
			Gizmos.color = Color.white;
			Vector3[] pegPositions = GetPegPositions(GetPegSplineDistances());
			for (int i = 0; i < pegPositions.Length; i++)
			{
				Gizmos.DrawSphere(pegPositions[i] + base.gameObject.transform.position, 0.25f);
			}
		}

		public void Init()
		{
			if (spriteShapeController == null)
			{
				spriteShapeController = GetComponent<SpriteShapeController>();
			}
			Spline spline = spriteShapeController.spline;
			beziersInSpline = SampleSpline(spline);
			totalSplineLength = 0f;
			Bezier[] array = beziersInSpline;
			foreach (Bezier bezier in array)
			{
				totalSplineLength += bezier.TotalArcLength;
			}
			float num = totalSplineLength / 0.5f;
			numPegs = Mathf.RoundToInt(num * pegDencity);
			_pegSplineDistances = GetPegSplineDistances();
			PrecalculateSplineDistancePositions();
			if (!isDummy && Application.IsPlaying(base.gameObject))
			{
				CreatePegs();
			}
		}

		private void PrecalculateSplineDistancePositions()
		{
			_precalculatedSplineDistances.Clear();
			_precalculatedSplinePositions.Clear();
			for (float num = 0f; num < totalSplineLength; num += 0.03f)
			{
				_precalculatedSplineDistances.Add(num);
				_precalculatedSplinePositions.Add(GetPositionForDistanceAlongSpline(num));
			}
		}

		private Vector3 GetPositionForDistanceAlongSpline(float distanceAlongSpline)
		{
			distanceAlongSpline %= totalSplineLength;
			if (distanceAlongSpline < 0f)
			{
				distanceAlongSpline += totalSplineLength;
			}
			Bezier bezier = null;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			Bezier[] array = beziersInSpline;
			foreach (Bezier bezier2 in array)
			{
				num3 = num;
				num += bezier2.TotalArcLength;
				num2 = num;
				if (distanceAlongSpline <= num)
				{
					bezier = bezier2;
					break;
				}
			}
			if (bezier == null)
			{
				Debug.LogError($"Coulnd't find bezier for distance {distanceAlongSpline}");
				return Vector3.zero;
			}
			float t = (distanceAlongSpline - num3) / (num2 - num3);
			return bezier.GetPoint(t);
		}

		private Bezier[] SampleSpline(Spline spline)
		{
			List<Bezier> list = new List<Bezier>();
			for (int i = 0; i < spline.GetPointCount() - 1; i++)
			{
				Vector3 position = spline.GetPosition(i);
				Vector3 p = spline.GetRightTangent(i) + position;
				Vector3 position2 = spline.GetPosition(i + 1);
				Vector3 p2 = spline.GetLeftTangent(i + 1) + position2;
				Bezier item = new Bezier(position, p, p2, position2, numSamplesPerBezier);
				list.Add(item);
			}
			return list.ToArray();
		}

		private List<float> GetPegSplineDistances()
		{
			List<float> list = new List<float>();
			float num = totalSplineLength / (float)numPegs;
			float num2 = 0f;
			for (int i = 0; i < numPegs; i++)
			{
				list.Add(num2);
				num2 += num;
			}
			return list;
		}

		private Vector3[] GetPegPositions(List<float> pegSplineDistances)
		{
			List<Vector3> list = new List<Vector3>();
			foreach (float pegSplineDistance in pegSplineDistances)
			{
				list.Add(GetPositionForDistanceAlongSpline(pegSplineDistance));
			}
			return list.ToArray();
		}

		private void Start()
		{
			if (pegPrefabs.Length == 0)
			{
				Debug.LogError("PegSplineFollow has no peg prefabs set.");
			}
			if (pegTypeSpawnRatio.Length != pegPrefabs.Length)
			{
				Debug.LogError("PegSplineFollow doesn't have an equal number of peg prefabs and spawn ratios set.");
			}
			Init();
		}

		private void CreatePegs()
		{
			if (_pegs != null && _pegs.Count > 0)
			{
				return;
			}
			_pegs = new List<Peg>();
			List<Peg> list = new List<Peg>();
			List<Bomb> list2 = new List<Bomb>();
			Vector3[] pegPositions = GetPegPositions(_pegSplineDistances);
			foreach (Vector3 vector in pegPositions)
			{
				float value = Random.value;
				for (int j = 0; j < pegPrefabs.Length; j++)
				{
					if (value <= pegTypeSpawnRatio[j])
					{
						Peg peg = Object.Instantiate(pegPrefabs[j], base.transform);
						peg.transform.position = vector + base.gameObject.transform.parent.position;
						_pegs.Add(peg);
						if (peg.GetComponent<Bomb>() == null)
						{
							list.Add(peg);
						}
						else
						{
							list2.Add(peg as Bomb);
						}
						break;
					}
				}
			}
			if (!Application.IsPlaying(base.gameObject))
			{
				return;
			}
			_battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
			_predictionManager = _battleController.PredictionManager;
			if (_battleController != null)
			{
				_battleController.AddPegs(list);
				_battleController.AddBombs(list2);
			}
			foreach (Peg item in list)
			{
				_predictionManager.CopyPegToSimulation(item.gameObject, null);
			}
			foreach (Bomb item2 in list2)
			{
				_predictionManager.CopyPegToSimulation(item2.gameObject, null);
			}
		}

		private void FixedUpdate()
		{
			List<float> list = new List<float>();
			for (int i = 0; i < _pegs.Count; i++)
			{
				Peg peg = _pegs[i];
				float num = _pegSplineDistances[i] + pegMoveSpeed * Time.deltaTime;
				list.Add(num);
				peg.transform.position = FindNextPegPosition(num) + base.gameObject.transform.parent.position;
			}
			_pegSplineDistances = list;
		}

		private Vector3 FindNextPegPosition(float distanceAlongSpline)
		{
			distanceAlongSpline %= totalSplineLength;
			if (distanceAlongSpline < 0f)
			{
				distanceAlongSpline += totalSplineLength;
			}
			int num = _precalculatedSplineDistances.BinarySearch(distanceAlongSpline);
			int num2 = num;
			if (num < 0)
			{
				num2 = ~num;
			}
			if (num2 == _precalculatedSplinePositions.Count)
			{
				num2 = 0;
			}
			return _precalculatedSplinePositions[num2];
		}
	}
}
