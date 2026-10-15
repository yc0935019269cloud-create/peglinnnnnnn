using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Orb Data")]
public class OrbData : ScriptableObject
{
	public float DamagePerPeg;

	public float CritDamagePerPeg;

	public Animator WidgetAnimator;
}
