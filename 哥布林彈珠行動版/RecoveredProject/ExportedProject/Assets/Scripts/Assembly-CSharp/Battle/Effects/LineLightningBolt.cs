using System.Collections.Generic;
using UnityEngine;

namespace Battle.Effects
{
	public class LineLightningBolt : MonoBehaviour
	{
		public int numberOfPoints = 5;

		public float length = 5f;

		public float maxHeight = 1f;

		public float minHeight = 0.5f;

		public int positionSets = 4;

		public float interval = 0.25f;

		private LineRenderer _lineRenderer;

		private List<Vector3[]> _positions;

		private float _timer;

		private int index;

		private void OnEnable()
		{
			_lineRenderer = GetComponent<LineRenderer>();
			_lineRenderer.positionCount = numberOfPoints;
			_positions = new List<Vector3[]>();
			for (int i = 0; i < positionSets; i++)
			{
				_positions.Add(GeneratePoints());
			}
		}

		private void Update()
		{
			if (_timer > interval)
			{
				_timer = 0f;
				_lineRenderer.SetPositions(_positions[index]);
				if (++index >= _positions.Count)
				{
					index = 0;
				}
			}
			else
			{
				_timer += Time.deltaTime;
			}
		}

		private Vector3[] GeneratePoints()
		{
			Vector3[] array = new Vector3[numberOfPoints];
			float num = length / (float)numberOfPoints;
			for (int i = 0; i < numberOfPoints; i++)
			{
				float x;
				float y;
				if (i == 0)
				{
					x = 0f;
					y = 0f;
				}
				else if (i == numberOfPoints - 1)
				{
					x = length;
					y = 0f;
				}
				else
				{
					x = num * (float)i + Random.Range(0f, num * 0.25f);
					int num2 = ((Random.Range(0, 2) > 0) ? 1 : (-1));
					y = Random.Range(minHeight, maxHeight) * (float)num2;
				}
				Vector3 vector = new Vector3(x, y);
				array[i] = vector;
			}
			return array;
		}
	}
}
