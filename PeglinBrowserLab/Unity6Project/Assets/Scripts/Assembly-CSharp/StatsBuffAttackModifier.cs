using Battle.Attacks;
using Battle.Attacks.AttackModifications;

public class StatsBuffAttackModifier : NextAttackModifier
{
	public override void UpdateAttackModifier()
	{
		AttackModifier attackModifier = base.attackModifier;
		Attack component = GetComponent<Attack>();
		if (component != null)
		{
			attackModifier.baseDamageModification = component.GetModifiedDamagePerPeg();
			attackModifier.critDamageModification = component.GetModifiedDamagePerPeg(1);
		}
		base.attackModifier = attackModifier;
	}
}
