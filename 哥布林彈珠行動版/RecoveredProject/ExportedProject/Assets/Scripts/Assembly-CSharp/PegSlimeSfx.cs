using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Peg Slime Sfx")]
public class PegSlimeSfx : ScriptableObject
{
	public AudioClip HealSlimeSfx;

	public AudioClip GetAudioClip(Peg.SlimeType slimeType)
	{
		_ = 15;
		return HealSlimeSfx;
	}
}
