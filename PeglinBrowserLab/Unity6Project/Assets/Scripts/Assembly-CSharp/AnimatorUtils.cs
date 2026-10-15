using UnityEngine;

public class AnimatorUtils : MonoBehaviour
{
	public static bool HasParameter(Animator anim, string parameter)
	{
		AnimatorControllerParameter[] parameters = anim.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == parameter)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyAnimationIsPlaying(Animator anim)
	{
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		Debug.Log($"State name: {currentAnimatorStateInfo}, length {currentAnimatorStateInfo.length}, normalizedTime {currentAnimatorStateInfo.normalizedTime}");
		if (!(currentAnimatorStateInfo.length <= 0f))
		{
			return currentAnimatorStateInfo.normalizedTime < 1f;
		}
		return true;
	}

	public static bool AnimationIsPlaying(Animator anim, string state)
	{
		return anim.GetCurrentAnimatorStateInfo(0).IsName(state);
	}
}
