using System.Collections;
using UnityEngine;

namespace Battle.Effects
{
	public class BonusSpecialPegEffect : MonoBehaviour
	{
		public Material _refreshMaterial;

		public Material _critMaterial;

		public Material _bombMaterial;

		public Material _bombRiggedMaterial;

		public Material _maxHPMaterial;

		public AudioClip _refreshClip;

		public AudioClip _critClip;

		public AudioClip _bombClip;

		public AudioClip _bombRiggedClip;

		public AudioClip _maxHPClip;

		private ParticleSystem _particleSystem;

		private ParticleSystemRenderer _particleRenderer;

		private AudioSource _audioSource;

		private void Awake()
		{
			_particleSystem = GetComponent<ParticleSystem>();
			_particleRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
			_audioSource = GetComponent<AudioSource>();
		}

		public void PlayEffect(Peg.PegType pegType, Vector3 pos, bool bombIsRigged = false)
		{
			Material material = null;
			AudioClip audioClip = null;
			switch (pegType)
			{
			case Peg.PegType.RESET:
				material = _refreshMaterial;
				audioClip = _refreshClip;
				break;
			case Peg.PegType.CRIT:
				material = _critMaterial;
				audioClip = _critClip;
				break;
			case Peg.PegType.BOMB:
				material = (bombIsRigged ? _bombRiggedMaterial : _bombMaterial);
				audioClip = (bombIsRigged ? _bombRiggedClip : _bombClip);
				break;
			case Peg.PegType.MAX_HP:
				material = _maxHPMaterial;
				audioClip = _maxHPClip;
				break;
			}
			if (material != null)
			{
				base.transform.position = pos;
				_particleRenderer.material = material;
				_particleSystem.Play();
				float num = _particleSystem.main.startLifetime.constant;
				if (audioClip != null)
				{
					_audioSource.PlayOneShot(audioClip);
					num = Mathf.Max(num, audioClip.length);
				}
				StartCoroutine(DisableGameObjectAfterParticles(num));
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}

		private IEnumerator DisableGameObjectAfterParticles(float time)
		{
			yield return new WaitForSeconds(time + 0.65f);
			base.gameObject.SetActive(value: false);
		}
	}
}
