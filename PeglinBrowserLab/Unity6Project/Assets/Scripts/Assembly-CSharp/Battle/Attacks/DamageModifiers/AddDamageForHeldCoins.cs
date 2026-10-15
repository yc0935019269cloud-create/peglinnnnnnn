using Battle.StatusEffects;
using Currency;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageForHeldCoins : AttackBaseDamageModifier
	{
		public int coinModifier = 30;

		private BattleController _battleController;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController sfController, int critCount = 0)
		{
			int num = Mathf.FloorToInt(CurrencyManager.Instance.GoldAmount / coinModifier);
			if (critCount > 0)
			{
				return num * 2;
			}
			return num;
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("DMG_PER_COINS", coinModifier.ToString());
			}
		}
	}
}
