using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Player Sfx")]
public class PlayerSfx : ScriptableObject
{
	public AudioClip PlayerHitSfx;

	public AudioClip PlayerPoisonHitSfx;

	public AudioClip PlayerDodgeSfx;

	public AudioClip ArmourBlockSfx;

	public AudioClip ArmourBreakSfx;

	public AudioClip LootRewardSfx;

	public AudioClip MaxHPIncreaseSfx;

	public AudioClip PlayerHitMaxHPSFX;
}
