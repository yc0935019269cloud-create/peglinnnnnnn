using Battle.Enemies;
using UnityEngine;

public class BarricadeEnemy : Enemy
{
	public override bool CanBeHealed => false;

	public override bool Move(Vector3 position, int slotIndex = -1)
	{
		return false;
	}

	public override void ToggleTargetedUI(bool on)
	{
		_targetingUI.GetComponent<TargetingUI>().SetTargeting(status: false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!_animationHasFinished)
		{
			_animationHasFinished = true;
		}
		_cbOnDamagedAnimationEnd?.Invoke(this);
	}

	public override void Heal(float amount)
	{
	}
}
