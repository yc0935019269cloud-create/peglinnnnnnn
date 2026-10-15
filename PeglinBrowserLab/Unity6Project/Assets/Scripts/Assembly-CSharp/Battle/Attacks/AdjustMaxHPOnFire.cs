using System.Globalization;
using I2.Loc;
using UnityEngine;

namespace Battle.Attacks
{
	public class AdjustMaxHPOnFire : MonoBehaviour
	{
		public float maxHPChange = 2f;

		private LocalizationParamsManager _paramsMan;

		private void SetDamageParam()
		{
			if (_paramsMan == null)
			{
				_paramsMan = GetComponent<LocalizationParamsManager>();
			}
			if (_paramsMan != null)
			{
				if (maxHPChange < 0f)
				{
					_paramsMan.SetParameterValue("MAX_HP_DEC", Mathf.Abs(maxHPChange).ToString(CultureInfo.InvariantCulture));
				}
				else
				{
					_paramsMan.SetParameterValue("MAX_HP_INC", maxHPChange.ToString(CultureInfo.InvariantCulture));
				}
			}
		}
	}
}
