using System.Collections.Generic;
using Battle;
using Battle.Enemies;
using Battle.StatusEffects;
using Data;
using Loading;
using Relics;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Spawner : MonoBehaviour
{
	protected enum SpawnerState
	{
		IDLE = 0,
		SPAWNING_START = 1,
		ENEMY_SPAWNING = 2,
		SPAWNING_END = 3
	}

	[Header("Scene Elements")]
	public Transform SpawnPointPosition;

	public int forAct = -1;

	protected RelicManager _relicManager;

	protected Animator _animator;

	protected List<Enemy> _enemies = new List<Enemy>();

	protected StatusEffectData _statusEffectData;

	protected List<WaveData> _upcomingSpawns = new List<WaveData>();

	protected SpawnerState _spawnerState;

	protected SpawnData _currentSpawnData;

	protected Enemy _currentEnemy;

	protected EnemyManager _enemyManager;

	protected EnemyInfoManager _enemyInfoManager;

	protected int _currentTurn;

	protected bool _currentAnimationEnded;

	public void Initialize(WaveData[] spawns, EnemyManager enemyManager, StatusEffectData statusEffectData, RelicManager relicManager, EnemyInfoManager enemyInfoManager)
	{
		_animator = GetComponent<Animator>();
		_upcomingSpawns.AddRange(spawns);
		_enemyManager = enemyManager;
		_statusEffectData = statusEffectData;
		_relicManager = relicManager;
		_enemyInfoManager = enemyInfoManager;
	}

	private void Update()
	{
		HandleCurrentSpawnState();
	}

	public bool IsSpawning()
	{
		return _spawnerState != SpawnerState.IDLE;
	}

	public void Spawn(bool force = false)
	{
		_currentSpawnData = null;
		int num = int.MaxValue;
		WaveData waveData = null;
		foreach (WaveData upcomingSpawn in _upcomingSpawns)
		{
			if (_currentTurn >= upcomingSpawn.turnNumber)
			{
				_currentSpawnData = upcomingSpawn.spawnData;
				if (CanSpawn(_currentSpawnData))
				{
					_spawnerState = SpawnerState.SPAWNING_START;
					_upcomingSpawns.Remove(upcomingSpawn);
					PlayAnimation(_currentSpawnData.SpawnPointInitialAnimationTrigger);
					_currentTurn++;
				}
				break;
			}
			if (upcomingSpawn.turnNumber < num)
			{
				num = upcomingSpawn.turnNumber;
				waveData = upcomingSpawn;
			}
		}
		if (_currentSpawnData == null && !force)
		{
			_spawnerState = SpawnerState.IDLE;
			_currentTurn++;
		}
		else if (((_currentSpawnData == null && waveData != null) & force) && CanSpawn(waveData.spawnData))
		{
			_currentTurn = num;
			_currentSpawnData = waveData.spawnData;
			PlayAnimation(_currentSpawnData.SpawnPointInitialAnimationTrigger);
			_upcomingSpawns.Remove(waveData);
			_spawnerState = SpawnerState.SPAWNING_START;
		}
	}

	protected virtual bool CanSpawn(SpawnData spawnData)
	{
		if (IsSpawning())
		{
			return false;
		}
		Enemy component = AssetLoading.Instance.EnemyPrefabs[spawnData.enemyAssetReference.RuntimeKey.ToString()].GetComponent<Enemy>();
		if (component != null)
		{
			if (component.IsFlying && !_enemyManager.IsFlyingSpawnPointClear())
			{
				return false;
			}
			if (!_enemyManager.IsGroundSpawnPointClear())
			{
				return false;
			}
		}
		return true;
	}

	private void HandleCurrentSpawnState()
	{
		if (_currentSpawnData == null)
		{
			_spawnerState = SpawnerState.IDLE;
			return;
		}
		switch (_spawnerState)
		{
		case SpawnerState.SPAWNING_START:
			if (HasAnimationEnded())
			{
				_currentAnimationEnded = false;
				SpawnEnemyObject();
				if (_currentEnemy == null)
				{
					Debug.LogError("Spawner::HandleSpawnState(): Spawned enemy prefab without Enemy script attached");
					break;
				}
				_enemies.Add(_currentEnemy);
				_spawnerState = SpawnerState.ENEMY_SPAWNING;
				_currentEnemy.RegisterOnSpawnAnimationEnd(OnSpawnAnimationEnd);
				_currentEnemy.PlaySpawningAnimation();
			}
			break;
		case SpawnerState.SPAWNING_END:
			if (HasAnimationEnded())
			{
				_currentAnimationEnded = false;
				_spawnerState = SpawnerState.IDLE;
			}
			break;
		case SpawnerState.ENEMY_SPAWNING:
			break;
		}
	}

	public void AnimationEnded()
	{
		_currentAnimationEnded = true;
	}

	public bool HasSpawnsLeft()
	{
		return _upcomingSpawns.Count != 0;
	}

	protected virtual void SpawnEnemyObject()
	{
		AssetReference assetReference;
		if (_currentSpawnData.randomizedAssets != null && _currentSpawnData.randomizedAssets.Length != 0)
		{
			int num = Random.Range(0, _currentSpawnData.randomizedAssets.Length);
			assetReference = _currentSpawnData.randomizedAssets[num];
		}
		else
		{
			assetReference = _currentSpawnData.enemyAssetReference;
		}
		string key = assetReference.RuntimeKey.ToString();
		if (!AssetLoading.Instance.EnemyPrefabs.ContainsKey(key))
		{
			Debug.LogWarning($"Did not find cached prefab for enemy {assetReference}");
			GameObject value = assetReference.LoadAssetAsync<GameObject>().WaitForCompletion();
			AssetLoading.Instance.EnemyPrefabs.Add(key, value);
		}
		_currentEnemy = Object.Instantiate(AssetLoading.Instance.EnemyPrefabs[key], _enemyManager.GetGroundSpawnPoint(), Quaternion.identity, base.transform).GetComponent<Enemy>();
		_currentEnemy.Initialize(_statusEffectData, _enemyManager, _relicManager, AssetLoading.Instance.EnemyPrefabs[key].name);
		if (_currentEnemy.IsFlying)
		{
			_currentEnemy.transform.position = _enemyManager.GetFlyingSpawnPoint();
		}
		_enemyManager.AddEnemy(_currentEnemy);
		_enemyInfoManager.PopSpawn();
	}

	private bool HasAnimationEnded()
	{
		if (_animator != null)
		{
			return _currentAnimationEnded;
		}
		return true;
	}

	private void OnSpawnAnimationEnd()
	{
		if (_spawnerState == SpawnerState.ENEMY_SPAWNING)
		{
			_spawnerState = SpawnerState.SPAWNING_END;
		}
		PlayAnimation(_currentSpawnData.SpawnPointFinalAnimationTrigger);
		_currentEnemy.UnregisterOnSpawnAnimationEnd(OnSpawnAnimationEnd);
	}

	private void PlayAnimation(string trigger)
	{
		if (!(_animator == null))
		{
			if (AnimatorUtils.HasParameter(_animator, trigger))
			{
				_currentAnimationEnded = false;
				_animator.SetTrigger(trigger);
			}
			else
			{
				_currentAnimationEnded = true;
			}
		}
	}
}
