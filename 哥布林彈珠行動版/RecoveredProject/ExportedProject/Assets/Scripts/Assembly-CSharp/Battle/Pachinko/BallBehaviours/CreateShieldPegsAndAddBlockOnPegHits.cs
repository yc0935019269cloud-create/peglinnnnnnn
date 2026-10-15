using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class CreateShieldPegsAndAddBlockOnPegHits : CreateShieldPegsOnPegHits
	{
		private PlayerStatusEffectController _playerStatusEffectController;

		protected override void Awake()
		{
			base.Awake();
			_playerStatusEffectController = Object.FindObjectOfType<PlayerStatusEffectController>();
		}

		public override bool ShouldShieldCurrentPeg(Peg peg)
		{
			bool num = base.ShouldShieldCurrentPeg(peg);
			if (num && _playerStatusEffectController != null)
			{
				_playerStatusEffectController.ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark), StatusEffectSource.PLAYER);
			}
			return num;
		}
	}
}
