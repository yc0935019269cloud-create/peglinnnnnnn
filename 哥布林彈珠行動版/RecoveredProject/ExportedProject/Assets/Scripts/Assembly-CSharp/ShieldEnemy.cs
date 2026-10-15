using Battle.Enemies;
using Battle.StatusEffects;
using Relics;
using UnityEngine;

public class ShieldEnemy : WalkEnemy
{
	[Header("Shield")]
	[SerializeField]
	private BarricadeEnemy _shield;

	public BarricadeEnemy shield => _shield;

	public override void Initialize(StatusEffectData statusEffectData, EnemyManager enemyManager, RelicManager relicManager, string enemyName)
	{
		base.Initialize(statusEffectData, enemyManager, relicManager, enemyName);
		_shield.Initialize(statusEffectData, enemyManager, relicManager, enemyName);
	}
}
