using System;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	[RequireComponent(typeof(Enemy))]
	public class EnemyTakesDamageOnPegActivated : MonoBehaviour
	{
		public float damage = 1f;

		private Enemy _enemy;

		private void Awake()
		{
			_enemy = GetComponent<Enemy>();
		}

		private void OnEnable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
		}

		private void OnDisable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
		}

		private void HandlePegActivated(Peg.PegType pegtype, Peg peg)
		{
			_enemy.Damage(1L, screenshake: false);
		}
	}
}
