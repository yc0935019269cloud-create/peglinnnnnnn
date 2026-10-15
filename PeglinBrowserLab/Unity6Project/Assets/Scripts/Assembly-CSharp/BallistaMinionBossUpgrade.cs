using Battle.Enemies.C20Upgrades;

public class BallistaMinionBossUpgrade : BossUpgrade
{
	public override void Upgrade()
	{
		_enemy.defaultMovement++;
		_enemy.MaxMovement++;
	}

	public override float GetUpgradedMaxHealth()
	{
		return _enemy.maxHealth * 2f;
	}
}
