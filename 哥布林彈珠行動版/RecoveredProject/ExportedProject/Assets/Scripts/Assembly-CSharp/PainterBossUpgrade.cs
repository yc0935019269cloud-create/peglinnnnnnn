using Battle.Enemies.C20Upgrades;

public class PainterBossUpgrade : BossUpgrade
{
	public override void Upgrade()
	{
		_enemy.DamagePerRangedAttack++;
	}
}
