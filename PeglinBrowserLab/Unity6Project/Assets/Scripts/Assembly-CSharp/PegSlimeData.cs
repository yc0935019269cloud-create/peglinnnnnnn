using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Peg Slime Data")]
public class PegSlimeData : ScriptableObject
{
	public Color enemyColor;

	public Color webColor;

	public Color yolkColor;

	public Color lightningColor;

	public Color bounceColor;

	public Color poisonColor;

	public Color healColor;

	public Color doubleDamageColor;

	public Color damageReductionColor;

	public PhysicsMaterial2D slowMaterial;

	public PhysicsMaterial2D noBounceMaterial;

	public PhysicsMaterial2D yolkMaterial;

	public PhysicsMaterial2D bouncyMaterial;
}
