using Battle;
using Battle.Attacks.DamageModifiers;
using I2.Loc;
using Relics;
using UnityEngine;

public class AddFlatDamageForSpecialPegCount : AttackFlatDamageModifier
{
	public Peg.PegType pegType;

	public bool riggedBomb;

	public float damageMultiplier = 1f;

	private PegManager _pegManager;

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("SPECIAL_PEG_DAMAGE_MULTIPLIER", damageMultiplier.ToString());
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
			return Mathf.RoundToInt((float)_pegManager.CountSpecialPegsOnBoard(pegType, riggedBomb) * damageMultiplier);
		}
		return 0;
	}
}
