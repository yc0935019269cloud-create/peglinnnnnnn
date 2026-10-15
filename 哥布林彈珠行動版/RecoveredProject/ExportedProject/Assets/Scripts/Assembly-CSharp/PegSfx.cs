using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Peg Sfx")]
public class PegSfx : ScriptableObject
{
	public AudioClip RegularPegSfx;

	public AudioClip CritPegSfx;

	public AudioClip ResetPegSfx;

	public AudioClip BouncerSfx;

	public AudioClip BallDestructionSfx;

	public AudioClip BombBeforeExplosionSfx;

	public AudioClip BombExplosionSfx;

	public AudioClip[] BonusCoinAddedClips;

	public AudioClip SpinfectionPegSfx;

	public AudioClip[] CoinCollectedClips;

	public AudioClip CoinStolenClip;

	public AudioClip[] CrystalPegBreakingClips;

	public AudioClip GetAudioClip(Peg.PegType type)
	{
		return type switch
		{
			Peg.PegType.REGULAR => RegularPegSfx, 
			Peg.PegType.CRIT => CritPegSfx, 
			Peg.PegType.RESET => ResetPegSfx, 
			Peg.PegType.BOUNCER => BouncerSfx, 
			Peg.PegType.SPINFECTION => SpinfectionPegSfx, 
			Peg.PegType.CRYSTAL => CrystalPegBreakingClips[Random.Range(0, CrystalPegBreakingClips.Length)], 
			_ => RegularPegSfx, 
		};
	}

	public AudioClip GetBonusCoinAddedClip()
	{
		return BonusCoinAddedClips[Random.Range(0, BonusCoinAddedClips.Length)];
	}

	public AudioClip GetCoinCollectedClip()
	{
		return CoinCollectedClips[Random.Range(0, CoinCollectedClips.Length)];
	}
}
