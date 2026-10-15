using Battle.Enemies;
using Battle.Enemies.Behaviours;
using UnityEngine;

public class LeshyBushBramballIncrementer : MonoBehaviour
{
	public int bramballIntensityIncreaseAmount = 1;

	private ApplyStatusEffectsToPlayerBehaviour _leshyApplyStatusEffectsToPlayerBehaviour;

	private ParticleSystem _leshyEnragedParticles;

	private void Awake()
	{
		LeshyBoss leshyBoss = Object.FindObjectOfType<LeshyBoss>();
		if (!(leshyBoss == null))
		{
			leshyBoss.GetComponent<Animator>().SetTrigger(Animator.StringToHash("spawn_enemy"));
			_leshyApplyStatusEffectsToPlayerBehaviour = leshyBoss.GetComponent<ApplyStatusEffectsToPlayerBehaviour>();
			_leshyEnragedParticles = leshyBoss.GetComponentInChildren<ParticleSystem>();
		}
	}

	public void IncreaseLeshyBossBramballApplicationIntensity()
	{
		if (_leshyApplyStatusEffectsToPlayerBehaviour != null)
		{
			_leshyApplyStatusEffectsToPlayerBehaviour.IncreaseStatusEffectApplicationIntencity(bramballIntensityIncreaseAmount);
			if (_leshyEnragedParticles != null)
			{
				_leshyEnragedParticles.Stop();
				_leshyEnragedParticles.Play();
			}
		}
	}
}
