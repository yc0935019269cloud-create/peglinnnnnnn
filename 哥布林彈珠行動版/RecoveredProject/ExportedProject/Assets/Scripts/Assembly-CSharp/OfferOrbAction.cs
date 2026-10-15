using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/OfferOrbAction")]
public class OfferOrbAction : ModifyStateAction
{
	[NonSerialized]
	public GameObject orbToRemove;

	[NonSerialized]
	public GameObject orbToUpgrade;

	public override ActionType GetActionType()
	{
		return ActionType.OFFER_ORB;
	}
}
