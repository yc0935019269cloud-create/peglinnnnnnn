using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.PegBehaviour;
using Map;
using MoreMountains.Tools;
using NaughtyAttributes;
using Peglin.OdinSerializer.Utilities;
using PeglinUtils;
using UnityEngine;

public class DragonPegLayoutSimulation : MonoBehaviour, IDummyPeg
{
	private class SandPegSpawner
	{
		public Vector2Int gridPosition;

		public int framesPerSpawn;

		public int framesUntilNextSpawn;

		public int totalSpawns;

		public int spawnsLeft;

		public SandPegSpawner(Vector2Int gridPosition, int framesPerSpawn, int totalSpawns)
		{
			this.gridPosition = gridPosition;
			this.framesPerSpawn = framesPerSpawn;
			framesUntilNextSpawn = framesPerSpawn;
			this.totalSpawns = totalSpawns;
			spawnsLeft = totalSpawns;
		}

		public bool TrySpawn()
		{
			if (spawnsLeft > 0 && --framesUntilNextSpawn <= 0)
			{
				framesUntilNextSpawn = framesPerSpawn;
				spawnsLeft--;
				return true;
			}
			return false;
		}
	}

	private class SandPeg
	{
		public Vector2Int gridPosition;

		public bool simulatedThisFrame;

		public SandPeg(Vector2Int gridPosition)
		{
			this.gridPosition = gridPosition;
			simulatedThisFrame = false;
		}
	}

	[HideInInspector]
	public string id = string.Empty;

	public GameObject pegPrefab;

	public int totalPegCountAvg = 250;

	public int totalPegCountVarience = 30;

	public int simulationFrames = 300;

	[Header("Board Params")]
	public Vector2 gridBottomLeft = new Vector2(-0.5f, -0.5f);

	public Vector2Int gridMaxSize = new Vector2Int(29, 20);

	public float distanceBetweenPegs = 0.5625f;

	[Header("Spawner Params")]
	[SerializeField]
	public List<Vector2Int> guarenteedSpawnerGridLocations = new List<Vector2Int>();

	[Tooltip("How much more pegs the middle spawners will spawn compared to the randomly generated spawners.")]
	public int extraSpawnsForPerGuarenteedSpawner = 30;

	public int spawnAmountVarience = 20;

	public int minExtraSpawners = 3;

	public int maxExtraSpawners = 5;

	public int minSpawerX = 4;

	public int maxSpawerX = 23;

	public int minXDistanceBetweenSpawners = 1;

	public int minXDistanceFromGuarenteedSpawners = 2;

	public int minSpawnerFramesPerSpawn = 1;

	public int maxSpawnerFramesPerSpawn = 1;

	private readonly List<Vector2Int> _obstacles = new List<Vector2Int>
	{
		new Vector2Int(3, 0),
		new Vector2Int(4, 0),
		new Vector2Int(10, 0),
		new Vector2Int(17, 0),
		new Vector2Int(23, 0),
		new Vector2Int(24, 0),
		new Vector2Int(3, 1),
		new Vector2Int(4, 1),
		new Vector2Int(10, 1),
		new Vector2Int(17, 1),
		new Vector2Int(23, 1),
		new Vector2Int(24, 1),
		new Vector2Int(3, 2),
		new Vector2Int(4, 2),
		new Vector2Int(9, 2),
		new Vector2Int(10, 2),
		new Vector2Int(11, 2),
		new Vector2Int(16, 2),
		new Vector2Int(17, 2),
		new Vector2Int(18, 2),
		new Vector2Int(23, 2),
		new Vector2Int(24, 2),
		new Vector2Int(3, 3),
		new Vector2Int(4, 3),
		new Vector2Int(9, 3),
		new Vector2Int(10, 3),
		new Vector2Int(11, 3),
		new Vector2Int(16, 3),
		new Vector2Int(17, 3),
		new Vector2Int(18, 3),
		new Vector2Int(23, 3),
		new Vector2Int(24, 3)
	};

	private List<SandPegSpawner> _currentSpawners;

	private List<List<SandPeg>> _grid;

	private BattleController _battleController;

	private PredictionManager _predictionManager;

	private bool _resimulating;

	private bool _isDummy;

	private List<SerializableRandomState> _bossMechanicsRandomStatesPerAct;

	private void Start()
	{
		if (!_isDummy)
		{
			if (StaticGameData.seededNodeData is SeededBattleNodeData seededBattleNodeData)
			{
				_bossMechanicsRandomStatesPerAct = seededBattleNodeData.bossMechanicsRandomStatesPerAct;
			}
			_battleController = GameObject.FindWithTag("GameController")?.GetComponent<BattleController>();
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			SimulateSandPegs();
		}
	}

