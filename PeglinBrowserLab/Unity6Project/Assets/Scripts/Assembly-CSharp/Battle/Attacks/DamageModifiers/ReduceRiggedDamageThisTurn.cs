using I2.Loc;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class ReduceRiggedDamageThisTurn : MonoBehaviour
	{
		public int reductionAmount = 2;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("RIGGED_REDUCTION", reductionAmount.ToString());
		}
	}
}
