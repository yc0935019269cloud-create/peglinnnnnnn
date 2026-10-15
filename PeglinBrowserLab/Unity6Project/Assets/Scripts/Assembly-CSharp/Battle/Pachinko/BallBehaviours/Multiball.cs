using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class Multiball : MonoBehaviour
	{
		public int multiballLevel = 2;

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue(LocParamKeys.MULTIBALL_LEVEL, multiballLevel.ToString());
		}
	}
}
