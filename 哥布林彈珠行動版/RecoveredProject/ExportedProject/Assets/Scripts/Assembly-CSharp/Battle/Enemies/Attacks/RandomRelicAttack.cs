using System.Collections.Generic;
using PeglinUtils;
using Relics;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class RandomRelicAttack : MonoBehaviour
	{
		[SerializeField]
		private RelicManager _relicManager;

		public SpriteRenderer spriteRenderer;

		private void Start()
		{
			List<Relic> list = new List<Relic>(_relicManager.RareRelicPool.ToArray());
			list.Shuffle();
			foreach (Relic item in list)
			{
				if (!_relicManager.RelicEffectActive(item.effect))
				{
					spriteRenderer.sprite = item.sprite;
					break;
				}
			}
		}
	}
}
