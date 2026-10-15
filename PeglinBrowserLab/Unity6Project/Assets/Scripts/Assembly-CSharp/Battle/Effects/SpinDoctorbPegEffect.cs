using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Effects
{
	public class SpinDoctorbPegEffect : MonoBehaviour
	{
		public List<ParticleSystem> _negativeToPositiveParticleSystems = new List<ParticleSystem>();

		public List<ParticleSystem> _positiveToNegativeParticleSystems = new List<ParticleSystem>();

		public void PlayEffect(Vector3 pos, bool isFlippingFromNegativeToPositive)
		{
			base.transform.position = pos;
			float num = -1f;
			if (isFlippingFromNegativeToPositive)
			{
				foreach (ParticleSystem positiveToNegativeParticleSystem in _positiveToNegativeParticleSystems)
				{
					positiveToNegativeParticleSystem.gameObject.SetActive(value: false);
				}
				foreach (ParticleSystem negativeToPositiveParticleSystem in _negativeToPositiveParticleSystems)
				{
					negativeToPositiveParticleSystem.gameObject.SetActive(value: true);
					negativeToPositiveParticleSystem.Play();
					if (negativeToPositiveParticleSystem.main.startLifetime.constant > num)
					{
						num = negativeToPositiveParticleSystem.main.startLifetime.constant;
					}
				}
			}
			else
			{
				foreach (ParticleSystem negativeToPositiveParticleSystem2 in _negativeToPositiveParticleSystems)
				{
					negativeToPositiveParticleSystem2.gameObject.SetActive(value: false);
				}
				foreach (ParticleSystem positiveToNegativeParticleSystem2 in _positiveToNegativeParticleSystems)
				{
					positiveToNegativeParticleSystem2.gameObject.SetActive(value: true);
					positiveToNegativeParticleSystem2.Play();
					if (positiveToNegativeParticleSystem2.main.startLifetime.constant > num)
					{
						num = positiveToNegativeParticleSystem2.main.startLifetime.constant;
					}
				}
			}
			StartCoroutine(DisableGameObjectAfterParticles(num));
		}

		private IEnumerator DisableGameObjectAfterParticles(float time)
		{
			yield return new WaitForSeconds(time);
			base.gameObject.SetActive(value: false);
		}
	}
}
