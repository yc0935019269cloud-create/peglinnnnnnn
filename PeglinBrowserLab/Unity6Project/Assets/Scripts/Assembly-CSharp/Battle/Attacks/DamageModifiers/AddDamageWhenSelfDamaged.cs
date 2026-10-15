using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageWhenSelfDamaged : AttackBaseDamageModifier
	{
		public int normalDamageBonus = 1;

		public int critDamageBonus = 2;

		private BattleController _battleController;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController sfController, int critCount = 0)
		{
			if (deckManager == null)
			{
				return 0;
			}
			int result = 0;
			if (_battleController == null)
			{
				_battleController = Object.FindObjectOfType<BattleController>();
			}
			if (_battleController != null)
			{
				result = _battleController.selfDamageCount * ((critCount == 0) ? normalDamageBonus : critDamageBonus);
			}
			return result;
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("SELF_DMG_BOOST_DMG", normalDamageBonus.ToString());
				component.SetParameterValue("SELF_DMG_BOOST_CRIT", critDamageBonus.ToString());
			}
		}
	}
}
