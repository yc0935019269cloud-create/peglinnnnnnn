using Battle;
using Battle.Enemies.C20Upgrades;
using UnityEngine;

public class DemonWallBossUpgrade : BossUpgrade
{
	public AudioClip demonWallRoar;

	public int extraTurns = 6;

	public GameObject pegDullingEffect;

	public int numberOfPegsToDullOnMove = 1;

	private PegManager _pegManager;

	private void Start()
	{
		_pegManager = Object.FindObjectOfType<BattleController>().pegManager;
	}

	public override void Upgrade()
	{
		GetComponent<AudioSource>().PlayOneShot(demonWallRoar);
		Object.FindObjectOfType<BattleController>().GiveEnemiesExtraTurns(extraTurns);
	}
}
