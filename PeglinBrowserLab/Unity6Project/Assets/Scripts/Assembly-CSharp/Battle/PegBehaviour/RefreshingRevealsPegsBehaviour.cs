using System;
using System.Collections.Generic;
using PeglinUtils;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class RefreshingRevealsPegsBehaviour : NonDummyPegboardBehaviour, ICustomCoinPegLayout
	{
		private struct PegObjectLocation
		{
			public Vector3 location;

			public GameObject prefab;

			public PegObjectLocation(Vector3 location, GameObject prefab)
			{
				this.location = location;
				this.prefab = prefab;
			}
		}

		public int pegsRevealedPerRefresh = 3;

		public Peg initialPeg;

		[HideInInspector]
		public string initialPegName;

		private int _totalCoinsToAdd;

		private int _addedCoinsCount;

		private int _totalPegs;

		private int _coinsToAddPerRefresh;

		private BattleController _battleController;

		private PegManager _pegManager;

		private PredictionManager _predictionManager;

		private List<PegObjectLocation> _pegObjectLocations = new List<PegObjectLocation>();

		private void Start()
		{
			if (isDummy)
			{
				return;
			}
			List<PegLocation> list = new List<PegLocation>(UnityEngine.Object.FindObjectsOfType<PegLocation>());
			_totalPegs = list.Count + 1;
			foreach (PegLocation item in list)
			{
				_pegObjectLocations.Add(new PegObjectLocation(item.transform.position, item.pegPrefab));
				UnityEngine.Object.Destroy(item.gameObject);
			}
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			_pegManager = _battleController.pegManager;
			int pegCount = _pegManager.GetPegCount(Peg.PegType.RESET);
			if (pegCount <= 0)
			{
				initialPeg.ConvertPegToType(Peg.PegType.RESET);
				initialPeg.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
			}
			CreatePegs(Mathf.Max(0, pegCount - 1), forceRefresh: true);
		}

		private void OnEnable()
		{
			if (!isDummy)
			{
				BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
				BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(OnTurnComplete));
				BattleController.OnPreRefreshActivated = (BattleController.PreRefreshActivated)Delegate.Combine(BattleController.OnPreRefreshActivated, new BattleController.PreRefreshActivated(OnPreRefresh));
				BattleController.OnStartBattleWithBombs = (BattleController.StartBattleWithBombs)Delegate.Combine(BattleController.OnStartBattleWithBombs, new BattleController.StartBattleWithBombs(OnStartBattleWithBombs));
			}
		}

		private void OnDisable()
		{
			if (!isDummy)
			{
				BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(OnCritActivated));
				BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(OnTurnComplete));
				BattleController.OnPreRefreshActivated = (BattleController.PreRefreshActivated)Delegate.Remove(BattleController.OnPreRefreshActivated, new BattleController.PreRefreshActivated(OnPreRefresh));
				BattleController.OnStartBattleWithBombs = (BattleController.StartBattleWithBombs)Delegate.Remove(BattleController.OnStartBattleWithBombs, new BattleController.StartBattleWithBombs(OnStartBattleWithBombs));
			}
		}

		private void OnStartBattleWithBombs(int numRegular, int numRigged)
		{
			CreatePegs(numRegular + numRigged, forceRefresh: false, Peg.PegType.BOMB);
		}

		private void OnTurnComplete()
		{
			CreatePegs(pegsRevealedPerRefresh);
		}

		private void OnPreRefresh()
		{
			CreatePegs(pegsRevealedPerRefresh);
		}

		private void OnCritActivated()
		{
			CreatePegs(pegsRevealedPerRefresh);
		}

		private void CreatePegs(int amount, bool forceRefresh = false, Peg.PegType toBeTurnedIntoType = Peg.PegType.NONE)
		{
			if (_pegManager == null)
			{
				return;
			}
			List<Peg> list = new List<Peg>();
			int num = Mathf.Min(amount, _pegObjectLocations.Count);
			_pegObjectLocations.Shuffle();
			if (_pegManager.GetPegCount(Peg.PegType.RESET) <= 0 && num > 0)
			{
				CreateExtraRefresh();
				num--;
			}
			List<PegObjectLocation> list2 = new List<PegObjectLocation>();
			int num2 = 0;
			while (num > 0 && num2 < _pegObjectLocations.Count)
			{
				Peg component = _pegObjectLocations[num2].prefab.GetComponent<Peg>();
				if (toBeTurnedIntoType != Peg.PegType.NONE && !component.SupportsPegType(toBeTurnedIntoType))
				{
					num2++;
					continue;
				}
				if (!forceRefresh || component.SupportsPegType(Peg.PegType.RESET))
				{
					GameObject obj = UnityEngine.Object.Instantiate(_pegObjectLocations[num2].prefab, base.transform);
					obj.transform.position = _pegObjectLocations[num2].location;
					Peg component2 = obj.GetComponent<Peg>();
					list.Add(component2);
					num--;
				}
				else
				{
					list2.Add(_pegObjectLocations[num2]);
				}
				num2++;
			}
			_pegObjectLocations.RemoveRange(0, num2);
			_pegObjectLocations.AddRange(list2);
			_battleController.AddPegs(list);
			foreach (Peg item in list)
			{
				_predictionManager.CopyPegToSimulation(item.gameObject, null);
			}
			int num3 = Mathf.Min(_coinsToAddPerRefresh, _totalCoinsToAdd - _addedCoinsCount);
			_pegManager.ApplyGoldToPegs(num3, doParticleEffect: false);
			_addedCoinsCount += num3;
		}

		private void CreateExtraRefresh()
		{
			_pegObjectLocations.Shuffle();
			int num = -1;
			for (int i = 0; i < _pegObjectLocations.Count; i++)
			{
				if (_pegObjectLocations[i].prefab.GetComponent<RegularPeg>() != null)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_pegObjectLocations[num].prefab, base.transform);
				gameObject.transform.position = _pegObjectLocations[num].location;
				Peg component = gameObject.GetComponent<Peg>();
				component.ConvertPegToType(Peg.PegType.RESET);
				component.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
				List<Peg> newPegs = new List<Peg> { component };
				_pegObjectLocations.Remove(_pegObjectLocations[num]);
				_battleController.AddPegs(newPegs);
				_predictionManager.CopyPegToSimulation(gameObject, null);
			}
		}

		public void SetCoinsToAdd(int newTotalCoinsToAdd)
		{
			_totalCoinsToAdd = newTotalCoinsToAdd;
			_coinsToAddPerRefresh = Mathf.CeilToInt((float)_totalCoinsToAdd / (float)_totalPegs * (float)pegsRevealedPerRefresh);
		}
	}
}
