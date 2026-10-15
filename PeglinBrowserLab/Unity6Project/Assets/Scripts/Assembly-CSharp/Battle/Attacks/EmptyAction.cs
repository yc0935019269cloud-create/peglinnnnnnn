using Battle.Enemies;

namespace Battle.Attacks
{
	public class EmptyAction : Attack
	{
		public override void Fire(AttackManager attackManager, Enemy target, int pegMultiplierTally, int numPegsHit, float dmgMult, long dmgBonus, int critCount = 0)
		{
			_attackManager.AttackAnimationEnded();
		}

		public override int GetModifiedDamagePerPeg(int critCount = 0, bool calculateStaticDamageModificersInPlace = true)
		{
			return 0;
		}

		public override long GetDamage(AttackManager attackManager, int pegMultipliersTally, float dmgMult, long bonus, int critCount = 0, bool displayNegative = false)
		{
			return 0L;
		}
	}
}
