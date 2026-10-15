using Battle.Enemies;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpellBehavior : MonoBehaviour
{
	public delegate void SpellHit(Enemy target);

	public delegate void SpellEnded();

	public SpellHit OnSpellHit = (Enemy target) =>
	{
	};

	public SpellEnded OnSpellEnded;

	private AudioSource _sfxAudioSource;

	[Header("Audio")]
	public AttackSfx AttackSfx;

	public Enemy target;

	public void Initialize(AudioSource sfxAudioSource)
	{
		_sfxAudioSource = sfxAudioSource;
		if (AttackSfx?.SlowAttackClip != null)
		{
			_sfxAudioSource?.PlayOneShot(AttackSfx.SlowAttackClip);
		}
	}

	public void AnimationHit()
	{
		_sfxAudioSource?.PlayOneShot(AttackSfx.OnHitClip);
		OnSpellHit?.Invoke(target);
	}

	public void AnimationEnd()
	{
		OnSpellEnded?.Invoke();
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}
}
