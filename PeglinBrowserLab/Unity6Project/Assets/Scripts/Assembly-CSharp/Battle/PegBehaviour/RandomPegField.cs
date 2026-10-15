using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class RandomPegField : NonDummyPegboardBehaviour
	{
		[Header("Pegs To Spawn Details")]
		public Peg[] pegPrefabs;

		[Tooltip("Series of floats corresponding to the odds of spawning a particular peg.Size should equal pegPrefabs and values should ascend up to 1.0")]
		public float[] pegTypeSpawnRatio;

		[Header("Configuration")]
		[Tooltip("How far apart do we want our objects to be?")]
		public float spawnObjectRadius = 0.5f;

		[Tooltip("How many tries before we give up on a spawn point?")]
		public int samplesBeforeRejection = 25;

		[Header("Shuffle Options")]
		public bool moveEveryTurn;

		[Tooltip("If we're going to move our pegs then we want to ensure we don't lose places.We'll sample higher and higher until all of our pegs can be placed.")]
		public int moveObjectsRetrySampleBoost = 5;

		[Tooltip("Beware of making this too long, or else the copy will happen mid-transform.")]
		public float moveTransformTime = 0.2f;

		private SpriteRenderer _spriteRenderer;

		private List<Peg> _pegs;

		private Vector3 _bottomLeft;

		private List<Vector2> generatedPoints;

		private void OnEnable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(TurnComplete));
		}

		private void OnDisable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(TurnComplete));
		}

		private void Start()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_spriteRenderer.enabled = false;
			if (!isDummy)
			{
				CreatePegs();
			}
		}

		private void CreatePegs()
		{
			if (_pegs != null && _pegs.Count > 0)
			{
				return;
			}
			_pegs = new List<Peg>();
			List<Peg> list = new List<Peg>();
			_bottomLeft = base.transform.position - _spriteRenderer.bounds.size * 0.5f;
			generatedPoints = FindSpawnPoints();
			foreach (Vector2 generatedPoint in generatedPoints)
			{
				float value = UnityEngine.Random.value;
				for (int i = 0; i < pegPrefabs.Length; i++)
				{
					if (value <= pegTypeSpawnRatio[i])
					{
						Peg peg = UnityEngine.Object.Instantiate(pegPrefabs[i], base.transform.parent);
						peg.transform.position = _bottomLeft + (Vector3)generatedPoint;
						_pegs.Add(peg);
						if (peg.GetComponent<Bomb>() == null)
						{
							list.Add(peg);
						}
						break;
					}
				}
			}
			if (Application.IsPlaying(base.gameObject))
			{
				BattleController battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
				if (battleController != null)
				{
					battleController.AddPegs(list);
				}
			}
		}

		private void TurnComplete()
		{
			if (!moveEveryTurn || isDummy)
			{
				return;
			}
			List<Vector2> list = FindSpawnPoints();
			while (list.Count < _pegs.Count)
			{
				samplesBeforeRejection += moveObjectsRetrySampleBoost;
				Debug.Log("Retry Shuffle on RandomPegField. Current Samples: " + samplesBeforeRejection);
				list = FindSpawnPoints();
			}
			for (int i = 0; i < _pegs.Count; i++)
			{
				Peg peg = _pegs[i];
				Vector3 spawnPoint = list[i];
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _pegs[i].transform.DOScale(Vector3.zero, moveTransformTime);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
				{
					MovePegAndScale(peg, spawnPoint);
				}));
			}
			StartCoroutine(RequestPredictionUpdate());
		}

		private IEnumerator RequestPredictionUpdate()
		{
			yield return new WaitForSeconds(2f * moveTransformTime + 0.02f);
			PredictionManager.RequestSimulationPegboardUpdate?.Invoke();
		}

		private void MovePegAndScale(Peg peg, Vector3 pos)
		{
			peg.transform.position = _bottomLeft + pos;
			peg.transform.DOScale(Vector3.one, moveTransformTime);
		}

		private List<Vector2> FindSpawnPoints()
		{
			if (_spriteRenderer == null)
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
				if (_spriteRenderer == null)
				{
					return new List<Vector2>();
				}
			}
			Vector2 vector = _spriteRenderer.bounds.size;
			float num = spawnObjectRadius / Mathf.Sqrt(2f);
			int[,] array = new int[Mathf.CeilToInt(vector.x / num), Mathf.CeilToInt(vector.y / num)];
			List<Vector2> list = new List<Vector2>();
			List<Vector2> list2 = new List<Vector2>();
			list2.Add(vector / 2f);
			while (list2.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list2.Count);
				Vector2 vector2 = list2[index];
				bool flag = false;
				for (int i = 0; i < samplesBeforeRejection; i++)
				{
					float f = UnityEngine.Random.value * MathF.PI * 2f;
					Vector2 vector3 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
					Vector2 vector4 = vector2 + vector3 * UnityEngine.Random.Range(spawnObjectRadius, 2f * spawnObjectRadius);
					if (IsValid(vector4, vector, num, list, array))
					{
						list.Add(vector4);
						list2.Add(vector4);
						array[(int)(vector4.x / num), (int)(vector4.y / num)] = list.Count;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.RemoveAt(index);
				}
			}
			return list;
		}

		private bool IsValid(Vector2 candidatePoint, Vector2 sampleRegionSize, float cellSize, List<Vector2> points, int[,] grid)
		{
			if (candidatePoint.x >= 0f && candidatePoint.x < sampleRegionSize.x && candidatePoint.y >= 0f && candidatePoint.y < sampleRegionSize.y)
			{
				int num = (int)(candidatePoint.x / cellSize);
				int num2 = (int)(candidatePoint.y / cellSize);
				int num3 = Mathf.Max(0, num - 2);
				int num4 = Mathf.Min(num + 2, grid.GetLength(0));
				int num5 = Mathf.Max(0, num2 - 2);
				int num6 = Mathf.Min(num2 + 2, grid.GetLength(1));
				for (int i = num3; i < num4; i++)
				{
					for (int j = num5; j < num6; j++)
					{
						int num7 = grid[i, j] - 1;
						if (num7 != -1 && (candidatePoint - points[num7]).sqrMagnitude < spawnObjectRadius * spawnObjectRadius)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		public bool AreAllPegsClear()
		{
			foreach (Peg peg in _pegs)
			{
				if (!peg.IsDisabled())
				{
					return false;
				}
			}
			return true;
		}

		public void ResetAllPegs(bool resetBombs = false)
		{
			foreach (Peg peg in _pegs)
			{
				if (peg.IsDisabled())
				{
					peg.Reset();
				}
			}
		}

		public bool ContainsPeg(Peg peg)
		{
			return _pegs.Contains(peg);
		}

		public void ConvertRandomPegToReset()
		{
			int num = UnityEngine.Random.Range(0, _pegs.Count);
			for (int num2 = _pegs.Count; num2 > 0; num2--)
			{
				Peg peg = _pegs[num];
				if (!peg.IsDisabled() && peg.SupportsPegType(Peg.PegType.RESET) && peg.pegType != Peg.PegType.RESET)
				{
					peg.ConvertPegToType(Peg.PegType.RESET);
					return;
				}
				num++;
				if (num >= _pegs.Count)
				{
					num = 0;
				}
			}
			Debug.LogWarning("RandomPegField->ConvertRandomPegToReset couldn't find a valid peg to convert!");
		}

		public Vector2 GetRandomPegPosition()
		{
			int num = UnityEngine.Random.Range(0, _pegs.Count);
			for (int num2 = _pegs.Count; num2 > 0; num2--)
			{
				Peg peg = _pegs[num];
				if (peg != null && !peg.IsDisabled())
				{
					return peg.transform.position;
				}
				num++;
				if (num >= _pegs.Count)
				{
					num = 0;
				}
			}
			return Vector2.zero;
		}
	}
}
