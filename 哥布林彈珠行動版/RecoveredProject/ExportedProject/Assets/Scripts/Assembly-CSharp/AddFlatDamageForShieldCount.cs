using Battle;
using Battle.Attacks.DamageModifiers;
using I2.Loc;
using Relics;
using UnityEngine;

public class AddFlatDamageForShieldCount : AttackFlatDamageModifier
{
	public float shieldDamageMultiplier = 1f;

	public float shieldPegBuffsDamageMultiplier = 1f;

	public float shieldPegDebuffsDamageMultiplier = 2f;

	private PegManager _pegManager;

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("SHIELD_DAMAGE_MULTIPLIER", shieldDamageMultiplier.ToString());
		}
		else
		{
			Debug.Log(base.gameObject.name + " does not have loc param manager   ");
		}
	}

	private void Start()
	{
		if (Object.FindObjectOfType<BattleController>() != null)
		{
			_pegManager = Object.FindObjectOfType<BattleController>().pegManager;
		}
	}

	public override int GetFlatDamageMod(DeckManager deckManager, RelicManager relicManager, int critCount = 0)
	{
		if (_pegManager != null)
		{
			var (num, num2, num3) = _pegManager.CountShieldPegsOnBoard();
			return Mathf.RoundToInt((float)num * shieldDamageMultiplier + (float)num2 * shieldPegBuffsDamageMultiplier + (float)(-num3) * shieldPegDebuffsDamageMultiplier);
		}
		return 0;
	}
}
