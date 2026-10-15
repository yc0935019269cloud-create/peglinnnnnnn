using System.Collections.Generic;
using Battle.Attacks;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionExecutor", menuName = "Managers/ActionExecutor")]
public class ActionExecutor : ScriptableObject
{
	public DeckManager deckManager;

	public void ExecuteActions(List<ModifyStateAction> actions)
	{
		foreach (ModifyStateAction action in actions)
		{
			switch (action.GetActionType())
			{
			case ModifyStateAction.ActionType.CHANGE_STAT:
			{
				ChangeStatAction changeStatAction = action as ChangeStatAction;
				changeStatAction.variableToAffect.Add(changeStatAction.amount);
				break;
			}
			case ModifyStateAction.ActionType.ADD_BALL:
			{
				AddBallAction addBallAction = action as AddBallAction;
				deckManager.AddOrbToDeck(addBallAction.ballToAdd);
				break;
			}
			case ModifyStateAction.ActionType.REMOVE_BALL:
			{
				RemoveOrbAction removeOrbAction = action as RemoveOrbAction;
				if (removeOrbAction.random || removeOrbAction.orbToRemove == null)
				{
					deckManager.RemoveRandomOrbFromDeck();
				}
				else
				{
					deckManager.RemoveSpecifiedOrbFromDeck(removeOrbAction.orbToRemove);
				}
				break;
			}
			case ModifyStateAction.ActionType.UPGRADE_BALL:
			{
				UpgradeBallAction upgradeBallAction = action as UpgradeBallAction;
				if (upgradeBallAction.BallToUpgrade == null)
				{
					deckManager.UpgradeRandomOrb();
				}
				else
				{
					upgradeBallAction.BallToUpgrade.GetComponent<Attack>();
				}
				break;
			}
			case ModifyStateAction.ActionType.OFFER_ORB:
			{
				OfferOrbAction offerOrbAction = action as OfferOrbAction;
				deckManager.UpgradeSpecificOrb(offerOrbAction.orbToUpgrade);
				deckManager.RemoveSpecifiedOrbFromDeck(offerOrbAction.orbToRemove);
				break;
			}
			default:
				Debug.LogWarning("ACTION TYPE " + action.GetActionType().ToString() + " NOT YET IMPLEMENTED.");
				break;
			}
		}
	}
}
