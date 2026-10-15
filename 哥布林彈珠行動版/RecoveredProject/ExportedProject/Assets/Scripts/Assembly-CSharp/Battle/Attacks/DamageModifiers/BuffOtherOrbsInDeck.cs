using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class BuffOtherOrbsInDeck : MonoBehaviour
	{
		public int regularDamage = 1;

		public int critDamage = 2;

		public List<string> orbLocStringNamesToBuff = new List<string>();

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			component.SetParameterValue("REG_DMG_TO_GIVE", regularDamage.ToString());
			component.SetParameterValue("CRIT_DMG_TO_GIVE", critDamage.ToString());
		}
	}
}
