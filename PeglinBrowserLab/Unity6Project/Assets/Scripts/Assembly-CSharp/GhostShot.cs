using Battle.Attacks;
using UnityEngine;

public class GhostShot : ShotBehavior
{
	private new Animator _anim;

	protected void OnEnable()
	{
		_anim = GetComponentInChildren<Animator>();
	}
}