	private IEnumerator ResimulateSandPegsOverFrames()
	{
		_resimulating = true;
		_grid = new List<List<SandPeg>>();
		for (int i = 0; i <= gridMaxSize.y; i++)
		{
			_grid.Add(new List<SandPeg>());
			for (int j = 0; j <= gridMaxSize.x; j++)
			{
				_grid[i].Add(null);
			}
		}
		CreateSandPegSpawners();
		int simulationStepsPerFrameWait = 1;
		int count = 0;
		for (int k = 0; k < simulationFrames; k++)
		{
			SimulateSandPegFrame();
			if (count++ >= simulationStepsPerFrameWait)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		_resimulating = false;
	}

	public IEnumerator WaitAndUpdateBoardAfterResimulation()
	{
		while (_resimulating)
		{
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(MovePegsToResimulationLayout());
	}

	private void SimulateSandPegs()
	{
		base.transform.MMDestroyAllChildren();
		_grid = new List<List<SandPeg>>();
		for (int i = 0; i <= gridMaxSize.y; i++)
		{
			_grid.Add(new List<SandPeg>());
			for (int j = 0; j <= gridMaxSize.x; j++)
			{
				_grid[i].Add(null);
			}
		}
		CreateSandPegSpawners();
		for (int k = 0; k < simulationFrames; k++)
		{
			SimulateSandPegFrame();
		}
		SpawnPegPrefabs();
	}

	private void SimulateSandPegFrame()
	{
		SpawnSandPegs();
		SimulateGrid();
	}

	private void SpawnSandPegs()
	{
		for (int i = 0; i < _currentSpawners.Count; i++)
		{
			if (!SandPegAtGridPosition(_currentSpawners[i].gridPosition) && _currentSpawners[i].TrySpawn())
			{
				_grid[_currentSpawners[i].gridPosition.y][_currentSpawners[i].gridPosition.x] = new SandPeg(_currentSpawners[i].gridPosition);
			}
		}
	}

	private void SimulateGrid()
	{
		for (int i = 0; i <= gridMaxSize.y; i++)
		{
			for (int j = 0; j <= gridMaxSize.x; j++)
			{
				SandPeg sandPeg = _grid[i][j];
				if (sandPeg != null)
				{
					sandPeg.simulatedThisFrame = false;
				}
			}
		}
		for (int k = 0; k <= gridMaxSize.y; k++)
		{
			for (int l = 0; l <= gridMaxSize.x; l++)
			{
				SandPeg sandPeg2 = _grid[k][l];
				if (sandPeg2 != null)
				{
					SimulateSandPeg(sandPeg2);
				}
			}
		}
	}

	private void SimulateSandPeg(SandPeg sandPeg)
	{
		sandPeg.simulatedThisFrame = true;
		Vector2Int gridPosition = sandPeg.gridPosition;
		if (!SandPegAtGridPosition(gridPosition + Vector2Int.down))
		{
			SwapGridCells(gridPosition, gridPosition + Vector2Int.down);
			return;
		}
		bool flag = !SandPegAtGridPosition(gridPosition + new Vector2Int(-1, -1));
		bool flag2 = !SandPegAtGridPosition(gridPosition + new Vector2Int(1, -1));
		if (flag & flag2)
		{
			float num = ((_bossMechanicsRandomStatesPerAct == null) ? UnityEngine.Random.Range(0f, 1f) : _bossMechanicsRandomStatesPerAct[2].GetRandomValue());
			if ((double)num < 0.5)
			{
				SwapGridCells(gridPosition, gridPosition + new Vector2Int(-1, -1));
			}
			else
			{
				SwapGridCells(gridPosition, gridPosition + new Vector2Int(1, -1));
			}
		}
		else if (flag)
		{
			SwapGridCells(gridPosition, gridPosition + new Vector2Int(-1, -1));
		}
		else if (flag2)
		{
			SwapGridCells(gridPosition, gridPosition + new Vector2Int(1, -1));
		}
	}

	private bool SandPegAtGridPosition(Vector2Int gridPos)
	{
		if (gridPos.x < 0 || gridPos.x > gridMaxSize.x || gridPos.y < 0 || gridPos.y > gridMaxSize.y)
		{
			return true;
		}
		if (_obstacles.Contains(gridPos))
		{
			return true;
		}
		return _grid[gridPos.y][gridPos.x] != null;
	}

	private void SwapGridCells(Vector2Int cell1, Vector2Int cell2)
	{
		SandPeg sandPeg = _grid[cell1.y][cell1.x];
		SandPeg sandPeg2 = _grid[cell2.y][cell2.x];
		_grid[cell2.y][cell2.x] = sandPeg;
		_grid[cell1.y][cell1.x] = sandPeg2;
		if (sandPeg != null)
		{
			sandPeg.gridPosition = new Vector2Int(cell2.x, cell2.y);
		}
		if (sandPeg2 != null)
		{
			sandPeg2.gridPosition = new Vector2Int(cell1.x, cell1.y);
		}
	}

	private void CreateSandPegSpawners()
	{
		bool flag = false;
		if (_bossMechanicsRandomStatesPerAct != null)
		{
			flag = true;
		}
		List<SandPegSpawner> list = new List<SandPegSpawner>();
		int num = (flag ? _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(minExtraSpawners, maxExtraSpawners + 1) : UnityEngine.Random.Range(minExtraSpawners, maxExtraSpawners + 1));
		int num2 = num + guarenteedSpawnerGridLocations.Count;
		int num3 = Mathf.RoundToInt((float)((flag ? _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(totalPegCountAvg - totalPegCountVarience, totalPegCountAvg + totalPegCountVarience + 1) : UnityEngine.Random.Range(totalPegCountAvg - totalPegCountVarience, totalPegCountAvg + totalPegCountVarience + 1)) - extraSpawnsForPerGuarenteedSpawner * guarenteedSpawnerGridLocations.Count) / (float)num2);
		List<int> list2 = new List<int>();
		int num4 = 0;
		for (int i = 0; i < num2 - 1; i++)
		{
			int num5 = (flag ? _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(-spawnAmountVarience, spawnAmountVarience + 1) : UnityEngine.Random.Range(-spawnAmountVarience, spawnAmountVarience + 1));
			list2.Add(num5);
			num4 += num5;
		}
		list2.Add(-num4);
		List<int> list3 = new List<int>();
		for (int j = minSpawerX; j < maxSpawerX; j++)
		{
			list3.Add(j);
		}
		int num6 = 0;
		foreach (Vector2Int guarenteedSpawnerGridLocation in guarenteedSpawnerGridLocations)
		{
			int framesPerSpawn = (flag ? _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(minSpawnerFramesPerSpawn, maxSpawnerFramesPerSpawn + 1) : UnityEngine.Random.Range(minSpawnerFramesPerSpawn, maxSpawnerFramesPerSpawn + 1));
			int totalSpawns = num3 + extraSpawnsForPerGuarenteedSpawner + list2[num6];
			num6++;
			SandPegSpawner item = new SandPegSpawner(guarenteedSpawnerGridLocation, framesPerSpawn, totalSpawns);
			list.Add(item);
			RemoveRangeFromList(list3, guarenteedSpawnerGridLocation.x, minXDistanceFromGuarenteedSpawners);
		}
		for (int k = 0; k < num; k++)
		{
			if (list3.Count <= 0)
			{
				Debug.LogWarning("Not enough space for all the SandPegSpawners. We will be short " + (num - k) + " spawners this time. (About " + (num - k) * num3 + " less pegs)");
				break;
			}
			int num7 = (flag ? list3[_bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(0, list3.Count)] : list3[UnityEngine.Random.Range(0, list3.Count)]);
			int framesPerSpawn2 = (flag ? _bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(minSpawnerFramesPerSpawn, maxSpawnerFramesPerSpawn + 1) : UnityEngine.Random.Range(minSpawnerFramesPerSpawn, maxSpawnerFramesPerSpawn + 1));
			int totalSpawns2 = num3 + list2[num6];
			num6++;
			SandPegSpawner item2 = new SandPegSpawner(new Vector2Int(num7, gridMaxSize.y), framesPerSpawn2, totalSpawns2);
			list.Add(item2);
			RemoveRangeFromList(list3, num7, minXDistanceBetweenSpawners);
		}
		_currentSpawners = list;
	}

	private void RemoveRangeFromList(List<int> list, int centerIndex, int range)
	{
		for (int i = centerIndex - range; i <= centerIndex + range; i++)
		{
			if (list.Contains(i))
			{
				list.Remove(i);
			}
		}
	}

	private void SpawnPegPrefabs()
	{
		List<Peg> list = new List<Peg>();
		for (int i = 0; i <= gridMaxSize.y; i++)
		{
			for (int j = 0; j <= gridMaxSize.x; j++)
			{
				if (_grid[i][j] != null)
				{
					GameObject obj = UnityEngine.Object.Instantiate(pegPrefab, base.transform);
					obj.name = $"Peg {j}:{i}";
					obj.transform.parent = base.transform;
					obj.transform.localPosition = (Vector3)gridBottomLeft + new Vector3(distanceBetweenPegs * (float)j, distanceBetweenPegs * (float)i, 0f);
					Peg component = obj.GetComponent<Peg>();
					if (i <= 3 && component != null && component is RegularPeg)
					{
						(component as RegularPeg).specialPegBlocker = true;
					}
					list.Add(component);
				}
			}
		}
		if (Application.IsPlaying(base.gameObject))
		{
			_battleController.ClearPegLists();
			if (_battleController != null)
			{
				_battleController.AddPegs(list);
			}
		}
		StartCoroutine(ResimulateSandPegsOverFrames());
	}

	private IEnumerator MovePegsToResimulationLayout()
	{
		List<Peg> list = new List<Peg>(_battleController.pegManager.allPegs.ToArray());
		if (_bossMechanicsRandomStatesPerAct != null)
		{
			list.Sort((Peg x, Peg y) => x.GetEntityId().CompareTo(y.GetEntityId()));
			_bossMechanicsRandomStatesPerAct[2].SeededShuffle(list);
		}
		else
		{
			list.Shuffle();
		}
		List<Peg> list2 = new List<Peg>();
		List<Peg> list3 = new List<Peg>();
		List<Peg> list4 = new List<Peg>();
		int num = 0;
		for (int num2 = 0; num2 <= gridMaxSize.y; num2++)
		{
			for (int num3 = 0; num3 <= gridMaxSize.x; num3++)
			{
				if (_grid[num2][num3] == null)
				{
					continue;
				}
				GameObject gameObject;
				Peg peg;
				if (num >= list.Count)
				{
					gameObject = UnityEngine.Object.Instantiate(pegPrefab, base.transform);
					gameObject.transform.parent = base.transform;
					gameObject.name = $"Peg {num3}:{num2}";
					peg = gameObject.GetComponent<Peg>();
					list2.Add(peg);
					list3.Add(peg);
				}
				else
				{
					gameObject = list[num].gameObject;
					peg = list[num];
					num++;
					list2.Add(peg);
				}
				gameObject.transform.localPosition = (Vector3)gridBottomLeft + new Vector3(distanceBetweenPegs * (float)num3, distanceBetweenPegs * (float)num2, 0f);
				if (!(peg != null) || !(peg is RegularPeg))
				{
					continue;
				}
				RegularPeg regularPeg = peg as RegularPeg;
				regularPeg.specialPegBlocker = false;
				if (num2 <= 3)
				{
					regularPeg.specialPegBlocker = true;
					if (regularPeg.pegType == Peg.PegType.RESET || regularPeg.pegType == Peg.PegType.CRIT)
					{
						list4.Add(peg);
					}
				}
			}
		}
		int num4 = 0;
		for (int num5 = num; num5 < list.Count; num5++)
		{
			Bomb componentInChildren = list[num5].GetComponentInChildren<Bomb>();
			if (list[num5].pegType == Peg.PegType.RESET || list[num5].pegType == Peg.PegType.CRIT || componentInChildren != null)
			{
				Peg peg2 = SwapPegWithOtherRandomPeg(list[num5], list2);
				if (peg2 != null)
				{
					list2.Remove(peg2);
					num4 += peg2.NumCoins();
					_predictionManager.DestroyPegInSimulation(peg2);
					UnityEngine.Object.Destroy(peg2.gameObject);
				}
				else
				{
					Debug.LogWarning($"Couldn't find a spot to put the {list[num5].pegType} in the new layout. Failsafing...");
					FailsafeAddSpecialPegToLayout(list[num5]);
				}
				list2.Add(list[num5]);
			}
			else
			{
				num4 += list[num5].NumCoins();
				_predictionManager.DestroyPegInSimulation(list[num5]);
				UnityEngine.Object.Destroy(list[num5].gameObject);
			}
		}
		_battleController.pegManager.ApplyGoldToPegs(num4, doParticleEffect: false);
		foreach (Peg item in list3)
		{
			SwapPegWithOtherRandomPeg(item, list2);
		}
		foreach (Peg item2 in list4)
		{
			SwapPegWithOtherRandomPeg(item2, list2);
		}
		if (Application.IsPlaying(base.gameObject))
		{
			_battleController.ClearPegLists();
			if (_battleController != null)
			{
				_battleController.AddPegs(list2);
				foreach (Peg item3 in list2)
				{
					_predictionManager.CopyPegToSimulation(item3.gameObject, this);
				}
			}
		}
		yield return new WaitForEndOfFrame();
		PredictionManager.RequestSimulationPegboardUpdate?.Invoke();
		yield return new WaitForEndOfFrame();
		StartCoroutine(ResimulateSandPegsOverFrames());
	}

	private Peg SwapPegWithOtherRandomPeg(Peg peg, List<Peg> pegList)
	{
		Vector3 localPosition = peg.transform.localPosition;
		for (int i = 0; i < 100; i++)
		{
			Peg peg2 = ((_bossMechanicsRandomStatesPerAct == null) ? pegList[UnityEngine.Random.Range(0, pegList.Count)] : pegList[_bossMechanicsRandomStatesPerAct[2].GetRandomRangeValue(0, pegList.Count)]);
			if (!(peg2 == peg) && peg2.pegType == Peg.PegType.REGULAR && !(peg2.GetComponentInChildren<Bomb>() != null))
			{
				RegularPeg regularPeg = peg2 as RegularPeg;
				if (regularPeg != null && !regularPeg.specialPegBlocker)
				{
					peg.transform.localPosition = peg2.transform.localPosition;
					peg2.transform.localPosition = localPosition;
					return peg2;
				}
			}
		}
		return null;
	}

	private void FailsafeAddSpecialPegToLayout(Peg specialPegToAdd)
	{
		for (int i = 4; i <= gridMaxSize.y; i++)
		{
			for (int j = 0; j <= gridMaxSize.x; j++)
			{
				if (_grid[i][j] == null)
				{
					Debug.Log("failsafe moving peg " + specialPegToAdd.name);
					specialPegToAdd.transform.localPosition = (Vector3)gridBottomLeft + new Vector3(distanceBetweenPegs * (float)j, distanceBetweenPegs * (float)i, 0f);
					_grid[i][j] = new SandPeg(new Vector2Int(j, i));
					return;
				}
			}
		}
		Debug.LogError("Can't find an open spot to add special peg. Should never get here.");
	}

	private void OnDrawGizmos()
	{
		if (!base.enabled)
		{
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft, 0.25f);
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(distanceBetweenPegs * (float)gridMaxSize.x, 0f), 0.25f);
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(0f, distanceBetweenPegs * (float)gridMaxSize.y), 0.25f);
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(distanceBetweenPegs * (float)gridMaxSize.x, distanceBetweenPegs * (float)gridMaxSize.y), 0.25f);
		Gizmos.color = Color.green;
		foreach (Vector2Int guarenteedSpawnerGridLocation in guarenteedSpawnerGridLocations)
		{
			Vector2 vector = guarenteedSpawnerGridLocation;
			Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(distanceBetweenPegs * vector.x, distanceBetweenPegs * vector.y), 0.25f);
		}
		Gizmos.color = new Color(0.2f, 1f, 0.2f);
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(distanceBetweenPegs * (float)minSpawerX, distanceBetweenPegs * (float)gridMaxSize.y), 0.25f);
		Gizmos.DrawSphere((Vector2)base.gameObject.transform.position + gridBottomLeft + new Vector2(distanceBetweenPegs * (float)maxSpawerX, distanceBetweenPegs * (float)gridMaxSize.y), 0.25f);
	}

	[Button(null, EButtonEnableMode.Always)]
	private void SpawnPegsForEditor()
	{
		SimulateSandPegs();
	}

	[Button(null, EButtonEnableMode.Always)]
	private void ClearEditorPegs()
	{
		base.transform.MMDestroyAllChildren();
	}

	public void GenerateId(bool force = false)
	{
		if (id.IsNullOrWhitespace() | force)
		{
			id = Guid.NewGuid().ToString();
		}
	}

	public string GetId()
	{
		return id;
	}

	public void SetDummyStatus(bool status)
	{
		_isDummy = status;
	}

	public void SetPegStatus(bool status)
	{
	}

	public bool GetPegStatus()
	{
		return true;
	}

	public bool IsSpecialPeg()
	{
		return false;
	}

	public void SetSpecialCollider(bool value)
	{
	}

	public Peg.PegType GetPegType()
	{
		return Peg.PegType.NONE;
	}

	public void SetPegType(Peg.PegType pegType)
	{
	}
}
