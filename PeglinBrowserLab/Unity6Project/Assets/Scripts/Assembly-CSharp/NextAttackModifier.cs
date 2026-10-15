using Battle.Attacks.AttackModifications;
using UnityEngine;

public class NextAttackModifier : MonoBehaviour
{
	public AttackModifier attackModifier;

	private void Awake()
	{
		if (attackModifier == null)
		{
			attackModifier = new AttackModifier();
		}
	}

	public virtual void UpdateAttackModifier()
	{
	}
}
