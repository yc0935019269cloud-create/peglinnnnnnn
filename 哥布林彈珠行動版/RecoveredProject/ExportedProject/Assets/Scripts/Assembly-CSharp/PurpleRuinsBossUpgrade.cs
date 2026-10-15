using System.Collections.Generic;
using Battle.Enemies.Behaviours;
using Battle.StatusEffects;

public class PurpleRuinsBossUpgrade : RuinsBossUpgrade
{
	public StatusEffect extraStatusEffectToAdd;

	public override void Upgrade()
	{
		BuffAlliesBehaviour component = GetComponent<BuffAlliesBehaviour>();
		component.additionalStatuses = new List<StatusEffect>(component.additionalStatuses) { extraStatusEffectToAdd }.ToArray();
	}
}
