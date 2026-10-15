using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/AddBallAction")]
public class AddBallAction : ModifyStateAction
{
	public GameObject ballToAdd;

	public override ActionType GetActionType()
	{
		return ActionType.ADD_BALL;
	}
}
