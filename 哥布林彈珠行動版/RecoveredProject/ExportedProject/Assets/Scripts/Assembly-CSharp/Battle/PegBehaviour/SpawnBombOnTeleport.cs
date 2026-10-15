using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Map;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class SpawnBombOnTeleport : MonoBehaviour
	{
		[SerializeField]
		private Bomb _bombPrefab;

		public float lerpAnimTime = 0.65f;

		private PegSpawnPosition[] possibleSpawnPositions;

		private TeleportEnemy _teleportEnemy;

		private void Awake()
		{
			_teleportEnemy = GetComponent<TeleportEnemy>();
		}

		private void Start()
		{
			possibleSpawnPositions = UnityEngine.Object.FindObjectsOfType<PegSpawnPosition>();
		}

		private void OnEnable()
		{
			TeleportEnemy teleportEnemy = _teleportEnemy;
			teleportEnemy.TeleportingBack = (Action<int>)Delegate.Combine(teleportEnemy.TeleportingBack, new Action<int>(CheckForBombSpawn));
		}

		private void OnDisable()
		{
			TeleportEnemy teleportEnemy = _teleportEnemy;
			teleportEnemy.TeleportingBack = (Action<int>)Delegate.Remove(teleportEnemy.TeleportingBack, new Action<int>(CheckForBombSpawn));
		}

		private void CheckForBombSpawn(int slot)
		{
			int randomRangeValue = (StaticGameData.seededNodeData as SeededBattleNodeData).bossMechanicsRandomStatesPerAct[0].GetRandomRangeValue(0, possibleSpawnPositions.Length);
			for (int i = 0; i < possibleSpawnPositions.Length; i++)
			{
				int num = (randomRangeValue + i) % possibleSpawnPositions.Length;
				PegSpawnPosition pegSpawnPosition = possibleSpawnPositions[num];
				if (pegSpawnPosition.GetComponentsInChildren<Bomb>().Length == 0)
				{
					Bomb bomb = pegSpawnPosition.GetComponentInChildren<Bomb>(includeInactive: true);
					if (bomb == null)
					{
						bomb = UnityEngine.Object.Instantiate(_bombPrefab, pegSpawnPosition.transform);
						Peg.OnPegAdded?.Invoke(bomb.gameObject);
					}
					else
					{
						bomb.gameObject.SetActive(value: true);
						bomb.CompletelyResetBomb();
					}
					TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = bomb.transform.DOScale(1f, lerpAnimTime).From(0f);
					tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(RequestPegboardUpdate));
					break;
				}
			}
		}

		private void RequestPegboardUpdate()
		{
			PredictionManager.RequestSimulationPegboardUpdate?.Invoke();
		}
	}
}
