using System.Globalization;
using Battle;
using Battle.Attacks;
using Battle.Enemies;
using I2.Loc;
using UnityEngine;

public class HealAction : Attack
{
	[Header("Heal Params:")]
	public float damageTargetedEnemyMultiplier;

	public float damageAllEnemiesMultiplier;

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("HEAL_DAMAGE_TARGETED", damageTargetedEnemyMultiplier.ToString(CultureInfo.InvariantCulture));
			component.SetParameterValue("HEAL_DAMAGE_ALL", damageAllEnemiesMultiplier.ToString(CultureInfo.InvariantCulture));
		}
	}

	public override void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long dmgBonus, int critCount = 0)
	{
		long damage = GetDamage(attackManager, pegMultipliersTally, dmgMult, dmgBonus, critCount);
		PlayerHealthController playerHealthController = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealthController>();
		if (playerHealthController != null)
		{
			DoHealAction(playerHealthController, damage);
		}
		_attackManager.WaitAndEndAnim();
	}

	public void DoHealAction(PlayerHealthController playerHealth, long healAmount)
	{
		float num = playerHealth.Heal(healAmount);
		if (num > 0f && damageTargetedEnemyMultiplier > 0f)
		{
			TargetingManager.OnDamageTargetedEnemy?.Invoke(Mathf.RoundToInt(num * damageTargetedEnemyMultiplier), Enemy.EnemyDamageSource.TargetedAttack);
		}
		else if (num > 0f && damageAllEnemiesMultiplier > 0f)
		{
			Enemy.OnAllEnemiesDamaged?.Invoke(Mathf.RoundToInt(num * damageAllEnemiesMultiplier), Enemy.EnemyDamageSource.AOE);
		}
	}
}
