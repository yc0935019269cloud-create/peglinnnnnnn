using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using PeglinUtils;
using UnityEngine;

[CreateAssetMenu(menuName = "Deck/Orb Pool")]
public class OrbPool : ScriptableObject
{
	[ReorderableList]
	public GameObject[] AvailableOrbs;

	private readonly List<GameObject> _availableOrbs = new List<GameObject>();

	public GameObject[] GetRandomOrbs(int numberOfOrbs)
	{
		if (_availableOrbs.Count == 0)
		{
			_availableOrbs.AddRange(AvailableOrbs);
		}
		_availableOrbs.Shuffle();
		GameObject[] array = new GameObject[numberOfOrbs];
		for (int i = 0; i < numberOfOrbs; i++)
		{
			array[i] = _availableOrbs[i];
		}
		return array;
	}

	private void OnValidate()
	{
		if (AvailableOrbs.Length != AvailableOrbs.Distinct().Count())
		{
			Debug.LogWarning("Orb Pool " + base.name + " contains duplicate items.");
		}
	}
}
