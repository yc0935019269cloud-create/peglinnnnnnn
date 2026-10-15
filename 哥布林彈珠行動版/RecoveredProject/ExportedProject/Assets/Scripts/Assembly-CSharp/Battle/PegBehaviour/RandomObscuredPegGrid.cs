using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class RandomObscuredPegGrid : NonDummyPegboardBehaviour
	{
		public enum CellPegType
		{
			None = 0,
			Peg = 1,
			Bomb = 2,
			Rigged = 3,
			Crit = 4,
			Refresh = 5
		}

		public AudioClip resetingSound;

		[Header("Peg Spawning Details")]
		public Peg[] pegPrefabs;

		public int[] pegTypeCounts;

		public PegGridObscurer pegGridObscurerPrefab;

		private PegGridObscurer[,] _pegGrid;

		private BattleController _controller;

		private AudioSource _audioSource;

		[Header("Configuration")]
		public int numCollisionRetries = 3;

		private float _cellSize = 1f;

		public Color adjacentRefreshColor = Color.green;

		public Color adjacentRiggedColor = Color.red;

		public Color adjacentCritColor = Color.yellow;

		public Color adjacentBombColor = Color.black;

		public Color adjacentPegColor = Color.white;

		public Color nothingAdjacentColor = Color.white;

		private SpriteRenderer _spriteRenderer;

		private Vector2 _bottomLeft;

		private int _gridWidth;

		private int _gridHeight;

		private List<SerializableRandomState> _bossMechanicsRandomStatesPerAct;

		private void Start()
		{
			_audioSource = GetComponent<AudioSource>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_spriteRenderer.enabled = false;
			_cellSize = pegGridObscurerPrefab.GetComponentInChildren<BoxCollider2D>().size.x;
			if (StaticGameData.seededNodeData is SeededBattleNodeData seededBattleNodeData)
			{
				_bossMechanicsRandomStatesPerAct = seededBattleNodeData.bossMechanicsRandomStatesPerAct;
			}
			if (!isDummy)
			{
				CreatePegs();
			}
		}

		private void OnEnable()
		{
			PegManager.OnSpecialPegsShuffled = (Action)Delegate.Combine(PegManager.OnSpecialPegsShuffled, new Action(HandleSpecialPegsShuffled));
		}

		private void OnDisable()
		{
			PegManager.OnSpecialPegsShuffled = (Action)Delegate.Remove(PegManager.OnSpecialPegsShuffled, new Action(HandleSpecialPegsShuffled));
		}

		public IEnumerator ResetObscurerGrid()
		{
			for (int y = 0; y < _gridHeight; y++)
			{
				_audioSource.PlayOneShot(resetingSound);
				for (int i = 0; i < _gridWidth; i++)
				{
					_pegGrid[i, y].ResetCell();
				}
				yield return new WaitForSeconds(0.5f);
			}
			PredictionManager.RequestSimulationPegboardUpdate?.Invoke();
		}

		private void CreatePegs()
		{
			_gridWidth = Mathf.RoundToInt(_spriteRenderer.bounds.size.x / _cellSize);
			_gridHeight = Mathf.RoundToInt(_spriteRenderer.bounds.size.y / _cellSize);
			_pegGrid = new PegGridObscurer[_gridWidth, _gridHeight];
			_bottomLeft = base.transform.position - _spriteRenderer.bounds.size * 0.5f;
			for (int i = 0; i < _gridWidth; i++)
			{
				for (int j = 0; j < _gridHeight; j++)
				{
					PegGridObscurer pegGridObscurer = UnityEngine.Object.Instantiate(pegGridObscurerPrefab, _bottomLeft + new Vector2(_cellSize / 2f + (float)i * _cellSize, _cellSize / 2f + (float)j * _cellSize), Quaternion.identity);
					pegGridObscurer.transform.parent = base.transform;
					pegGridObscurer.Init(this, i, j);
					_pegGrid[i, j] = pegGridObscurer;
				}
			}
			List<Peg> list = new List<Peg>();
			List<Bomb> list2 = new List<Bomb>();
			for (int k = 0; k < pegPrefabs.Length; k++)
			{
				for (int l = 0; l < pegTypeCounts[k]; l++)
				{
					for (int m = 0; m < numCollisionRetries; m++)
					{
						int num;
						int num2;
						if (_bossMechanicsRandomStatesPerAct != null)
						{
							num = _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(0, _gridWidth);
							num2 = _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(0, _gridHeight);
						}
						else
						{
							num = UnityEngine.Random.Range(0, _gridWidth);
							num2 = UnityEngine.Random.Range(0, _gridHeight);
						}
						if (_pegGrid[num, num2].assignedPeg == null)
						{
							Vector2 vector = _bottomLeft + new Vector2(((float)num + 0.5f) * _cellSize, ((float)num2 + 0.5f) * _cellSize);
							Peg peg = UnityEngine.Object.Instantiate(pegPrefabs[k], vector, Quaternion.identity);
							peg.transform.parent = _pegGrid[num, num2].transform;
							if (peg is Bomb)
							{
								list2.Add(peg as Bomb);
							}
							else
							{
								list.Add(peg);
							}
							_pegGrid[num, num2].assignedPeg = peg;
							break;
						}
					}
				}
			}
			if (Application.IsPlaying(base.gameObject))
			{
				_controller = UnityEngine.Object.FindObjectOfType<BattleController>();
				if (_controller != null)
				{
					_controller.AddPegs(list);
					_controller.pegManager.AddBombs(list2);
				}
			}
		}

		public void HandlePoppedGridCell(PegGridObscurer cell, int x, int y, bool updateNeighbours = true)
		{
			CellPegType cellPegType = CellPegType.None;
			for (int i = -1; i < 2; i++)
			{
				int num = x + i;
				if (num < 0 || num >= _gridWidth)
				{
					continue;
				}
				for (int j = -1; j < 2; j++)
				{
					int num2 = y + j;
					if (num2 < 0 || num2 >= _gridHeight || (i == 0 && j == 0))
					{
						continue;
					}
					PegGridObscurer pegGridObscurer = _pegGrid[num, num2];
					if (pegGridObscurer.isRevealed)
					{
						if (updateNeighbours)
						{
							HandlePoppedGridCell(pegGridObscurer, num, num2, updateNeighbours: false);
						}
						continue;
					}
					Peg assignedPeg = _pegGrid[num, num2].assignedPeg;
					if (assignedPeg != null)
					{
						switch (assignedPeg.pegType)
						{
						case Peg.PegType.RESET:
							cellPegType = (CellPegType)Mathf.Max(5, (int)cellPegType);
							break;
						case Peg.PegType.CRIT:
							cellPegType = (CellPegType)Mathf.Max(4, (int)cellPegType);
							break;
						case Peg.PegType.REGULAR:
							cellPegType = (CellPegType)Mathf.Max(1, (int)cellPegType);
							break;
						case Peg.PegType.BOMB:
							cellPegType = (CellPegType)Mathf.Max(((Bomb)assignedPeg).isRigged ? 3 : 2, (int)cellPegType);
							break;
						}
					}
				}
			}
			if (cellPegType == CellPegType.None)
			{
				for (int k = -1; k < 2; k++)
				{
					int num3 = x + k;
					if (num3 < 0 || num3 >= _gridWidth)
					{
						continue;
					}
					for (int l = -1; l < 2; l++)
					{
						int num4 = y + l;
						if (num4 >= 0 && num4 < _gridHeight && (num3 != x || num4 != y))
						{
							PegGridObscurer pegGridObscurer2 = _pegGrid[num3, num4];
							if (!pegGridObscurer2.isRevealed && pegGridObscurer2.assignedPeg == null)
							{
								pegGridObscurer2.RevealCell();
							}
						}
					}
				}
			}
			RecolourGridCell(cell, cellPegType);
		}

		private void RecolourGridCell(PegGridObscurer cell, CellPegType highestPriNeighbour)
		{
			cell.SetRevealedCellColour(highestPriNeighbour, GetColorForGridCell(highestPriNeighbour));
		}

		private Color GetColorForGridCell(CellPegType cellType)
		{
			return cellType switch
			{
				CellPegType.Refresh => adjacentRefreshColor, 
				CellPegType.Rigged => adjacentRiggedColor, 
				CellPegType.Crit => adjacentCritColor, 
				CellPegType.Bomb => adjacentBombColor, 
				CellPegType.Peg => adjacentPegColor, 
				CellPegType.None => nothingAdjacentColor, 
				_ => Color.white, 
			};
		}

		private void HandleSpecialPegsShuffled()
		{
			RecheckAllCellsOfPegType(CellPegType.Refresh);
			RecheckAllCellsOfPegType(CellPegType.Crit);
		}

		public void RecheckAllCellsOfPegType(CellPegType cellPegType)
		{
			for (int i = 0; i < _gridWidth; i++)
			{
				for (int j = 0; j < _gridHeight; j++)
				{
					if (_pegGrid[i, j].isRevealed && _pegGrid[i, j].revealedPegType == cellPegType)
					{
						HandlePoppedGridCell(_pegGrid[i, j], i, j, updateNeighbours: false);
					}
				}
			}
		}
	}
}
