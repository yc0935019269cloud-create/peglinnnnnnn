using UnityEngine;

namespace PeglinUI.UIUtils
{
	public static class OrbUIUtils
	{
		public struct OrbVisuals
		{
			public Sprite sprite;

			public float scale;

			public Color color;

			public Vector2 pivot;

			public OrbVisuals(Sprite sprite, float scale, Color color)
			{
				this.sprite = sprite;
				this.scale = scale;
				this.color = color;
				pivot = GetSpritePivotPointRelative(sprite);
			}
		}

		public static OrbVisuals GetOrbVisuals(GameObject orbPrefab)
		{
			SpriteRenderer componentInChildren = orbPrefab.GetComponentInChildren<SpriteRenderer>();
			return new OrbVisuals(componentInChildren.sprite, componentInChildren.transform.localScale.x, componentInChildren.color);
		}

		public static Vector2 GetSpritePivotPointRelative(Sprite sprite)
		{
			float x = sprite.pivot.x / sprite.rect.width;
			float y = sprite.pivot.y / sprite.rect.height;
			return new Vector2(x, y);
		}
	}
}
