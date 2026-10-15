using System.Collections.Generic;
using Battle.Enemies;
using Battle.Enemies.Behaviours.SpiritOfRadia;
using Battle.PegBehaviour;
using UnityEngine;

namespace PeglinDebug
{
	public class SpiritOfRadiaDebugDisplayPegBoundaries : MonoBehaviour
	{
		private const float BLACK_HOLE_RADIUS = 1.6f;

		private const float EYE_RADIUS = 1f;

		public bool includeC20BlackHoles = true;

		public SpiritOfRadiaBoss spiritOfRadiaBoss;

		public SpiritOfRadiaBossUpgrade spiritOfRadiaBossUpgrade;

		public AddBlackHoles spiritOfRadiaAddBlackHoles;

		public DebugDisplayPegBoundaries debugDisplayPegBoundaries;

		public PegLayoutAlternator pegLayoutAlternator;

		private void Awake()
		{
			if (Application.isPlaying)
			{
				base.enabled = false;
			}
		}

		private void OnValidate()
		{
			if (spiritOfRadiaBoss != null)
			{
				debugDisplayPegBoundaries.alternateSpawns = spiritOfRadiaBoss.phase2OrbStartingPositions;
			}
		}

		private void OnDrawGizmos()
		{
			if (!(spiritOfRadiaBoss != null))
			{
				return;
			}
			if (pegLayoutAlternator._pegboardB.activeInHierarchy)
			{
				Vector3 vector = base.transform.position + Vector3.up * debugDisplayPegBoundaries.yOffset;
				Vector3 vector2 = vector + new Vector3(0f - debugDisplayPegBoundaries.xBoundary, debugDisplayPegBoundaries.yBoundary);
				Vector3 to = vector + new Vector3(debugDisplayPegBoundaries.xBoundary, debugDisplayPegBoundaries.yBoundary);
				Vector3 vector3 = vector + new Vector3(0f - debugDisplayPegBoundaries.xBoundary, debugDisplayPegBoundaries.yBoundary + SpiritOfRadiaBoss.EXTRA_PEGBOARD_HEIGHT);
				Vector3 vector4 = vector + new Vector3(debugDisplayPegBoundaries.xBoundary, debugDisplayPegBoundaries.yBoundary + SpiritOfRadiaBoss.EXTRA_PEGBOARD_HEIGHT);
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(vector2, vector3);
				Gizmos.DrawLine(vector3, vector4);
				Gizmos.DrawLine(vector4, to);
			}
			spiritOfRadiaBoss.GetComponent<AddBlackHoles>();
			Vector2[] array;
			if (pegLayoutAlternator._pegboardB.activeInHierarchy)
			{
				List<Vector2> list = new List<Vector2>(spiritOfRadiaBoss.phase2BlackHoleCoordinates);
				if (includeC20BlackHoles)
				{
					list.Add(spiritOfRadiaBossUpgrade.extraBlackHoleLocationPhase2);
				}
				array = list.ToArray();
			}
			else
			{
				List<Vector2> list2 = new List<Vector2>(spiritOfRadiaAddBlackHoles.blackHoleCoordinates);
				if (includeC20BlackHoles)
				{
					list2.Add(spiritOfRadiaBossUpgrade.extraBlackHoleLocationPhase1);
				}
				array = list2.ToArray();
			}
			Gizmos.color = Color.black;
			Vector2[] array2 = array;
			foreach (Vector2 vector5 in array2)
			{
				Gizmos.DrawSphere(base.transform.position + (Vector3)vector5, 1.6f);
			}
			if (pegLayoutAlternator._pegboardB.activeInHierarchy)
			{
				Gizmos.color = Color.green;
				array2 = spiritOfRadiaBoss.phase2OrbStartingPositions;
				foreach (Vector2 vector6 in array2)
				{
					Gizmos.DrawSphere(base.transform.position + (Vector3)vector6 + debugDisplayPegBoundaries.spawnPointOffset, 0.2f);
				}
			}
			if (pegLayoutAlternator._pegboardB.activeInHierarchy)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(base.transform.position + new Vector3(spiritOfRadiaBoss.phase2EyePos.x, spiritOfRadiaBoss.phase2EyePos.y + 0.5f, base.transform.position.z), 1f);
			}
			if (pegLayoutAlternator._pegboardA.activeInHierarchy)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(base.transform.position + debugDisplayPegBoundaries.orbSpawnPoint + debugDisplayPegBoundaries.spawnPointOffset, 0.2f);
			}
		}
	}
}
