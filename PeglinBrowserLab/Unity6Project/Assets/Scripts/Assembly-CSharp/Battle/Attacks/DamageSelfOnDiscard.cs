using I2.Loc;
using UnityEngine;

namespace Battle.Attacks
{
	public class DamageSelfOnDiscard : MonoBehaviour
	{
		public float selfDamage = 4f;

		public bool unblockable;

		private LocalizationParamsManager _paramsMan;

		private void SetDamageParam()
		{
			if (_paramsMan == null)
			{
				_paramsMan = GetComponent<LocalizationParamsManager>();
			}
			if (_paramsMan != null)
			{
				_paramsMan.SetParameterValue("DISCARD_DAMAGE", selfDamage.ToString());
				_paramsMan.SetParameterValue("DISCARD_DAMAGE_INC", selfDamage.ToString());
			}
		}
	}
}
