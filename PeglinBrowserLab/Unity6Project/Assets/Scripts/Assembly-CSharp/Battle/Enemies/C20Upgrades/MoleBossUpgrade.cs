using Battle.EnemyBehaviours;

namespace Battle.Enemies.C20Upgrades
{
	public class MoleBossUpgrade : BossUpgrade
	{
		public override void Upgrade()
		{
			if (TryGetComponent<BuffAlliesWhenAttackedReceiver>(out var component))
			{
				component.buffAmount = 2;
			}
		}
	}
}
