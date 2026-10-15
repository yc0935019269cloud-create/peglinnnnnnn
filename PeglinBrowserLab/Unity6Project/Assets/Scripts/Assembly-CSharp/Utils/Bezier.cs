using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class Bezier
	{
		private int nSegments;

		private float[] arcLengths;

		public float TotalArcLength { get; protected set; }

		public Vector3[] ObjectPositions { get; protected set; }

		public Vector3[] Normals { get; protected set; }

		public Vector3[] MirroredNormals { get; protected set; }

		public Vector3 p0 { get; protected set; }

		public Vector3 p1 { get; protected set; }

		public Vector3 p2 { get; protected set; }

		public Vector3 p3 { get; protected set; }

		public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int nSegments)
		{
			this.p0 = p0;
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
			this.nSegments = nSegments;
			TotalArcLength = 0f;
			SampleCurve();
		}

		public Vector3 GetPoint(float t)
		{
			return GetPoint(p0, p1, p2, p3, t);
		}

		public Vector3 GetNormal(float t)
		{
			return GetNormal(p0, p1, p2, p3, t);
		}

		public void PlaceObjectsAlongCurve(float objectLengthMax, float objectLengthMin, bool centered = true)
		{
			float num = 0.05f;
			float num2 = 1f;
			float num3 = objectLengthMax;
			float num4 = TotalArcLength % num3;
			float num5 = 0.15f;
			while (num4 > num5 && num2 > 0f)
			{
				num3 = Mathf.Lerp(objectLengthMin, objectLengthMax, num2);
				num4 = TotalArcLength % num3;
				num2 -= num;
			}
			int num6 = Mathf.FloorToInt(TotalArcLength / num3);
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			Normals = new Vector3[num6];
			for (int i = 0; i < num6; i++)
			{
				float num7 = (centered ? (num3 / 2f) : 0f);
				float parametricValueForTargetLength = GetParametricValueForTargetLength(num3 * (float)i - num7, arcLengths);
				list.Add(GetPoint(parametricValueForTargetLength));
				list2.Add(GetNormal(parametricValueForTargetLength));
			}
			ObjectPositions = list.ToArray();
			Normals = list2.ToArray();
		}

		private void SampleCurve()
		{
			float num = 1f / (float)nSegments;
			arcLengths = new float[nSegments + 1];
			arcLengths[0] = 0f;
			Vector3 vector = GetPoint(p0, p1, p2, p3, 0f);
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 1; i < nSegments + 1; i++)
			{
				num2 += num;
				Vector3 point = GetPoint(p0, p1, p2, p3, num2);
				Vector3 vector2 = vector - point;
				num3 += Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y);
				arcLengths[i] = num3;
				vector = point;
			}
			TotalArcLength = num3;
		}

		public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float num = 1f - t;
			return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
		}

		public static float GetParametricValueForTargetLength(float targetLength, float[] arcLengths)
		{
			int num = 0;
			int num2 = 0;
			int num3 = arcLengths.Length - 1;
			while (num < num3)
			{
				num2 = num + (((num3 - num) / 2) | 0);
				if (arcLengths[num2] < targetLength)
				{
					num = num2 + 1;
				}
				else
				{
					num3 = num2;
				}
			}
			if (arcLengths[num2] > targetLength)
			{
				num2--;
			}
			float num4 = arcLengths[num2];
			if (num4 == targetLength)
			{
				return num2 / (arcLengths.Length - 1);
			}
			return ((float)num2 + (targetLength - num4) / (arcLengths[num2 + 1] - num4)) / (float)(arcLengths.Length - 1);
		}

		public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float num = 1f - t;
			return 3f * num * num * (p1 - p0) + 6f * num * t * (p2 - p1) + 3f * t * t * (p3 - p2);
		}

		public static Vector3 GetNormal(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			Vector3 firstDerivative = GetFirstDerivative(p0, p1, p2, p3, t);
			float num = Mathf.Sqrt(firstDerivative.x * firstDerivative.x + firstDerivative.y * firstDerivative.y);
			return new Vector3((0f - firstDerivative.y) / num, firstDerivative.x / num) * 0.2f;
		}
	}
}
