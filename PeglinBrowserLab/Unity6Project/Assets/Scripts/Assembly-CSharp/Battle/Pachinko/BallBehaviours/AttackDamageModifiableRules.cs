using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AttackDamageModifiableRules : MonoBehaviour
	{
		public bool baseDamageNonMod;

		public bool critDamageNonMod;

		public bool regularBoostAppliesToCrit;

		public bool critBoostAppliesToRegular;

		public float regularBoostAppliesToCritMod = 1f;

		public float critBoostAppliesToRegularMod = 1f;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("REG_TO_CRIT_BOOST_MOD", regularBoostAppliesToCritMod.ToString());
				component.SetParameterValue("CRIT_TO_REG_BOOST_MOD", critBoostAppliesToRegularMod.ToString());
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager   ");
			}
		}
	}
}
