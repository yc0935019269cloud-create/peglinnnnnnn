using System.Globalization;
using Battle.Attacks.Interfaces;
using I2.Loc;
using UnityEngine;

namespace Battle.Attacks
{
	public class AttackDamagesSelf : MonoBehaviour, IModifiesBattleStats
	{
		public float selfDamage = 3f;

		public float selfDamageIncrease;

		public bool blockable;

		private LocalizationParamsManager _paramsMan;

		private void SetParams()
		{
			if (_paramsMan == null)
			{
				_paramsMan = GetComponent<LocalizationParamsManager>();
			}
			if (_paramsMan != null)
			{
				_paramsMan.SetParameterValue("SELF_DAMAGE", selfDamage.ToString(CultureInfo.InvariantCulture));
				_paramsMan.SetParameterValue("SELF_DAMAGE_INC", selfDamageIncrease.ToString(CultureInfo.InvariantCulture));
				if (selfDamage < 0f)
				{
					_paramsMan.SetParameterValue("HEAL_ON_FIRE", Mathf.Abs(selfDamage).ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		public void IncreaseSelfDamage()
		{
			if (selfDamageIncrease != 0f)
			{
				selfDamage += selfDamageIncrease;
				SetParams();
			}
		}

		public void ModifyStatsOnFire()
		{
			IncreaseSelfDamage();
		}
	}
}
