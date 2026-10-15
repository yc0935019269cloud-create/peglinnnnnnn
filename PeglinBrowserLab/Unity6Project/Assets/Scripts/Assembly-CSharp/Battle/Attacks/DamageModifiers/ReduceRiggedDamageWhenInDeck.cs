using I2.Loc;
using UnityEngine;

namespace Battle.Attacks.DamageModifiers
{
	public class ReduceRiggedDamageWhenInDeck : MonoBehaviour
	{
		public int reductionAmount = 1;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("RIGGED_REDUCTION_WHEN_IN_DECK", reductionAmount.ToString());
		}
	}
}
