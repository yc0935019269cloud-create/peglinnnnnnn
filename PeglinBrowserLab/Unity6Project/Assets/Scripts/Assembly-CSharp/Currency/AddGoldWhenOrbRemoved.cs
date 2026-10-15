using System.Globalization;
using I2.Loc;
using UnityEngine;

namespace Currency
{
	public class AddGoldWhenOrbRemoved : MonoBehaviour
	{
		public int goldToAdd = 100;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("ADD_GOLD", goldToAdd.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
