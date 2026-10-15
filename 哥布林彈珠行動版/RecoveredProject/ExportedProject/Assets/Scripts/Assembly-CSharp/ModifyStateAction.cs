using UnityEngine;

public abstract class ModifyStateAction : ScriptableObject
{
	public enum ActionType
	{
		CHANGE_STAT = 0,
		ADD_BALL = 1,
		REMOVE_BALL = 2,
		UPGRADE_BALL = 3,
		OFFER_ORB = 4
	}

	public abstract ActionType GetActionType();
}
