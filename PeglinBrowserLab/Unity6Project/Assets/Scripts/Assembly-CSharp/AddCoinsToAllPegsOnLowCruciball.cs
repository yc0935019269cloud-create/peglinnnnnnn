using Cruciball;
using UnityEngine;

public class AddCoinsToAllPegsOnLowCruciball : MonoBehaviour
{
	public CruciballManager cruciballManager;

	private void Awake()
	{
		if (cruciballManager.IsLessGoldAvailable())
		{
			return;
		}
		Peg[] componentsInChildren = GetComponentsInChildren<Peg>();
		foreach (Peg peg in componentsInChildren)
		{
			if (!(peg is Bomb))
			{
				peg.numCoinOverride = 1;
			}
		}
	}
}
