using System.Collections.Generic;
using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageForStonesInDeck : AttackBaseDamageModifier
	{
		public int damagePerStone = 1;

		public int critPerStone = 2;

		public bool includeBoulder;

		public bool includeOrbelisk;

		private LocalizationParamsManager _paramsManager;

		public override int GetStaticDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController sfxCtrl, int critCount = 0)
		{
			if (deckManager == null)
			{
				return 0;
			}
			int num = 0;
			bool flag = true;
			if (deckManager.battleDeck == null || deckManager.battleDeck.Count == 0)
			{
				flag = false;
			}
			else
			{
				foreach (GameObject item in deckManager.battleDeck)
				{
					if (item == null)
					{
						flag = false;
						break;
					}
				}
			}
			List<GameObject> list = ((!flag) ? DeckManager.completeDeck : deckManager.battleDeck);
			if (list != null)
			{
				foreach (GameObject item2 in list)
				{
					if (item2 != null)
					{
						Attack component = item2.GetComponent<Attack>();
						if ((component != null && (component.IsStone || (includeBoulder && component.IsBoulder))) || (includeOrbelisk && component.IsOrbelisk))
						{
							num += ((critCount == 0) ? damagePerStone : critPerStone);
						}
					}
				}
			}
			return num;
		}

		private void SetLocParams()
		{
			_paramsManager.SetParameterValue("STONE_BONUS_DMG", damagePerStone.ToString());
			_paramsManager.SetParameterValue("STONE_BONUS_CRIT", critPerStone.ToString());
		}
	}
}
