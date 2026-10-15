using System.Collections.Generic;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegSquareMovementManager : MonoBehaviour
	{
		public List<PegSquareMovement> pegMovers = new List<PegSquareMovement>();

		private void Start()
		{
			TellMoversToMove();
		}

		public void TellMoversToMove()
		{
			foreach (PegSquareMovement pegMover in pegMovers)
			{
				pegMover.DoMove();
			}
		}

		public void AddPegMover(PegSquareMovement m)
		{
			pegMovers.Add(m);
		}
	}
}
