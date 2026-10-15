using System;
using System.Collections;
using Battle.EnemyBehaviours;
using Battle.Pachinko.Obstacles;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies.Behaviours.SpiritOfRadia
{
	public class AddBlackHoles : AlternateEnemyAction
	{
		public string effectPegString = "";

		public string animOverride = "";

		public bool blackHoleOnSpawn = true;

		public bool shuffleHoleSpawnLocations = true;

		public Vector2[] blackHoleCoordinates;

		public GameObject blackHolePrefab;

		private Animator _anim;

		private int _blackHolesCounter;

		private GameObject[] _blackHoleInstances;

		private PredictionManager _predictionManager;

		private bool moveAndShoot;

		private SpiritOfRadiaBoss _spiritOfRadia;

		private EffectPeg _effectPeg;

		public GameObject[] blackHoleInstances => _blackHoleInstances;

		private void Start()
		{
			_anim = _enemy.GetComponent<Animator>();
			_blackHolesCounter = 0;
			_blackHoleInstances = new GameObject[blackHoleCoordinates.Length];
			ShuffleBlackHoleLocations(blackHoleCoordinates);
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			_spiritOfRadia = GetComponent<SpiritOfRadiaBoss>();
			WalkEnemy walkEnemy = _enemy as WalkEnemy;
			if (walkEnemy != null && walkEnemy.MoveAndShoot)
			{
				Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Combine(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(OnEnemyAttack));
				moveAndShoot = true;
			}
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			if (blackHoleOnSpawn)
			{
				CreateBlackhole();
			}
		}

		private void OnDestroy()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			if (moveAndShoot)
			{
				Enemy.OnEnemyAttack = (Enemy.EnemyAttacked)Delegate.Remove(Enemy.OnEnemyAttack, new Enemy.EnemyAttacked(OnEnemyAttack));
			}
		}

		private void OnEnemyAttack(float damage, bool melee, Enemy enemy, PlayerHealthController.DamageSource source, bool forceMaxHPDamage = false)
		{
			if (!(enemy != _enemy) && ConditionsMet())
			{
				CreateBlackhole();
			}
		}

		private void OnEnemyDestroyed(Enemy enemy)
		{
			if (!(enemy != _enemy))
			{
				DestroyBlackHoles();
			}
		}

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Combine(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			EffectPeg.OnEffectPegCreated = (EffectPeg.EffectPegCreated)Delegate.Remove(EffectPeg.OnEffectPegCreated, new EffectPeg.EffectPegCreated(OnEffectPegCreated));
		}

		private void OnEffectPegCreated(EffectPeg effectPeg)
		{
			if (_spiritOfRadia != null && _effectPeg == null && effectPeg.effectName == effectPegString)
			{
				_effectPeg = effectPeg;
				EffectPeg effectPeg2 = _effectPeg;
				effectPeg2.OnEffectPegToggled = (EffectPeg.EffectPegToggled)Delegate.Combine(effectPeg2.OnEffectPegToggled, new EffectPeg.EffectPegToggled(OnEffectPegToggled));
			}
		}

		private void OnEffectPegToggled(bool state)
		{
			if (!state)
			{
				DestroyBlackHoles();
			}
		}

		private void DestroyBlackHoles()
		{
			for (int i = 0; i < _blackHoleInstances.Length; i++)
			{
				if (_blackHoleInstances[i] != null)
				{
					_predictionManager.DestroyObjectInSimulation(_blackHoleInstances[i]);
					UnityEngine.Object.Destroy(_blackHoleInstances[i]);
				}
			}
			_blackHolesCounter = 0;
			_blackHoleInstances = new GameObject[blackHoleCoordinates.Length];
			ShuffleBlackHoleLocations(blackHoleCoordinates);
		}

		public override bool ConditionsMet()
		{
			if (_blackHolesCounter >= blackHoleCoordinates.Length || (_spiritOfRadia != null && _effectPeg != null && !_effectPeg.pegActive))
			{
				return false;
			}
			return true;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			if (_blackHolesCounter < blackHoleCoordinates.Length)
			{
				CreateBlackhole();
			}
			if (animOverride != "")
			{
				_anim.SetTrigger(animOverride);
				yield return WaitObjectAllocator.waitForEndOfFrame;
				float timer = 0f;
				AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
				while (timer <= stateInfo.length)
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
				}
			}
			_enemy.CurrentlyActing = false;
		}

		private void CreateBlackhole()
		{
			PlaySFX();
			PegboardBlackHole component = UnityEngine.Object.Instantiate(blackHolePrefab, blackHoleCoordinates[_blackHolesCounter], Quaternion.identity).GetComponent<PegboardBlackHole>();
			component.gravityRange = 4f;
			component.gravityStrength = 4f;
			_blackHoleInstances[_blackHolesCounter] = component.gameObject;
			_blackHolesCounter++;
			_predictionManager.AddObjectToSimulationScene(component.gameObject);
		}

		private void ShuffleBlackHoleLocations(Vector2[] locations)
		{
			if (shuffleHoleSpawnLocations)
			{
				for (int i = 0; i < locations.Length; i++)
				{
					Vector2 vector = locations[i];
					int num = UnityEngine.Random.Range(i, locations.Length);
					locations[i] = locations[num];
					locations[num] = vector;
				}
			}
		}
	}
}
