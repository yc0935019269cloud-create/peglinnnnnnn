using System.Collections;
using Battle.Enemies;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public abstract class AlternateEnemyAction : MonoBehaviour
	{
		public int priority;

		public int chargeTurns;

		[Tooltip("Free Actions don't take up the enemies turn to do.")]
		public bool isFreeAction;

		public bool forceAllowAlternateActionsAfterThis;

		protected Enemy _enemy;

		protected int _currentChargeTurns;

		public AudioClip audioClip;

		protected AudioSource _audioSource;

		public int currentChargeTurns => _currentChargeTurns;

		protected virtual void Awake()
		{
			_enemy = GetComponent<Enemy>();
			_audioSource = _enemy.GetComponent<AudioSource>();
		}

		public virtual bool IncrementAndCheckCharge()
		{
			if (chargeTurns == 0)
			{
				return true;
			}
			_currentChargeTurns = (_currentChargeTurns + 1) % (chargeTurns + 1);
			if (_currentChargeTurns == 0)
			{
				return true;
			}
			return false;
		}

		protected void PlaySFX()
		{
			if (audioClip != null)
			{
				_audioSource.PlayOneShot(audioClip);
			}
		}

		public abstract bool ConditionsMet();

		public abstract IEnumerator TakeAction(Vector3 target);
	}
}
