using Relics;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/AddRelicAction")]
public class AddRelic : ModifyStateAction
{
	[Tooltip("If null, we'll roll one with the given options.")]
	public Relic relic;

	[Header("Random Parameters")]
	public RelicRarity rarity;

	public override ActionType GetActionType()
	{
		return ActionType.ADD_BALL;
	}
}
