using System;
using System.Globalization;
using System.Linq.Expressions;
using UnityEngine;

namespace AutoLetterbox
{
	public static class Util
	{
		public static string GrootWhatAreYouDoing()
		{
			return "I am Groot.";
		}

		public static float AsPositive(float value)
		{
			return Mathf.Abs(value);
		}

		public static float AsNegative(float value)
		{
			return 0f - Mathf.Abs(value);
		}

		public static float BezierCurve(float[] p, float t)
		{
			if (p.Length > 2)
			{
				float[] array = new float[p.Length - 1];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Lerp(p[i], p[i + 1], t);
				}
				return BezierCurve(array, t);
			}
			if (p.Length == 2)
			{
				return Lerp(p[0], p[1], t);
			}
			Debug.Log("WARNING: A class attempted to get a Bezier Curve with less than two points!");
			return 0f;
		}

		public static Vector3 BezierCurve(Vector3[] p, float t)
		{
			if (p.Length > 2)
			{
				Vector3[] array = new Vector3[p.Length - 1];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Lerp(p[i], p[i + 1], t);
				}
				return BezierCurve(array, t);
			}
			if (p.Length == 2)
			{
				return Lerp(p[0], p[1], t);
			}
			return Vector3.zero;
		}

		public static Vector3 CalculateReflectedVelocity(Vector3 originalVelocity, Vector3 normalOfCollision)
		{
			Vector3 vector = -normalOfCollision;
			Vector3 normalized = originalVelocity.normalized;
			normalized = new Vector3(normalized.x - vector.x, normalized.y - vector.y, 0f);
			if (normalized.sqrMagnitude == 0f)
			{
				normalized = -originalVelocity.normalized;
			}
			return normalized;
		}

		public static float Clamp(float min, float max, float value)
		{
			value = ((value < min) ? min : value);
			value = ((value > max) ? max : value);
			return value;
		}

		public static int Clamp(int min, int max, int value)
		{
			value = ((value < min) ? min : value);
			value = ((value > max) ? max : value);
			return value;
		}

		public static string ColorToHex(Color32 color)
		{
			return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		}

		public static Vector2 DegreesToVector(float _angle)
		{
			return new Vector2((float)Math.Cos(_angle * (MathF.PI / 180f)), (float)Math.Sin(_angle * (MathF.PI / 180f)));
		}

		public static float Difference(float a, float b)
		{
			if (!(a > b))
			{
				return b - a;
			}
			return a - b;
		}

		public static int Difference(int a, int b)
		{
			if (a <= b)
			{
				return b - a;
			}
			return a - b;
		}

		public static Vector3 DirectionVector(Vector3 origin, Vector3 target)
		{
			return (target - origin).normalized;
		}

		public static void Error(object sender, string error)
		{
			if (sender != null)
			{
				Debug.Log(sender.GetType().ToString() + ": " + error);
			}
			else
			{
				Debug.Log("NULL SENDER: " + error);
			}
		}

		public static string GetMemberName<T>(Expression<Func<T>> expression)
		{
			return ((MemberExpression)expression.Body).Member.Name;
		}

		public static Color HexToColor(string hex)
		{
			byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			return new Color32(r, g, b, byte.MaxValue);
		}

		public static int IndexToFlags(int _index)
		{
			return _index switch
			{
				0 => 1, 
				1 => 2, 
				2 => 4, 
				3 => 8, 
				4 => 16, 
				5 => 32, 
				6 => 64, 
				7 => 128, 
				8 => 256, 
				9 => 512, 
				_ => -1, 
			};
		}

		public static bool IsOdd(int value)
		{
			return value % 2 != 0;
		}

		public static Vector3 Lerp(Vector3 p1, Vector3 p2, float t)
		{
			return p1 + (p2 - p1) * t;
		}

		public static float Lerp(float p1, float p2, float t)
		{
			return p1 + (p2 - p1) * t;
		}

		public static void NullError(object sender, string variableName, string extraNotes = "")
		{
			if (sender == null)
			{
				Debug.Log("NULL SENDER: " + variableName + " is null! " + extraNotes);
				return;
			}
			Debug.Log(sender.GetType().ToString() + ": " + variableName + " is null! " + extraNotes);
		}

		public static float Percent(float min, float max, float value)
		{
			if (max - min == 0f)
			{
				return 0f;
			}
			return Clamp(0f, 1f, (value - min) / (max - min));
		}

		public static float PercentUnclampled(float min, float max, float value)
		{
			if (max - min == 0f)
			{
				Debug.Log("WARNING: A class attempted to find an unclamped percentage of 0!");
				return 0f;
			}
			return (value - min) / (max - min);
		}

		public static bool RectContainsRect(Vector2 extremeMinA, Vector2 extremeMaxA, Vector2 extremeMinB, Vector2 extremeMaxB)
		{
			if (extremeMinA.y > extremeMaxB.y || extremeMaxA.y < extremeMinB.y || extremeMinA.x > extremeMaxB.x || extremeMaxA.x < extremeMinB.x)
			{
				return false;
			}
			return true;
		}

		public static Vector3 ReflectOnXAxis(Vector3 _vector)
		{
			_vector.x = 0f - _vector.x;
			return _vector;
		}

		public static Vector3 ReflectOnXandYAxis(Vector3 _vector)
		{
			_vector.x = 0f - _vector.x;
			_vector.y = 0f - _vector.y;
			return _vector;
		}

		public static Vector3 ReflectOnYAxis(Vector3 _vector)
		{
			_vector.y = 0f - _vector.y;
			return _vector;
		}

		public static float VectorToDegrees(Vector2 _vector)
		{
			float num = Mathf.Atan2(_vector.y, _vector.x) * 57.29578f;
			if (!(num > 0f))
			{
				return num + 360f;
			}
			return num;
		}

		public static Vector3 XYplaneUpDirection()
		{
			return new Vector3(0f, 0f, -1f);
		}

		public static bool IsPointOnMainCamera(Vector3 _point)
		{
			if (Camera.main == null)
			{
				return false;
			}
			Vector3 vector = Camera.main.WorldToScreenPoint(_point);
			if (vector.x < 0f || vector.y > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height)
			{
				return false;
			}
			return true;
		}
	}
}
