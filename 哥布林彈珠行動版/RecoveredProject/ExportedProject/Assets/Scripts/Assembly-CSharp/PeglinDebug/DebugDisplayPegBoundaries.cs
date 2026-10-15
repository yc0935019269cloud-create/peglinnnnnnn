using Battle.PegBehaviour;
using NaughtyAttributes;
using UnityEngine;

namespace PeglinDebug
{
	public class DebugDisplayPegBoundaries : MonoBehaviour
	{
		public float xBoundary = 8f;

		public float yBoundary = 5.6f;

		public float yOffset = -2.12f;

		public Vector3 spawnPointOffset = new Vector3(0f, -1.5f, 0f);

		public Vector3 orbSpawnPoint = new Vector3(0f, 4f);

		public Vector2[] alternateSpawns;

		public Vector2[] bottomBumperPositions = new Vector2[4]
		{
			new Vector2(-5.5f, -8f),
			new Vector2(-2f, -8f),
			new Vector2(2f, -8f),
			new Vector2(5.5f, -8f)
		};

		public float bottomBumperSize;

		[ReadOnly]
		public int numPegs;

		private void Awake()
		{
			if (Application.isPlaying)
			{
				base.enabled = false;
			}
		}

		private void OnValidate()
		{
			CountPegs();
		}

		[Button(null, EButtonEnableMode.Always)]
		private void CountPegs()
		{
			numPegs = 0;
			RotatingPegCircle[] componentsInChildren = GetComponentsInChildren<RotatingPegCircle>();
			foreach (RotatingPegCircle rotatingPegCircle in componentsInChildren)
			{
				numPegs += rotatingPegCircle.numPegs;
			}
			numPegs += GetComponentsInChildren<Peg>().Length;
		}

		[Button(null, EButtonEnableMode.Always)]
		private void GeneratePegboardData()
		{
		}

		private void OnDrawGizmos()
		{
			Vector3 vector = base.transform.position + Vector3.up * yOffset;
			Vector3 vector2 = vector + new Vector3(0f - xBoundary, 0f - yBoundary);
			Vector3 vector3 = vector + new Vector3(xBoundary, 0f - yBoundary);
			Vector3 vector4 = vector + new Vector3(xBoundary, yBoundary);
			Vector3 vector5 = vector + new Vector3(0f - xBoundary, yBoundary);
			Gizmos.DrawLine(vector2, vector3);
			Gizmos.DrawLine(vector3, vector4);
			Gizmos.DrawLine(vector4, vector5);
			Gizmos.DrawLine(vector5, vector2);
			if (bottomBumperPositions != null && bottomBumperPositions.Length != 0)
			{
				Vector2[] array = bottomBumperPositions;
				foreach (Vector2 vector6 in array)
				{
					Gizmos.DrawSphere(base.transform.position + (Vector3)vector6, bottomBumperSize);
				}
			}
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position + orbSpawnPoint + spawnPointOffset, 0.2f);
			if (alternateSpawns != null && alternateSpawns.Length != 0)
			{
				Vector2[] array = alternateSpawns;
				foreach (Vector2 vector7 in array)
				{
					Gizmos.DrawSphere(base.transform.position + (Vector3)vector7 + spawnPointOffset, 0.2f);
				}
			}
		}
	}
}
