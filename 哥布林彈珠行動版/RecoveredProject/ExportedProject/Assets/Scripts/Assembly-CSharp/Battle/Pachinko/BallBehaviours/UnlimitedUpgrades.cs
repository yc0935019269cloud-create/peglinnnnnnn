using Battle.Attacks.DamageModifiers;
using Battle.StatusEffects;
using I2.Loc;
using Relics;

namespace Battle.Pachinko.BallBehaviours
{
	public class UnlimitedUpgrades : AttackBaseDamageModifier
	{
		public int damageIncrease = 1;

		public int critIncrease = 2;

		private int _timesUpgraded;

		public int timesUpgraded
		{
			get
			{
				return _timesUpgraded;
			}
			set
			{
				_timesUpgraded = value;
			}
		}

		public override int GetStaticDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			if (critCount >= 1)
			{
				return critIncrease * _timesUpgraded;
			}
			return damageIncrease * _timesUpgraded;
		}

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue("UPG_DMG", damageIncrease.ToString());
			component.SetParameterValue("UPG_CRIT", critIncrease.ToString());
		}
	}
}
