using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Enemy Sfx")]
public class EnemySfx : ScriptableObject
{
	public AudioClip[] DamagedClip;

	public AudioClip[] DeadClips;

	public AudioClip MeleeAttackClip;

	public AudioClip RangedAttackClip;

	public AudioClip[] SpecialClips;

	public AudioClip GetRandomEnemyDamagedClip()
	{
		return DamagedClip[Random.Range(0, DamagedClip.Length)];
	}

	public AudioClip GetRandomEnemyDeadClip()
	{
		return DeadClips[Random.Range(0, DeadClips.Length)];
	}

	public AudioClip GetRandomEnemySpecialClip()
	{
		return SpecialClips[Random.Range(0, SpecialClips.Length)];
	}
}
