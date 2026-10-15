using UnityEngine;

namespace Peglin.PegMinigame
{
	public abstract class Reward
	{
		public abstract void Grant();

		public abstract Sprite GetSprite();

		public abstract Color GetSpriteColor();

		public abstract Vector3 GetSpriteLocalScale();

		public abstract void DrawTooltip(Canvas canvas, Vector3 position);

		public abstract void DisableTooltip();
	}
}
