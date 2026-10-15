using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Enemies;
using Battle.PegBehaviour;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class CreateOrClaimPegboardShieldPegs : MonoBehaviour
	{
		public int numShieldPegs = 3;

		private List<PegShieldOverlay> _myShieldPegs;

		private BattleController _battleController;

		private void OnEnable()
		{
			PegShieldOverlay.OnShieldPegDestroyed = (PegShieldOverlay.ShieldPegDestroyedEvent)Delegate.Combine(PegShieldOverlay.OnShieldPegDestroyed, new PegShieldOverlay.ShieldPegDestroyedEvent(CheckShieldPegDestroyed));
		}

		private void OnDisable()
		{
			PegShieldOverlay.OnShieldPegDestroyed = (PegShieldOverlay.ShieldPegDestroyedEvent)Delegate.Remove(PegShieldOverlay.OnShieldPegDestroyed, new PegShieldOverlay.ShieldPegDestroyedEvent(CheckShieldPegDestroyed));
		}

		private void Start()
		{
			StartCoroutine(CheckForShieldPegs());
			_battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
		}

		private IEnumerator CheckForShieldPegs()
		{
			yield return null;
			_myShieldPegs = new List<PegShieldOverlay>();
			PegShieldOverlay[] array = UnityEngine.Object.FindObjectsOfType<PegShieldOverlay>();
			array.Shuffle();
			int num = 0;
			PegShieldOverlay[] array2 = array;
			foreach (PegShieldOverlay pegShieldOverlay in array2)
			{
				if (!pegShieldOverlay.isClaimed)
				{
					_myShieldPegs.Add(pegShieldOverlay);
					pegShieldOverlay.isClaimed = true;
					if (++num >= numShieldPegs)
					{
						break;
					}
				}
			}
			int num2 = numShieldPegs - num;
			if (num2 <= 0)
			{
				yield break;
			}
			List<Peg> list = _battleController.pegManager.ApplyShieldToRegularPegs(num2);
			if (list == null || list.Count <= 0)
			{
				yield break;
			}
			foreach (Peg item in list)
			{
				_myShieldPegs.Add(item.GetComponentInChildren<PegShieldOverlay>());
			}
		}

		public void OnEnemyDeath()
		{
			int num = 0;
			if (_myShieldPegs != null && _myShieldPegs.Count > 0)
			{
				foreach (PegShieldOverlay myShieldPeg in _myShieldPegs)
				{
					if (myShieldPeg != null && myShieldPeg.gameObject.activeInHierarchy && myShieldPeg.hitCount < myShieldPeg.hitLimit)
					{
						myShieldPeg.GetComponentInParent<Peg>().BreakShieldPegImmediately();
						num++;
					}
				}
				_myShieldPegs.Clear();
			}
			if (num >= numShieldPegs)
			{
				return;
			}
			PegShieldOverlay[] array = UnityEngine.Object.FindObjectsOfType<PegShieldOverlay>();
			array.Shuffle();
			int num2 = numShieldPegs - num;
			PegShieldOverlay[] array2 = array;
			foreach (PegShieldOverlay pegShieldOverlay in array2)
			{
				if (pegShieldOverlay.gameObject.activeInHierarchy && !pegShieldOverlay.isClaimed)
				{
					pegShieldOverlay.GetComponentInParent<Peg>().BreakShieldPegImmediately();
					if (--num2 <= 0)
					{
						break;
					}
				}
			}
		}

		private void CheckShieldPegDestroyed(PegShieldOverlay pegShield)
		{
			if (_myShieldPegs.Contains(pegShield) && TryGetComponent<Enemy>(out var component))
			{
				component.Damage(30L);
			}
		}
	}
}
