using UnityEngine;

namespace Battle.Attacks.AttackModifications
{
	[CreateAssetMenu(menuName = "Battle/Base Attack Modifier Data")]
	public class BaseAttackModifierData : ScriptableObject
	{
		[Header("Rubbery")]
		public PhysicsMaterial2D rubberMaterial;

		[Header("Attack All")]
		public GameObject attackAllShot;
	}
}
