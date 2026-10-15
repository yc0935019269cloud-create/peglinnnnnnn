using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/RemoveOrbAction")]
public class RemoveOrbAction : ModifyStateAction
{
	public bool random = true;

	public GameObject orbToRemove;

	public override ActionType GetActionType()
	{
		return ActionType.REMOVE_BALL;
	}
}
