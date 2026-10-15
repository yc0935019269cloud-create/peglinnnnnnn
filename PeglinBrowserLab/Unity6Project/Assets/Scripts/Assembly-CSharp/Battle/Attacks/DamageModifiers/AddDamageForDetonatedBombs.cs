using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddDamageForDetonatedBombs : AttackBaseDamageModifier
	{
		public int damagePerRegularBomb = 1;

		public int damagePerRiggedBomb = 1;

		private LocalizationParamsManager _paramsManager;

		private BattleController _battleController;

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController sfController, int critCount = 0)
		{
			int num = 0;
			if (_battleController == null)
			{
				_battleController = Object.FindObjectOfType<BattleController>();
			}
			if (_battleController != null)
			{
				num += _battleController.bombsThrownThisBattleRegular * damagePerRegularBomb + _battleController.bombsThrownThisBattleRigged * damagePerRiggedBomb;
			}
			return num;
		}

		private void SetLocParams()
		{
			_paramsManager.SetParameterValue("BOMB_DMG", damagePerRegularBomb.ToString());
			_paramsManager.SetParameterValue("BOMB_CRIT", damagePerRegularBomb.ToString());
			_paramsManager.SetParameterValue("RIGGED_DMG", damagePerRiggedBomb.ToString());
			_paramsManager.SetParameterValue("RIGGED_CRIT", damagePerRiggedBomb.ToString());
		}
	}
}
