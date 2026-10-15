using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Enemies;
using NaughtyAttributes;
using PeglinUtils;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegLayoutAlternator : NonDummyPegboardBehaviour
	{
		private bool _pegboardAActive = true;

		[SerializeField]
		public GameObject _pegboardA;

		[SerializeField]
		public GameObject _pegboardB;

		private List<Peg> _pegboardAPegs = new List<Peg>();

		private List<Bomb> _pegboardABombs = new List<Bomb>();

		private List<Peg> _pegboardBPegs = new List<Peg>();

		private List<Bomb> _pegboardBBombs = new List<Bomb>();

		private PredictionManager _predictionManager;

		public bool pegboardAActive => _pegboardAActive;

		public List<Peg> pegboardAPegs => _pegboardAPegs;

		public List<Bomb> pegboardABombs => _pegboardABombs;

		public List<Peg> pegboardBPegs => _pegboardBPegs;

		public List<Bomb> pegboardBBombs => _pegboardBBombs;

		private IEnumerator Start()
		{
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			if (!isDummy)
			{
				Debug.Log("Setting pegboard A active");
				yield return WaitObjectAllocator.waitForEndOfFrame;
				_pegboardB.SetActive(value: false);
				_pegboardAPegs = new List<Peg>(_pegboardA.GetComponentsInChildren<Peg>(includeInactive: true));
				_pegboardABombs = new List<Bomb>(_pegboardA.GetComponentsInChildren<Bomb>(includeInactive: true));
				_pegboardBPegs = new List<Peg>(_pegboardB.GetComponentsInChildren<Peg>(includeInactive: true));
				_pegboardBBombs = new List<Bomb>(_pegboardB.GetComponentsInChildren<Bomb>(includeInactive: true));
			}
		}

		private void OnEnable()
		{
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(AlternateLayouts));
		}

		private void OnDisable()
		{
			SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(AlternateLayouts));
		}

		public void AlternateLayouts()
		{
			Debug.Log("Alternate layouts");
			_pegboardAActive = !_pegboardAActive;
			_pegboardA.SetActive(_pegboardAActive);
			_pegboardB.SetActive(!_pegboardAActive);
			_predictionManager.UpdateAllPegsStatus();
		}

		[Button(null, EButtonEnableMode.Always)]
		public void SwapLayoutsInEditor()
		{
			GameObject pegboardA = _pegboardA;
			GameObject pegboardB = _pegboardB;
			_pegboardB = pegboardA;
			_pegboardA = pegboardB;
		}

		[Button(null, EButtonEnableMode.Always)]
		public void ToggleActiveLayout()
		{
			_pegboardAActive = !_pegboardA.activeInHierarchy;
			_pegboardA.SetActive(_pegboardAActive);
			_pegboardB.SetActive(!_pegboardAActive);
		}
	}
}
