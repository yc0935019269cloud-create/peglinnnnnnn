using Currency;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddFlatDamageForHeldCoins : AttackFlatDamageModifier
	{
		public float multiplyCoinAmount = 1f;

		public override int GetFlatDamageMod(DeckManager deckManager, RelicManager relicManager, int critCount = 0)
		{
			return Mathf.RoundToInt((float)CurrencyManager.Instance.GoldAmount * multiplyCoinAmount);
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("DMG_PER_COINS_MUlTIPLIER", multiplyCoinAmount.ToString());
			}
		}
	}
}
