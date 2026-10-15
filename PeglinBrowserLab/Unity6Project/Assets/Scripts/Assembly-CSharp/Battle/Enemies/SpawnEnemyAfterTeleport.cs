using System;
using System.Collections;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies
{
	[RequireComponent(typeof(TeleportEnemy))]
	public class SpawnEnemyAfterTeleport : MonoBehaviour
	{
		public Enemy enemyToSpawn;

		private TeleportEnemy _teleportEnemy;

		private void Awake()
		{
			_teleportEnemy = GetComponent<TeleportEnemy>();
		}

		private void OnEnable()
		{
			TeleportEnemy teleportEnemy = _teleportEnemy;
			teleportEnemy.TeleportingForward = (Action<int>)Delegate.Combine(teleportEnemy.TeleportingForward, new Action<int>(EnemyTeleported));
			TeleportEnemy teleportEnemy2 = _teleportEnemy;
			teleportEnemy2.TeleportingBack = (Action<int>)Delegate.Combine(teleportEnemy2.TeleportingBack, new Action<int>(EnemyTeleported));
		}

		private void OnDisable()
		{
			TeleportEnemy teleportEnemy = _teleportEnemy;
			teleportEnemy.TeleportingForward = (Action<int>)Delegate.Remove(teleportEnemy.TeleportingForward, new Action<int>(EnemyTeleported));
			TeleportEnemy teleportEnemy2 = _teleportEnemy;
			teleportEnemy2.TeleportingBack = (Action<int>)Delegate.Remove(teleportEnemy2.TeleportingBack, new Action<int>(EnemyTeleported));
		}

		private void EnemyTeleported(int slot)
		{
			StartCoroutine(InstantiateEnemy(slot));
		}

		private IEnumerator InstantiateEnemy(int slot)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (slot > -1 && _teleportEnemy.enemyManager.IsGroundSlotClear(slot))
			{
				Vector3 spawnPointForGroundSlot = _teleportEnemy.enemyManager.GetSpawnPointForGroundSlot(slot);
				Enemy component = UnityEngine.Object.Instantiate(enemyToSpawn, spawnPointForGroundSlot, Quaternion.identity).GetComponent<Enemy>();
				component.Initialize(_teleportEnemy.GetComponentInChildren<StatusEffectIconManager>().EffectData, _teleportEnemy.enemyManager, _teleportEnemy.relicManager, enemyToSpawn.name);
				_teleportEnemy.enemyManager.AddEnemy(component, slot);
			}
		}
	}
}
