using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Attack")]
public class EnemyAttack : ScriptableObject
{
	public int AttackRange = 2;

	public string MeleeAttackTrigger = "sword_slash";

	public string RangedAttackAnimation = "pea_shot";
}
