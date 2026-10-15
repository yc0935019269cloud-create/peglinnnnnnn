using Relics;
using UnityEngine;

namespace Map.Doodads
{
	public class ReplaceIconForMinibossMadness : MonoBehaviour
	{
		public RelicManager relicManager;

		public Sprite newSprite;

		private void Start()
		{
			if (relicManager.RelicEffectActive(RelicEffect.ONLY_MINIBOSSES))
			{
				GetComponent<SpriteRenderer>().sprite = newSprite;
			}
		}
	}
}
