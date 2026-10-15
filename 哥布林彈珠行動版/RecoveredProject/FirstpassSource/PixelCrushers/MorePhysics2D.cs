using UnityEngine;

namespace PixelCrushers
{
	public static class MorePhysics2D
	{
		private static RaycastHit2D[] raycastResults = new RaycastHit2D[20];

		public static bool queriesStartInColliders
		{
			get
			{
				return Physics2D.queriesStartInColliders;
			}
			set
			{
				Physics2D.queriesStartInColliders = value;
			}
		}

		public static int maxRaycastResults
		{
			get
			{
				return raycastResults.Length;
			}
			set
			{
				if (value != raycastResults.Length)
				{
					raycastResults = new RaycastHit2D[value];
				}
			}
		}

		public static GameObject Raycast2DWithoutSelf(Transform source, Transform destination, LayerMask layerMask)
		{
			Vector2 start = new Vector2(source.position.x, source.position.y);
			Vector2 end = new Vector2(destination.position.x, destination.position.y);
			bool num = queriesStartInColliders;
			queriesStartInColliders = false;
			int num2 = Physics2D.LinecastNonAlloc(start, end, raycastResults, layerMask);
			queriesStartInColliders = num;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit2D raycastHit2D = raycastResults[i];
				if (!(raycastHit2D.transform == source))
				{
					return raycastHit2D.collider.gameObject;
				}
			}
			return null;
		}
	}
}
