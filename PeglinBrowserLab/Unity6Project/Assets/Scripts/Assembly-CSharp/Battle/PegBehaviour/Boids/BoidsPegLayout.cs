using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Relics;
using UnityEngine;

namespace Battle.PegBehaviour.Boids
{
	public class BoidsPegLayout : NonDummyPegboardBehaviour
	{
		public RelicManager relicManager;

		[Header("Peg Spawning Details")]
		public RegularPeg pegPrefab;

		public RegularPeg pegPrefabForDroppingPegs;

		public bool shouldDropAdditionalPegs;

		public int pegCount;

		public int bombCount = 4;

		public int numStepsPerShot = 8;

		public int numCollisionRetries = 3;

		public ContactFilter2D boidSpawnContactFilter;

		private BattleController _controller;

		private List<Peg> _movingPegs;

		[Header("Boid Sim Configuration")]
		public int initialSteps = 8;

		public float spawnCollisionRadius = 0.6f;

		private SpriteRenderer _spriteRenderer;

		private void OnEnable()
		{
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
		}

		private void OnDisable()
		{
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(HandleEnemyTurnEnd));
		}

		private void FixedUpdate()
		{
			if (numStepsPerShot < 0)
			{
				UpdateBoids();
			}
		}

		private void Start()
		{
			if (!isDummy)
			{
				CreatePegs();
			}
		}

		[Button("Debug Create Pegs", EButtonEnableMode.Always)]
		private void CreatePegs()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_spriteRenderer.enabled = false;
			_movingPegs = new List<Peg>();
			List<Bomb> bombs = new List<Bomb>();
			for (int i = 0; i < pegCount; i++)
			{
				for (int j = 0; j < numCollisionRetries; j++)
				{
					Bounds bounds = _spriteRenderer.bounds;
					Vector2 vector = base.transform.position - bounds.size * 0.5f + new Vector3(UnityEngine.Random.Range(0.25f, bounds.size.x - 0.25f), UnityEngine.Random.Range(0.25f, bounds.size.y - 0.25f));
					if (Physics2D.OverlapCircle(vector, spawnCollisionRadius, boidSpawnContactFilter, new Collider2D[1]) <= 0)
					{
						RegularPeg regularPeg = UnityEngine.Object.Instantiate(shouldDropAdditionalPegs ? pegPrefabForDroppingPegs : pegPrefab, vector, Quaternion.identity, base.transform.parent);
						BoidPeg component = regularPeg.GetComponent<BoidPeg>();
						if (component != null)
						{
							component.bottomLeft = base.transform.position - bounds.size * 0.5f;
							component.topRight = base.transform.position + bounds.size * 0.5f;
						}
						_movingPegs.Add(regularPeg);
						break;
					}
				}
			}
			if (Application.IsPlaying(base.gameObject))
			{
				_controller = UnityEngine.Object.FindObjectOfType<BattleController>();
				if (_controller != null)
				{
					_controller.AddPegs(_movingPegs);
					_controller.pegManager.AddBombs(bombs);
				}
			}
			BattleController battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			if (battleController != null)
			{
				battleController.pegManager.ConvertPegsToBombs(relicManager.RelicEffectActive(RelicEffect.DOUBLE_BOMBS_ON_MAP) ? (bombCount * 2) : bombCount);
			}
			for (int k = 0; k < initialSteps; k++)
			{
				UpdateBoids();
			}
		}

		[Button("Debug Simulate Move Step", EButtonEnableMode.Always)]
		private void HandleEnemyTurnEnd()
		{
			for (int i = 0; i < numStepsPerShot; i++)
			{
				UpdateBoids();
			}
		}

		private void UpdateBoids()
		{
			List<Peg> movingPegs = _movingPegs;
			if (movingPegs == null || movingPegs.Count <= 0)
			{
				return;
			}
			foreach (Peg movingPeg in _movingPegs)
			{
				BoidPeg component = movingPeg.GetComponent<BoidPeg>();
				if (component != null)
				{
					component.RecalculateVelocity();
					component.PerformMove();
				}
			}
		}

		[Button("Delete Debug Pegs", EButtonEnableMode.Always)]
		private void DeleteMovingPegs()
		{
			foreach (Peg movingPeg in _movingPegs)
			{
				UnityEngine.Object.DestroyImmediate(movingPeg.gameObject);
			}
		}
	}
}
