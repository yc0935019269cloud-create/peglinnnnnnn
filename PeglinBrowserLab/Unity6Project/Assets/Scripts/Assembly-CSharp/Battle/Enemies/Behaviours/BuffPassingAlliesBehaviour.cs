using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class BuffPassingAlliesBehaviour : EffectPassingEnemiesBehaviour
	{
		public bool alsoBuffSelf;

		public StatusEffect[] statusEffects;

		public string animOverride;

		public AudioClip buffSoundEffect;

		private Animator _animator;

		private AudioSource _audioSource;

		protected override void Awake()
		{
			base.Awake();
			_animator = GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();
		}

		protected override void ApplyEffect(Enemy enemy)
		{
			if (_audioSource != null && buffSoundEffect != null)
			{
				_audioSource.PlayOneShot(buffSoundEffect);
			}
			StatusEffect[] array = statusEffects;
			foreach (StatusEffect statusEffect in array)
			{
				enemy.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, statusEffect.Intensity), StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
			}
			if (alsoBuffSelf)
			{
				array = statusEffects;
				foreach (StatusEffect statusEffect2 in array)
				{
					_enemy.ApplyStatusEffect(new StatusEffect(statusEffect2.EffectType, statusEffect2.Intensity), StatusEffectSource.ENEMY, allowKnockOnEffects: true, ignoreStats: true);
				}
			}
			if (!string.IsNullOrEmpty(animOverride))
			{
				_animator.SetTrigger(animOverride);
			}
		}
	}
}
