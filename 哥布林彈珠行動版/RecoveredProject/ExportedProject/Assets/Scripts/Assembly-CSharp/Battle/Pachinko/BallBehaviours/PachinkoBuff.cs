using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class PachinkoBuff : MonoBehaviour
	{
		public int buffAmount = 1;

		public void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue(LocParamKeys.BUFF_AMOUNT, buffAmount.ToString());
		}
	}
}
