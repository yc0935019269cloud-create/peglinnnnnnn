using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class AddFlatDamageBonusPerStatusEffectStackOnPlayer : AttackFlatDamageModifier
	{
		public StatusEffectType effect;

		public int damagePerStatusEffectStack = 1;

		private LocalizationParamsManager _paramsManager;

		private PlayerStatusEffectController _playerStatusEffectController;

		public override int GetFlatDamageMod(DeckManager deckManager, RelicManager relicManager, int critCount = 0)
		{
			int result = 0;
			if (_playerStatusEffectController == null)
			{
				_playerStatusEffectController = Object.FindObjectOfType<PlayerStatusEffectController>();
			}
			if (_playerStatusEffectController != null)
			{
				result = _playerStatusEffectController.EffectStrength(effect) * damagePerStatusEffectStack;
			}
			return result;
		}

		private void OnValidate()
		{
			_paramsManager = GetComponent<LocalizationParamsManager>();
			SetLocParams();
		}

		private void SetLocParams()
		{
			_paramsManager.SetParameterValue("DAMAGE_PER_STACK", damagePerStatusEffectStack.ToString());
		}
	}
}
