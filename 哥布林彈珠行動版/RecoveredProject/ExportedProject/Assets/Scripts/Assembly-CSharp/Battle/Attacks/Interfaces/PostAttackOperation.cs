using Battle.StatusEffects;

namespace Battle.Attacks.Interfaces
{
	public interface PostAttackOperation
	{
		void HandleAttackFinished(PlayerStatusEffectController statusController);
	}
}
