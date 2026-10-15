namespace Battle.Enemies.Behaviours
{
	public class HealPassingAlliesBehaviour : EffectPassingEnemiesBehaviour
	{
		public float healAmount = 30f;

		protected override void ApplyEffect(Enemy enemy)
		{
			if (enemy.CurrentHealth < enemy.maxHealth)
			{
				enemy.Heal(healAmount);
			}
		}
	}
}
