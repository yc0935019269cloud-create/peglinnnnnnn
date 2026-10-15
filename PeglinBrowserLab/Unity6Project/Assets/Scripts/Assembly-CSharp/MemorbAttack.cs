using Battle.Attacks;
using I2.Loc;
using UnityEngine;

public class MemorbAttack : TargetedAttack
{
	[Header("Memorb Specific")]
	public float pegsThisBattleDamageMod = 0.25f;

	private LocalizationParamsManager _paramsManager;

	private LocalizationParamsManager paramsManager
	{
		get
		{
			if (_paramsManager == null)
			{
				_paramsManager = GetComponent<LocalizationParamsManager>();
			}
			return _paramsManager;
		}
	}

	public override void UpdateBattleParameters()
	{
		base.UpdateBattleParameters();
		string paramValue = "";
		int num = Mathf.FloorToInt((float)AttackManager.NumPegsHitThisBattle * pegsThisBattleDamageMod);
		if (num >= 1)
		{
			paramValue = "(<style=dmg_bonus>+" + num + "</style>)";
		}
		paramsManager.SetParameterValue(LocParamKeys.MEMORB_DAMAGE_TOTAL, paramValue);
	}

	public override void ClearBattleParameters()
	{
		base.ClearBattleParameters();
		paramsManager.SetParameterValue(LocParamKeys.MEMORB_DAMAGE_TOTAL, "");
	}

	public override long GetDamage(AttackManager attackManager, int pegMultipliersTally, float dmgMult, long bonus, int critCount = 0, bool displayNegative = false)
	{
		return (long)Mathf.Floor((float)AttackManager.NumPegsHitThisBattle * pegsThisBattleDamageMod * dmgMult) + base.GetDamage(attackManager, pegMultipliersTally, dmgMult, bonus, critCount, displayNegative);
	}
}
