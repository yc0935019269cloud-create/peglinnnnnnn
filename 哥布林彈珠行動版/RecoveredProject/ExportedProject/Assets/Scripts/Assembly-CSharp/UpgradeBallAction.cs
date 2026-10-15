using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/UpgradeBallAction")]
public class UpgradeBallAction : ModifyStateAction
{
	public GameObject BallToUpgrade;

	public override ActionType GetActionType()
	{
		return ActionType.UPGRADE_BALL;
	}
}
