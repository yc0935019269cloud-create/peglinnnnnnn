using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AddCoinsWhenDrawn : MonoBehaviour
	{
		public int numToCreate = 1;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("COINS_ON_DRAW", numToCreate.ToString());
			}
		}
	}
}
