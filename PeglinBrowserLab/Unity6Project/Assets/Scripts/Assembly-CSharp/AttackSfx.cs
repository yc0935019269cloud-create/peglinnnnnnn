using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Attack Sfx")]
public class AttackSfx : ScriptableObject
{
	public AudioClip SlowAttackClip;

	public AudioClip FastAttackClip;

	public AudioClip OnHitClip;

	public AudioClip OnWhiffClip;

	public FloatVariable ForceSfxThreshold;
}
