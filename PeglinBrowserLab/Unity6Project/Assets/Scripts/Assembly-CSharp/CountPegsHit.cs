using System;
using Battle;
using UnityEngine;

public class CountPegsHit : MonoBehaviour
{
	private int count;

	private void OnEnable()
	{
		Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, (Peg.PegHitEvent)((Peg.PegType x, Peg y) =>
		{
			count++;
		}));
		BattleController.OnBombThrown = (Action)Delegate.Combine(BattleController.OnBombThrown, (Action)(() =>
		{
			Debug.Log($"Pegs destroyed {count}");
			count = 0;
		}));
	}
}
