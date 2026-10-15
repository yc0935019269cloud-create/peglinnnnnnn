using Battle.Enemies;
using Battle.Enemies.C20Upgrades;
using Battle.StatusEffects;
using UnityEngine;

public class LeshyBossUpgrade : BossUpgrade
{
	public int stumpSummonSlot = 4;

	public GameObject stumpEnemy;

	public override void Upgrade()
	{
		Vector3 spawnPointForGroundSlot = _enemy.enemyManager.GetSpawnPointForGroundSlot(stumpSummonSlot);
		Enemy component = Object.Instantiate(stumpEnemy, spawnPointForGroundSlot, Quaternion.identity).GetComponent<Enemy>();
		component.Initialize(_enemy.GetComponentInChildren<StatusEffectIconManager>().EffectData, _enemy.enemyManager, _enemy.relicManager, stumpEnemy.name);
		_enemy.enemyManager.AddEnemy(component, stumpSummonSlot);
	}
}
