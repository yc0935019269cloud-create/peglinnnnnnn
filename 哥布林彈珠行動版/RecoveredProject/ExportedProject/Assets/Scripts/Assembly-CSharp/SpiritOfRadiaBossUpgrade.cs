using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Enemies;
using Battle.Enemies.Behaviours.SpiritOfRadia;
using Battle.Enemies.C20Upgrades;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using UnityEngine;

public class SpiritOfRadiaBossUpgrade : BossUpgrade
{
	public int effectPegRechargeLengthChange = -1;

	public int extraLifelink = 150;

	public int flamePegsToAddAfterSlotsFull = 2;

	public int extraMuscircle = 1;

	public Vector2 extraBlackHoleLocationPhase1;

	public Vector2 extraBlackHoleLocationPhase2;

	public override void Upgrade()
	{
		AddFireToSlot component = _enemy.GetComponent<AddFireToSlot>();
		component.addFlamePegsOnceBottomsSlotsFull = true;
		component.flamePegsToAdd = flamePegsToAddAfterSlotsFull;
		_enemy.GetComponent<ApplyLifelinkOnSpawn>().lifelinkIntensity += extraLifelink;
		_enemy.GetComponent<ApplyResistancesOnSpawn>().multiplyIntangiballInsteadOfRemove = true;
		_enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
		AddBlackHoles component2 = _enemy.GetComponent<AddBlackHoles>();
		component2.blackHoleCoordinates = new List<Vector2>(component2.blackHoleCoordinates) { extraBlackHoleLocationPhase1 }.ToArray();
		SpiritOfRadiaBoss obj = _enemy as SpiritOfRadiaBoss;
		obj.phase2BlackHoleCoordinates = new List<Vector2>(obj.phase2BlackHoleCoordinates) { extraBlackHoleLocationPhase2 }.ToArray();
		StartCoroutine(WaitAndChangeEffectPegRechargeLength());
	}

	private IEnumerator WaitAndChangeEffectPegRechargeLength()
	{
		yield return null;
		EffectPeg[] array = Object.FindObjectsOfType<EffectPeg>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].turnsToRecharge += effectPegRechargeLengthChange;
		}
	}
}
