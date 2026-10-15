using System.Collections;
using System.Collections.Generic;
using Battle.PegBehaviour;
using PeglinUtils;
using UnityEngine;

public class PaintingBoardSection : NonDummyPegboardBehaviour
{
	[Tooltip("Used to determin what painting gets this section of pegs. Increasing index numbers are givento paintings from left to right on the battlefield.")]
	public List<int> forPaintingInSlotNumbers = new List<int>();

	public BoxCollider2D sectionCollider;

	private List<RegularPeg> _section;

	public List<RegularPeg> section => _section;

	private IEnumerator Start()
	{
		if (!isDummy)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_section = FindPegsInSection();
		}
	}

	private List<RegularPeg> FindPegsInSection()
	{
		ContactFilter2D contactFilter = default;
		contactFilter.NoFilter();
		List<Collider2D> list = new List<Collider2D>();
		sectionCollider.OverlapCollider(contactFilter, list);
		List<RegularPeg> list2 = new List<RegularPeg>();
		foreach (Collider2D item in list)
		{
			RegularPeg component = item.GetComponent<RegularPeg>();
			if (component != null && !list2.Contains(component) && component.canBeDull)
			{
				list2.Add(component);
			}
		}
		return list2;
	}
}
