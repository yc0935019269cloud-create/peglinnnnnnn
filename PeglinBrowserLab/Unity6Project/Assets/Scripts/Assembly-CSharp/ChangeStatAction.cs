using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Actions/ChangeStatAction")]
public class ChangeStatAction : ModifyStateAction
{
	public FloatVariable variableToAffect;

	public float amount;

	public override ActionType GetActionType()
	{
		return ActionType.CHANGE_STAT;
	}
}
