using I2.Loc;
using UnityEngine;

namespace Battle.Attacks
{
	public class EchoAttack : ProjectileAttack
	{
		[Header("Echo Specific")]
		public float prevAttackDmgMod = 0.5f;

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
			int num = Mathf.FloorToInt((float)AttackManager.PreviousAttackDamage * prevAttackDmgMod);
			if (num >= 1)
			{
				paramValue = "(<style=dmg_bonus>+" + num + "</style>)";
			}
			paramsManager.SetParameterValue("ECHO_DMG", paramValue);
		}

		public override void ClearBattleParameters()
		{
			base.ClearBattleParameters();
			paramsManager.SetParameterValue("ECHO_DMG", "");
		}
	}
}
